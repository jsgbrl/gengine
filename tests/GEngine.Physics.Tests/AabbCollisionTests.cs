using GEngine.Core;
using GEngine.Physics.NarrowPhase;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="AabbCollision"/>.</summary>
public sealed class AabbCollisionTests
{
    private static readonly Aabb Wall = new(new Vector2(0.0f, 0.0f), new Vector2(100.0f, 100.0f));

    [Test]
    public void TwoBoxesThatDoNotOverlap_HaveNoTranslation()
    {
        var far = Aabb.FromCenterSize(new Vector2(500.0f, 500.0f), new Vector2(10.0f, 10.0f));
        Assert.IsFalse(AabbCollision.TryGetMinimumTranslation(far, Wall, out Vector2 translation));
        Assert.AreEqual(Vector2.Zero, translation);
    }

    [Test]
    public void TwoBoxesThatOnlyTouch_HaveNoTranslation()
    {
        var touching = Aabb.FromCenterSize(new Vector2(105.0f, 50.0f), new Vector2(10.0f, 10.0f));
        Assert.IsFalse(AabbCollision.TryGetMinimumTranslation(touching, Wall, out _));
    }

    [Test]
    public void ABoxSunkIntoTheTop_IsPushedStraightUp()
    {
        var sunk = Aabb.FromCenterSize(new Vector2(50.0f, 2.0f), new Vector2(10.0f, 10.0f));
        Assert.IsTrue(AabbCollision.TryGetMinimumTranslation(sunk, Wall, out Vector2 translation));
        Assert.ApproximatelyEqual(0.0f, translation.X);
        Assert.ApproximatelyEqual(-7.0f, translation.Y);
    }

    [Test]
    public void ABoxSunkIntoTheLeftEdge_IsPushedStraightLeft()
    {
        var sunk = Aabb.FromCenterSize(new Vector2(2.0f, 50.0f), new Vector2(10.0f, 10.0f));
        Assert.IsTrue(AabbCollision.TryGetMinimumTranslation(sunk, Wall, out Vector2 translation));
        Assert.ApproximatelyEqual(-7.0f, translation.X);
        Assert.ApproximatelyEqual(0.0f, translation.Y);
    }

    [Test]
    public void TheShorterAxisWins_WhichIsWhyASunkPlayerIsLiftedAndNotThrownSideways()
    {
        var sunk = Aabb.FromCenterSize(new Vector2(50.0f, 104.0f), new Vector2(20.0f, 20.0f));
        Assert.IsTrue(AabbCollision.TryGetMinimumTranslation(sunk, Wall, out Vector2 translation));
        Assert.ApproximatelyEqual(0.0f, translation.X);
        Assert.ApproximatelyEqual(6.0f, translation.Y);
    }

    [Test]
    public void ApplyingTheTranslation_LeavesTheBoxesTouchingAndNotOverlapping()
    {
        var sunk = Aabb.FromCenterSize(new Vector2(50.0f, 2.0f), new Vector2(10.0f, 10.0f));
        AabbCollision.TryGetMinimumTranslation(sunk, Wall, out Vector2 translation);
        Aabb separated = sunk.Translated(translation);
        Assert.IsFalse(separated.Intersects(Wall));
        Assert.IsTrue(separated.Touches(Wall));
    }

    [Test]
    public void OverlapDepth_ReportsBothAxesAndNeverGoesNegative()
    {
        var overlapping = Aabb.FromCenterSize(new Vector2(98.0f, 96.0f), new Vector2(10.0f, 10.0f));
        Vector2 depth = AabbCollision.OverlapDepth(overlapping, Wall);
        Assert.ApproximatelyEqual(7.0f, depth.X);
        Assert.ApproximatelyEqual(9.0f, depth.Y);

        var apart = Aabb.FromCenterSize(new Vector2(500.0f, 500.0f), new Vector2(10.0f, 10.0f));
        Assert.AreEqual(Vector2.Zero, AabbCollision.OverlapDepth(apart, Wall));
    }
}
