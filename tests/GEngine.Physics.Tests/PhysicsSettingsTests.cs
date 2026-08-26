using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="PhysicsSettings"/>.</summary>
public sealed class PhysicsSettingsTests
{
    [Test]
    public void Defaults_PullDownAndAllowAFewDepenetrationPasses()
    {
        var settings = new PhysicsSettings();
        Assert.ApproximatelyEqual(0.0f, settings.Gravity.X);
        Assert.IsTrue(settings.Gravity.Y > 0.0f, "on a screen-space axis, down is positive");
        Assert.IsTrue(settings.DepenetrationPasses >= 1);
        Assert.IsTrue(settings.RestingSpeedPixelsPerSecond > 0.0f);
    }

    [Test]
    public void Default_IsSharedAndMatchesAFreshInstance()
    {
        Assert.AreEqual(new PhysicsSettings().Gravity, PhysicsSettings.Default.Gravity);
        Assert.AreSame(PhysicsSettings.Default, PhysicsSettings.Default);
    }

    [Test]
    public void EverySettingCanBeOverriddenAtConstruction()
    {
        var settings = new PhysicsSettings { DepenetrationPasses = 9, TileFriction = 0.5f };
        Assert.AreEqual(9, settings.DepenetrationPasses);
        Assert.ApproximatelyEqual(0.5f, settings.TileFriction);
    }
}
