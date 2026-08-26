using GEngine.Core;
using GEngine.Physics.BroadPhase;
using GEngine.Physics.Tiles;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>
/// The promise that makes a replay possible: the same world, stepped the same number of
/// times, ends in exactly the same state - not nearly, exactly. Ten thousand steps is about
/// three minutes of play, which is long enough for any drift to show.
/// </summary>
public sealed class PhysicsDeterminismTests
{
    private const float FixedDelta = 1.0f / 60.0f;
    private const int LongRun = 10000;

    [Test]
    public void TwoIdenticalWorlds_EndOnTheSameHash()
    {
        long first = RunAndHash(new SpatialHashGrid(), LongRun);
        long second = RunAndHash(new SpatialHashGrid(), LongRun);
        Assert.AreEqual(first, second);
    }

    [Test]
    public void ChangingTheBroadPhase_DoesNotChangeTheOutcome()
    {
        long withGrid = RunAndHash(new SpatialHashGrid(), 600);
        long withBruteForce = RunAndHash(new BruteForceBroadPhase(), 600);
        Assert.AreEqual(withGrid, withBruteForce, "an optimisation may change the cost, never the result");
    }

    [Test]
    public void OneExtraStep_DoesChangeTheHash_SoTheHashIsNotAConstant()
    {
        Assert.AreNotEqual(RunAndHash(new SpatialHashGrid(), 100), RunAndHash(new SpatialHashGrid(), 101));
    }

    [Test]
    public void NothingBecomesNaNOrInfiniteAfterTenThousandSteps()
    {
        PhysicsWorld world = Build(new SpatialHashGrid());
        for (int step = 0; step < LongRun; step++)
        {
            world.Step(FixedDelta);
        }

        foreach (RigidBody2D body in world.Bodies)
        {
            Assert.IsFinite(body.Position.X);
            Assert.IsFinite(body.Position.Y);
            Assert.IsFinite(body.Velocity.X);
            Assert.IsFinite(body.Velocity.Y);
        }
    }

    [Test]
    public void EverythingStaysInsideTheRoom()
    {
        PhysicsWorld world = Build(new SpatialHashGrid());
        for (int step = 0; step < LongRun; step++)
        {
            world.Step(FixedDelta);
        }

        foreach (RigidBody2D body in world.Bodies)
        {
            Assert.IsInRange(body.Position.X, -10.0f, 650.0f);
            Assert.IsInRange(body.Position.Y, -10.0f, 490.0f);
        }
    }

    private static long RunAndHash(IBroadPhase broadPhase, int steps)
    {
        PhysicsWorld world = Build(broadPhase);
        for (int step = 0; step < steps; step++)
        {
            world.Step(FixedDelta);
        }

        return world.StateHash();
    }

    // A closed room, a tiled floor, and a dozen bouncing balls thrown at it from fixed
    // positions with fixed velocities. No random numbers anywhere: rule 7 of the build
    // prompt, and the only reason this test can assert equality at all.
    private static PhysicsWorld Build(IBroadPhase broadPhase)
    {
        var world = new PhysicsWorld(PhysicsSettings.Default, broadPhase);
        AddWalls(world);
        world.Tiles = Floor();
        for (int index = 0; index < 12; index++)
        {
            RigidBody2D ball = world.Add(new RigidBody2D(
                BodyType.Dynamic,
                new Vector2(60.0f + (index * 40.0f), 50.0f + (index % 4 * 30.0f)),
                new Vector2(12.0f, 12.0f)));
            ball.Restitution = 0.6f;
            ball.Velocity = new Vector2(((index % 5) - 2) * 90.0f, (index % 3) * 70.0f);
        }

        return world;
    }

    private static void AddWalls(PhysicsWorld world)
    {
        world.Add(new RigidBody2D(BodyType.Static, new Vector2(-10.0f, 240.0f), new Vector2(20.0f, 520.0f)));
        world.Add(new RigidBody2D(BodyType.Static, new Vector2(650.0f, 240.0f), new Vector2(20.0f, 520.0f)));
        world.Add(new RigidBody2D(BodyType.Static, new Vector2(320.0f, -10.0f), new Vector2(680.0f, 20.0f)));
    }

    private static TileCollisionSource Floor()
    {
        var tiles = new TileCollisionSource(40, 30, 16.0f);
        for (int column = 0; column < 40; column++)
        {
            tiles.Set(column, 29, TileCollision.Solid);
        }

        return tiles;
    }
}
