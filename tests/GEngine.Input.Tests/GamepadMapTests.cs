using GEngine.Core.Contracts;
using GEngine.Input.Gamepad;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>Covers <see cref="GamepadMap"/>.</summary>
public sealed class GamepadMapTests
{
    [Test]
    public void ANewMapIsEmpty()
    {
        Assert.AreEqual(0, new GamepadMap().Count);
    }

    [Test]
    public void TheDefaultMapPutsJumpOnCrossAndPauseOnOptions()
    {
        var map = GamepadMap.CreateDefault();
        Assert.AreEqual(InputAction.Jump, map.Bindings[GamepadButtons.Cross]);
        Assert.AreEqual(InputAction.Run, map.Bindings[GamepadButtons.Square]);
        Assert.AreEqual(InputAction.Pause, map.Bindings[GamepadButtons.Options]);
    }

    [Test]
    public void CreateDefault_ReturnsAFreshMapEachTime()
    {
        GamepadMap.CreateDefault().Unbind(GamepadButtons.Cross);
        Assert.IsTrue(GamepadMap.CreateDefault().Bindings.ContainsKey(GamepadButtons.Cross));
    }

    [Test]
    public void Bind_ReplacesWhateverTheButtonDidBefore()
    {
        var map = new GamepadMap();
        map.Bind(GamepadButtons.Cross, InputAction.Jump);
        map.Bind(GamepadButtons.Cross, InputAction.Run);
        Assert.AreEqual(InputAction.Run, map.Bindings[GamepadButtons.Cross]);
        Assert.AreEqual(1, map.Count);
    }

    [Test]
    public void Unbind_RemovesTheButtonAndSaysWhetherItWasThere()
    {
        var map = new GamepadMap();
        map.Bind(GamepadButtons.Cross, InputAction.Jump);
        Assert.IsTrue(map.Unbind(GamepadButtons.Cross));
        Assert.IsFalse(map.Unbind(GamepadButtons.Cross));
    }

    [Test]
    public void ButtonsFor_IsSortedSoAListingIsReproducible()
    {
        var map = GamepadMap.CreateDefault();
        Assert.AreEqual(GamepadButtons.Square, map.ButtonsFor(InputAction.Run)[0]);
        Assert.AreEqual(GamepadButtons.RightTrigger, map.ButtonsFor(InputAction.Run)[1]);
    }

    [Test]
    public void Describe_SaysStickOrDPadForTheMovementActions()
    {
        var map = GamepadMap.CreateDefault();
        Assert.AreEqual("STICK OR D-PAD", map.Describe(InputAction.MoveLeft));
        Assert.AreEqual("STICK OR D-PAD", map.Describe(InputAction.MoveDown));
    }

    [Test]
    public void Describe_NamesTheButtonsOrSaysUnbound()
    {
        var map = GamepadMap.CreateDefault();
        Assert.AreEqual("CROSS", map.Describe(InputAction.Jump));
        Assert.AreEqual("SQUARE OR RIGHTTRIGGER", map.Describe(InputAction.Run));
        Assert.AreEqual("UNBOUND", new GamepadMap().Describe(InputAction.Jump));
    }
}
