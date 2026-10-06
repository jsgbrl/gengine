// The controllers gengine recognises, as a table rather than as a pair of constants buried
// in an if. Adding the next revision of the hardware is one line here and nothing else.

using System.Collections.Generic;

namespace GEngine.Input.Hid;

/// <summary>The gamepads the engine knows how to decode.</summary>
public static class KnownDevices
{
    /// <summary>Sony Interactive Entertainment.</summary>
    public const int SonyVendorId = 0x054C;

    /// <summary>The DualSense that shipped with the PlayStation 5.</summary>
    public const int DualSenseProductId = 0x0CE6;

    /// <summary>The DualSense Edge.</summary>
    public const int DualSenseEdgeProductId = 0x0DF2;

    /// <summary>How many bytes one USB report is.</summary>
    public const int UsbReportLength = 64;

    /// <summary>Every product identifier that decodes as a DualSense, with its name.</summary>
    public static IReadOnlyDictionary<int, string> DualSenseProducts { get; } = new Dictionary<int, string>
    {
        [DualSenseProductId] = "DualSense",
        [DualSenseEdgeProductId] = "DualSense Edge",
    };

    /// <summary>Whether a device is a controller this engine can decode.</summary>
    /// <param name="device">The device, as enumeration described it.</param>
    /// <returns>True when it is a DualSense of some kind.</returns>
    public static bool IsDualSense(HidDeviceInfo device) =>
        device.VendorId == SonyVendorId && DualSenseProducts.ContainsKey(device.ProductId);

    /// <summary>The name of a known controller, or an empty string.</summary>
    /// <param name="device">The device, as enumeration described it.</param>
    /// <returns>The product name.</returns>
    public static string NameOf(HidDeviceInfo device) =>
        IsDualSense(device) ? DualSenseProducts[device.ProductId] : string.Empty;

    /// <summary>Picks the first DualSense out of a list of devices, ignoring everything else.</summary>
    /// <param name="devices">Every device the system reported.</param>
    /// <param name="found">The controller, when there was one.</param>
    /// <returns>True when a controller was found.</returns>
    public static bool TryFindDualSense(IReadOnlyList<HidDeviceInfo> devices, out HidDeviceInfo found)
    {
        foreach (HidDeviceInfo device in devices)
        {
            if (IsDualSense(device))
            {
                found = device;
                return true;
            }
        }

        found = default;
        return false;
    }
}
