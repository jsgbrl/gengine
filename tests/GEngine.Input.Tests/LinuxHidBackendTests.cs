using GEngine.Input.Hid.Interop.Linux;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>
/// Covers the part of the Linux backend that is pure text: reading a device's identity out of
/// the uevent file the kernel writes. Everything else there is file system access, and the
/// file system is the operating system's job to test.
/// </summary>
public sealed class LinuxHidBackendTests
{
    private const string DualSenseUevent = """
        DRIVER=playstation
        HID_ID=0003:0000054C:00000CE6
        HID_NAME=Sony Interactive Entertainment DualSense Wireless Controller
        HID_PHYS=usb-0000:00:14.0-3/input0
        MODALIAS=hid:b0003g0001v0000054Cp00000CE6
        """;

    [Test]
    public void TheVendorAndProductComeOutOfTheHidIdLine()
    {
        Assert.IsTrue(LinuxHidBackend.TryReadIds(DualSenseUevent, out int vendorId, out int productId));
        Assert.AreEqual(0x054C, vendorId);
        Assert.AreEqual(0x0CE6, productId);
    }

    [Test]
    public void TheNameComesOutOfTheHidNameLine()
    {
        Assert.IsTrue(LinuxHidBackend.ReadName(DualSenseUevent).Contains("DualSense", System.StringComparison.Ordinal));
    }

    [Test]
    public void AFileWithNoHidIdIsNotAHidDevice()
    {
        Assert.IsFalse(LinuxHidBackend.TryReadIds("DRIVER=usbhid\nMODALIAS=usb:v046Dp\n", out _, out _));
        Assert.AreEqual(string.Empty, LinuxHidBackend.ReadName("DRIVER=usbhid\n"));
    }

    [Test]
    public void AMalformedHidIdIsRefusedRatherThanHalfRead()
    {
        Assert.IsFalse(LinuxHidBackend.TryReadIds("HID_ID=0003:0000054C\n", out _, out _));
        Assert.IsFalse(LinuxHidBackend.TryReadIds("HID_ID=nonsense:here:now\n", out _, out _));
    }

    [Test]
    public void WindowsLineEndingsAreReadTheSameWay()
    {
        Assert.IsTrue(LinuxHidBackend.TryReadIds("HID_ID=0003:0000054C:00000CE6\r\n", out int vendorId, out _));
        Assert.AreEqual(0x054C, vendorId);
    }

    [Test]
    public void ThePathsItLooksInAreTheOnesTheKernelUses()
    {
        Assert.AreEqual("/sys/class/hidraw", LinuxHidBackend.SysClassPath);
        Assert.AreEqual("/dev/", LinuxHidBackend.DevicePathPrefix);
    }
}
