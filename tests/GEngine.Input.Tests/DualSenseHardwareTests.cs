using GEngine.Input.Gamepad;
using GEngine.Input.Hid;
using GEngine.Input.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>
/// Every one of these runs against a report captured from a real DualSense over USB, one
/// button at a time, with `examples/04-gamepad-probe.cs`. They are what turned the byte table
/// in the documentation from a hypothesis into a measurement - see docs/input-and-gamepad.md
/// for what was measured, how, and what is still inferred.
/// </summary>
public sealed class DualSenseHardwareTests
{
    [TestCase("square.txt", GamepadButtons.Square)]
    [TestCase("cross.txt", GamepadButtons.Cross)]
    [TestCase("circle.txt", GamepadButtons.Circle)]
    [TestCase("triangle.txt", GamepadButtons.Triangle)]
    [TestCase("l1.txt", GamepadButtons.LeftShoulder)]
    [TestCase("r1.txt", GamepadButtons.RightShoulder)]
    [TestCase("l2.txt", GamepadButtons.LeftTrigger)]
    [TestCase("r2.txt", GamepadButtons.RightTrigger)]
    [TestCase("create.txt", GamepadButtons.Create)]
    [TestCase("options.txt", GamepadButtons.Options)]
    [TestCase("l3.txt", GamepadButtons.LeftStick)]
    [TestCase("r3.txt", GamepadButtons.RightStick)]
    [TestCase("ps.txt", GamepadButtons.PlayStation)]
    [TestCase("touchpad.txt", GamepadButtons.Touchpad)]
    [TestCase("mute.txt", GamepadButtons.Mute)]
    public void EachButtonDecodesToItselfAndToNothingElse(string fixture, GamepadButtons expected)
    {
        Assert.IsTrue(DualSenseReport.TryDecode(Fixtures.Report(fixture), out GamepadState state));
        Assert.AreEqual(expected, state.Buttons, fixture + " holds exactly one button");
    }

    [TestCase("hat-north.txt", HatDirection.North)]
    [TestCase("hat-northeast.txt", HatDirection.NorthEast)]
    [TestCase("hat-east.txt", HatDirection.East)]
    [TestCase("hat-southeast.txt", HatDirection.SouthEast)]
    [TestCase("hat-south.txt", HatDirection.South)]
    [TestCase("hat-southwest.txt", HatDirection.SouthWest)]
    [TestCase("hat-west.txt", HatDirection.West)]
    [TestCase("hat-northwest.txt", HatDirection.NorthWest)]
    public void EachDirectionOfTheDPadDecodesToItself(string fixture, HatDirection expected)
    {
        Assert.IsTrue(DualSenseReport.TryDecode(Fixtures.Report(fixture), out GamepadState state));
        Assert.AreEqual(expected, state.Hat);
        Assert.AreEqual(GamepadButtons.None, state.Buttons, "a direction is not a button press");
    }

    [Test]
    public void PressingTheDPadDoesNotMoveTheSticks()
    {
        DualSenseReport.TryDecode(Fixtures.Report("hat-south.txt"), out GamepadState state);
        Assert.IsTrue(state.LeftStick.Length < 0.1f, "the hat and the stick are different hardware");
    }

    [Test]
    public void TheTriggersReportAnAnalogueValueAlongsideTheirButton()
    {
        DualSenseReport.TryDecode(Fixtures.Report("l2.txt"), out GamepadState left);
        Assert.IsTrue(left.IsDown(GamepadButtons.LeftTrigger));
        Assert.IsTrue(left.LeftTrigger > 0.0f, "the digital bit came with an analogue value");
        Assert.ApproximatelyEqual(0.0f, left.RightTrigger);

        DualSenseReport.TryDecode(Fixtures.Report("r2.txt"), out GamepadState right);
        Assert.IsTrue(right.IsDown(GamepadButtons.RightTrigger));
        Assert.IsTrue(right.RightTrigger > 0.0f);
    }

    [Test]
    public void EveryCapturedReportIsAUsbReportOfSixtyFourBytes()
    {
        foreach (string fixture in Fixtures.Names())
        {
            byte[] report = HidReportHex.Parse(Fixtures.Text(Name(fixture)));
            if (fixture.EndsWith("bluetooth.txt", System.StringComparison.Ordinal))
            {
                continue;
            }

            Assert.AreEqual(DualSenseReport.UsbLength, report.Length, fixture);
            Assert.AreEqual(HidTransport.Usb, DualSenseReport.TransportOf(report), fixture);
        }
    }

    [Test]
    public void EveryFixtureSaysWhetherItWasCapturedOrConstructed()
    {
        foreach (string fixture in Fixtures.Names())
        {
            string text = Fixtures.Text(Name(fixture));
            bool captured = text.Contains("CAPTURED FROM HARDWARE", System.StringComparison.Ordinal);
            bool constructed = text.Contains("CONSTRUCTED, NOT CAPTURED", System.StringComparison.Ordinal)
                || text.Contains("NOT CAPTURED FROM HARDWARE", System.StringComparison.Ordinal);
            Assert.IsTrue(captured || constructed, fixture + " says where it came from");
        }
    }

    // Resource names arrive fully qualified; the fixture loader wants the tail.
    private static string Name(string resource) => resource[(resource.LastIndexOf(".dualsense.", System.StringComparison.Ordinal) + ".dualsense.".Length)..];
}
