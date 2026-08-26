// An open HID device: read bytes until it goes away. Three lines, because that is genuinely
// all three operating systems agree on, and because everything above this line - decoding,
// dead zones, hot-plug - can then be written once and tested without hardware.

using System;

namespace GEngine.Input.Hid;

/// <summary>An opened HID device that reports fixed-size packets.</summary>
public interface IHidDevice : IDisposable
{
    /// <summary>What the system called this device.</summary>
    string Path { get; }

    /// <summary>False once the device has gone away or been closed.</summary>
    bool IsOpen { get; }

    /// <summary>
    /// Reads one report, blocking until one arrives. Returning zero means the device is gone;
    /// the caller must treat that as an unplug rather than as an empty report.
    /// </summary>
    /// <param name="buffer">Where to put the report. Must be at least as long as one report.</param>
    /// <returns>How many bytes were read, or zero when the device has gone.</returns>
    int Read(byte[] buffer);
}
