using System;
using GEngine.Core.Contracts;
using GEngine.Input.Scripted;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>Covers <see cref="InputScript"/>.</summary>
public sealed class InputScriptTests
{
    [Test]
    public void ANewScriptIsEmpty()
    {
        var script = new InputScript();
        Assert.AreEqual(0, script.FrameCount);
        Assert.AreEqual(0, script.HoldCount);
        Assert.IsFalse(script.IsHeld(0, InputAction.Jump));
    }

    [Test]
    public void AHoldCoversBothEndsOfItsRange()
    {
        InputScript script = new InputScript().Hold(InputAction.Jump, 10, 12);
        Assert.IsFalse(script.IsHeld(9, InputAction.Jump));
        Assert.IsTrue(script.IsHeld(10, InputAction.Jump));
        Assert.IsTrue(script.IsHeld(12, InputAction.Jump));
        Assert.IsFalse(script.IsHeld(13, InputAction.Jump));
    }

    [Test]
    public void FrameCount_IsOnePastTheLastFrameAnyHoldCovers()
    {
        InputScript script = new InputScript().Hold(InputAction.Jump, 10, 12).Hold(InputAction.Run, 0, 40);
        Assert.AreEqual(41, script.FrameCount);
    }

    [Test]
    public void HoldsOfDifferentActionsOverlapWithoutInterfering()
    {
        InputScript script = new InputScript().Hold(InputAction.MoveRight, 0, 100).Hold(InputAction.Jump, 50, 51);
        Assert.IsTrue(script.IsHeld(50, InputAction.MoveRight));
        Assert.IsTrue(script.IsHeld(50, InputAction.Jump));
        Assert.IsFalse(script.IsHeld(52, InputAction.Jump));
    }

    [Test]
    public void ABackwardsOrNegativeRangeIsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () => new InputScript().Hold(InputAction.Jump, 5, 4));
        Assert.Throws<ArgumentOutOfRangeException>(static () => new InputScript().Hold(InputAction.Jump, -1, 4));
    }

    [Test]
    public void Parse_ReadsRangesAndSingleFrames()
    {
        var script = InputScript.Parse("# a run\n0-400 MoveRight\n60 Jump\n");
        Assert.IsTrue(script.IsHeld(400, InputAction.MoveRight));
        Assert.IsTrue(script.IsHeld(60, InputAction.Jump));
        Assert.IsFalse(script.IsHeld(61, InputAction.Jump));
    }

    [Test]
    public void Parse_ReadsSeveralActionsOnOneLine()
    {
        var script = InputScript.Parse("0-10 MoveRight Run\n");
        Assert.IsTrue(script.IsHeld(5, InputAction.MoveRight));
        Assert.IsTrue(script.IsHeld(5, InputAction.Run));
    }

    [Test]
    public void Parse_ReadsNamesWithoutRegardToCase()
    {
        Assert.IsTrue(InputScript.Parse("0 jump\n").IsHeld(0, InputAction.Jump));
    }

    [Test]
    public void Parse_RejectsAMalformedLine()
    {
        Assert.Throws<FormatException>(static () => InputScript.Parse("0-10\n"));
        Assert.Throws<FormatException>(static () => InputScript.Parse("early Jump\n"));
        Assert.Throws<FormatException>(static () => InputScript.Parse("0-late Jump\n"));
        Assert.Throws<FormatException>(static () => InputScript.Parse("0-10 Dance\n"));
    }

    [Test]
    public void WritingAndReadingAScriptGivesTheSameScriptBack()
    {
        InputScript original = new InputScript().Hold(InputAction.MoveRight, 0, 400).Hold(InputAction.Jump, 60, 70);
        var reloaded = InputScript.Parse(original.ToText());
        Assert.AreEqual(original.FrameCount, reloaded.FrameCount);
        Assert.MatchesSnapshot(reloaded.ToText(), original.ToText());
        Assert.MatchesSnapshot(original.ToText(), "0-400 MoveRight\n60-70 Jump");
    }
}
