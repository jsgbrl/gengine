using GEngine.Core.Contracts;
using GEngine.Input.Gamepad;
using GEngine.Input.Hid;
using GEngine.Input.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>
/// Covers <see cref="DualSenseGamepad"/> with no controller plugged in: connecting, decoding,
/// mapping to actions, refusing Bluetooth, and surviving the cable coming out mid-level.
/// </summary>
public sealed partial class DualSenseGamepadTests
{
    private const float Frame = 1.0f / 60.0f;

    private static readonly HidDeviceInfo Controller =
        new(KnownDevices.SonyVendorId, KnownDevices.DualSenseProductId, "fake://pad", "DualSense");

    private MemoryLogger _logger = new();

    [Setup]
    public void Setup() => _logger = new MemoryLogger();

    [Test]
    public void ItStartsDisconnectedAndLooksForNothingUntilTheFirstPoll()
    {
        using DualSenseGamepad pad = Gamepad(new FakeHidBackend());
        Assert.IsFalse(pad.IsConnected);
        Assert.AreEqual(GamepadState.Neutral, pad.State);
    }

    [Test]
    public void ItFindsAControllerOnTheFirstRescanAndSaysSo()
    {
        using DualSenseGamepad pad = Gamepad(BackendWith(Neutral()));
        pad.Poll(2.0f);
        Assert.IsTrue(pad.IsConnected);
        Assert.IsTrue(_logger.Contains("using a DualSense"));
    }

    [Test]
    public void ItDoesNotRescanMoreOftenThanItWasTold()
    {
        FakeHidBackend backend = BackendWith(Neutral());
        using DualSenseGamepad pad = Gamepad(backend);
        pad.Poll(Frame);
        Assert.IsFalse(pad.IsConnected, "a sixtieth of a second is not a second");
        Assert.AreEqual(0, backend.OpenCount);
    }

    [Test]
    public void ItIgnoresEveryHidDeviceThatIsNotAController()
    {
        FakeHidBackend backend = new FakeHidBackend()
            .With(new HidDeviceInfo(0x046D, 0xC52B, "fake://keyboard"), new FakeHidDevice())
            .With(new HidDeviceInfo(0x05AC, 0x0259, "fake://mouse"), new FakeHidDevice());

        using DualSenseGamepad pad = Gamepad(backend);
        pad.Poll(2.0f);
        Assert.IsFalse(pad.IsConnected);
        Assert.AreEqual(0, backend.OpenCount);
    }

    [Test]
    public void AControllerItCannotOpenIsReportedRatherThanIgnored()
    {
        FakeHidBackend backend = BackendWith(Neutral());
        backend.RefusesToOpen = true;
        using DualSenseGamepad pad = Gamepad(backend);
        pad.Poll(2.0f);
        Assert.IsFalse(pad.IsConnected);
        Assert.IsTrue(_logger.Contains("could not open it"));
    }

    [Test]
    public void PressingCrossAsksForJump()
    {
        using DualSenseGamepad pad = Connected(Fixtures.Report("cross.txt"));
        pad.Poll(Frame);
        Assert.IsTrue(pad.IsDown(InputAction.Jump));
        Assert.ApproximatelyEqual(1.0f, pad.AxisValue(InputAction.Jump));
        Assert.IsFalse(pad.IsDown(InputAction.Run));
    }

    [Test]
    public void PushingTheStickRightAsksForMoveRight_Analogically()
    {
        byte[] halfRight = Neutral();
        halfRight[1] = 0xC0;
        using DualSenseGamepad pad = Connected(halfRight);
        pad.Poll(Frame);
        Assert.IsTrue(pad.AxisValue(InputAction.MoveRight) > 0.3f);
        Assert.IsTrue(pad.AxisValue(InputAction.MoveRight) < 0.7f, "half way is half way, not all the way");
        Assert.ApproximatelyEqual(0.0f, pad.AxisValue(InputAction.MoveLeft));
    }

    [Test]
    public void AStickInsideTheDeadZoneAsksForNothing()
    {
        byte[] nudged = Neutral();
        nudged[1] = 0x85;
        using DualSenseGamepad pad = Connected(nudged);
        pad.Poll(Frame);
        Assert.ApproximatelyEqual(0.0f, pad.AxisValue(InputAction.MoveRight));
    }

    [Test]
    public void TheDPadAsksForMovementAtFullStrength()
    {
        using DualSenseGamepad pad = Connected(Fixtures.Report("hat-east.txt"));
        pad.Poll(Frame);
        Assert.ApproximatelyEqual(1.0f, pad.AxisValue(InputAction.MoveRight));
    }

    private static byte[] Neutral() => Fixtures.Report("neutral.txt");

    private static FakeHidBackend BackendWith(byte[] report) =>
        new FakeHidBackend().With(Controller, new FakeHidDevice().Queue(report));

    private DualSenseGamepad Gamepad(IHidBackend backend) =>
        new(backend, GamepadMap.CreateDefault(), _logger) { Reading = GamepadReading.WhenPolled };

    private DualSenseGamepad Connected(byte[] report)
    {
        DualSenseGamepad pad = Gamepad(BackendWith(report));
        pad.Poll(2.0f);
        return pad;
    }
}
