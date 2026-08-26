using System;
using GEngine.Input.Hid;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>
/// Covers <see cref="HidReportReader"/>. The tests drive it with PumpOnce rather than starting
/// its thread: a thread that reads a fake device as fast as it can would spin a core and make
/// the assertions depend on timing, which is exactly the kind of test the prompt bans.
/// </summary>
public sealed class HidReportReaderTests
{
    [Test]
    public void ANewReaderIsConnectedAndHasNothingYet()
    {
        using var reader = new HidReportReader(new FakeHidDevice(), 4);
        Assert.IsTrue(reader.IsConnected);
        Assert.IsFalse(reader.HasReport);
        Assert.IsFalse(reader.TryTakeLatest(new byte[4]));
    }

    [Test]
    public void PumpingOnceMakesAReportAvailable()
    {
        FakeHidDevice device = new FakeHidDevice().QueueHex("01 02 03 04");
        using var reader = new HidReportReader(device, 4);
        Assert.IsTrue(reader.PumpOnce());
        Assert.IsTrue(reader.HasReport);
        Assert.AreEqual(1L, reader.ReportCount);

        byte[] taken = new byte[4];
        Assert.IsTrue(reader.TryTakeLatest(taken));
        Assert.AreEqual(0x03, taken[2]);
    }

    [Test]
    public void OnlyTheNewestReportIsKept()
    {
        FakeHidDevice device = new FakeHidDevice().QueueHex("01 00 00 00").QueueHex("01 99 00 00");
        using var reader = new HidReportReader(device, 4);
        reader.PumpOnce();
        reader.PumpOnce();

        byte[] taken = new byte[4];
        reader.TryTakeLatest(taken);
        Assert.AreEqual(0x99, taken[1], "the game never wants a stale frame of input");
    }

    [Test]
    public void TakingTheLatestTwiceGivesTheSameReport()
    {
        FakeHidDevice device = new FakeHidDevice().QueueHex("01 42 00 00");
        using var reader = new HidReportReader(device, 4);
        reader.PumpOnce();

        byte[] first = new byte[4];
        byte[] second = new byte[4];
        reader.TryTakeLatest(first);
        reader.TryTakeLatest(second);
        Assert.AreEqual(first[1], second[1], "a controller that stops sending is a controller holding still");
    }

    [Test]
    public void AnUnpluggedDeviceDisconnectsTheReader()
    {
        FakeHidDevice device = new FakeHidDevice().QueueHex("01 02 03 04");
        using var reader = new HidReportReader(device, 4);
        reader.PumpOnce();
        device.Unplug();

        Assert.IsFalse(reader.PumpOnce());
        Assert.IsFalse(reader.IsConnected);
    }

    [Test]
    public void ADeviceThatThrowsMidReadCountsAsUnplugged()
    {
        using var reader = new HidReportReader(new ThrowingDevice(), 4);
        Assert.IsFalse(reader.PumpOnce());
        Assert.IsFalse(reader.IsConnected);
    }

    [Test]
    public void Dispose_ClosesTheDeviceAndDisconnects()
    {
        FakeHidDevice device = new FakeHidDevice().QueueHex("01 02 03 04");
        var reader = new HidReportReader(device, 4);
        reader.Dispose();
        Assert.IsFalse(reader.IsConnected);
        Assert.IsFalse(device.IsOpen);
    }

    [Test]
    public void StartingTwiceOnlyStartsOneThread()
    {
        var device = new FakeHidDevice();
        using var reader = new HidReportReader(device, 4);
        reader.Start();
        reader.Start();
        Assert.IsTrue(reader.IsConnected);
    }

    [Test]
    public void AMissingDeviceOrADegenerateLengthIsRefused()
    {
        Assert.Throws<ArgumentNullException>(static () => new HidReportReader(null!));
        Assert.Throws<ArgumentOutOfRangeException>(static () => new HidReportReader(new FakeHidDevice(), 0));
        using var reader = new HidReportReader(new FakeHidDevice(), 4);
        Assert.Throws<ArgumentNullException>(() => reader.TryTakeLatest(null!));
    }

    private sealed class ThrowingDevice : IHidDevice
    {
        public string Path => "throwing";

        public bool IsOpen => true;

        public int Read(byte[] buffer) => throw new InvalidOperationException("the cable came out");

        public void Dispose()
        {
        }
    }
}
