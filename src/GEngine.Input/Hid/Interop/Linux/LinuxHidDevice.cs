// An open /dev/hidraw file. There is nothing platform-specific left at this point: the
// kernel already turned the USB traffic into a stream of fixed-size packets, and reading one
// is reading a file.

using System;
using System.IO;

namespace GEngine.Input.Hid.Interop.Linux;

/// <summary>A HID device opened as a file on Linux.</summary>
public sealed class LinuxHidDevice : IHidDevice
{
    private readonly FileStream _stream;

    /// <summary>Creates a device over an open stream.</summary>
    /// <param name="path">The device node, such as /dev/hidraw0.</param>
    /// <param name="stream">The open stream. This device takes ownership of it.</param>
    public LinuxHidDevice(string path, FileStream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        Path = path;
        _stream = stream;
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
        if (!IsOpen)
        {
            return 0;
        }

        try
        {
            return _stream.Read(buffer, 0, buffer.Length);
        }
        catch (IOException)
        {
            IsOpen = false;
            return 0;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        IsOpen = false;
        _stream.Dispose();
    }
}
