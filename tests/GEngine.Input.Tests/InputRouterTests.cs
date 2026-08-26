using System;
using GEngine.Core.Contracts;
using GEngine.Input.Actions;
using GEngine.Input.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>
/// Covers <see cref="InputRouter"/>. The point of the router is that swapping input sources
/// costs nothing, because there is no current source to swap: every connected one is polled
/// every frame and the strongest report wins.
/// </summary>
public sealed class InputRouterTests
{
    private const float Frame = 1.0f / 60.0f;

    private InputState _state = new();
    private InputRouter _router = new(new InputState());

    [Setup]
    public void Setup()
    {
        _state = new InputState();
        _router = new InputRouter(_state);
    }

    [Test]
    public void ANewRouterHasNoSourcesAndNoActiveOne()
    {
        Assert.AreEqual(0, _router.Backends.Count);
        Assert.AreEqual(0, _router.ConnectedCount);
        Assert.IsNull(_router.ActiveBackend);
        Assert.AreSame(_state, _router.State);
    }

    [Test]
    public void PollingCopiesWhatASourceReportsIntoTheState()
    {
        SwitchableBackend pad = Add("pad");
        pad.Set(InputAction.MoveRight, 0.8f);
        _router.Poll(Frame);
        Assert.ApproximatelyEqual(0.8f, _state.AxisValue(InputAction.MoveRight));
        Assert.IsTrue(_state.IsDown(InputAction.MoveRight));
    }

    [Test]
    public void TwoSourcesAskingForTheSameThing_DoNotCancelOut()
    {
        SwitchableBackend pad = Add("pad");
        SwitchableBackend keyboard = Add("keyboard");
        pad.Set(InputAction.MoveLeft, 0.4f);
        keyboard.Set(InputAction.MoveLeft, 1.0f);
        _router.Poll(Frame);
        Assert.ApproximatelyEqual(1.0f, _state.AxisValue(InputAction.MoveLeft));
    }

    [Test]
    public void ADisconnectedSourceIsNotPolledAtAll()
    {
        SwitchableBackend pad = Add("pad");
        pad.IsConnected = false;
        _router.Poll(Frame);
        Assert.AreEqual(0, pad.PollCount);
        Assert.AreEqual(0, _router.ConnectedCount);
    }

    [Test]
    public void UnpluggingOneSourceMidGame_LeavesTheOtherAnsweringWithoutLosingAFrame()
    {
        SwitchableBackend pad = Add("pad");
        SwitchableBackend keyboard = Add("keyboard");
        pad.Set(InputAction.MoveRight, 1.0f);

        _router.Poll(Frame);
        Assert.IsTrue(_state.IsDown(InputAction.MoveRight));

        pad.IsConnected = false;
        keyboard.Set(InputAction.MoveRight, 1.0f);
        _router.Poll(Frame);

        Assert.IsTrue(_state.IsDown(InputAction.MoveRight), "the very next frame still moves right");
        Assert.IsFalse(_state.WasPressedThisFrame(InputAction.MoveRight), "and it never let go");
    }

    [Test]
    public void ActiveBackend_IsWhicheverSourceLastSaidSomething()
    {
        SwitchableBackend pad = Add("pad");
        SwitchableBackend keyboard = Add("keyboard");
        pad.Set(InputAction.Jump, 1.0f);
        _router.Poll(Frame);
        Assert.AreSame(pad, _router.ActiveBackend);

        pad.Set(InputAction.Jump, 0.0f);
        keyboard.Set(InputAction.Jump, 1.0f);
        _router.Poll(Frame);
        Assert.AreSame(keyboard, _router.ActiveBackend);
    }

    [Test]
    public void ActiveBackend_StaysPutWhileNobodyIsTouchingAnything()
    {
        SwitchableBackend pad = Add("pad");
        pad.Set(InputAction.Jump, 1.0f);
        _router.Poll(Frame);
        pad.Set(InputAction.Jump, 0.0f);
        _router.Poll(Frame);
        Assert.AreSame(pad, _router.ActiveBackend, "the title screen keeps showing gamepad controls");
    }

    [Test]
    public void Remove_TakesTheSourceOutAndDisposesIt()
    {
        SwitchableBackend pad = Add("pad");
        Assert.IsTrue(_router.Remove(pad));
        Assert.AreEqual(1, pad.DisposeCount);
        Assert.AreEqual(0, _router.Backends.Count);
        Assert.IsFalse(_router.Remove(pad));
    }

    [Test]
    public void RemovingTheActiveSource_ClearsIt()
    {
        SwitchableBackend pad = Add("pad");
        pad.Set(InputAction.Jump, 1.0f);
        _router.Poll(Frame);
        _router.Remove(pad);
        Assert.IsNull(_router.ActiveBackend);
    }

    [Test]
    public void Dispose_DisposesEverySourceAndForgetsThemAll()
    {
        SwitchableBackend pad = Add("pad");
        SwitchableBackend keyboard = Add("keyboard");
        _router.Dispose();
        Assert.AreEqual(1, pad.DisposeCount);
        Assert.AreEqual(1, keyboard.DisposeCount);
        Assert.AreEqual(0, _router.Backends.Count);
    }

    [Test]
    public void EdgesSurviveAcrossFrames()
    {
        SwitchableBackend pad = Add("pad");
        pad.Set(InputAction.Jump, 1.0f);
        _router.Poll(Frame);
        Assert.IsTrue(_state.WasPressedThisFrame(InputAction.Jump));

        pad.Set(InputAction.Jump, 0.0f);
        _router.Poll(Frame);
        Assert.IsTrue(_state.WasReleasedThisFrame(InputAction.Jump));
    }

    [Test]
    public void AMissingStateOrSourceIsRefused()
    {
        Assert.Throws<ArgumentNullException>(static () => new InputRouter(null!));
        Assert.Throws<ArgumentNullException>(() => _router.Add(null!));
    }

    private SwitchableBackend Add(string name)
    {
        var backend = new SwitchableBackend(name);
        _router.Add(backend);
        return backend;
    }
}
