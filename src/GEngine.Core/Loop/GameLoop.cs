// The heart of the engine. The shape below is the one Glenn Fiedler describes in
// "Fix Your Timestep!": measure the real frame, clamp it, add it to an accumulator, run
// as many fixed steps as fit, and hand the leftover to the renderer as an interpolation
// factor. Everything else in gengine is arranged around these twenty lines.

using System;
using GEngine.Core.Time;

namespace GEngine.Core.Loop;

/// <summary>
/// Drives an <see cref="IGame"/> with a variable update, a fixed simulation step and an
/// interpolated render. Feed it a <see cref="ManualClock"/> and every frame it produces
/// is reproducible; feed it a <see cref="StopwatchClock"/> and it is the game.
/// </summary>
public sealed class GameLoop
{
    private readonly IClock _clock;
    private readonly IGame _game;
    private readonly GameLoopSettings _settings;
    private readonly FramePacer _pacer;
    private double _previousSeconds;
    private float _accumulatorSeconds;
    private float _windowSeconds;
    private int _windowFrames;

    /// <summary>Creates a loop.</summary>
    /// <param name="clock">Where time comes from.</param>
    /// <param name="game">What to drive.</param>
    /// <param name="settings">How to tune it.</param>
    public GameLoop(IClock clock, IGame game, GameLoopSettings settings)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ArgumentNullException.ThrowIfNull(game);
        ArgumentNullException.ThrowIfNull(settings);
        _clock = clock;
        _game = game;
        _settings = settings;
        _pacer = new FramePacer(clock, settings.TargetFramesPerSecond);
        _previousSeconds = clock.ElapsedSeconds;
    }

    /// <summary>Counters and timings of the run so far.</summary>
    public GameLoopStatistics Statistics { get; } = new();

    /// <summary>True between <see cref="Run"/> and <see cref="Stop"/>.</summary>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Runs frames until <see cref="Stop"/> is called. A game that needs to own its own
    /// outer loop can call <see cref="Tick"/> instead; this method is only the while.
    /// </summary>
    public void Run()
    {
        IsRunning = true;
        _previousSeconds = _clock.ElapsedSeconds;
        _pacer.Reset();
        while (IsRunning)
        {
            Tick();
        }
    }

    /// <summary>Asks the loop to return after the current frame.</summary>
    public void Stop() => IsRunning = false;

    /// <summary>
    /// Runs exactly one frame: measure, record, update, step, render, pace. The statistics
    /// are recorded before the callbacks and not after, so a heads-up display drawn in
    /// <see cref="IGame.Render"/> shows the frame it is part of rather than the last one.
    /// </summary>
    public void Tick()
    {
        float frameSeconds = MeasureFrame();
        _accumulatorSeconds += frameSeconds;
        Record(frameSeconds);

        _game.Update(frameSeconds);
        StepFixed();
        _game.Render(_accumulatorSeconds / _settings.FixedDeltaSeconds);

        _pacer.WaitForNextFrame();
    }

    /// <summary>
    /// How long the last frame really took, clamped to
    /// <see cref="GameLoopSettings.MaximumFrameSeconds"/>. The clamp is the guard against
    /// the spiral of death: without it a frame that took two seconds asks for a hundred
    /// and twenty fixed steps, those steps take longer than two seconds to run, and the
    /// next frame asks for even more.
    /// </summary>
    /// <returns>The clamped frame time, in seconds.</returns>
    private float MeasureFrame()
    {
        double now = _clock.ElapsedSeconds;
        float elapsed = (float)(now - _previousSeconds);
        _previousSeconds = now;
        return MathG.Clamp(elapsed, 0.0f, _settings.MaximumFrameSeconds);
    }

    private void StepFixed()
    {
        float fixedDelta = _settings.FixedDeltaSeconds;
        while (_accumulatorSeconds >= fixedDelta)
        {
            _game.FixedUpdate(fixedDelta);
            _accumulatorSeconds -= fixedDelta;
            Statistics.FixedStepCount++;
        }
    }

    // Frames counted in a window, divided by how long that window really lasted. The
    // reading only changes when a window closes, which is also what makes it readable on a
    // HUD: a number that changes sixty times a second cannot be read at all.
    private void Record(float frameSeconds)
    {
        Statistics.FrameCount++;
        Statistics.LastFrameSeconds = frameSeconds;
        _windowSeconds += frameSeconds;
        _windowFrames++;
        if (_windowSeconds < _settings.FrameRateWindowSeconds || _windowSeconds <= 0.0f)
        {
            return;
        }

        Statistics.FramesPerSecond = _windowFrames / _windowSeconds;
        _windowSeconds = 0.0f;
        _windowFrames = 0;
    }
}
