// Linux, where a HID device is a file and there is no interop at all.
//
// Every raw HID device appears as /dev/hidrawN, and the kernel describes it in
// /sys/class/hidraw/hidrawN/device/uevent - a few lines of key=value, one of which is
//
//     HID_ID=0003:0000054C:00000CE6
//            bus  vendor    product
//
// So enumeration is reading text files and opening one is opening a file. Which is also why
// permissions are the whole story here: /dev/hidraw* is root-only on most distributions
// until a udev rule says otherwise, and docs/platforms.md gives that rule as step one.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace GEngine.Input.Hid.Interop.Linux;

/// <summary>The HID backend for Linux, over /dev/hidraw and /sys/class/hidraw.</summary>
public sealed class LinuxHidBackend : IHidBackend
{
    /// <summary>Where the kernel lists the raw HID devices.</summary>
    public const string SysClassPath = "/sys/class/hidraw";

    /// <summary>Where the devices themselves live.</summary>
    public const string DevicePathPrefix = "/dev/";

    /// <inheritdoc/>
    public string Name => "linux hidraw";

    /// <inheritdoc/>
    public IReadOnlyList<HidDeviceInfo> Enumerate()
    {
        List<HidDeviceInfo> found = [];
        if (!Directory.Exists(SysClassPath))
        {
            return found;
        }

        foreach (string entry in Directory.EnumerateDirectories(SysClassPath))
        {
            AddIfHid(entry, found);
        }

        found.Sort(static (left, right) => string.CompareOrdinal(left.Path, right.Path));
        return found;
    }

    /// <inheritdoc/>
    public IHidDevice? Open(HidDeviceInfo device)
    {
        try
        {
            var stream = new FileStream(device.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return new LinuxHidDevice(device.Path, stream);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>Reads the vendor and product out of a uevent file.</summary>
    /// <param name="uevent">The contents of a device's uevent file.</param>
    /// <param name="vendorId">The vendor identifier, when the file had one.</param>
    /// <param name="productId">The product identifier, when the file had one.</param>
    /// <returns>True when the file described a HID device.</returns>
    public static bool TryReadIds(string uevent, out int vendorId, out int productId)
    {
        ArgumentNullException.ThrowIfNull(uevent);
        vendorId = 0;
        productId = 0;
        foreach (string line in uevent.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            if (line.StartsWith("HID_ID=", StringComparison.Ordinal))
            {
                return TryReadHidId(line["HID_ID=".Length..], out vendorId, out productId);
            }
        }

        return false;
    }

    /// <summary>Reads the human-readable name out of a uevent file.</summary>
    /// <param name="uevent">The contents of a device's uevent file.</param>
    /// <returns>The name, or an empty string.</returns>
    public static string ReadName(string uevent)
    {
        ArgumentNullException.ThrowIfNull(uevent);
        foreach (string line in uevent.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            if (line.StartsWith("HID_NAME=", StringComparison.Ordinal))
            {
                return line["HID_NAME=".Length..].Trim();
            }
        }

        return string.Empty;
    }

    private static bool TryReadHidId(string value, out int vendorId, out int productId)
    {
        vendorId = 0;
        productId = 0;
        string[] parts = value.Trim().Split(':');
        return parts.Length == 3
            && int.TryParse(parts[1], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out vendorId)
            && int.TryParse(parts[2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out productId);
    }

    private static void AddIfHid(string entry, List<HidDeviceInfo> found)
    {
        string uevent = Path.Combine(entry, "device", "uevent");
        if (!File.Exists(uevent))
        {
            return;
        }

        string text = File.ReadAllText(uevent);
        if (TryReadIds(text, out int vendorId, out int productId))
        {
            string node = DevicePathPrefix + Path.GetFileName(entry);
            found.Add(new HidDeviceInfo(vendorId, productId, node, ReadName(text)));
        }
    }
}
