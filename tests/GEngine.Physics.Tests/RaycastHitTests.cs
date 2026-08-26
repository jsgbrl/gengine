using GEngine.Core;
using GEngine.Physics.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="RaycastHit"/>.</summary>
public sealed class RaycastHitTests
{
    [Test]
    public void Miss_HasNoBody()
    {
        RaycastHit miss = RaycastHit.Miss;
        Assert.IsFalse(miss.IsHit);
        Assert.IsNull(miss.Body);
        Assert.AreEqual("miss", miss.ToString());
    }

    [Test]
    public void AHitKeepsEverythingItWasGiven()
    {
        var bodies = new BodySet();
        RigidBody2D body = bodies.AddBoxAt(10.0f, 0.0f);
        var hit = new RaycastHit(body, new Vector2(5.0f, 0.0f), new Vector2(-1.0f, 0.0f), 5.0f);
        Assert.IsTrue(hit.IsHit);
        Assert.AreSame(body, hit.Body);
        Assert.AreEqual(new Vector2(5.0f, 0.0f), hit.Point);
        Assert.ApproximatelyEqual(5.0f, hit.Distance);
        Assert.AreEqual("hit 1 at (5, 0)", hit.ToString());
    }

    [Test]
    public void Equality_ComparesEveryField()
    {
        var bodies = new BodySet();
        RigidBody2D body = bodies.AddBoxAt(10.0f, 0.0f);
        var hit = new RaycastHit(body, Vector2.Zero, Vector2.UnitX, 1.0f);
        Assert.IsTrue(hit == new RaycastHit(body, Vector2.Zero, Vector2.UnitX, 1.0f));
        Assert.IsTrue(hit != RaycastHit.Miss);
        Assert.IsTrue(hit.Equals((object)new RaycastHit(body, Vector2.Zero, Vector2.UnitX, 1.0f)));
        Assert.IsFalse(hit.Equals("not a hit"));
        Assert.AreEqual(hit.GetHashCode(), new RaycastHit(body, Vector2.Zero, Vector2.UnitX, 1.0f).GetHashCode());
    }
}
