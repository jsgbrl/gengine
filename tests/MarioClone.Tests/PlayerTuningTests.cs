using GEngine.Testing;
using MarioClone.Actors;

namespace MarioClone.Tests;

/// <summary>
/// Every number that decides how the game feels, in one object. These tests do not check the
/// values - a value is a taste, and changing it is allowed - they check the relationships
/// between them, which are not tastes. A run slower than a walk is a bug in any game.
/// </summary>
public sealed class PlayerTuningTests
{
    [Test]
    public void RunningIsFasterThanWalking()
    {
        PlayerTuning tuning = PlayerTuning.Default;
        Assert.IsTrue(tuning.RunSpeedPixelsPerSecond > tuning.WalkSpeedPixelsPerSecond);
    }

    // Skidding is a turn, not a stop, and it has to bite harder than ordinary friction or
    // turning around at a run feels like walking on ice.
    [Test]
    public void SkiddingBitesHarderThanFriction()
    {
        PlayerTuning tuning = PlayerTuning.Default;
        Assert.IsTrue(tuning.SkidPixelsPerSecondSquared > tuning.FrictionPixelsPerSecondSquared);
    }

    [Test]
    public void TheAirIsHarderToSteerInThanTheGround()
    {
        Assert.IsInRange(PlayerTuning.Default.AirControl, 0.0f, 1.0f);
    }

    [Test]
    public void AJumpGoesUpAndGravityComesDown()
    {
        PlayerTuning tuning = PlayerTuning.Default;
        Assert.IsTrue(tuning.JumpVelocityPixelsPerSecond < 0.0f, "the screen's Y axis points down");
        Assert.IsTrue(tuning.GravityPixelsPerSecondSquared > 0.0f);
        Assert.IsTrue(tuning.StompBouncePixelsPerSecond < 0.0f, "a stomp bounces up");
        Assert.IsTrue(
            tuning.StompBouncePixelsPerSecond > tuning.JumpVelocityPixelsPerSecond,
            "and bounces less than a jump, or stomping would be the best way to travel");
    }

    [Test]
    public void CuttingAJumpShortensItWithoutStoppingIt()
    {
        Assert.IsInRange(PlayerTuning.Default.JumpCutFactor, 0.0f, 1.0f);
    }

    // Both mercies are measured in tenths of a second. Long enough to catch a human's timing,
    // short enough that nobody can point at the moment it happened.
    [Test]
    public void TheTwoMerciesAreShortButRealSpansOfTime()
    {
        Assert.IsInRange(PlayerTuning.Default.CoyoteTimeSeconds, 0.02f, 0.25f);
        Assert.IsInRange(PlayerTuning.Default.JumpBufferSeconds, 0.02f, 0.25f);
    }

    [Test]
    public void FallingHasALimitAndItIsSlowerThanATileIsTall()
    {
        PlayerTuning tuning = PlayerTuning.Default;
        Assert.IsTrue(tuning.MaximumFallSpeedPixelsPerSecond > 0.0f);
        Assert.IsTrue(
            tuning.MaximumFallSpeedPixelsPerSecond / 60.0f < 8.0f,
            "one fixed step must never cross a whole tile, or a floor can be missed");
    }

    [Test]
    public void EveryNumberIsARealNumber()
    {
        PlayerTuning tuning = PlayerTuning.Default;
        Assert.IsFinite(tuning.AccelerationPixelsPerSecondSquared);
        Assert.IsFinite(tuning.InvulnerabilitySeconds);
        Assert.IsFinite(tuning.PitDepthPixels);
    }

    [Test]
    public void ATuningCanBeChangedWithoutChangingTheDefault()
    {
        var faster = new PlayerTuning { WalkSpeedPixelsPerSecond = 999.0f };
        Assert.ApproximatelyEqual(999.0f, faster.WalkSpeedPixelsPerSecond, 0.001f);
        Assert.AreNotEqual(999.0f, PlayerTuning.Default.WalkSpeedPixelsPerSecond, "the default is shared");
    }
}
