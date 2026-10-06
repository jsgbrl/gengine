// The half of the movement tests about surfaces that are not simply solid: a platform you can
// jump up through, a platform that carries you, a wall you started inside, and a tilemap.

using GEngine.Core;
using GEngine.Physics.Tiles;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <content>One-way platforms, moving platforms, depenetration and tiles.</content>
public sealed partial class PhysicsWorldMovementTests
{
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
}
