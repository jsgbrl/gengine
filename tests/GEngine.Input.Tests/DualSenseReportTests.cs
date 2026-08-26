using GEngine.Core;
using GEngine.Input.Gamepad;
using GEngine.Input.Hid;
using GEngine.Input.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>
/// Covers <see cref="DualSenseReport"/> against the recorded fixtures. Not one of these tests
/// needs a controller plugged in, which is the whole point of keeping the decoder a pure
/// function of sixty-four bytes.
/// </summary>
public sealed class DualSenseReportTests
{
    [Test]
    public void ANeutralReportDecodesToANeutralController()
    {
        Assert.IsTrue(DualSenseReport.TryDecode(Fixtures.Report("neutral.txt"), out GamepadState state));
        Assert.AreEqual(Vector2.Zero, state.LeftStick);
        Assert.AreEqual(Vector2.Zero, state.RightStick);
        Assert.AreEqual(GamepadButtons.None, state.Buttons);
        Assert.AreEqual(HatDirection.Neutral, state.Hat);
    }

    [Test]
    public void ACentredStickIsExactlyZero_NotNearlyZero()
    {
        DualSenseReport.TryDecode(Fixtures.Report("neutral.txt"), out GamepadState state);
        Assert.IsTrue(state.LeftStick == Vector2.Zero, "128 maps to exactly zero on both halves");
    }

    // The fixture behind this one was captured from a real DualSense lying still on a desk.
    // It does not report the centre, and that is not a fault: this is what every stick does,
    // and it is the whole reason the dead zone is not optional.
    [Test]
    public void ARealStickAtRestIsNearTheCentreButNotAtIt()
    {
        Assert.IsTrue(DualSenseReport.TryDecode(Fixtures.Report("resting.txt"), out GamepadState state));
        Assert.IsTrue(state.LeftStick != Vector2.Zero, "hardware does not rest at exactly zero");
        Assert.IsTrue(state.LeftStick.Length < 0.05f, "but it is close");
        Assert.IsTrue(state.RightStick.Length < 0.15f);
        Assert.AreEqual(GamepadButtons.None, state.Buttons);
        Assert.AreEqual(HatDirection.Neutral, state.Hat);
    }

    [Test]
    public void TheDeadZoneTurnsARealRestingStickIntoNoMovementAtAll()
    {
        DualSenseReport.TryDecode(Fixtures.Report("resting.txt"), out GamepadState state);
        Assert.AreEqual(Vector2.Zero, DeadZone.ApplyRadial(state.LeftStick));
        Assert.AreEqual(Vector2.Zero, DeadZone.ApplyRadial(state.RightStick));
    }

    [Test]
    public void ACapturedReportIsAUsbReportOfSixtyFourBytes()
    {
        byte[] captured = Fixtures.Report("resting.txt");
        Assert.AreEqual(DualSenseReport.UsbLength, captured.Length);
        Assert.AreEqual(HidTransport.Usb, DualSenseReport.TransportOf(captured));
        Assert.AreEqual(0xAA, DualSenseReport.SequenceOf(captured));
    }

    [Test]
    public void TheFaceButtonsDecodeToTheirOwnBits()
    {
        DualSenseReport.TryDecode(Fixtures.Report("cross.txt"), out GamepadState cross);
        Assert.IsTrue(cross.IsDown(GamepadButtons.Cross));
        Assert.IsFalse(cross.IsDown(GamepadButtons.Circle));

        DualSenseReport.TryDecode(Fixtures.Report("all-face.txt"), out GamepadState all);
        Assert.IsTrue(all.IsDown(GamepadButtons.Square | GamepadButtons.Cross));
        Assert.IsTrue(all.IsDown(GamepadButtons.Circle | GamepadButtons.Triangle));
    }

    [Test]
    public void TheShoulderAndMenuButtonsDecodeToTheirOwnBits()
    {
        DualSenseReport.TryDecode(Fixtures.Report("shoulders.txt"), out GamepadState state);
        Assert.IsTrue(state.IsDown(GamepadButtons.LeftShoulder));
        Assert.IsTrue(state.IsDown(GamepadButtons.RightShoulder));
        Assert.IsTrue(state.IsDown(GamepadButtons.Create));
        Assert.IsTrue(state.IsDown(GamepadButtons.Options));
        Assert.IsTrue(state.IsDown(GamepadButtons.LeftStick));
        Assert.IsTrue(state.IsDown(GamepadButtons.RightStick));
        Assert.IsFalse(state.IsDown(GamepadButtons.PlayStation));
    }

    [Test]
    public void TheSystemButtonsDecodeToTheirOwnBits()
    {
        DualSenseReport.TryDecode(Fixtures.Report("system.txt"), out GamepadState state);
        Assert.IsTrue(state.IsDown(GamepadButtons.PlayStation));
        Assert.IsTrue(state.IsDown(GamepadButtons.Touchpad));
        Assert.IsTrue(state.IsDown(GamepadButtons.Mute));
    }

    [Test]
    public void PushingTheStickLeftGivesMinusOneOnX()
    {
        DualSenseReport.TryDecode(Fixtures.Report("stick-left.txt"), out GamepadState state);
        Assert.ApproximatelyEqual(-1.0f, state.LeftStick.X);
        Assert.ApproximatelyEqual(0.0f, state.LeftStick.Y);
    }

    [Test]
    public void PushingTheStickUpGivesMinusOneOnY_BecauseTheScreenAxisPointsDown()
    {
        DualSenseReport.TryDecode(Fixtures.Report("stick-up.txt"), out GamepadState state);
        Assert.ApproximatelyEqual(-1.0f, state.LeftStick.Y);
    }

    [Test]
    public void PushingTheStickDownAndRightGivesPlusOneOnBothAxes()
    {
        DualSenseReport.TryDecode(Fixtures.Report("stick-down-right.txt"), out GamepadState state);
        Assert.ApproximatelyEqual(1.0f, state.LeftStick.X);
        Assert.ApproximatelyEqual(1.0f, state.LeftStick.Y);
    }

    [Test]
    public void TriggersDecodeAsAnalogueAndAsButtons()
    {
        DualSenseReport.TryDecode(Fixtures.Report("triggers.txt"), out GamepadState state);
        Assert.ApproximatelyEqual(0.502f, state.LeftTrigger, 0.01f);
        Assert.ApproximatelyEqual(0.502f, state.RightTrigger, 0.01f);
        Assert.IsTrue(state.IsDown(GamepadButtons.LeftTrigger));
        Assert.IsTrue(state.IsDown(GamepadButtons.RightTrigger));
    }

    [Test]
    public void TheHatDecodesFromTheLowHalfOfItsByte()
    {
        DualSenseReport.TryDecode(Fixtures.Report("hat-east.txt"), out GamepadState state);
        Assert.AreEqual(HatDirection.East, state.Hat);
    }

    [Test]
    public void TheSequenceCounterIsReadableWithoutDecodingTheRest()
    {
        Assert.AreEqual(0x2A, DualSenseReport.SequenceOf(Fixtures.Report("sequence.txt")));
        Assert.AreEqual(0, DualSenseReport.SequenceOf([]));
    }

    [Test]
    public void ABluetoothReportIsRecognisedAndRefused()
    {
        byte[] report = Fixtures.Report("bluetooth.txt");
        Assert.AreEqual(HidTransport.Bluetooth, DualSenseReport.TransportOf(report));
        Assert.IsFalse(DualSenseReport.TryDecode(report, out GamepadState state));
        Assert.AreEqual(GamepadState.Neutral, state);
    }

    [Test]
    public void AnEmptyOrTruncatedReportIsRefusedRatherThanReadPastItsEnd()
    {
        Assert.AreEqual(HidTransport.Unknown, DualSenseReport.TransportOf([]));
        Assert.IsFalse(DualSenseReport.TryDecode([], out _));
        Assert.IsFalse(DualSenseReport.TryDecode([0x01, 0x80, 0x80], out _));
    }

    [Test]
    public void EveryFixtureIsSixtyFourBytes_ExceptTheBluetoothOne()
    {
        Assert.AreEqual(DualSenseReport.UsbLength, Fixtures.Report("neutral.txt").Length);
        Assert.AreEqual(78, Fixtures.Report("bluetooth.txt").Length);
    }
}
