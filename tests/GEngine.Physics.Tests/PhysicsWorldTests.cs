using System;
using GEngine.Core;
using GEngine.Physics.BroadPhase;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="PhysicsWorld"/>: bookkeeping, gravity and free fall.</summary>
public sealed class PhysicsWorldTests
{
    private const float FixedDelta = 1.0f / 60.0f;

    private PhysicsWorld _world = new(PhysicsSettings.Default, new SpatialHashGrid());

    [Setup]
    public void Setup() => _world = new PhysicsWorld(PhysicsSettings.Default, new SpatialHashGrid());

    [Test]
    public void ANewWorld_TakesItsGravityFromTheSettings()
    {
        Assert.AreEqual(PhysicsSettings.Default.Gravity, _world.Gravity);
        Assert.AreEqual(0, _world.Bodies.Count);
        Assert.AreEqual(0L, _world.StepCount);
    }

    [Test]
    public void Add_GivesEveryBodyItsOwnIdentity_CountingFromOne()
    {
        Assert.AreEqual(1, _world.Add(Falling()).Id);
        Assert.AreEqual(2, _world.Add(Falling()).Id);
        Assert.AreEqual(2, _world.Bodies.Count);
    }

    [Test]
    public void Remove_TakesTheBodyOutAndSaysWhetherItWasThere()
    {
        RigidBody2D body = _world.Add(Falling());
        Assert.IsTrue(_world.Remove(body));
        Assert.IsFalse(_world.Remove(body));
        Assert.AreEqual(0, _world.Bodies.Count);
    }

    [Test]
    public void AMissingBody_IsRefusedRatherThanIgnored()
    {
        Assert.Throws<ArgumentNullException>(() => _world.Add(null!));
        Assert.Throws<ArgumentNullException>(() => _world.Remove(null!));
    }

    [Test]
    public void Step_CountsSteps()
    {
        _world.Step(FixedDelta);
        _world.Step(FixedDelta);
        Assert.AreEqual(2L, _world.StepCount);
    }

    // Semi-implicit Euler adds gravity to the velocity and then moves with the new value, so
    // after n steps the body has fallen g dt^2 n(n+1)/2. That is the exact answer for this
    // integrator, and it sits half a step ahead of the textbook parabola.
    [Test]
    public void FreeFall_MatchesTheClosedFormOfTheIntegratorExactly()
    {
        RigidBody2D body = _world.Add(Falling());
        for (int step = 0; step < 60; step++)
        {
            _world.Step(FixedDelta);
        }

        float gravity = _world.Gravity.Y;
        float expected = gravity * FixedDelta * FixedDelta * (60.0f * 61.0f / 2.0f);
        Assert.ApproximatelyEqual(expected, body.Position.Y, 0.05f);
    }

    [Test]
    public void FreeFall_MatchesTheTextbookParabolaToWithinHalfAStep()
    {
        RigidBody2D body = _world.Add(Falling());
        for (int step = 0; step < 60; step++)
        {
            _world.Step(FixedDelta);
        }

        float continuous = 0.5f * _world.Gravity.Y * 1.0f * 1.0f;
        float halfStepAhead = 0.5f * _world.Gravity.Y * FixedDelta * 1.0f;
        Assert.ApproximatelyEqual(continuous + halfStepAhead, body.Position.Y, 0.05f);
    }

    [Test]
    public void FreeFall_ReachesTheVelocityGravityTimesTimePredicts()
    {
        RigidBody2D body = _world.Add(Falling());
        for (int step = 0; step < 60; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.ApproximatelyEqual(_world.Gravity.Y, body.Velocity.Y, 0.05f);
    }

    [Test]
    public void GravityScale_ScalesTheFall_AndZeroTurnsItOff()
    {
        // Well apart from each other: three bodies stacked on the same spot would push one
        // another aside, and this test is about gravity, not about contacts.
        RigidBody2D floating = _world.Add(FallingAt(0.0f));
        floating.GravityScale = 0.0f;
        RigidBody2D heavy = _world.Add(FallingAt(100.0f));
        heavy.GravityScale = 2.0f;
        RigidBody2D normal = _world.Add(FallingAt(200.0f));

        for (int step = 0; step < 30; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.ApproximatelyEqual(0.0f, floating.Position.Y);
        Assert.ApproximatelyEqual(2.0f * normal.Position.Y, heavy.Position.Y, 0.01f);
    }

    [Test]
    public void MaxVelocity_IsTerminalVelocity()
    {
        RigidBody2D body = _world.Add(Falling());
        body.MaxVelocity = new Vector2(1000.0f, 200.0f);
        for (int step = 0; step < 120; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.ApproximatelyEqual(200.0f, body.Velocity.Y);
    }

    [Test]
    public void Drag_BleedsOffVelocityWithoutReversingIt()
    {
        RigidBody2D body = _world.Add(Falling());
        body.GravityScale = 0.0f;
        body.Drag = 2.0f;
        body.Velocity = new Vector2(100.0f, 0.0f);
        for (int step = 0; step < 60; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.IsTrue(body.Velocity.X > 0.0f, "drag slows a body, it does not push it backwards");
        Assert.IsTrue(body.Velocity.X < 20.0f);
    }

    [Test]
    public void AStaticBody_IsNeverMovedByGravity()
    {
        RigidBody2D wall = _world.Add(new RigidBody2D(BodyType.Static, new Vector2(0.0f, 0.0f), Vector2.One));
        for (int step = 0; step < 60; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.AreEqual(Vector2.Zero, wall.Position);
    }

    private static RigidBody2D Falling() => FallingAt(0.0f);

    private static RigidBody2D FallingAt(float x) =>
        new(BodyType.Dynamic, new Vector2(x, 0.0f), new Vector2(10.0f, 10.0f));
}
