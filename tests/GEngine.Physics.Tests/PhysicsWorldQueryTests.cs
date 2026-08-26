using System.Collections.Generic;
using GEngine.Core;
using GEngine.Physics.BroadPhase;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers the queries a game asks between steps.</summary>
public sealed class PhysicsWorldQueryTests
{
    private PhysicsWorld _world = new(PhysicsSettings.Default, new SpatialHashGrid());

    [Setup]
    public void Setup() => _world = new PhysicsWorld(PhysicsSettings.Default, new SpatialHashGrid());

    [Test]
    public void Raycast_FindsTheNearestBodyAlongTheRay()
    {
        AddStatic(new Vector2(200.0f, 0.0f));
        RigidBody2D near = AddStatic(new Vector2(100.0f, 0.0f));

        Assert.IsTrue(_world.Raycast(Vector2.Zero, Vector2.UnitX, 500.0f, out RaycastHit hit));
        Assert.AreSame(near, hit.Body);
        Assert.ApproximatelyEqual(95.0f, hit.Distance, 0.01f);
        Assert.AreEqual(new Vector2(-1.0f, 0.0f), hit.Normal);
    }

    [Test]
    public void Raycast_ReportsWhereItTouched()
    {
        AddStatic(new Vector2(100.0f, 0.0f));
        _world.Raycast(Vector2.Zero, Vector2.UnitX, 500.0f, out RaycastHit hit);
        Assert.ApproximatelyEqual(95.0f, hit.Point.X, 0.01f);
        Assert.ApproximatelyEqual(0.0f, hit.Point.Y, 0.01f);
    }

    [Test]
    public void Raycast_StopsAtItsGivenDistance()
    {
        AddStatic(new Vector2(100.0f, 0.0f));
        Assert.IsFalse(_world.Raycast(Vector2.Zero, Vector2.UnitX, 50.0f, out RaycastHit hit));
        Assert.IsFalse(hit.IsHit);
    }

    [Test]
    public void Raycast_IntoEmptySpace_Misses()
    {
        Assert.IsFalse(_world.Raycast(Vector2.Zero, Vector2.UnitY, 500.0f, out _));
    }

    [Test]
    public void Raycast_NormalisesTheDirectionItWasGiven()
    {
        AddStatic(new Vector2(100.0f, 0.0f));
        _world.Raycast(Vector2.Zero, new Vector2(17.0f, 0.0f), 500.0f, out RaycastHit hit);
        Assert.ApproximatelyEqual(95.0f, hit.Distance, 0.01f);
    }

    [Test]
    public void OverlapBox_ReturnsEveryBodyInTheArea()
    {
        AddStatic(new Vector2(0.0f, 0.0f));
        AddStatic(new Vector2(20.0f, 0.0f));
        AddStatic(new Vector2(500.0f, 0.0f));

        List<RigidBody2D> found = [];
        _world.OverlapBox(new Aabb(new Vector2(-50.0f, -50.0f), new Vector2(50.0f, 50.0f)), ~0, found);
        Assert.AreEqual(2, found.Count);
    }

    [Test]
    public void OverlapBox_HonoursTheLayerMask()
    {
        RigidBody2D wanted = AddStatic(new Vector2(0.0f, 0.0f));
        RigidBody2D ignored = AddStatic(new Vector2(20.0f, 0.0f));
        wanted.Layer = 0b01;
        ignored.Layer = 0b10;

        List<RigidBody2D> found = [];
        _world.OverlapBox(new Aabb(new Vector2(-50.0f, -50.0f), new Vector2(50.0f, 50.0f)), 0b01, found);
        Assert.AreEqual(1, found.Count);
        Assert.AreSame(wanted, found[0]);
    }

    [Test]
    public void QueryPoint_FindsWhatIsUnderAPoint_AndNullInEmptySpace()
    {
        RigidBody2D body = AddStatic(new Vector2(0.0f, 0.0f));
        Assert.AreSame(body, _world.QueryPoint(new Vector2(3.0f, 3.0f), ~0));
        Assert.IsNull(_world.QueryPoint(new Vector2(300.0f, 300.0f), ~0));
    }

    [Test]
    public void QueryPoint_HonoursTheLayerMask()
    {
        RigidBody2D body = AddStatic(new Vector2(0.0f, 0.0f));
        body.Layer = 0b10;
        Assert.IsNull(_world.QueryPoint(Vector2.Zero, 0b01));
        Assert.AreSame(body, _world.QueryPoint(Vector2.Zero, 0b10));
    }

    [Test]
    public void Refresh_FilesBodiesMovedByHandWithoutSteppingTheSimulation()
    {
        RigidBody2D body = AddStatic(new Vector2(0.0f, 0.0f));
        _world.Refresh();
        body.Position = new Vector2(1000.0f, 0.0f);
        _world.Refresh();
        Assert.IsNull(_world.QueryPoint(Vector2.Zero, ~0));
        Assert.AreSame(body, _world.QueryPoint(new Vector2(1000.0f, 0.0f), ~0));
    }

    private RigidBody2D AddStatic(Vector2 position) =>
        _world.Add(new RigidBody2D(BodyType.Static, position, new Vector2(10.0f, 10.0f)));
}
