using GEngine.Core.Loop;
using GEngine.Core.Time;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="FramePacer"/> against a clock that never really waits.</summary>
public sealed class FramePacerTests
{
    private ManualClock _clock = new();

    [Setup]
    public void Setup() => _clock = new ManualClock();

    [Test]
    public void ATargetOfZero_DisablesPacing()
    {
        var pacer = new FramePacer(_clock, 0);
        pacer.WaitForNextFrame();
        Assert.IsFalse(pacer.IsPacing);
        Assert.AreEqual(0, _clock.SleepCount);
        Assert.ApproximatelyEqual(0.0f, (float)_clock.ElapsedSeconds);
    }

    [Test]
    public void WaitForNextFrame_WaitsUntilTheFrameIsDue()
    {
        var pacer = new FramePacer(_clock, 60);
        pacer.WaitForNextFrame();
        Assert.IsTrue(_clock.ElapsedSeconds >= 1.0 / 60.0);
    }

    [Test]
    public void WaitForNextFrame_SleepsOnceForTheBulk_ThenSpinsOutTheRest()
    {
        var pacer = new FramePacer(_clock, 60);
        pacer.WaitForNextFrame();
        Assert.IsTrue(_clock.SleepCount > 1, "one long sleep plus at least one spin");
    }

    [Test]
    public void WaitForNextFrame_WhenTheFrameAlreadyOverran_DoesNotWaitAtAll()
    {
        var pacer = new FramePacer(_clock, 60);
        _clock.AdvanceSeconds(1.0);
        pacer.WaitForNextFrame();
        Assert.AreEqual(0, _clock.SleepCount);
    }

    [Test]
    public void WaitForNextFrame_AfterOverrunning_DropsTheDebtInsteadOfRunningFramesEarly()
    {
        var pacer = new FramePacer(_clock, 60);
        _clock.AdvanceSeconds(1.0);
        pacer.WaitForNextFrame();
        double afterCatchUp = _clock.ElapsedSeconds;
        pacer.WaitForNextFrame();
        Assert.IsTrue(_clock.ElapsedSeconds - afterCatchUp >= 1.0 / 60.0);
    }

    [Test]
    public void Reset_AimsAtOneFrameFromNow()
    {
        var pacer = new FramePacer(_clock, 60);
        _clock.AdvanceSeconds(5.0);
        pacer.Reset();
        pacer.WaitForNextFrame();
        Assert.IsTrue(_clock.ElapsedSeconds >= 5.0 + (1.0 / 60.0));
    }
}
