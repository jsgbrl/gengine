using System;
using System.Collections.Generic;
using GEngine.Core.Contracts;
using GEngine.Input.Actions;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>Covers <see cref="InputMap"/>.</summary>
public sealed class InputMapTests
{
    [Test]
    public void ANewMapIsEmpty()
    {
        Assert.AreEqual(0, new InputMap().Count);
        Assert.IsFalse(new InputMap().TryGetAction(ConsoleKey.Spacebar, out _));
    }

    [Test]
    public void TheDefaultMapCoversEveryActionAGameNeeds()
    {
        var map = InputMap.CreateDefault();
        foreach (InputAction action in InputActions.All)
        {
            Assert.IsTrue(map.KeysFor(action).Count > 0, action + " is bound to something");
        }
    }

    [Test]
    public void TheDefaultMapOffersArrowsAndLetters()
    {
        var map = InputMap.CreateDefault();
        Assert.IsTrue(map.TryGetAction(ConsoleKey.LeftArrow, out InputAction fromArrow));
        Assert.IsTrue(map.TryGetAction(ConsoleKey.A, out InputAction fromLetter));
        Assert.AreEqual(InputAction.MoveLeft, fromArrow);
        Assert.AreEqual(InputAction.MoveLeft, fromLetter);
    }

    [Test]
    public void CreateDefault_ReturnsAFreshMapEachTime()
    {
        var first = InputMap.CreateDefault();
        first.Unbind(ConsoleKey.Spacebar);
        Assert.IsTrue(InputMap.CreateDefault().TryGetAction(ConsoleKey.Spacebar, out _));
    }

    [Test]
    public void Bind_ReplacesWhateverTheKeyDidBefore()
    {
        var map = new InputMap();
        map.Bind(ConsoleKey.Q, InputAction.Jump);
        map.Bind(ConsoleKey.Q, InputAction.Run);
        map.TryGetAction(ConsoleKey.Q, out InputAction action);
        Assert.AreEqual(InputAction.Run, action);
        Assert.AreEqual(1, map.Count);
    }

    [Test]
    public void Unbind_RemovesTheKeyAndSaysWhetherItWasThere()
    {
        var map = new InputMap();
        map.Bind(ConsoleKey.Q, InputAction.Jump);
        Assert.IsTrue(map.Unbind(ConsoleKey.Q));
        Assert.IsFalse(map.Unbind(ConsoleKey.Q));
    }

    [Test]
    public void KeysFor_IsSortedSoAListingIsReproducible()
    {
        var map = new InputMap();
        map.Bind(ConsoleKey.Z, InputAction.Jump);
        map.Bind(ConsoleKey.A, InputAction.Jump);
        IReadOnlyList<ConsoleKey> keys = map.KeysFor(InputAction.Jump);
        Assert.AreEqual(ConsoleKey.A, keys[0]);
        Assert.AreEqual(ConsoleKey.Z, keys[1]);
    }

    [Test]
    public void Describe_ReadsAsSomethingATitleScreenCanPrint()
    {
        var map = new InputMap();
        map.Bind(ConsoleKey.A, InputAction.MoveLeft);
        map.Bind(ConsoleKey.LeftArrow, InputAction.MoveLeft);
        Assert.AreEqual("LEFTARROW OR A", map.Describe(InputAction.MoveLeft));
        Assert.AreEqual("UNBOUND", map.Describe(InputAction.Pause));
    }
}
