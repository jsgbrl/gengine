// Holding a frame rate without burning a core. Thread.Sleep is coarse and differently
// coarse on each system - around a millisecond on Linux and macOS, up to fifteen on
// Windows with the default timer - so the pacer sleeps the bulk of the wait and spins the
// last few milliseconds, where sleeping would overshoot the frame.

using System;
using GEngine.Core.Time;

namespace GEngine.Core.Loop;

/// <summary>Waits until the next frame is due, using a hybrid of sleeping and spinning.</summary>
public sealed class FramePacer
{
    /// <summary>How much of the wait is spun rather than slept, in seconds.</summary>
    public const double SpinMarginSeconds = 0.004;

    private readonly IClock _clock;
    private readonly double _frameSeconds;
    private double _nextFrameSeconds;

    /// <summary>Creates a pacer.</summary>
    /// <param name="clock">The clock to wait against.</param>
    /// <param name="targetFramesPerSecond">Frames per second to aim for. Zero disables pacing.</param>
    public FramePacer(IClock clock, int targetFramesPerSecond)
    {
        ArgumentNullException.ThrowIfNull(clock);
        _clock = clock;
        _frameSeconds = targetFramesPerSecond > 0 ? 1.0 / targetFramesPerSecond : 0.0;
        _nextFrameSeconds = clock.ElapsedSeconds;
    }

    /// <summary>True when this pacer waits at all.</summary>
    public bool IsPacing => _frameSeconds > 0.0;

    /// <summary>Starts the cadence again from now, discarding any accumulated lateness.</summary>
    public void Reset() => _nextFrameSeconds = _clock.ElapsedSeconds;

    /// <summary>
    /// Blocks until the next frame is due. When the previous frame overran, the pacer does
    /// not try to make the time back by running the next frames early: it drops the debt
    /// and re-aims from now, because catching up is how a stutter turns into a stampede.
    /// </summary>
    public void WaitForNextFrame()
    {
        if (!IsPacing)
        {
            return;
        }

        _nextFrameSeconds += _frameSeconds;
        if (_clock.ElapsedSeconds >= _nextFrameSeconds)
        {
            Reset();
            return;
        }

        WaitOut();
    }

    private void WaitOut()
    {
        while (true)
        {
            double remaining = _nextFrameSeconds - _clock.ElapsedSeconds;
            if (remaining <= 0.0)
            {
                return;
            }

            _clock.Sleep(remaining > SpinMarginSeconds
                ? TimeSpan.FromSeconds(remaining - SpinMarginSeconds)
                : TimeSpan.Zero);
        }
    }
}
