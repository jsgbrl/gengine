using GEngine.Input.Hid;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>
/// What a HID enumeration hands back: two identifiers, a path and a name. It is compared by
/// value because the same controller found twice is the same controller, and the reader has to
/// be able to say so without keeping the object that first described it.
/// </summary>
public sealed class HidDeviceInfoTests
{
    [Test]
    public void ItRemembersWhatItWasToldAndDefaultsTheNameToNothing()
    {
        var device = new HidDeviceInfo(0x054C, 0x0CE6, "/dev/hidraw0", "DualSense");
        Assert.AreEqual(0x054C, device.VendorId);
        Assert.AreEqual(0x0CE6, device.ProductId);
        Assert.AreEqual("/dev/hidraw0", device.Path);
        Assert.AreEqual("DualSense", device.ProductName);
        Assert.AreEqual(string.Empty, new HidDeviceInfo(1, 2, "p").ProductName);
    }

    // The name is what a driver felt like reporting and differs between systems for the same
    // hardware, so it is deliberately not part of what makes two devices the same device.
    [Test]
    public void TwoDevicesAtTheSamePathAreTheSameDeviceWhateverTheyAreCalled()
    {
        var left = new HidDeviceInfo(0x054C, 0x0CE6, "/dev/hidraw0", "DualSense");
        var right = new HidDeviceInfo(0x054C, 0x0CE6, "/dev/hidraw0", "Wireless Controller");
        Assert.IsTrue(left == right);
        Assert.IsFalse(left != right);
        Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        Assert.IsTrue(left.Equals((object)right));
    }

    [Test]
    public void ADifferentPathOrIdentifierIsADifferentDevice()
    {
        var device = new HidDeviceInfo(0x054C, 0x0CE6, "/dev/hidraw0");
        Assert.IsTrue(device != new HidDeviceInfo(0x054C, 0x0CE6, "/dev/hidraw1"));
        Assert.IsTrue(device != new HidDeviceInfo(0x054C, 0x0DF2, "/dev/hidraw0"));
        Assert.IsTrue(device != new HidDeviceInfo(0x045E, 0x0CE6, "/dev/hidraw0"));
        Assert.IsFalse(device.Equals("not a device"));
    }

    // The identifiers are hexadecimal everywhere they are written down - on the device, in the
    // Linux uevent file, in Sony's documentation - so printing them in decimal would mean the
    // reader converting every time they compared a log line with a fixture.
    [Test]
    public void ItPrintsItsIdentifiersInHexadecimal()
    {
        var device = new HidDeviceInfo(0x054C, 0x0CE6, "/dev/hidraw0", "DualSense");
        string printed = device.ToString();
        Assert.IsTrue(printed.Contains("054C", System.StringComparison.OrdinalIgnoreCase), printed);
        Assert.IsTrue(printed.Contains("0CE6", System.StringComparison.OrdinalIgnoreCase), printed);
        Assert.IsTrue(printed.Contains("DualSense", System.StringComparison.Ordinal), printed);
    }
}
