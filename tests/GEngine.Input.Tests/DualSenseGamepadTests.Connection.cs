// The half of the gamepad tests that are about a controller coming and going: an edge between
// two reports, an unplug mid-game, and a plug back in.

using System;
using GEngine.Core.Contracts;
using GEngine.Input.Gamepad;
using GEngine.Input.Hid;
using GEngine.Input.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <content>Connecting, disconnecting and reconnecting.</content>
public sealed partial class DualSenseGamepadTests
{
    [Test]
    public void ChangingReportsBetweenPollsIsSeenAsAnEdge()
    {
        FakeHidDevice device = new FakeHidDevice().Queue(Fixtures.Report("cross.txt")).Queue(Neutral());
        device.RepeatsLastReport = true;
        using DualSenseGamepad pad = Gamepad(new FakeHidBackend().With(Controller, device));
        pad.Poll(2.0f);

        pad.Poll(Frame);
        Assert.IsTrue(pad.IsDown(InputAction.Jump));
        pad.Poll(Frame);
        Assert.IsFalse(pad.IsDown(InputAction.Jump), "the button came up between two reports");
    }

    [Test]
    public void UnpluggingMidGameDisconnectsTheBackendAndSaysWhy()
    {
        FakeHidDevice device = new FakeHidDevice().Queue(Neutral());
        using DualSenseGamepad pad = Gamepad(new FakeHidBackend().With(Controller, device));
        pad.Poll(2.0f);
        pad.Poll(Frame);
        Assert.IsTrue(pad.IsConnected);

        device.Unplug();
        pad.Poll(Frame);
        pad.Poll(Frame);

        Assert.IsFalse(pad.IsConnected);
        Assert.IsTrue(_logger.Contains("unplugged"));
    }

    [Test]
    public void AfterUnpluggingItAsksForNothingRatherThanHoldingTheLastFrame()
    {
        FakeHidDevice device = new FakeHidDevice().Queue(Fixtures.Report("cross.txt"));
        using DualSenseGamepad pad = Gamepad(new FakeHidBackend().With(Controller, device));
        pad.Poll(2.0f);
        pad.Poll(Frame);
        Assert.IsTrue(pad.IsDown(InputAction.Jump));

        device.Unplug();
        pad.Poll(Frame);
        pad.Poll(Frame);
        Assert.IsFalse(pad.IsDown(InputAction.Jump), "a controller on the floor is not holding jump");
    }

    [Test]
    public void PluggingItBackInPicksItUpAgain()
    {
        FakeHidBackend backend = new FakeHidBackend()
            .With(Controller, new FakeHidDevice().Queue(Neutral()));
        using DualSenseGamepad pad = Gamepad(backend);
        pad.Poll(2.0f);
        pad.Poll(Frame);
        Assert.IsTrue(pad.IsConnected);

        backend.UnplugAll();
        pad.Poll(Frame);
        pad.Poll(Frame);
        Assert.IsFalse(pad.IsConnected, "the cable came out");

        backend.With(Controller, new FakeHidDevice().Queue(Fixtures.Report("cross.txt")));
        pad.Poll(2.0f);
        pad.Poll(Frame);
        Assert.IsTrue(pad.IsConnected, "and went back in");
        Assert.IsTrue(pad.IsDown(InputAction.Jump), "the new controller is being read, not the old one");
    }

    [Test]
    public void ABluetoothControllerIsRefusedWithAMessageThatSaysWhatToDo()
    {
        using DualSenseGamepad pad = Connected(Fixtures.Report("bluetooth.txt"));
        pad.Poll(Frame);
        Assert.IsFalse(pad.IsConnected);
        Assert.IsTrue(_logger.Contains("Bluetooth"));
        Assert.IsTrue(_logger.Contains("USB cable"));
        Assert.AreEqual(GamepadState.Neutral, pad.State);
    }

    [Test]
    public void RebindingTheMapTakesEffectOnTheNextPoll()
    {
        using DualSenseGamepad pad = Connected(Fixtures.Report("cross.txt"));
        pad.Map.Bind(GamepadButtons.Cross, InputAction.Run);
        pad.Poll(Frame);
        Assert.IsTrue(pad.IsDown(InputAction.Run));
        Assert.IsFalse(pad.IsDown(InputAction.Jump));
    }

    [Test]
    public void AMissingBackendMapOrLoggerIsRefused()
    {
        Assert.Throws<ArgumentNullException>(
            static () => new DualSenseGamepad(null!, GamepadMap.CreateDefault(), NullLogger.Instance));
        Assert.Throws<ArgumentNullException>(
            static () => new DualSenseGamepad(new FakeHidBackend(), null!, NullLogger.Instance));
        Assert.Throws<ArgumentNullException>(
            static () => new DualSenseGamepad(new FakeHidBackend(), GamepadMap.CreateDefault(), null!));
    }
}
