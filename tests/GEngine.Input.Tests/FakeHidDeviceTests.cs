using System;
using GEngine.Input.Hid;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>
/// The double every gamepad test is built on, which is exactly why it needs tests of its own: a
/// bug in a double is a test that passes while the engine is broken. It ships in the engine
/// rather than in the test project so that somebody writing a game against gengine can use it
/// to test their own input handling without a controller.
/// </summary>
public sealed class FakeHidDeviceTests
{
    [Test]
    public void ANewDeviceIsOpenAndEmpty()
    {
        var device = new FakeHidDevice();
        Assert.IsTrue(device.IsOpen);
        Assert.AreEqual(0, device.ReportCount);
        Assert.AreEqual(0, device.ReadCount);
        Assert.AreEqual("fake://dualsense", device.Path);
    }

    [Test]
    public void ReadingAnEmptyDeviceReturnsNothingRatherThanBlocking()
    {
        var device = new FakeHidDevice();
        Assert.AreEqual(0, device.Read(new byte[64]), "no reports queued, nothing read");
    }

    [Test]
    public void QueuedReportsComeBackInOrder()
    {
        FakeHidDevice device = new FakeHidDevice().QueueHex("01 02").QueueHex("03 04");
        byte[] buffer = new byte[2];

        Assert.AreEqual(2, device.Read(buffer));
        Assert.AreEqual(1, buffer[0]);

        Assert.AreEqual(2, device.Read(buffer));
        Assert.AreEqual(3, buffer[0]);
        Assert.AreEqual(2, device.ReadCount);
    }

    // A real controller sends sixty reports a second whether anything changed or not, so the
    // default is to keep sending the last one. Turning that off is how a test says "and then
    // the controller went quiet".
    [Test]
    public void ByDefaultItKeepsSendingTheLastReportForEver()
    {
        FakeHidDevice device = new FakeHidDevice().QueueHex("01 09");
        byte[] buffer = new byte[2];
        device.Read(buffer);

        Assert.AreEqual(2, device.Read(buffer), "still reporting");
        Assert.AreEqual(9, buffer[1], "and reporting the same thing");
    }

    [Test]
    public void ADeviceThatDoesNotRepeatGoesQuietWhenItRunsOut()
    {
        FakeHidDevice device = new FakeHidDevice().QueueHex("01 09");
        device.RepeatsLastReport = false;
        byte[] buffer = new byte[2];

        Assert.AreEqual(2, device.Read(buffer));
        Assert.AreEqual(0, device.Read(buffer), "and then nothing");
    }

    [Test]
    public void AReportLongerThanTheBufferIsTruncatedRatherThanOverflowing()
    {
        FakeHidDevice device = new FakeHidDevice().QueueHex("01 02 03 04");
        byte[] buffer = new byte[2];
        Assert.AreEqual(2, device.Read(buffer), "only what fits");
    }

    [Test]
    public void UnpluggingAndDisposingBothCloseIt()
    {
        FakeHidDevice unplugged = new FakeHidDevice().QueueHex("01 02");
        unplugged.Unplug();
        Assert.IsFalse(unplugged.IsOpen);
        Assert.AreEqual(0, unplugged.Read(new byte[2]), "a gone device reports nothing");

        var disposed = new FakeHidDevice();
        disposed.Dispose();
        Assert.IsFalse(disposed.IsOpen);
    }

    [Test]
    public void ItRefusesNothingWhereAReportShouldBe()
    {
        var device = new FakeHidDevice();
        Assert.Throws<ArgumentNullException>(() => device.Queue(null!));
        Assert.Throws<ArgumentNullException>(() => device.Read(null!));
    }
}
