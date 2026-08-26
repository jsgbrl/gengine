// A controller made of text. It hands out recorded reports one Read at a time, and can be
// told to stop - which is what an unplugged cable looks like from up here, and the only way
// to test hot-plug without a cable.

using System;
using System.Collections.Generic;

namespace GEngine.Input.Hid;

/// <summary>A HID device that replays recorded reports.</summary>
public sealed class FakeHidDevice : IHidDevice
{
    private readonly List<byte[]> _reports = [];
    private int _next;

    /// <summary>Creates a device with no reports in it yet.</summary>
    /// <param name="path">What to report as the device path.</param>
    public FakeHidDevice(string path = "fake://dualsense")
    {
        Path = path;
    }

    /// <inheritdoc/>
    public string Path { get; }

    /// <inheritdoc/>
    public bool IsOpen { get; private set; } = true;

    /// <summary>How many reports are queued.</summary>
    public int ReportCount => _reports.Count;

    /// <summary>How many reads have been served.</summary>
    public int ReadCount { get; private set; }

    /// <summary>When true, the last report is repeated for ever instead of running out.</summary>
    public bool RepeatsLastReport { get; set; } = true;

    /// <summary>Queues a report.</summary>
    /// <param name="report">The bytes, as the device would send them.</param>
    /// <returns>This device, so reports can be chained.</returns>
    public FakeHidDevice Queue(byte[] report)
    {
        ArgumentNullException.ThrowIfNull(report);
        _reports.Add(report);
        return this;
    }

    /// <summary>Queues a report written in hexadecimal.</summary>
    /// <param name="hex">The report, such as "01 80 80 80 80 00 00 00 08 00 00".</param>
    /// <returns>This device, so reports can be chained.</returns>
    public FakeHidDevice QueueHex(string hex) => Queue(HidReportHex.Parse(hex));

    /// <summary>Pulls the cable out. Every later read reports the device as gone.</summary>
    public void Unplug() => IsOpen = false;

    /// <inheritdoc/>
    public int Read(byte[] buffer)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        if (!IsOpen || _reports.Count == 0)
        {
            return 0;
        }

        byte[] report = _reports[Math.Min(_next, _reports.Count - 1)];
        if (_next >= _reports.Count && !RepeatsLastReport)
        {
            return 0;
        }

        _next++;
        ReadCount++;
        int length = Math.Min(report.Length, buffer.Length);
        Array.Copy(report, buffer, length);
        return length;
    }

    /// <inheritdoc/>
    public void Dispose() => IsOpen = false;
}
