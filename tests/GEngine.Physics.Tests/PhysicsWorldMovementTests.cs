using System;
using GEngine.Core;
using GEngine.Physics.BroadPhase;
using GEngine.Physics.Tiles;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>
/// Covers movement against the level: tunnelling, resting stability, bouncing, one-way
/// platforms and moving platforms. Every one of these is a bug a platformer ships with if
/// its solver is written the obvious way.
/// </summary>
public sealed class PhysicsWorldMovementTests
{
    private const float FixedDelta = 1.0f / 60.0f;

    private PhysicsWorld _world = new(PhysicsSettings.Default, new SpatialHashGrid());

    [Setup]
    public void Setup() => _world = new PhysicsWorld(PhysicsSettings.Default, new SpatialHashGrid());

    [Test]
    public void AtTenThousandPixelsASecond_ABodyDoesNotPassThroughAOnePixelWall()
    {
        AddWall(new Vector2(500.0f, 0.0f), new Vector2(1.0f, 400.0f));
        RigidBody2D bullet = AddDynamic(new Vector2(0.0f, 0.0f), new Vector2(4.0f, 4.0f));
        bullet.GravityScale = 0.0f;
        bullet.Velocity = new Vector2(10000.0f, 0.0f);

        for (int step = 0; step < 10; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.IsTrue(bullet.Position.X < 500.0f, "the bullet is still on the near side of the wall");
        Assert.ApproximatelyEqual(497.5f, bullet.Position.X, 0.01f);
    }

    [Test]
    public void ABodyAtRestOnTheFloor_DoesNotSinkOrShiverAfterTenThousandSteps()
    {
        AddWall(new Vector2(0.0f, 100.0f), new Vector2(1000.0f, 20.0f));
        RigidBody2D body = AddDynamic(new Vector2(0.0f, 80.0f), new Vector2(10.0f, 10.0f));

        // Let it land first: the interesting question is what happens to a body that is
        // already at rest, not how it got there.
        for (int step = 0; step < 30; step++)
        {
            _world.Step(FixedDelta);
        }

        float settled = body.Position.Y;
        for (int step = 0; step < 10000; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.ApproximatelyEqual(settled, body.Position.Y, 0.0f);
        Assert.ApproximatelyEqual(85.0f, body.Position.Y, 0.01f, "the box sits exactly on the floor");
        Assert.IsTrue(body.IsGrounded);
    }

    [Test]
    public void RestitutionOfZero_StopsTheBodyDead()
    {
        RigidBody2D ball = DropBall(restitution: 0.0f);
        for (int step = 0; step < 120; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.ApproximatelyEqual(0.0f, ball.Velocity.Y, 20.0f);
        Assert.ApproximatelyEqual(85.0f, ball.Position.Y, 0.5f);
    }

    [Test]
    public void RestitutionOfOne_SendsTheBodyBackUpAtTheSpeedItArrived()
    {
        RigidBody2D ball = DropBall(restitution: 1.0f);
        float fastestFall = 0.0f;
        for (int step = 0; step < 40; step++)
        {
            _world.Step(FixedDelta);
            fastestFall = MathF.Max(fastestFall, ball.Velocity.Y);
            if (ball.Velocity.Y < 0.0f)
            {
                break;
            }
        }

        Assert.IsTrue(ball.Velocity.Y < 0.0f, "the ball is on its way back up");
        Assert.ApproximatelyEqual(fastestFall, -ball.Velocity.Y, 20.0f);
    }

    [Test]
    public void AOneWayPlatform_CatchesABodyThatFallsOntoIt()
    {
        AddOneWay(new Vector2(0.0f, 100.0f));
        RigidBody2D body = AddDynamic(new Vector2(0.0f, 60.0f), new Vector2(10.0f, 10.0f));
        for (int step = 0; step < 60; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.IsTrue(body.IsGrounded);
        Assert.ApproximatelyEqual(85.0f, body.Position.Y, 0.5f);
    }

    [Test]
    public void AOneWayPlatform_LetsABodyJumpUpThroughIt()
    {
        AddOneWay(new Vector2(0.0f, 100.0f));
        RigidBody2D body = AddDynamic(new Vector2(0.0f, 140.0f), new Vector2(10.0f, 10.0f));
        body.Velocity = new Vector2(0.0f, -600.0f);
        for (int step = 0; step < 5; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.IsTrue(body.Position.Y < 95.0f, "the body went straight through from below");
    }

    [Test]
    public void AKinematicPlatform_CarriesWhateverIsStandingOnIt()
    {
        RigidBody2D platform = _world.Add(
            new RigidBody2D(BodyType.Kinematic, new Vector2(0.0f, 100.0f), new Vector2(200.0f, 20.0f)));
        platform.Velocity = new Vector2(60.0f, 0.0f);
        RigidBody2D passenger = AddDynamic(new Vector2(0.0f, 84.9f), new Vector2(10.0f, 10.0f));

        // One step to land, then measure how far both travel together.
        _world.Step(FixedDelta);
        float platformStart = platform.Position.X;
        float passengerStart = passenger.Position.X;
        for (int step = 0; step < 60; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.ApproximatelyEqual(60.0f, platform.Position.X - platformStart, 0.5f);
        Assert.ApproximatelyEqual(60.0f, passenger.Position.X - passengerStart, 0.5f);
    }

    [Test]
    public void ABodySpawnedInsideAWall_IsPushedOutInsteadOfBeingTrapped()
    {
        AddWall(new Vector2(0.0f, 100.0f), new Vector2(200.0f, 40.0f));
        RigidBody2D body = AddDynamic(new Vector2(0.0f, 96.0f), new Vector2(10.0f, 10.0f));

        _world.Step(FixedDelta);

        Assert.IsFalse(body.Bounds.Intersects(new Aabb(new Vector2(-100.0f, 80.0f), new Vector2(100.0f, 120.0f))));
        Assert.ApproximatelyEqual(75.0f, body.Position.Y, 0.5f);
    }

    [Test]
    public void ABodyPressedIntoAWall_StopsInsteadOfPushingTheLevel()
    {
        RigidBody2D wall = _world.Add(
            new RigidBody2D(BodyType.Static, new Vector2(100.0f, 0.0f), new Vector2(20.0f, 200.0f)));
        RigidBody2D body = AddDynamic(new Vector2(0.0f, 0.0f), new Vector2(10.0f, 10.0f));
        body.GravityScale = 0.0f;
        body.Velocity = new Vector2(300.0f, 0.0f);

        for (int step = 0; step < 60; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.ApproximatelyEqual(85.0f, body.Position.X, 0.01f);
        Assert.AreEqual(new Vector2(100.0f, 0.0f), wall.Position);
    }

    [Test]
    public void TilesStopABodyJustLikeStaticBodiesDo()
    {
        var tiles = new TileCollisionSource(20, 20, 16.0f);
        for (int column = 0; column < 20; column++)
        {
            tiles.Set(column, 10, TileCollision.Solid);
        }

        _world.Tiles = tiles;
        RigidBody2D body = AddDynamic(new Vector2(50.0f, 100.0f), new Vector2(10.0f, 10.0f));
        for (int step = 0; step < 60; step++)
        {
            _world.Step(FixedDelta);
        }

        Assert.IsTrue(body.IsGrounded);
        Assert.ApproximatelyEqual(155.0f, body.Position.Y, 0.01f);
        Assert.IsNull(body.Ground, "a tile has no body, so there is nothing to stand on");
    }

    private RigidBody2D DropBall(float restitution)
    {
        AddWall(new Vector2(0.0f, 100.0f), new Vector2(400.0f, 20.0f));
        RigidBody2D ball = AddDynamic(new Vector2(0.0f, 0.0f), new Vector2(10.0f, 10.0f));
        ball.Restitution = restitution;
        return ball;
    }

    private RigidBody2D AddDynamic(Vector2 position, Vector2 size) =>
        _world.Add(new RigidBody2D(BodyType.Dynamic, position, size));

    private void AddWall(Vector2 position, Vector2 size) =>
        _world.Add(new RigidBody2D(BodyType.Static, position, size));

    private void AddOneWay(Vector2 position)
    {
        RigidBody2D platform = _world.Add(
            new RigidBody2D(BodyType.Static, position, new Vector2(200.0f, 20.0f)));
        platform.IsOneWay = true;
    }
}
