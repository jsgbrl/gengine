// An open HID handle. ReadFile blocks until the device sends a report, which is exactly what
// HidReportReader's background thread wants - and exactly why that thread has to be a
// background thread: a blocked ReadFile cannot be cancelled from managed code, so the process
// has to be allowed to exit with the thread still inside it.

using System;
using Microsoft.Win32.SafeHandles;

namespace GEngine.Input.Hid.Interop.Windows;

/// <summary>A HID device opened through kernel32 on Windows.</summary>
public sealed class WindowsHidDevice : IHidDevice
{
    private readonly SafeFileHandle _handle;

    /// <summary>Creates a device over an open handle.</summary>
    /// <param name="path">The device path the system gave.</param>
    /// <param name="handle">The open handle. This device takes ownership of it.</param>
    public WindowsHidDevice(string path, SafeFileHandle handle)
    {
        ArgumentNullException.ThrowIfNull(handle);
        Path = path;
        _handle = handle;
        IsOpen = !handle.IsInvalid;
    }

    /// <inheritdoc/>
    public string Path { get; }

    /// <inheritdoc/>
    public bool IsOpen { get; private set; }

    /// <inheritdoc/>
    public int Read(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (!IsOpen)
        {
            return 0;
        }

        if (!NativeMethods.ReadFile(_handle, buffer, buffer.Length, out int read, IntPtr.Zero))
        {
            IsOpen = false;
            return 0;
        }

        return read;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        IsOpen = false;
        _handle.Dispose();
    }
}
