using GEngine.Core;
using GEngine.Input.Gamepad;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>Covers <see cref="GamepadState"/>.</summary>
public sealed class GamepadStateTests
{
    [Test]
    public void NeutralIsCentredWithNothingPressed_AndTheHatSaysSo()
    {
        GamepadState state = GamepadState.Neutral;
        Assert.AreEqual(Vector2.Zero, state.LeftStick);
        Assert.AreEqual(GamepadButtons.None, state.Buttons);
        Assert.AreEqual(HatDirection.Neutral, state.Hat, "the default enum value is north, not neutral");
    }

    [Test]
    public void TriggersAreReadableByNameAndAsAVector()
    {
        var state = new GamepadState(Vector2.Zero, Vector2.Zero, new Vector2(0.25f, 0.75f), GamepadButtons.None);
        Assert.ApproximatelyEqual(0.25f, state.LeftTrigger);
        Assert.ApproximatelyEqual(0.75f, state.RightTrigger);
        Assert.AreEqual(new Vector2(0.25f, 0.75f), state.Triggers);
    }

    [Test]
    public void IsDown_AsksForEveryNamedButton()
    {
        GamepadState state = Pressing(GamepadButtons.Cross | GamepadButtons.Square);
        Assert.IsTrue(state.IsDown(GamepadButtons.Cross));
        Assert.IsTrue(state.IsDown(GamepadButtons.Cross | GamepadButtons.Square));
        Assert.IsFalse(state.IsDown(GamepadButtons.Cross | GamepadButtons.Circle));
    }

    [Test]
    public void IsAnyDown_AsksForAtLeastOne()
    {
        GamepadState state = Pressing(GamepadButtons.Cross);
        Assert.IsTrue(state.IsAnyDown(GamepadButtons.Cross | GamepadButtons.Circle));
        Assert.IsFalse(state.IsAnyDown(GamepadButtons.Circle | GamepadButtons.Triangle));
    }

    [Test]
    public void Equality_ComparesEveryField()
    {
        GamepadState state = Pressing(GamepadButtons.Cross);
        Assert.IsTrue(state == Pressing(GamepadButtons.Cross));
        Assert.IsTrue(state != Pressing(GamepadButtons.Circle));
        Assert.IsTrue(state.Equals((object)Pressing(GamepadButtons.Cross)));
        Assert.IsFalse(state.Equals("not a gamepad"));
        Assert.AreEqual(state.GetHashCode(), Pressing(GamepadButtons.Cross).GetHashCode());
    }

    [Test]
    public void TwoStatesDifferingOnlyInTheHatAreNotEqual()
    {
        var north = new GamepadState(Vector2.Zero, Vector2.Zero, Vector2.Zero, GamepadButtons.None);
        GamepadState neutral = north with { Hat = HatDirection.Neutral };
        Assert.IsTrue(north != neutral);
    }

    [Test]
    public void ToString_NamesEverythingThatIsHappening()
    {
        string text = Pressing(GamepadButtons.Cross).ToString();
        Assert.IsTrue(text.Contains("Cross", System.StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("hat", System.StringComparison.Ordinal));
    }

    private static GamepadState Pressing(GamepadButtons buttons) =>
        new GamepadState(Vector2.Zero, Vector2.Zero, Vector2.Zero, buttons) { Hat = HatDirection.Neutral };
}
