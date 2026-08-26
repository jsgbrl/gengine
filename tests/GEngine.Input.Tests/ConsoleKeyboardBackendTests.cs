using System;
using GEngine.Core.Contracts;
using GEngine.Input.Actions;
using GEngine.Input.Keyboard;
using GEngine.Input.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>
/// Covers <see cref="ConsoleKeyboardBackend"/>, and above all the decay it is built on: a
/// terminal delivers key presses and never key releases, so an action stays on for a while
/// after the last press that asked for it.
/// </summary>
public sealed class ConsoleKeyboardBackendTests
{
    private const float Frame = 1.0f / 60.0f;

    private ScriptedKeyReader _reader = new();
    private ConsoleKeyboardBackend _keyboard = new(new ScriptedKeyReader(), InputMap.CreateDefault());

    [Setup]
    public void Setup()
    {
        _reader = new ScriptedKeyReader();
        _keyboard = new ConsoleKeyboardBackend(_reader, InputMap.CreateDefault());
    }

    [Test]
    public void AKeyboardIsAlwaysConnectedAndNamesItself()
    {
        Assert.IsTrue(_keyboard.IsConnected);
        Assert.AreEqual("keyboard", _keyboard.Name);
    }

    [Test]
    public void APressedKeyTurnsItsActionOn()
    {
        _reader.Press(ConsoleKey.Spacebar);
        _keyboard.Poll(Frame);
        Assert.IsTrue(_keyboard.IsDown(InputAction.Jump));
        Assert.ApproximatelyEqual(1.0f, _keyboard.AxisValue(InputAction.Jump));
    }

    [Test]
    public void AnUnboundKeyIsConsumedAndDoesNothing()
    {
        _reader.Press(ConsoleKey.F12);
        _keyboard.Poll(Frame);
        Assert.AreEqual(1, _keyboard.LastKeyCount);
        foreach (InputAction action in InputActions.All)
        {
            Assert.IsFalse(_keyboard.IsDown(action));
        }
    }

    [Test]
    public void AnActionStaysOnForTheDecayAndThenGoesOff()
    {
        _reader.Press(ConsoleKey.Spacebar);
        _keyboard.Poll(Frame);
        for (int frame = 0; frame < 10; frame++)
        {
            _keyboard.Poll(Frame);
        }

        Assert.IsTrue(_keyboard.IsDown(InputAction.Jump), "ten frames is inside the decay");

        for (int frame = 0; frame < 10; frame++)
        {
            _keyboard.Poll(Frame);
        }

        Assert.IsFalse(_keyboard.IsDown(InputAction.Jump), "twenty is past it");
    }

    [Test]
    public void EveryRepeatRefreshesTheDecay()
    {
        for (int frame = 0; frame < 60; frame++)
        {
            _reader.Press(ConsoleKey.RightArrow);
            _keyboard.Poll(Frame);
            Assert.IsTrue(_keyboard.IsDown(InputAction.MoveRight));
        }
    }

    [Test]
    public void TheDecayCanBeChanged()
    {
        var quick = new ConsoleKeyboardBackend(_reader, InputMap.CreateDefault()) { DecaySeconds = Frame };
        _reader.Press(ConsoleKey.Spacebar);
        quick.Poll(Frame);
        Assert.IsTrue(quick.IsDown(InputAction.Jump));
        quick.Poll(Frame);
        Assert.IsFalse(quick.IsDown(InputAction.Jump));
    }

    [Test]
    public void SeveralKeysInOnePollAllTakeEffect()
    {
        _reader.Press(ConsoleKey.RightArrow, ConsoleKey.Spacebar, ConsoleKey.X);
        _keyboard.Poll(Frame);
        Assert.AreEqual(3, _keyboard.LastKeyCount);
        Assert.IsTrue(_keyboard.IsDown(InputAction.MoveRight));
        Assert.IsTrue(_keyboard.IsDown(InputAction.Jump));
        Assert.IsTrue(_keyboard.IsDown(InputAction.Run));
    }

    [Test]
    public void RebindingTheMapTakesEffectOnTheNextPoll()
    {
        _keyboard.Map.Bind(ConsoleKey.Q, InputAction.Jump);
        _reader.Press(ConsoleKey.Q);
        _keyboard.Poll(Frame);
        Assert.IsTrue(_keyboard.IsDown(InputAction.Jump));
    }

    [Test]
    public void Clear_ForgetsEveryHeldAction()
    {
        _reader.Press(ConsoleKey.Spacebar);
        _keyboard.Poll(Frame);
        _keyboard.Clear();
        Assert.IsFalse(_keyboard.IsDown(InputAction.Jump));
    }

    [Test]
    public void Dispose_ForgetsEveryHeldActionToo()
    {
        _reader.Press(ConsoleKey.Spacebar);
        _keyboard.Poll(Frame);
        _keyboard.Dispose();
        Assert.IsFalse(_keyboard.IsDown(InputAction.Jump));
    }

    [Test]
    public void AMissingReaderOrMapIsRefused()
    {
        Assert.Throws<ArgumentNullException>(static () => new ConsoleKeyboardBackend(null!, InputMap.CreateDefault()));
        Assert.Throws<ArgumentNullException>(() => new ConsoleKeyboardBackend(_reader, null!));
    }
}
