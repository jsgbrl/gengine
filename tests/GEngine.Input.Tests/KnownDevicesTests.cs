using System.Collections.Generic;
using GEngine.Input.Hid;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>Covers <see cref="KnownDevices"/> and the enumeration filter it drives.</summary>
public sealed class KnownDevicesTests
{
    [Test]
    public void ADualSenseIsRecognisedByItsVendorAndProduct()
    {
        Assert.IsTrue(KnownDevices.IsDualSense(Device(KnownDevices.SonyVendorId, KnownDevices.DualSenseProductId)));
        Assert.IsTrue(KnownDevices.IsDualSense(Device(KnownDevices.SonyVendorId, KnownDevices.DualSenseEdgeProductId)));
    }

    [Test]
    public void AnotherSonyProductIsNotADualSense()
    {
        Assert.IsFalse(KnownDevices.IsDualSense(Device(KnownDevices.SonyVendorId, 0x1234)));
    }

    [Test]
    public void AnotherVendorIsNotADualSense()
    {
        Assert.IsFalse(KnownDevices.IsDualSense(Device(0x046D, KnownDevices.DualSenseProductId)));
    }

    [Test]
    public void EachKnownControllerHasAName()
    {
        Assert.AreEqual("DualSense", KnownDevices.NameOf(Device(KnownDevices.SonyVendorId, KnownDevices.DualSenseProductId)));
        Assert.AreEqual("DualSense Edge", KnownDevices.NameOf(Device(KnownDevices.SonyVendorId, KnownDevices.DualSenseEdgeProductId)));
        Assert.AreEqual(string.Empty, KnownDevices.NameOf(Device(0x046D, 0xC52B)));
    }

    [Test]
    public void EnumerationIgnoresKeyboardsMiceAndEverythingElse()
    {
        List<HidDeviceInfo> devices =
        [
            Device(0x046D, 0xC52B, "keyboard"),
            Device(0x05AC, 0x0259, "mouse"),
            Device(KnownDevices.SonyVendorId, KnownDevices.DualSenseProductId, "controller"),
            Device(0x1532, 0x0084, "headset"),
        ];

        Assert.IsTrue(KnownDevices.TryFindDualSense(devices, out HidDeviceInfo found));
        Assert.AreEqual("controller", found.Path);
    }

    [Test]
    public void AWorldWithNoControllerInItReportsNothingFound()
    {
        Assert.IsFalse(KnownDevices.TryFindDualSense([Device(0x046D, 0xC52B)], out _));
        Assert.IsFalse(KnownDevices.TryFindDualSense([], out _));
    }

    private static HidDeviceInfo Device(int vendorId, int productId, string path = "path") =>
        new(vendorId, productId, path);
}
