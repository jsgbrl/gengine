using System;
using GEngine.Core.Contracts;
using GEngine.Input.Actions;
using GEngine.Rendering;
using GEngine.Rendering.Assets;
using GEngine.Testing;
using MarioClone.Audio;
using MarioClone.Game;
using MarioClone.Tests.Doubles;

namespace MarioClone.Tests;

/// <summary>
/// The game as the loop sees it: three callbacks and a state machine. What is worth testing is
/// the machine, because a transition that should be impossible - unpausing into a death,
/// restarting from the title - is a bug nobody finds until somebody pauses at the wrong moment.
/// </summary>
public sealed class MarioGameTests
{
    private const float FixedDelta = 1.0f / 60.0f;

    [Test]
    public void AGameStartsOnTheTitleScreen()
    {
        using var harness = new Harness();
        Assert.AreEqual(GameStateKind.Title, harness.Game.State);
        Assert.IsFalse(harness.Game.IsFinished);
    }

    [Test]
    public void ConfirmStartsTheLevelAndSoDoesJump()
    {
        using var harness = new Harness();
        harness.Press(InputAction.Confirm);
        Assert.AreEqual(GameStateKind.Playing, harness.Game.State);

        using var second = new Harness();
        second.Press(InputAction.Jump);
        Assert.AreEqual(GameStateKind.Playing, second.Game.State, "a player reaching for jump gets a game");
    }

    [Test]
    public void CancelOnTheTitleScreenLeaves()
    {
        using var harness = new Harness();
        harness.Press(InputAction.Cancel);
        Assert.IsTrue(harness.Game.IsFinished, "the loop stops on the next frame");
    }

    // Both Pause and Cancel pause, so a player reaching for Escape gets something rather than
    // nothing; from the pause screen Cancel again goes back to the title.
    [Test]
    public void PauseAndCancelBothPauseAndPauseUnpauses()
    {
        using var harness = new Harness(startsImmediately: true);
        harness.Press(InputAction.Pause);
        Assert.AreEqual(GameStateKind.Paused, harness.Game.State);

        harness.Press(InputAction.Pause);
        Assert.AreEqual(GameStateKind.Playing, harness.Game.State);

        harness.Press(InputAction.Cancel);
        Assert.AreEqual(GameStateKind.Paused, harness.Game.State, "escape pauses too");

        harness.Press(InputAction.Cancel);
        Assert.AreEqual(GameStateKind.Title, harness.Game.State);
    }

    [Test]
    public void APausedGameIsFrozen()
    {
        using var harness = new Harness(startsImmediately: true);
        harness.Hold(InputAction.MoveRight);
        harness.Step(30);
        float moved = harness.Game.World.Player!.Position.X;

        float clock = harness.Game.Session.TimeLeftSeconds;

        harness.Press(InputAction.Pause);
        harness.Hold(InputAction.MoveRight);
        harness.Step(30);

        Assert.ApproximatelyEqual(moved, harness.Game.World.Player!.Position.X, 0.001f, "nothing moved");
        Assert.ApproximatelyEqual(clock, harness.Game.Session.TimeLeftSeconds, 0.0f, "and the clock stopped too");
    }

    [Test]
    public void TheCameraStartsOnThePlayerAndFollowsHim()
    {
        using var harness = new Harness(startsImmediately: true);
        float start = harness.Game.Camera.Position.X;
        harness.Hold(InputAction.MoveRight, InputAction.Run);
        harness.Step(240);

        Assert.IsTrue(harness.Game.World.Player!.Position.X > 200.0f, "he got a long way down the level");
        Assert.IsTrue(harness.Game.Camera.Position.X > start, "and the camera went with him");
    }

    [Test]
    public void RenderingChangesNothingAboutTheGame()
    {
        using var harness = new Harness(startsImmediately: true);
        harness.Hold(InputAction.MoveRight);
        harness.Step(20);
        float before = harness.Game.World.Player!.Position.X;

        for (int frame = 0; frame < 5; frame++)
        {
            harness.Game.Render(0.5f);
        }

        Assert.ApproximatelyEqual(before, harness.Game.World.Player!.Position.X, 0.0f, "draw reads, never writes");
        Assert.AreEqual(GameStateKind.Playing, harness.Game.State);
    }

    [Test]
    public void EveryStateDrawsSomethingWithoutFalling()
    {
        using var harness = new Harness(startsImmediately: true);
        harness.Game.Render(0.0f);
        harness.Press(InputAction.Pause);
        harness.Game.Render(0.0f);
        harness.Press(InputAction.Cancel);
        harness.Game.Render(0.0f);
        Assert.AreEqual(GameStateKind.Title, harness.Game.State, "title, playing and paused all drew");
    }

    [Test]
    public void FixedUpdateOnlyRunsWhileThereIsALevelToRun()
    {
        using var harness = new Harness();
        harness.Game.FixedUpdate(FixedDelta);
        Assert.AreEqual(GameStateKind.Title, harness.Game.State, "the title screen has no physics");
    }

    [Test]
    public void AGameWithoutSettingsIsRefused()
    {
        Assert.Throws<ArgumentNullException>(() => new MarioGame(null!));
    }

    // A game needs a renderer, a router, assets and somewhere for sound to go; this builds the
    // smallest set of those that is still the real game.
    private sealed class Harness : IDisposable
    {
        private readonly HeadlessRenderer _renderer = new(160, 96);
        private readonly InputRouter _router;
        private readonly HeldInput _input = new();

        public Harness(bool startsImmediately = false)
        {
            _router = new InputRouter(new InputState());
            _router.Add(_input);
            Game = new MarioGame(new MarioGameSettings
            {
                Renderer = _renderer,
                Router = _router,
                Assets = new EmbeddedAssetSource(typeof(MarioGame).Assembly),
                Audio = new MemoryAudioBackend(),
                StartsImmediately = startsImmediately,
            });
        }

        public MarioGame Game { get; }

        public void Hold(params InputAction[] actions) => _input.Hold(actions);

        // A press is an edge, so it has to come back up: holding Pause for ever pauses once.
        public void Press(InputAction action)
        {
            Hold(action);
            Step();
            _input.Release();
            Step();
        }

        public void Step(int steps = 1)
        {
            for (int step = 0; step < steps; step++)
            {
                Game.Update(FixedDelta);
                Game.FixedUpdate(FixedDelta);
            }
        }

        // The router disposes every source it was given, so the second call here is redundant -
        // but a field of a disposable type has to be disposed by the type that holds it, and
        // HeldInput.Dispose only clears a set, so saying it twice costs nothing.
        public void Dispose()
        {
            _router.Dispose();
            _input.Dispose();
            _renderer.Dispose();
        }
    }
}
