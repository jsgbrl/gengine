using System;
using System.IO;
using System.Reflection;
using GEngine.Core.Contracts;
using GEngine.Input.Actions;
using GEngine.Input.Gamepad;
using GEngine.Input.Hid;
using GEngine.Input.Keyboard;
using GEngine.Input.Scripted;
using GEngine.Rendering;
using GEngine.Rendering.Assets;
using GEngine.Testing;
using MarioClone.Audio;
using MarioClone.Game;
using MarioClone.Tests.Doubles;

namespace MarioClone.Tests;

/// <summary>
/// The test the whole engine is arranged around. One recorded run of world 1-1 is played back
/// twice: once as key presses through the keyboard backend, once as sixty-four byte HID reports
/// through the DualSense decoder. Both have to reach the flag, and both have to reach it with
/// the same score - because if they do, then nothing between the device and the player knows
/// which device it was.
///
/// Nothing is drawn: the renderer is headless, the clock never ticks, and the run is a sequence
/// of fixed steps. That is what makes it reproducible, and reproducibility is what makes it a
/// test rather than a demo.
/// </summary>
public sealed class ReplayTests
{
    private const float FixedDelta = 1.0f / 60.0f;

    [Test]
    public void TheRecordedRunReachesTheFlagOnAKeyboard()
    {
        Outcome outcome = PlayOnKeyboard();
        Assert.IsTrue(outcome.ReachedGoal, "the keyboard run finished the level");
        Assert.AreEqual(GameStateKind.LevelComplete, outcome.State);
    }

    [Test]
    public void TheRecordedRunReachesTheFlagOnADualSense()
    {
        Outcome outcome = PlayOnGamepad();
        Assert.IsTrue(outcome.ReachedGoal, "the gamepad run finished the level");
        Assert.AreEqual(GameStateKind.LevelComplete, outcome.State);
    }

    [Test]
    public void BothDevicesProduceExactlyTheSameRun()
    {
        Outcome keyboard = PlayOnKeyboard();
        Outcome gamepad = PlayOnGamepad();
        Assert.AreEqual(keyboard.Score, gamepad.Score, "same score");
        Assert.AreEqual(keyboard.Coins, gamepad.Coins, "same coins");
        Assert.ApproximatelyEqual(keyboard.FinishedAtX, gamepad.FinishedAtX, 0.01f, "same finishing position");
    }

    [Test]
    public void PlayingTheSameRunTwiceGivesTheSameResult()
    {
        Outcome first = PlayOnKeyboard();
        Outcome second = PlayOnKeyboard();
        Assert.AreEqual(first.Frames, second.Frames);
        Assert.AreEqual(first.Score, second.Score);
        Assert.ApproximatelyEqual(first.FinishedAtX, second.FinishedAtX, 0.0f);
    }

    [Test]
    public void TheRecordedRunCollectsSomethingOnTheWay()
    {
        Outcome outcome = PlayOnKeyboard();
        Assert.IsTrue(outcome.Coins > 0, "it picked up coins");
        Assert.IsTrue(outcome.Score > GameSession.GoalScore, "and scored more than the flag alone");
    }

    private static Outcome PlayOnKeyboard()
    {
        InputScript script = LoadScript();
        var state = new InputState();
        using var router = new InputRouter(state);
        var map = InputMap.CreateDefault();
        var reader = new ScriptedKeyReader(script, map);

        // One frame of decay, so releasing a key takes effect on the very next frame. The real
        // game keeps two tenths of a second, because a terminal never reports a key coming up;
        // a replay knows exactly when the key came up, so it does not need the guess.
        router.Add(new ConsoleKeyboardBackend(reader, map) { DecaySeconds = FixedDelta });
        return Play(script, router, frame => reader.LoadFrame(frame));
    }

    private static Outcome PlayOnGamepad()
    {
        InputScript script = LoadScript();
        var state = new InputState();
        using var router = new InputRouter(state);
        FakeHidBackend backend = new FakeHidBackend().With(ReplayReports.Controller, ReplayReports.Device(script));
        var pad = new DualSenseGamepad(backend, GamepadMap.CreateDefault(), NullLogger.Instance)
        {
            Reading = GamepadReading.WhenPolled,
        };

        pad.Poll(2.0f);
        Assert.IsTrue(pad.IsConnected, "the controller was found before the run started");
        router.Add(pad);
        return Play(script, router, static _ => { });
    }

    private static Outcome Play(InputScript script, InputRouter router, Action<int> beforeFrame)
    {
        using var renderer = new HeadlessRenderer(160, 96);
        MarioGame game = Build(renderer, router);
        int frame = 0;
        for (; frame < script.FrameCount && game.State == GameStateKind.Playing; frame++)
        {
            beforeFrame(frame);
            game.Update(FixedDelta);
            game.FixedUpdate(FixedDelta);
        }

        return new Outcome(
            game.World.Player?.HasReachedGoal ?? false,
            game.State,
            frame,
            game.Session.Score,
            game.Session.Coins,
            game.World.Player?.Position.X ?? 0.0f);
    }

    private static MarioGame Build(HeadlessRenderer renderer, InputRouter router) =>
        new(new MarioGameSettings
        {
            Renderer = renderer,
            Router = router,
            Assets = new EmbeddedAssetSource(typeof(MarioGame).Assembly),
            Audio = new MemoryAudioBackend(),
            StartsImmediately = true,
        });

    private static InputScript LoadScript()
    {
        Assembly assembly = typeof(ReplayTests).Assembly;
        foreach (string resource in assembly.GetManifestResourceNames())
        {
            if (!resource.EndsWith("replay-1-1.txt", StringComparison.Ordinal))
            {
                continue;
            }

            using Stream stream = assembly.GetManifestResourceStream(resource)!;
            using var reader = new StreamReader(stream);
            return InputScript.Parse(reader.ReadToEnd());
        }

        throw new FileNotFoundException("the recorded run is missing from the test assembly");
    }

    private sealed record Outcome(
        bool ReachedGoal,
        GameStateKind State,
        int Frames,
        int Score,
        int Coins,
        float FinishedAtX);
}
