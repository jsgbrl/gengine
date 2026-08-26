// The clock tests use. Nothing here waits for anything: time moves only when a test says
// so, which is what makes a physics run reproducible to the last bit.

using System;

namespace GEngine.Core.Time;

/// <summary>
/// A clock driven by hand. <see cref="Sleep"/> advances instead of blocking, so code that
/// paces itself against a real clock still terminates - and the test can assert how often
/// it tried to sleep.
/// </summary>
public sealed class ManualClock : IClock
{
    /// <summary>How far a zero-length sleep - a spin - moves this clock.</summary>
    public static readonly TimeSpan DefaultSpinStep = TimeSpan.FromMilliseconds(0.1);

    /// <inheritdoc/>
    public double ElapsedSeconds { get; private set; }

    /// <summary>How far a spin advances the clock. Only a test that measures spinning needs to change it.</summary>
    public TimeSpan SpinStep { get; set; } = DefaultSpinStep;

    /// <summary>How many times something asked this clock to wait.</summary>
    public int SleepCount { get; private set; }

    /// <summary>Moves the clock forward.</summary>
    /// <param name="duration">How far forward. Must not be negative: the clock is monotonic.</param>
    /// <exception cref="ArgumentOutOfRangeException">The duration is negative.</exception>
    public void Advance(TimeSpan duration) => AdvanceSeconds(duration.TotalSeconds);

    /// <summary>
    /// Moves the clock forward by a number of seconds. This is the primitive and
    /// <see cref="Advance"/> delegates to it, not the other way round: TimeSpan stores
    /// hundred-nanosecond ticks, and rounding a sixtieth of a second into ticks makes a
    /// frame come out a hair short - which is enough to lose one fixed step in sixty.
    /// </summary>
    /// <param name="seconds">How far forward, in seconds.</param>
    /// <exception cref="ArgumentOutOfRangeException">The duration is negative.</exception>
    public void AdvanceSeconds(double seconds)
    {
        if (seconds < 0.0)
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), "a monotonic clock cannot go backwards");
        }

        ElapsedSeconds += seconds;
    }

    /// <inheritdoc/>
    public void Sleep(TimeSpan duration)
    {
        SleepCount++;
        Advance(duration > TimeSpan.Zero ? duration : SpinStep);
    }
}
