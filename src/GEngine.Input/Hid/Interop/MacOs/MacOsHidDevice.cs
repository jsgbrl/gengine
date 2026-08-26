// An open IOKit device, read through its own run loop.
//
// This is the one place where the three systems genuinely differ in shape. On Linux and
// Windows a read blocks until a report arrives; on macOS nothing blocks - the device calls
// back into a run loop, and running that loop is the caller's job. So Read runs the loop in
// short slices until the callback fires, which turns a push into the pull the rest of the
// engine expects, on the same thread and with no second thread to synchronise.
//
// Two details are easy to get wrong and are called out below: the buffer given to IOKit has
// to be pinned for as long as IOKit holds it, and the delegate has to be kept alive by a
// field or the collector will free the function pointer that IOKit is about to call.

using System;
using System.Runtime.InteropServices;

namespace GEngine.Input.Hid.Interop.MacOs;

/// <summary>A HID device opened through IOKit on macOS.</summary>
public sealed class MacOsHidDevice : IHidDevice
{
    /// <summary>How long one slice of the run loop waits before checking whether to give up.</summary>
    public const double RunLoopSliceSeconds = 0.05;

    private readonly NativeMethods.InputReportCallback _callback;
    private readonly byte[] _reportBuffer;
    private readonly GCHandle _pinned;
    private readonly IntPtr _device;
    private readonly IntPtr _runLoopMode;
    private IntPtr _runLoop;
    private int _receivedLength;

    /// <summary>Creates a device over an opened IOHIDDeviceRef.</summary>
    /// <param name="path">The identity the backend gave this device.</param>
    /// <param name="device">The IOHIDDeviceRef. This device releases it.</param>
    /// <param name="reportLength">How many bytes one report is.</param>
    public MacOsHidDevice(string path, IntPtr device, int reportLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(reportLength);
        Path = path;
        _device = device;
        _reportBuffer = new byte[reportLength];

        // Pinned because IOKit keeps the address and writes into it from its own thread; the
        // callback is held in a field for the same reason, one level up.
        _pinned = GCHandle.Alloc(_reportBuffer, GCHandleType.Pinned);
        _callback = OnReport;
        _runLoopMode = CoreFoundationStrings.Create(CoreFoundationStrings.DefaultRunLoopMode);
        NativeMethods.IOHIDDeviceRegisterInputReportCallback(
            device,
            _pinned.AddrOfPinnedObject(),
            new IntPtr(reportLength),
            _callback,
            IntPtr.Zero);

        IsOpen = true;
    }

    /// <inheritdoc/>
    public string Path { get; }

    /// <inheritdoc/>
    public bool IsOpen { get; private set; }

    /// <inheritdoc/>
    public int Read(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        ScheduleOnThisThread();
        while (IsOpen)
        {
            int result = NativeMethods.CFRunLoopRunInMode(_runLoopMode, RunLoopSliceSeconds, true);
            if (result is NativeMethods.RunLoopFinished or NativeMethods.RunLoopStopped)
            {
                IsOpen = false;
                return 0;
            }

            int taken = Take(buffer);
            if (taken > 0)
            {
                return taken;
            }
        }

        return 0;
    }

    /// <summary>
    /// Releases the pinned buffer and the Core Foundation objects. The finalizer is here
    /// because those are unmanaged: a device dropped without a Dispose would otherwise leave a
    /// pinned array in the heap and a retained IOKit object for the life of the process.
    /// </summary>
    ~MacOsHidDevice()
    {
        ReleaseEverything();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        ReleaseEverything();
        GC.SuppressFinalize(this);
    }

    private void ReleaseEverything()
    {
        IsOpen = false;
        if (_runLoop != IntPtr.Zero)
        {
            NativeMethods.IOHIDDeviceUnscheduleFromRunLoop(_device, _runLoop, _runLoopMode);
            _runLoop = IntPtr.Zero;
        }

        _ = NativeMethods.IOHIDDeviceClose(_device, 0);
        CoreFoundationStrings.Release(_runLoopMode);
        CoreFoundationStrings.Release(_device);
        if (_pinned.IsAllocated)
        {
            _pinned.Free();
        }
    }

    // Scheduling has to happen on the thread that will run the loop, and that thread is
    // whichever one first calls Read - the reader thread. Doing it in the constructor would
    // schedule the device on the game thread's run loop, which nobody ever runs.
    private void ScheduleOnThisThread()
    {
        if (_runLoop != IntPtr.Zero)
        {
            return;
        }

        _runLoop = NativeMethods.CFRunLoopGetCurrent();
        NativeMethods.IOHIDDeviceScheduleWithRunLoop(_device, _runLoop, _runLoopMode);
    }

    private int Take(byte[] buffer)
    {
        int length = _receivedLength;
        if (length <= 0)
        {
            return 0;
        }

        _receivedLength = 0;
        int copied = Math.Min(length, buffer.Length);
        Array.Copy(_reportBuffer, buffer, copied);
        return copied;
    }

    // IOKit has already written the report into the pinned buffer, so the pointer it passes is
    // the buffer this object owns and there is nothing to copy here.
    private void OnReport(IntPtr context, int result, IntPtr sender, uint type, uint reportId, IntPtr report, IntPtr reportLength)
    {
        if (result == 0)
        {
            // Never trust the length past the size of the buffer that was handed over.
            _receivedLength = (int)Math.Min(reportLength.ToInt64(), _reportBuffer.Length);
        }
    }
}
