using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="Aabb"/>, with the touching-edge case first.</summary>
public sealed class AabbTests
{
    private static readonly Aabb Unit = new(new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f));

    [Test]
    public void Intersects_IsFalseForBoxesThatOnlyTouchAlongAnEdge()
    {
        var neighbour = new Aabb(new Vector2(1.0f, 0.0f), new Vector2(2.0f, 1.0f));
        Assert.IsFalse(Unit.Intersects(neighbour));
        Assert.IsTrue(Unit.Touches(neighbour));
    }

    [Test]
    public void Intersects_IsTrueForTheSmallestPossibleOverlap()
    {
        var overlapping = new Aabb(new Vector2(0.999f, 0.0f), new Vector2(2.0f, 1.0f));
        Assert.IsTrue(Unit.Intersects(overlapping));
    }

    [Test]
    public void Intersects_IsFalseForBoxesApartOnOnlyOneAxis()
    {
        var below = new Aabb(new Vector2(0.0f, 2.0f), new Vector2(1.0f, 3.0f));
        Assert.IsFalse(Unit.Intersects(below));
        Assert.IsFalse(Unit.Touches(below));
    }

    [Test]
    public void Contains_IncludesTheEdgeAndTheCorner()
    {
        Assert.IsTrue(Unit.Contains(new Vector2(0.0f, 0.0f)));
        Assert.IsTrue(Unit.Contains(new Vector2(1.0f, 1.0f)));
        Assert.IsTrue(Unit.Contains(new Vector2(0.5f, 0.5f)));
        Assert.IsFalse(Unit.Contains(new Vector2(1.001f, 0.5f)));
    }

    [Test]
    public void FromCenterSize_AndTheCornersAgree()
    {
        var box = Aabb.FromCenterSize(new Vector2(5.0f, 5.0f), new Vector2(2.0f, 4.0f));
        Assert.AreEqual(new Vector2(4.0f, 3.0f), box.Min);
        Assert.AreEqual(new Vector2(6.0f, 7.0f), box.Max);
        Assert.AreEqual(new Vector2(5.0f, 5.0f), box.Center);
        Assert.AreEqual(new Vector2(2.0f, 4.0f), box.Size);
        Assert.AreEqual(new Vector2(1.0f, 2.0f), box.HalfSize);
    }

    [Test]
    public void FromCorners_SortsTheCornersWhicheverWayRound()
    {
        var box = Aabb.FromCorners(new Vector2(3.0f, 4.0f), new Vector2(1.0f, 2.0f));
        Assert.AreEqual(new Vector2(1.0f, 2.0f), box.Min);
        Assert.AreEqual(new Vector2(3.0f, 4.0f), box.Max);
    }

    [Test]
    public void Edges_FollowTheScreenAxisWhereTopIsTheSmallerY()
    {
        Assert.ApproximatelyEqual(0.0f, Unit.Left);
        Assert.ApproximatelyEqual(1.0f, Unit.Right);
        Assert.ApproximatelyEqual(0.0f, Unit.Top);
        Assert.ApproximatelyEqual(1.0f, Unit.Bottom);
    }

    [Test]
    public void Translated_MovesBothCornersByTheSameOffset()
    {
        Aabb moved = Unit.Translated(new Vector2(2.0f, 3.0f));
        Assert.AreEqual(new Vector2(2.0f, 3.0f), moved.Min);
        Assert.AreEqual(new Vector2(3.0f, 4.0f), moved.Max);
    }

    [Test]
    public void Expanded_GrowsEverySide()
    {
        Aabb grown = Unit.Expanded(new Vector2(1.0f, 1.0f));
        Assert.AreEqual(new Vector2(-1.0f, -1.0f), grown.Min);
        Assert.AreEqual(new Vector2(2.0f, 2.0f), grown.Max);
    }

    [Test]
    public void Union_CoversBothBoxes()
    {
        var far = new Aabb(new Vector2(5.0f, 5.0f), new Vector2(6.0f, 6.0f));
        Aabb union = Unit.Union(far);
        Assert.AreEqual(new Vector2(0.0f, 0.0f), union.Min);
        Assert.AreEqual(new Vector2(6.0f, 6.0f), union.Max);
    }

    [Test]
    public void ClosestPoint_ReturnsThePointItselfWhenItIsInside()
    {
        var inside = new Vector2(0.25f, 0.75f);
        Assert.AreEqual(inside, Unit.ClosestPoint(inside));
        Assert.AreEqual(new Vector2(1.0f, 0.0f), Unit.ClosestPoint(new Vector2(4.0f, -4.0f)));
    }

    [Test]
    public void Equality_ComparesBothCorners()
    {
        var same = new Aabb(new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f));
        Assert.IsTrue(Unit == same);
        Assert.IsFalse(Unit != same);
        Assert.IsTrue(Unit.Equals((object)same));
        Assert.IsFalse(Unit.Equals("not a box"));
        Assert.AreEqual(Unit.GetHashCode(), same.GetHashCode());
    }

    [Test]
    public void ToString_ShowsBothCorners()
    {
        Assert.AreEqual("[(0, 0) .. (1, 1)]", Unit.ToString());
    }
}
