using GEngine.Core.Loop;
using GEngine.Core.Tests.Doubles;
using GEngine.Core.Time;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="GameLoop"/>: the fixed step, the clamp and the interpolation.</summary>
public sealed class GameLoopTests
{
    private const double OneSixtieth = 1.0 / 60.0;

    private ManualClock _clock = new();
    private RecordingGame _game = new();

    [Setup]
    public void Setup()
    {
        _clock = new ManualClock();
        _game = new RecordingGame();
    }

    [Test]
    public void OneSecondOfFrames_ProducesExactlySixtyFixedSteps()
    {
        GameLoop loop = Loop();
        for (int frame = 0; frame < 60; frame++)
        {
            _clock.AdvanceSeconds(OneSixtieth);
            loop.Tick();
        }

        Assert.AreEqual(60L, loop.Statistics.FixedStepCount);
        Assert.AreEqual(60, _game.UpdateCount);
        Assert.AreEqual(60, _game.RenderCount);
    }

    [Test]
    public void EveryFixedStepGetsTheSameDelta_WhateverTheFrameTook()
    {
        GameLoop loop = Loop();
        _clock.AdvanceSeconds(0.037);
        loop.Tick();
        Assert.ApproximatelyEqual(1.0f / 60.0f, _game.LastFixedDeltaSeconds);
    }

    [Test]
    public void ALongFrame_IsClampedSoTheAccumulatorCannotSpiral()
    {
        GameLoop loop = Loop();
        _clock.AdvanceSeconds(10.0);
        loop.Tick();
        Assert.ApproximatelyEqual(0.25f, loop.Statistics.LastFrameSeconds);
        Assert.IsTrue(loop.Statistics.FixedStepCount <= 15, "a quarter second is at most fifteen sixtieths");
    }

    [Test]
    public void RaisingTheClamp_LetsTheSameLongFrameRunAllOfItsSteps()
    {
        GameLoop loop = Loop(new GameLoopSettings { MaximumFrameSeconds = 10.0f, TargetFramesPerSecond = 0 });
        _clock.AdvanceSeconds(10.0);
        loop.Tick();
        Assert.IsTrue(loop.Statistics.FixedStepCount > 500, "ten seconds is about six hundred sixtieths");
    }

    [Test]
    public void Render_ReceivesHowFarTheSimulationIsBetweenTwoFixedSteps()
    {
        GameLoop loop = Loop();
        _clock.AdvanceSeconds(OneSixtieth / 2.0);
        loop.Tick();
        Assert.AreEqual(0, _game.FixedUpdateCount);
        Assert.ApproximatelyEqual(0.5f, _game.LastInterpolation, 1e-3f);
    }

    [Test]
    public void OneFrame_CallsUpdateThenTheFixedStepsThenRender()
    {
        GameLoop loop = Loop();
        _clock.AdvanceSeconds(OneSixtieth);
        loop.Tick();
        Assert.MatchesSnapshot(string.Join("\n", _game.Calls), "update\nfixed\nrender");
    }

    [Test]
    public void Run_KeepsTickingUntilStopIsCalled()
    {
        GameLoop loop = Loop();
        _game.StopAfterFirstRender = loop;
        loop.Run();
        Assert.AreEqual(1L, loop.Statistics.FrameCount);
        Assert.IsFalse(loop.IsRunning);
    }

    [Test]
    public void Statistics_ReportNoFrameRateUntilTheFirstWindowCloses()
    {
        GameLoop loop = Loop();
        _clock.AdvanceSeconds(OneSixtieth);
        loop.Tick();
        Assert.ApproximatelyEqual(0.0f, loop.Statistics.FramesPerSecond);
    }

    [Test]
    public void Statistics_ReportTheTrueRateOfTheWindowThatJustClosed()
    {
        GameLoop loop = Loop();
        for (int frame = 0; frame < 30; frame++)
        {
            _clock.AdvanceSeconds(OneSixtieth);
            loop.Tick();
        }

        Assert.ApproximatelyEqual(60.0f, loop.Statistics.FramesPerSecond, 0.01f);
    }

    [Test]
    public void Statistics_ReportHalfTheRateWhenEveryFrameTakesTwiceAsLong()
    {
        GameLoop loop = Loop();
        for (int frame = 0; frame < 15; frame++)
        {
            _clock.AdvanceSeconds(OneSixtieth * 2.0);
            loop.Tick();
        }

        Assert.ApproximatelyEqual(30.0f, loop.Statistics.FramesPerSecond, 0.01f);
    }

    [Test]
    public void AWindowOfZeroLengthFrames_NeverDividesByZero()
    {
        GameLoop loop = Loop();
        loop.Tick();
        Assert.ApproximatelyEqual(0.0f, loop.Statistics.FramesPerSecond);
        Assert.AreEqual(1L, loop.Statistics.FrameCount);
    }

    private GameLoop Loop(GameLoopSettings? settings = null)
    {
        return new GameLoop(_clock, _game, settings ?? new GameLoopSettings { TargetFramesPerSecond = 0 });
    }
}
