using System;
using GEngine.Core.Time;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>
/// Covers <see cref="StopwatchClock"/>. These tests assert the two properties that hold
/// regardless of how fast the machine is - it starts at zero or later, and it never goes
/// backwards - and never assert a duration, which is how a clock test becomes flaky.
/// </summary>
public sealed class StopwatchClockTests
{
    [Test]
    public void ElapsedSeconds_StartsAtOrAfterZero()
    {
        Assert.IsTrue(new StopwatchClock().ElapsedSeconds >= 0.0);
    }

    [Test]
    public void ElapsedSeconds_NeverGoesBackwards()
    {
        var clock = new StopwatchClock();
        double first = clock.ElapsedSeconds;
        double second = clock.ElapsedSeconds;
        Assert.IsTrue(second >= first);
    }

    [Test]
    public void Sleep_OfZero_Returns_AndDoesNotThrow()
    {
        var clock = new StopwatchClock();
        clock.Sleep(TimeSpan.Zero);
        Assert.IsTrue(clock.ElapsedSeconds >= 0.0);
    }

    [Test]
    public void Sleep_OfANegativeDuration_IsTreatedAsASpin()
    {
        var clock = new StopwatchClock();
        clock.Sleep(TimeSpan.FromSeconds(-1.0));
        Assert.IsTrue(clock.ElapsedSeconds >= 0.0);
    }
}
