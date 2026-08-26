using GEngine.Core.Contracts;
using GEngine.Input.Actions;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>Covers <see cref="InputState"/>: the three questions and the hold time.</summary>
public sealed class InputStateTests
{
    private InputState _state = new();

    [Setup]
    public void Setup() => _state = new InputState();

    [Test]
    public void ANewStateHasNothingDown()
    {
        Assert.IsFalse(_state.IsDown(InputAction.Jump));
        Assert.IsFalse(_state.WasPressedThisFrame(InputAction.Jump));
        Assert.ApproximatelyEqual(0.0f, _state.AxisValue(InputAction.Jump));
    }

    [Test]
    public void AReportedActionIsDownForThatFrame()
    {
        Frame(InputAction.Jump);
        Assert.IsTrue(_state.IsDown(InputAction.Jump));
        Assert.ApproximatelyEqual(1.0f, _state.AxisValue(InputAction.Jump));
    }

    [Test]
    public void WasPressedThisFrame_IsTrueOnlyOnTheFrameItWentDown()
    {
        Frame(InputAction.Jump);
        Assert.IsTrue(_state.WasPressedThisFrame(InputAction.Jump));
        Frame(InputAction.Jump);
        Assert.IsFalse(_state.WasPressedThisFrame(InputAction.Jump), "still down is not just pressed");
        Assert.IsTrue(_state.IsDown(InputAction.Jump));
    }

    [Test]
    public void WasReleasedThisFrame_IsTrueOnlyOnTheFrameItCameUp()
    {
        Frame(InputAction.Jump);
        Frame();
        Assert.IsTrue(_state.WasReleasedThisFrame(InputAction.Jump));
        Frame();
        Assert.IsFalse(_state.WasReleasedThisFrame(InputAction.Jump));
    }

    [Test]
    public void PressAndReleaseInConsecutiveFrames_AreBothSeen()
    {
        Frame(InputAction.Jump);
        Assert.IsTrue(_state.WasPressedThisFrame(InputAction.Jump));
        Frame();
        Assert.IsTrue(_state.WasReleasedThisFrame(InputAction.Jump));
    }

    [Test]
    public void HoldTime_AccumulatesWhileDownAndResetsWhenReleased()
    {
        Frame(InputAction.Run);
        Frame(InputAction.Run);
        Assert.ApproximatelyEqual(2.0f / 60.0f, _state.HoldTimeSeconds(InputAction.Run), 1e-4f);
        Frame();
        Assert.ApproximatelyEqual(0.0f, _state.HoldTimeSeconds(InputAction.Run));
    }

    [Test]
    public void TheStrongestReportOfAFrameWins()
    {
        _state.Begin();
        _state.Report(InputAction.MoveLeft, 0.3f);
        _state.Report(InputAction.MoveLeft, 0.9f);
        _state.Report(InputAction.MoveLeft, 0.1f);
        _state.End(1.0f / 60.0f);
        Assert.ApproximatelyEqual(0.9f, _state.AxisValue(InputAction.MoveLeft));
    }

    [Test]
    public void AnAnalogueValueBelowTheThresholdIsNotDown()
    {
        _state.Begin();
        _state.Report(InputAction.MoveLeft, 0.4f);
        _state.End(1.0f / 60.0f);
        Assert.IsFalse(_state.IsDown(InputAction.MoveLeft));
        Assert.ApproximatelyEqual(0.4f, _state.AxisValue(InputAction.MoveLeft), 0.001f);
    }

    [Test]
    public void TheThresholdCanBeChanged()
    {
        var sensitive = new InputState { DownThreshold = 0.1f };
        sensitive.Begin();
        sensitive.Report(InputAction.MoveLeft, 0.2f);
        sensitive.End(1.0f / 60.0f);
        Assert.IsTrue(sensitive.IsDown(InputAction.MoveLeft));
    }

    [Test]
    public void AReportAboveOneIsClampedRatherThanTrusted()
    {
        _state.Begin();
        _state.Report(InputAction.MoveLeft, 5.0f);
        _state.End(1.0f / 60.0f);
        Assert.ApproximatelyEqual(1.0f, _state.AxisValue(InputAction.MoveLeft));
    }

    [Test]
    public void Clear_ForgetsEverythingIncludingTheEdges()
    {
        Frame(InputAction.Jump);
        _state.Clear();
        Assert.IsFalse(_state.IsDown(InputAction.Jump));
        Assert.IsFalse(_state.WasReleasedThisFrame(InputAction.Jump));
        Assert.ApproximatelyEqual(0.0f, _state.HoldTimeSeconds(InputAction.Jump));
    }

    private void Frame(params InputAction[] held)
    {
        _state.Begin();
        foreach (InputAction action in held)
        {
            _state.Report(action, 1.0f);
        }

        _state.End(1.0f / 60.0f);
    }
}
