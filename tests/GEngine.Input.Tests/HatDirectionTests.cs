using GEngine.Core;
using GEngine.Input.Gamepad;
using GEngine.Input.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>
/// Covers the D-pad: all eight directions plus neutral, decoded from the neutral fixture with
/// its hat nibble changed - which is exactly what the controller does to that byte.
/// </summary>
public sealed class HatDirectionTests
{
    [TestCase(0, HatDirection.North)]
    [TestCase(1, HatDirection.NorthEast)]
    [TestCase(2, HatDirection.East)]
    [TestCase(3, HatDirection.SouthEast)]
    [TestCase(4, HatDirection.South)]
    [TestCase(5, HatDirection.SouthWest)]
    [TestCase(6, HatDirection.West)]
    [TestCase(7, HatDirection.NorthWest)]
    [TestCase(8, HatDirection.Neutral)]
    public void EveryDirectionDecodesFromItsNibble(int nibble, HatDirection expected)
    {
        byte[] report = Fixtures.Report("neutral.txt");
        report[8] = (byte)nibble;
        DualSenseReport.TryDecode(report, out GamepadState state);
        Assert.AreEqual(expected, state.Hat);
    }

    [Test]
    public void TheFaceButtonsInTheSameByteDoNotDisturbTheHat()
    {
        byte[] report = Fixtures.Report("neutral.txt");
        report[8] = 0xF2;
        DualSenseReport.TryDecode(report, out GamepadState state);
        Assert.AreEqual(HatDirection.East, state.Hat);
        Assert.IsTrue(state.IsDown(GamepadButtons.Triangle));
    }

    [TestCase(9)]
    [TestCase(15)]
    public void AnUnexpectedNibbleReadsAsNeutralRatherThanAsGarbage(int nibble)
    {
        Assert.AreEqual(HatDirection.Neutral, DualSenseReport.HatOf((byte)nibble));
    }

    [Test]
    public void NorthIsNegativeYAndSouthIsPositiveY()
    {
        Assert.AreEqual(new Vector2(0.0f, -1.0f), HatDirections.ToVector(HatDirection.North));
        Assert.AreEqual(new Vector2(0.0f, 1.0f), HatDirections.ToVector(HatDirection.South));
    }

    [Test]
    public void EastIsPositiveXAndWestIsNegativeX()
    {
        Assert.AreEqual(new Vector2(1.0f, 0.0f), HatDirections.ToVector(HatDirection.East));
        Assert.AreEqual(new Vector2(-1.0f, 0.0f), HatDirections.ToVector(HatDirection.West));
    }

    [Test]
    public void ADiagonalIsBothAxesAtFullStrength()
    {
        Assert.AreEqual(new Vector2(1.0f, -1.0f), HatDirections.ToVector(HatDirection.NorthEast));
        Assert.AreEqual(new Vector2(-1.0f, 1.0f), HatDirections.ToVector(HatDirection.SouthWest));
    }

    [Test]
    public void NeutralIsNoMovementAtAll()
    {
        Assert.AreEqual(Vector2.Zero, HatDirections.ToVector(HatDirection.Neutral));
    }
}
