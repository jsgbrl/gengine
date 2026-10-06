using System;
using GEngine.Core;
using GEngine.Physics.BroadPhase;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>
/// Covers movement against the level: tunnelling, resting stability, bouncing, one-way
/// platforms and moving platforms. Every one of these is a bug a platformer ships with if
/// its solver is written the obvious way.
/// </summary>
public sealed partial class PhysicsWorldMovementTests
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
