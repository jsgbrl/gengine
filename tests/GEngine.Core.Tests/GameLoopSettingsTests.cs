using GEngine.Core.Loop;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="GameLoopSettings"/>.</summary>
public sealed class GameLoopSettingsTests
{
    [Test]
    public void Defaults_AreSixtyStepsSixtyFramesAQuarterSecondClampAndAHalfSecondWindow()
    {
        var settings = new GameLoopSettings();
        Assert.ApproximatelyEqual(1.0f / 60.0f, settings.FixedDeltaSeconds);
        Assert.ApproximatelyEqual(0.25f, settings.MaximumFrameSeconds);
        Assert.AreEqual(60, settings.TargetFramesPerSecond);
        Assert.ApproximatelyEqual(0.5f, settings.FrameRateWindowSeconds);
    }

    [Test]
    public void Default_IsSharedAndMatchesAFreshInstance()
    {
        Assert.ApproximatelyEqual(new GameLoopSettings().FixedDeltaSeconds, GameLoopSettings.Default.FixedDeltaSeconds);
        Assert.AreSame(GameLoopSettings.Default, GameLoopSettings.Default);
    }

    [Test]
    public void EverySettingCanBeOverriddenAtConstruction()
    {
        var settings = new GameLoopSettings { FixedDeltaSeconds = 0.02f, TargetFramesPerSecond = 144 };
        Assert.ApproximatelyEqual(0.02f, settings.FixedDeltaSeconds);
        Assert.AreEqual(144, settings.TargetFramesPerSecond);
    }
}
