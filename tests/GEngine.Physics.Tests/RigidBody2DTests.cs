using System;
using GEngine.Core;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="RigidBody2D"/>.</summary>
public sealed class RigidBody2DTests
{
    [Test]
    public void ANewBody_HasTheDefaultsAPlatformerWants()
    {
        RigidBody2D body = Dynamic();
        Assert.ApproximatelyEqual(1.0f, body.Mass);
        Assert.ApproximatelyEqual(1.0f, body.GravityScale);
        Assert.ApproximatelyEqual(0.0f, body.Restitution);
        Assert.AreEqual(1, body.Layer);
        Assert.AreEqual(~0, body.Mask);
        Assert.IsFalse(body.IsTrigger);
        Assert.IsFalse(body.IsGrounded);
    }

    [Test]
    public void Bounds_AreCentredOnThePosition()
    {
        RigidBody2D body = Dynamic();
        body.Position = new Vector2(100.0f, 50.0f);
        Assert.AreEqual(new Vector2(95.0f, 45.0f), body.Bounds.Min);
        Assert.AreEqual(new Vector2(105.0f, 55.0f), body.Bounds.Max);
        Assert.AreEqual(new Vector2(5.0f, 5.0f), body.HalfSize);
    }

    [Test]
    public void InverseMass_IsZeroForAnythingThatCannotBePushed()
    {
        Assert.ApproximatelyEqual(1.0f, Dynamic().InverseMass);
        Assert.ApproximatelyEqual(0.0f, new RigidBody2D(BodyType.Static, Vector2.Zero, Vector2.One).InverseMass);
        Assert.ApproximatelyEqual(0.0f, new RigidBody2D(BodyType.Kinematic, Vector2.Zero, Vector2.One).InverseMass);
    }

    [Test]
    public void Mass_IsClampedAwayFromZero_SoInverseMassIsNeverInfinite()
    {
        RigidBody2D body = Dynamic();
        body.Mass = 0.0f;
        Assert.IsFinite(body.InverseMass);
        body.Mass = -5.0f;
        Assert.IsFinite(body.InverseMass);
    }

    [Test]
    public void Mass_ChangesTheInverse()
    {
        RigidBody2D body = Dynamic();
        body.Mass = 4.0f;
        Assert.ApproximatelyEqual(0.25f, body.InverseMass);
    }

    [Test]
    public void AddImpulse_DividesByTheMass()
    {
        RigidBody2D body = Dynamic();
        body.Mass = 2.0f;
        body.AddImpulse(new Vector2(10.0f, 0.0f));
        Assert.ApproximatelyEqual(5.0f, body.Velocity.X);
    }

    [Test]
    public void AddImpulse_DoesNothingToAStaticBody()
    {
        var wall = new RigidBody2D(BodyType.Static, Vector2.Zero, Vector2.One);
        wall.AddImpulse(new Vector2(1000.0f, 0.0f));
        Assert.AreEqual(Vector2.Zero, wall.Velocity);
    }

    [Test]
    public void CollidesWith_NeedsBothMasksToAdmitTheOtherLayer()
    {
        RigidBody2D first = Dynamic();
        RigidBody2D second = Dynamic();
        first.Layer = 0b01;
        second.Layer = 0b10;

        first.Mask = 0b10;
        second.Mask = 0b01;
        Assert.IsTrue(first.CollidesWith(second));

        second.Mask = 0b10;
        Assert.IsFalse(first.CollidesWith(second), "collision is mutual or it is nothing");
    }

    [Test]
    public void CollidesWith_RefusesAMissingBody()
    {
        Assert.Throws<ArgumentNullException>(() => Dynamic().CollidesWith(null!));
    }

    private static RigidBody2D Dynamic() =>
        new(BodyType.Dynamic, Vector2.Zero, new Vector2(10.0f, 10.0f));
}
