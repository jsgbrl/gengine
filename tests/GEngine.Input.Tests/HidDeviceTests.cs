using System;
using System.Collections.Generic;
using GEngine.Input.Hid;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>Covers <see cref="HidDeviceInfo"/>, <see cref="FakeHidDevice"/> and <see cref="FakeHidBackend"/>.</summary>
public sealed class HidDeviceTests
{
    private static readonly HidDeviceInfo Controller =
        new(KnownDevices.SonyVendorId, KnownDevices.DualSenseProductId, "fake://pad", "DualSense");

    [Test]
    public void ADescriptionKeepsWhatEnumerationFound()
    {
        Assert.AreEqual(KnownDevices.SonyVendorId, Controller.VendorId);
        Assert.AreEqual("fake://pad", Controller.Path);
        Assert.AreEqual("DualSense", Controller.ProductName);
    }

    [Test]
    public void TwoDescriptionsOfTheSameDeviceAreEqual()
    {
        var same = new HidDeviceInfo(KnownDevices.SonyVendorId, KnownDevices.DualSenseProductId, "fake://pad", "other");
        Assert.IsTrue(Controller == same, "the name is a label, the path is the identity");
        Assert.IsTrue(Controller.Equals((object)same));
        Assert.AreEqual(Controller.GetHashCode(), same.GetHashCode());
        Assert.IsFalse(Controller.Equals("not a device"));
    }

    [Test]
    public void TwoDescriptionsOfDifferentDevicesAreNotEqual()
    {
        Assert.IsTrue(Controller != new HidDeviceInfo(Controller.VendorId, Controller.ProductId, "fake://other"));
    }

    [Test]
    public void ToString_ShowsTheIdentifiersInHex()
    {
        Assert.AreEqual("054C:0CE6 DualSense at fake://pad", Controller.ToString());
    }

    [Test]
    public void AFakeDeviceHandsOutItsReportsInOrder()
    {
        FakeHidDevice device = new FakeHidDevice().QueueHex("01 02 03").QueueHex("04 05 06");
        byte[] buffer = new byte[3];
        Assert.AreEqual(3, device.Read(buffer));
        Assert.AreEqual(0x01, buffer[0]);
        Assert.AreEqual(3, device.Read(buffer));
        Assert.AreEqual(0x04, buffer[0]);
        Assert.AreEqual(2, device.ReadCount);
    }

    [Test]
    public void AFakeDeviceRepeatsItsLastReportLikeARealControllerDoes()
    {
        FakeHidDevice device = new FakeHidDevice().QueueHex("01 02 03");
        byte[] buffer = new byte[3];
        device.Read(buffer);
        Assert.AreEqual(3, device.Read(buffer), "a controller keeps sending while nothing changes");
    }

    [Test]
    public void AnUnpluggedDeviceReportsNothingForEver()
    {
        FakeHidDevice device = new FakeHidDevice().QueueHex("01 02 03");
        device.Unplug();
        Assert.AreEqual(0, device.Read(new byte[3]));
        Assert.IsFalse(device.IsOpen);
    }

    [Test]
    public void ADisposedDeviceIsClosed()
    {
        var device = new FakeHidDevice();
        device.Dispose();
        Assert.IsFalse(device.IsOpen);
    }

    [Test]
    public void AFakeBackendEnumeratesWhatItWasGiven_SortedByPath()
    {
        FakeHidBackend backend = new FakeHidBackend()
            .With(new HidDeviceInfo(1, 2, "zzz"), new FakeHidDevice())
            .With(new HidDeviceInfo(3, 4, "aaa"), new FakeHidDevice());

        IReadOnlyList<HidDeviceInfo> devices = backend.Enumerate();
        Assert.AreEqual("aaa", devices[0].Path);
        Assert.AreEqual("zzz", devices[1].Path);
    }

    [Test]
    public void OpeningAKnownDeviceGivesItBack()
    {
        var pad = new FakeHidDevice();
        FakeHidBackend backend = new FakeHidBackend().With(Controller, pad);
        Assert.AreSame(pad, backend.Open(Controller));
        Assert.AreEqual(1, backend.OpenCount);
    }

    [Test]
    public void OpeningSomethingThatIsNotThere_OrRefused_GivesNull()
    {
        FakeHidBackend backend = new FakeHidBackend().With(Controller, new FakeHidDevice());
        Assert.IsNull(backend.Open(new HidDeviceInfo(1, 2, "missing")));
        backend.RefusesToOpen = true;
        Assert.IsNull(backend.Open(Controller), "this is what a permission failure looks like");
    }

    [Test]
    public void UnpluggingEverythingEmptiesTheEnumerationAndClosesTheDevices()
    {
        var pad = new FakeHidDevice();
        FakeHidBackend backend = new FakeHidBackend().With(Controller, pad);
        backend.UnplugAll();
        Assert.AreEqual(0, backend.Enumerate().Count);
        Assert.IsFalse(pad.IsOpen);
    }

    [Test]
    public void AMissingReportOrBufferIsRefused()
    {
        Assert.Throws<ArgumentNullException>(static () => new FakeHidDevice().Queue(null!));
        Assert.Throws<ArgumentNullException>(static () => new FakeHidDevice().Read(null!));
    }
}
