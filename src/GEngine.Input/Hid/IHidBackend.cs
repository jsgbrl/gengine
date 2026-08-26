// Finding and opening devices. One interface, three implementations - Adapter, one per
// operating system - and a fake that plays recorded reports so everything above it can be
// tested with no controller plugged in.

using System.Collections.Generic;

namespace GEngine.Input.Hid;

/// <summary>Enumerates and opens the HID devices of one system.</summary>
public interface IHidBackend
{
    /// <summary>Human-readable name, shown in diagnostics.</summary>
    string Name { get; }

    /// <summary>Lists every HID device the system will admit to, sorted by path.</summary>
    /// <returns>The devices. An empty list is a normal answer, not an error.</returns>
    IReadOnlyList<HidDeviceInfo> Enumerate();

    /// <summary>Opens a device for reading.</summary>
    /// <param name="device">One of the devices <see cref="Enumerate"/> returned.</param>
    /// <returns>The open device, or null when it could not be opened.</returns>
    IHidDevice? Open(HidDeviceInfo device);
}
