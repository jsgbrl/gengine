using GEngine.Core;
using GEngine.Physics.NarrowPhase;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="SweptAabb"/>, the continuous collision test.</summary>
public sealed class SweptAabbTests
{
    private static readonly Aabb Wall = new(new Vector2(100.0f, 0.0f), new Vector2(101.0f, 100.0f));

    [Test]
    public void AMovementThatStopsShortOfTheWall_Misses()
    {
        SweepResult result = SweptAabb.Sweep(Box(0.0f, 50.0f), new Vector2(50.0f, 0.0f), Wall);
        Assert.IsFalse(result.IsHit);
    }

    [Test]
    public void AMovementThroughAOnePixelWall_StillHitsIt()
    {
        SweepResult result = SweptAabb.Sweep(Box(0.0f, 50.0f), new Vector2(10000.0f, 0.0f), Wall);
        Assert.IsTrue(result.IsHit, "a wall one pixel thick is not a hole");
        Assert.AreEqual(new Vector2(-1.0f, 0.0f), result.Normal);
    }

    [Test]
    public void TheHitTime_IsTheFractionOfTheMovementBeforeTheTouch()
    {
        SweepResult result = SweptAabb.Sweep(Box(0.0f, 50.0f), new Vector2(200.0f, 0.0f), Wall);
        Assert.ApproximatelyEqual(0.475f, result.Time, 1e-3f);
    }

    [Test]
    public void AMovementPastTheWall_OnAnotherRow_Misses()
    {
        SweepResult result = SweptAabb.Sweep(Box(0.0f, 500.0f), new Vector2(10000.0f, 0.0f), Wall);
        Assert.IsFalse(result.IsHit);
    }

    [Test]
    public void FallingOntoAFloor_GivesANormalPointingUp()
    {
        var floor = new Aabb(new Vector2(0.0f, 100.0f), new Vector2(200.0f, 120.0f));
        SweepResult result = SweptAabb.Sweep(Box(50.0f, 0.0f), new Vector2(0.0f, 500.0f), floor);
        Assert.IsTrue(result.IsHit);
        Assert.AreEqual(new Vector2(0.0f, -1.0f), result.Normal);
    }

    [Test]
    public void RisingIntoACeiling_GivesANormalPointingDown()
    {
        var ceiling = new Aabb(new Vector2(0.0f, -50.0f), new Vector2(200.0f, -40.0f));
        SweepResult result = SweptAabb.Sweep(Box(50.0f, 0.0f), new Vector2(0.0f, -500.0f), ceiling);
        Assert.AreEqual(new Vector2(0.0f, 1.0f), result.Normal);
    }

    [Test]
    public void ABoxAlreadyRestingOnASurface_HitsItAtTimeZero()
    {
        var floor = new Aabb(new Vector2(0.0f, 100.0f), new Vector2(200.0f, 120.0f));
        SweepResult result = SweptAabb.Sweep(Box(50.0f, 95.0f), new Vector2(0.0f, 1.0f), floor);
        Assert.IsTrue(result.IsHit);
        Assert.ApproximatelyEqual(0.0f, result.Time);
    }

    [Test]
    public void ABoxSlidingAlongASurfaceItTouches_DoesNotHitIt()
    {
        var floor = new Aabb(new Vector2(0.0f, 100.0f), new Vector2(200.0f, 120.0f));
        SweepResult result = SweptAabb.Sweep(Box(50.0f, 95.0f), new Vector2(10.0f, 0.0f), floor);
        Assert.IsFalse(result.IsHit, "touching is not overlapping");
    }

    [Test]
    public void Cast_TreatsTheRayAsAZeroSizedBox()
    {
        SweepResult result = SweptAabb.Cast(new Vector2(0.0f, 50.0f), new Vector2(200.0f, 0.0f), Wall);
        Assert.IsTrue(result.IsHit);
        Assert.ApproximatelyEqual(0.5f, result.Time, 1e-3f);
    }

    [Test]
    public void AZeroLengthMovement_Misses()
    {
        Assert.IsFalse(SweptAabb.Sweep(Box(0.0f, 50.0f), Vector2.Zero, Wall).IsHit);
    }

    // A ten by ten box centred at the given point.
    private static Aabb Box(float x, float y) => Aabb.FromCenterSize(new Vector2(x, y), new Vector2(10.0f, 10.0f));
}
