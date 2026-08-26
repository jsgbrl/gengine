// Reading a controller without stalling the game.
//
// A HID read blocks until the device sends something. A DualSense sends about 250 reports a
// second, so at 60 frames a second the game would spend most of every frame waiting - and
// when the cable comes out it would wait for ever. So the read lives on its own thread, which
// keeps only the newest report; the game takes a copy when it feels like it and never waits.
//
// The thread is a background thread on purpose. A blocked native read cannot be interrupted
// from managed code, so on the way out the process must be allowed to exit with the thread
// still sitting inside it - which is exactly what IsBackground means.

using System;
using System.Threading;

namespace GEngine.Input.Hid;

/// <summary>Reads a HID device on a background thread and publishes the newest report.</summary>
public sealed class HidReportReader : IDisposable
{
    private readonly object _gate = new();
    private readonly IHidDevice _device;
    private readonly byte[] _scratch;
    private readonly byte[] _latest;
    private Thread? _thread;
    private volatile bool _isRunning;
    private int _latestLength;

    /// <summary>Creates a reader over an open device.</summary>
    /// <param name="device">The device to read.</param>
    /// <param name="reportLength">How many bytes one report is.</param>
    public HidReportReader(IHidDevice device, int reportLength = KnownDevices.UsbReportLength)
    {
        ArgumentNullException.ThrowIfNull(device);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(reportLength);
        _device = device;
        _scratch = new byte[reportLength];
        _latest = new byte[reportLength];
    }

    /// <summary>False once the device has gone away.</summary>
    public bool IsConnected { get; private set; } = true;

    /// <summary>How many reports have arrived since the reader was created.</summary>
    public long ReportCount { get; private set; }

    /// <summary>True once at least one report has arrived.</summary>
    public bool HasReport => ReportCount > 0;

    /// <summary>How many bytes the most recent report was. Zero before the first one.</summary>
    public int LastReportLength => _latestLength;

    /// <summary>Starts the background thread. Calling it twice does nothing.</summary>
    public void Start()
    {
        if (_thread is not null)
        {
            return;
        }

        _isRunning = true;
        _thread = new Thread(ReadUntilGone)
        {
            IsBackground = true,
            Name = "gengine hid reader",
        };

        _thread.Start();
    }

    /// <summary>Reads exactly one report, blocking. This is what the thread does in a loop.</summary>
    /// <returns>False when the device has gone, in which case the reader is disconnected.</returns>
    public bool PumpOnce()
    {
        int read = ReadSafely();
        if (read <= 0)
        {
            IsConnected = false;
            return false;
        }

        lock (_gate)
        {
            Array.Copy(_scratch, _latest, read);
            _latestLength = read;
            ReportCount++;
        }

        return true;
    }

    /// <summary>Copies the newest report into a buffer.</summary>
    /// <param name="destination">Where to put it. Must be at least one report long.</param>
    /// <returns>False when no report has arrived yet.</returns>
    public bool TryTakeLatest(byte[] destination)
    {
        ArgumentNullException.ThrowIfNull(destination);
        lock (_gate)
        {
            if (_latestLength == 0)
            {
                return false;
            }

            Array.Copy(_latest, destination, Math.Min(_latestLength, destination.Length));
            return true;
        }
    }

    /// <summary>Stops the thread and closes the device.</summary>
    public void Dispose()
    {
        _isRunning = false;
        IsConnected = false;
        _device.Dispose();
        _thread = null;
    }

    private void ReadUntilGone()
    {
        while (_isRunning && PumpOnce())
        {
            // Everything happens in PumpOnce; the loop exists only to keep doing it.
        }
    }

    // A device that vanishes mid-read throws whatever its operating system throws. From up
    // here the answer is the same in every case: it is gone.
    private int ReadSafely()
    {
        try
        {
            return _device.Read(_scratch);
        }
        catch (Exception)
        {
            return 0;
        }
    }
}
