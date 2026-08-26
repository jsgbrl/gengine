using System;
using GEngine.Core.Contracts;
using GEngine.Input.Actions;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>Covers reading and writing the text form of an <see cref="InputMap"/>.</summary>
public sealed class InputMapTextTests
{
    [Test]
    public void AWellFormedFileBinds()
    {
        var map = InputMap.Parse("# controls\nLeftArrow MoveLeft\nSpacebar  Jump\n");
        Assert.AreEqual(2, map.Count);
        Assert.IsTrue(map.TryGetAction(ConsoleKey.Spacebar, out InputAction action));
        Assert.AreEqual(InputAction.Jump, action);
    }

    [Test]
    public void NamesAreReadWithoutRegardToCase()
    {
        var map = InputMap.Parse("leftarrow moveleft\n");
        Assert.IsTrue(map.TryGetAction(ConsoleKey.LeftArrow, out _));
    }

    [Test]
    public void CommentsAndBlankLinesAreIgnored()
    {
        Assert.AreEqual(1, InputMap.Parse("\n# a comment\n\nQ Jump\n").Count);
    }

    [Test]
    public void AnUnknownKeyIsNamedInTheError()
    {
        FormatException failure = Assert.Throws<FormatException>(static () => InputMap.Parse("Banana Jump\n"));
        Assert.IsTrue(failure.Message.Contains("Banana", StringComparison.Ordinal));
    }

    [Test]
    public void AnUnknownActionIsNamedInTheError()
    {
        FormatException failure = Assert.Throws<FormatException>(static () => InputMap.Parse("Q Dance\n"));
        Assert.IsTrue(failure.Message.Contains("Dance", StringComparison.Ordinal));
    }

    [Test]
    public void AMalformedLineIsAnError()
    {
        Assert.Throws<FormatException>(static () => InputMap.Parse("Q\n"));
        Assert.Throws<FormatException>(static () => InputMap.Parse("Q Jump Run\n"));
    }

    [Test]
    public void WritingAndReadingAMapGivesTheSameMapBack()
    {
        var original = InputMap.CreateDefault();
        var reloaded = InputMap.Parse(original.ToText());
        Assert.AreEqual(original.Count, reloaded.Count);
        Assert.MatchesSnapshot(reloaded.ToText(), original.ToText());
    }

    [Test]
    public void TheTextFormIsGroupedByActionInDeclarationOrder()
    {
        var map = new InputMap();
        map.Bind(ConsoleKey.Q, InputAction.Cancel);
        map.Bind(ConsoleKey.W, InputAction.MoveLeft);
        Assert.MatchesSnapshot(map.ToText(), "W MoveLeft\nQ Cancel");
    }
}
