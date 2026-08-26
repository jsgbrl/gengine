using System;
using GEngine.Core.Contracts;
using GEngine.Input.Scripted;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>Covers <see cref="FakeInputBackend"/>.</summary>
public sealed class FakeInputBackendTests
{
    private const float Frame = 1.0f / 60.0f;

    [Test]
    public void ItPlaysOneFramePerPoll()
    {
        var backend = new FakeInputBackend(new InputScript().Hold(InputAction.Jump, 1, 1));
        backend.Poll(Frame);
        Assert.IsFalse(backend.IsDown(InputAction.Jump), "frame zero has nothing in it");
        backend.Poll(Frame);
        Assert.IsTrue(backend.IsDown(InputAction.Jump), "frame one does");
        backend.Poll(Frame);
        Assert.IsFalse(backend.IsDown(InputAction.Jump));
    }

    [Test]
    public void NothingIsDownBeforeTheFirstPoll()
    {
        var backend = new FakeInputBackend(new InputScript().Hold(InputAction.Jump, 0, 10));
        Assert.IsFalse(backend.IsDown(InputAction.Jump));
        Assert.AreEqual(0, backend.Frame);
    }

    [Test]
    public void AxisValueIsOneOrZero_BecauseAScriptIsDigital()
    {
        var backend = new FakeInputBackend(new InputScript().Hold(InputAction.MoveRight, 0, 0));
        backend.Poll(Frame);
        Assert.ApproximatelyEqual(1.0f, backend.AxisValue(InputAction.MoveRight));
        Assert.ApproximatelyEqual(0.0f, backend.AxisValue(InputAction.Jump));
    }

    [Test]
    public void AFinishedScriptDisconnectsRatherThanHoldingItsLastFrame()
    {
        var backend = new FakeInputBackend(new InputScript().Hold(InputAction.Jump, 0, 1));
        Assert.IsTrue(backend.IsConnected);
        backend.Poll(Frame);
        backend.Poll(Frame);
        Assert.IsFalse(backend.IsConnected);
    }

    [Test]
    public void AnEmptyScriptIsDisconnectedFromTheStart()
    {
        Assert.IsFalse(new FakeInputBackend(new InputScript()).IsConnected);
    }

    [Test]
    public void Rewind_StartsTheScriptAgain()
    {
        var backend = new FakeInputBackend(new InputScript().Hold(InputAction.Jump, 0, 0));
        backend.Poll(Frame);
        backend.Rewind();
        Assert.AreEqual(0, backend.Frame);
        Assert.IsFalse(backend.IsDown(InputAction.Jump));
        Assert.IsTrue(backend.IsConnected);
    }

    [Test]
    public void TwoRunsOfTheSameScriptAreIdentical()
    {
        InputScript script = new InputScript().Hold(InputAction.MoveRight, 0, 5).Hold(InputAction.Jump, 2, 3);
        Assert.MatchesSnapshot(Play(script), Play(script));
    }

    [Test]
    public void AMissingScriptIsRefused()
    {
        Assert.Throws<ArgumentNullException>(static () => new FakeInputBackend(null!));
    }

    private static string Play(InputScript script)
    {
        var backend = new FakeInputBackend(script);
        var log = new System.Text.StringBuilder();
        while (backend.IsConnected)
        {
            backend.Poll(Frame);
            log.Append(backend.IsDown(InputAction.MoveRight) ? 'R' : '.');
            log.Append(backend.IsDown(InputAction.Jump) ? 'J' : '.');
            log.Append('\n');
        }

        return log.ToString();
    }
}
