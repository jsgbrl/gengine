using GEngine.Core;
using GEngine.Physics.NarrowPhase;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="SweepResult"/>.</summary>
public sealed class SweepResultTests
{
    [Test]
    public void Miss_ReportsNoHitAndAFullMovement()
    {
        SweepResult miss = SweepResult.Miss;
        Assert.IsFalse(miss.IsHit);
        Assert.ApproximatelyEqual(1.0f, miss.Time);
        Assert.AreEqual(Vector2.Zero, miss.Normal);
        Assert.AreEqual("miss", miss.ToString());
    }

    [Test]
    public void Hit_KeepsItsTimeAndNormal()
    {
        var hit = SweepResult.Hit(0.25f, new Vector2(0.0f, -1.0f));
        Assert.IsTrue(hit.IsHit);
        Assert.ApproximatelyEqual(0.25f, hit.Time);
        Assert.AreEqual(new Vector2(0.0f, -1.0f), hit.Normal);
    }

    [Test]
    public void Equality_ComparesEveryField()
    {
        var hit = SweepResult.Hit(0.25f, Vector2.UnitX);
        Assert.IsTrue(hit == SweepResult.Hit(0.25f, Vector2.UnitX));
        Assert.IsTrue(hit != SweepResult.Hit(0.5f, Vector2.UnitX));
        Assert.IsTrue(hit.Equals((object)SweepResult.Hit(0.25f, Vector2.UnitX)));
        Assert.IsFalse(hit.Equals("not a result"));
        Assert.AreEqual(hit.GetHashCode(), SweepResult.Hit(0.25f, Vector2.UnitX).GetHashCode());
    }

    [Test]
    public void ToString_ShowsTheTimeAndTheFace()
    {
        Assert.AreEqual("hit at 0.5 along (0, -1)", SweepResult.Hit(0.5f, new Vector2(0.0f, -1.0f)).ToString());
    }
}
