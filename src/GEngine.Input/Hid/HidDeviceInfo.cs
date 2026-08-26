// What enumeration found: enough to recognise a device and enough to open it. The path is
// whatever the operating system calls the device, and the engine never parses it - it hands
// it straight back to the same operating system.

using System;
using System.Globalization;

namespace GEngine.Input.Hid;

/// <summary>A HID device the system reported.</summary>
public readonly struct HidDeviceInfo : IEquatable<HidDeviceInfo>
{
    /// <summary>Creates a description.</summary>
    /// <param name="vendorId">USB vendor identifier.</param>
    /// <param name="productId">USB product identifier.</param>
    /// <param name="path">Whatever the operating system calls this device.</param>
    /// <param name="productName">Human-readable name, when the system offered one.</param>
    public HidDeviceInfo(int vendorId, int productId, string path, string productName = "")
    {
        VendorId = vendorId;
        ProductId = productId;
        Path = path;
        ProductName = productName;
    }

    /// <summary>USB vendor identifier. Sony is 0x054C.</summary>
    public int VendorId { get; }

    /// <summary>USB product identifier. A DualSense is 0x0CE6.</summary>
    public int ProductId { get; }

    /// <summary>Whatever the operating system calls this device.</summary>
    public string Path { get; }

    /// <summary>Human-readable name, when the system offered one.</summary>
    public string ProductName { get; }

    /// <summary>Compares two descriptions.</summary>
    /// <param name="other">The description to compare with.</param>
    /// <returns>True when they name the same device.</returns>
    public bool Equals(HidDeviceInfo other) =>
        VendorId == other.VendorId
        && ProductId == other.ProductId
        && string.Equals(Path, other.Path, StringComparison.Ordinal);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is HidDeviceInfo other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(VendorId, ProductId, Path);

    /// <inheritdoc/>
    public override string ToString() => string.Format(
        CultureInfo.InvariantCulture,
        "{0:X4}:{1:X4} {2} at {3}",
        VendorId,
        ProductId,
        ProductName,
        Path);

    /// <summary>Equality.</summary>
    /// <param name="left">First description.</param>
    /// <param name="right">Second description.</param>
    /// <returns>True when they name the same device.</returns>
    public static bool operator ==(HidDeviceInfo left, HidDeviceInfo right) => left.Equals(right);

    /// <summary>Inequality.</summary>
    /// <param name="left">First description.</param>
    /// <param name="right">Second description.</param>
    /// <returns>True when they differ.</returns>
    public static bool operator !=(HidDeviceInfo left, HidDeviceInfo right) => !left.Equals(right);
}
