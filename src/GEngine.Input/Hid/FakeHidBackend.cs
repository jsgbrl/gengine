// A system with whatever devices a test says it has. Enumeration can be changed between
// calls, which is how "the controller was unplugged and plugged back in" is written down.

using System;
using System.Collections.Generic;

namespace GEngine.Input.Hid;

/// <summary>A HID backend over a list of devices a test controls.</summary>
public sealed class FakeHidBackend : IHidBackend
{
    private readonly List<HidDeviceInfo> _devices = [];
    private readonly Dictionary<string, FakeHidDevice> _openable = [];

    /// <inheritdoc/>
    public string Name => "fake";

    /// <summary>How many times a device has been opened through this backend.</summary>
    public int OpenCount { get; private set; }

    /// <summary>When true, <see cref="Open"/> refuses, as a permission failure would.</summary>
    public bool RefusesToOpen { get; set; }

    /// <summary>Adds a device to what enumeration reports, and what opening it returns.</summary>
    /// <param name="info">How the device describes itself.</param>
    /// <param name="device">What opening it gives back.</param>
    /// <returns>This backend, so devices can be chained.</returns>
    public FakeHidBackend With(HidDeviceInfo info, FakeHidDevice device)
    {
        ArgumentNullException.ThrowIfNull(device);
        _devices.Add(info);
        _openable[info.Path] = device;
        return this;
    }

    /// <summary>Removes every device, as unplugging the last one would.</summary>
    public void UnplugAll()
    {
        foreach (FakeHidDevice device in _openable.Values)
        {
            device.Unplug();
        }

        _devices.Clear();
        _openable.Clear();
    }

    /// <inheritdoc/>
    public IReadOnlyList<HidDeviceInfo> Enumerate()
    {
        List<HidDeviceInfo> found = [.. _devices];
        found.Sort(static (left, right) => string.CompareOrdinal(left.Path, right.Path));
        return found;
    }

    /// <inheritdoc/>
    public IHidDevice? Open(HidDeviceInfo device)
    {
        if (RefusesToOpen || !_openable.TryGetValue(device.Path, out FakeHidDevice? opened))
        {
            return null;
        }

        OpenCount++;
        return opened;
    }
}
