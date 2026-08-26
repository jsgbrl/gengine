using System;
using GEngine.Core.Time;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="ManualClock"/>.</summary>
public sealed class ManualClockTests
{
    private ManualClock _clock = new();

    [Setup]
    public void Setup() => _clock = new ManualClock();

    [Test]
    public void ANewClock_StartsAtZero()
    {
        Assert.ApproximatelyEqual(0.0f, (float)_clock.ElapsedSeconds);
    }

    [Test]
    public void Advance_MovesTheClockForward()
    {
        _clock.AdvanceSeconds(1.5);
        _clock.Advance(TimeSpan.FromSeconds(0.5));
        Assert.ApproximatelyEqual(2.0f, (float)_clock.ElapsedSeconds);
    }

    [Test]
    public void Advance_RefusesToGoBackwards()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => _clock.Advance(TimeSpan.FromSeconds(-1.0)));
    }

    [Test]
    public void Sleep_AdvancesByTheRequestedDuration_InsteadOfBlocking()
    {
        _clock.Sleep(TimeSpan.FromSeconds(0.25));
        Assert.ApproximatelyEqual(0.25f, (float)_clock.ElapsedSeconds);
        Assert.AreEqual(1, _clock.SleepCount);
    }

    [Test]
    public void Sleep_OfZero_AdvancesBySpinStep_SoASpinLoopTerminates()
    {
        _clock.SpinStep = TimeSpan.FromSeconds(0.01);
        _clock.Sleep(TimeSpan.Zero);
        Assert.ApproximatelyEqual(0.01f, (float)_clock.ElapsedSeconds);
    }
}
