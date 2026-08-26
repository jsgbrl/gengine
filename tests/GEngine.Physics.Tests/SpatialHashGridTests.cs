using System;
using System.Collections.Generic;
using GEngine.Core;
using GEngine.Physics.BroadPhase;
using GEngine.Physics.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="SpatialHashGrid"/>.</summary>
public sealed class SpatialHashGridTests
{
    private BodySet _bodies = new();
    private SpatialHashGrid _grid = new(32.0f);
    private List<BroadPhasePair> _pairs = [];

    [Setup]
    public void Setup()
    {
        _bodies = new BodySet();
        _grid = new SpatialHashGrid(32.0f);
        _pairs = [];
    }

    [Test]
    public void ItIsNamedSoTheExamplesCanSayWhichOneRan()
    {
        Assert.AreEqual("spatial hash", _grid.Name);
    }

    [Test]
    public void ACellSizeOfZeroOrLess_IsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () => new SpatialHashGrid(0.0f));
        Assert.Throws<ArgumentOutOfRangeException>(static () => new SpatialHashGrid(-1.0f));
    }

    [Test]
    public void BodiesFarApart_NeverGetCompared()
    {
        _bodies.AddBoxAt(0.0f, 0.0f);
        _bodies.AddBoxAt(5000.0f, 5000.0f);
        Update();
        Assert.AreEqual(0, _grid.LastComparisonCount);
        Assert.AreEqual(0, _pairs.Count);
    }

    [Test]
    public void ABoxStraddlingFourCells_IsFiledInAllOfThem()
    {
        _bodies.Add(BodyType.Dynamic, new Vector2(32.0f, 32.0f), new Vector2(20.0f, 20.0f));
        _grid.Update(_bodies.All);
        Assert.AreEqual(4, _grid.OccupiedCellCount);
    }

    [Test]
    public void APairFoundInTwoCellsAtOnce_IsReportedOnlyOnce()
    {
        _bodies.Add(BodyType.Dynamic, new Vector2(32.0f, 32.0f), new Vector2(40.0f, 40.0f));
        _bodies.Add(BodyType.Dynamic, new Vector2(36.0f, 36.0f), new Vector2(40.0f, 40.0f));
        Update();
        Assert.AreEqual(1, _pairs.Count);
    }

    [Test]
    public void Query_ReturnsEachBodyOnceAndInIdentityOrder()
    {
        _bodies.Add(BodyType.Dynamic, new Vector2(32.0f, 32.0f), new Vector2(40.0f, 40.0f));
        _bodies.Add(BodyType.Dynamic, new Vector2(40.0f, 40.0f), new Vector2(40.0f, 40.0f));
        _grid.Update(_bodies.All);
        List<RigidBody2D> found = [];
        _grid.Query(new Aabb(new Vector2(0.0f, 0.0f), new Vector2(80.0f, 80.0f)), found);
        Assert.AreEqual(2, found.Count);
        Assert.AreEqual(1, found[0].Id);
        Assert.AreEqual(2, found[1].Id);
    }

    [Test]
    public void UpdatingTwice_DoesNotLeaveTheOldPositionsBehind()
    {
        RigidBody2D body = _bodies.AddBoxAt(0.0f, 0.0f);
        _grid.Update(_bodies.All);
        body.Position = new Vector2(1000.0f, 1000.0f);
        _grid.Update(_bodies.All);
        List<RigidBody2D> found = [];
        _grid.Query(Aabb.FromCenterSize(Vector2.Zero, new Vector2(20.0f, 20.0f)), found);
        Assert.AreEqual(0, found.Count);
    }

    private void Update()
    {
        _grid.Update(_bodies.All);
        _grid.FindPairs(_pairs);
    }
}
