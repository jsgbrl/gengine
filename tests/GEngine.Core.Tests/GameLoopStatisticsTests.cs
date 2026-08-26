using GEngine.Core.Loop;
using GEngine.Core.Tests.Doubles;
using GEngine.Core.Time;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>
/// Covers <see cref="GameLoopStatistics"/>. The counters are written by the loop and read
/// by everyone else, so the test drives a real loop rather than poking the fields: that is
/// the only way the type is ever used.
/// </summary>
public sealed class GameLoopStatisticsTests
{
    [Test]
    public void ANewInstance_StartsAtZero()
    {
        var statistics = new GameLoopStatistics();
        Assert.AreEqual(0L, statistics.FrameCount);
        Assert.AreEqual(0L, statistics.FixedStepCount);
        Assert.ApproximatelyEqual(0.0f, statistics.LastFrameSeconds);
        Assert.ApproximatelyEqual(0.0f, statistics.FramesPerSecond);
    }

    [Test]
    public void ARunningLoop_FillsEveryCounter()
    {
        GameLoopStatistics statistics = RunThirtyFrames();
        Assert.AreEqual(30L, statistics.FrameCount);
        Assert.AreEqual(30L, statistics.FixedStepCount);
        Assert.ApproximatelyEqual(1.0f / 60.0f, statistics.LastFrameSeconds);
        Assert.IsTrue(statistics.FramesPerSecond > 0.0f);
    }

    [Test]
    public void Reset_ClearsEveryCounter()
    {
        GameLoopStatistics statistics = RunThirtyFrames();
        statistics.Reset();
        Assert.AreEqual(0L, statistics.FrameCount);
        Assert.AreEqual(0L, statistics.FixedStepCount);
        Assert.ApproximatelyEqual(0.0f, statistics.LastFrameSeconds);
        Assert.ApproximatelyEqual(0.0f, statistics.FramesPerSecond);
    }

    private static GameLoopStatistics RunThirtyFrames()
    {
        var clock = new ManualClock();
        var loop = new GameLoop(clock, new RecordingGame(), new GameLoopSettings { TargetFramesPerSecond = 0 });
        for (int frame = 0; frame < 30; frame++)
        {
            clock.AdvanceSeconds(1.0 / 60.0);
            loop.Tick();
        }

        return loop.Statistics;
    }
}
