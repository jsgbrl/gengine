using System;
using System.Collections.Generic;
using GEngine.Input.Hid;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>
/// A HID layer with no hardware under it. Everything the three real backends promise, this one
/// promises too - including the two failures that are easy to forget: a device that enumerates
/// and then refuses to open, and a device that is simply not there.
/// </summary>
public sealed class FakeHidBackendTests
{
    private static readonly HidDeviceInfo Controller =
        new(KnownDevices.SonyVendorId, KnownDevices.DualSenseProductId, "fake://one", "DualSense");

    private static readonly HidDeviceInfo Other = new(0x045E, 0x028E, "fake://two", "Something Else");

    [Test]
    public void AnEmptyBackendFindsNothingAndOpensNothing()
    {
        var backend = new FakeHidBackend();
        Assert.AreEqual(0, backend.Enumerate().Count);
        Assert.IsNull(backend.Open(Controller));
        Assert.AreEqual("fake", backend.Name);
    }

    [Test]
    public void ADeviceThatWasAddedIsFoundAndCanBeOpened()
    {
        FakeHidBackend backend = new FakeHidBackend().With(Controller, new FakeHidDevice());
        Assert.AreEqual(1, backend.Enumerate().Count);
        Assert.AreEqual(Controller, backend.Enumerate()[0]);

        Assert.IsNotNull(backend.Open(Controller));
        Assert.AreEqual(1, backend.OpenCount, "and it counted the open");
    }

    // The real backends sort so that two runs on the same machine see devices in the same
    // order. Reproducibility is not a property a test double gets to opt out of.
    [Test]
    public void DevicesComeBackInAStableOrderWhicheverOrderTheyWereAdded()
    {
        FakeHidBackend backend = new FakeHidBackend().With(Other, new FakeHidDevice()).With(Controller, new FakeHidDevice());
        IReadOnlyList<HidDeviceInfo> found = backend.Enumerate();
        Assert.AreEqual("fake://one", found[0].Path);
        Assert.AreEqual("fake://two", found[1].Path);
    }

    [Test]
    public void ABackendThatRefusesToOpenStillEnumerates()
    {
        FakeHidBackend backend = new FakeHidBackend().With(Controller, new FakeHidDevice());
        backend.RefusesToOpen = true;

        Assert.AreEqual(1, backend.Enumerate().Count, "the controller is visible");
        Assert.IsNull(backend.Open(Controller), "and cannot be opened, which is a real failure");
        Assert.AreEqual(0, backend.OpenCount);
    }

    [Test]
    public void UnpluggingEverythingEmptiesTheBackendAndClosesWhatWasOpen()
    {
        var device = new FakeHidDevice();
        FakeHidBackend backend = new FakeHidBackend().With(Controller, device);
        IHidDevice? opened = backend.Open(Controller);

        backend.UnplugAll();

        Assert.AreEqual(0, backend.Enumerate().Count);
        Assert.IsNull(backend.Open(Controller));
        Assert.IsFalse(device.IsOpen, "the handle somebody is holding went dead too");
        Assert.IsNotNull(opened);
    }

    [Test]
    public void ItRefusesNothingWhereADeviceShouldBe()
    {
        var backend = new FakeHidBackend();
        Assert.Throws<ArgumentNullException>(() => backend.With(Controller, null!));
    }
}
