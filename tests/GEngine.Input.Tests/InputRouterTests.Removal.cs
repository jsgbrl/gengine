// The half of the router tests about sources leaving: removed, disposed, or unplugged while
// they were the one the title screen was naming.

using System;
using GEngine.Core.Contracts;
using GEngine.Input.Actions;
using GEngine.Input.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <content>Removing sources, and what happens to the active one.</content>
public sealed partial class InputRouterTests
{
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
}
