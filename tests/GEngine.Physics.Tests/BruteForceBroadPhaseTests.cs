using System.Collections.Generic;
using GEngine.Core;
using GEngine.Physics.BroadPhase;
using GEngine.Physics.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="BruteForceBroadPhase"/>.</summary>
public sealed class BruteForceBroadPhaseTests
{
    private BodySet _bodies = new();
    private BruteForceBroadPhase _broadPhase = new();
    private List<BroadPhasePair> _pairs = [];

    [Setup]
    public void Setup()
    {
        _bodies = new BodySet();
        _broadPhase = new BruteForceBroadPhase();
        _pairs = [];
    }

    [Test]
    public void ItIsNamedSoTheExamplesCanSayWhichOneRan()
    {
        Assert.AreEqual("brute force", _broadPhase.Name);
    }

    [Test]
    public void ItFindsOverlappingPairsAndIgnoresTheRest()
    {
        _bodies.AddBoxAt(0.0f, 0.0f);
        _bodies.AddBoxAt(5.0f, 0.0f);
        _bodies.AddBoxAt(500.0f, 0.0f);
        Update();
        Assert.AreEqual(1, _pairs.Count);
        Assert.AreEqual("1 with 2", _pairs[0].ToString());
    }

    [Test]
    public void ItComparesEveryPairExactlyOnce()
    {
        for (int index = 0; index < 10; index++)
        {
            _bodies.AddBoxAt(index * 100.0f, 0.0f);
        }

        Update();
        Assert.AreEqual(45, _broadPhase.LastComparisonCount, "ten choose two");
    }

    [Test]
    public void FindPairs_ClearsTheBufferItWasGiven()
    {
        _bodies.AddBoxAt(0.0f, 0.0f);
        Update();
        Update();
        Assert.AreEqual(0, _pairs.Count);
    }

    [Test]
    public void Query_ReturnsTheBodiesOverlappingAnArea()
    {
        _bodies.AddBoxAt(0.0f, 0.0f);
        _bodies.AddBoxAt(500.0f, 0.0f);
        _broadPhase.Update(_bodies.All);
        List<RigidBody2D> found = [];
        _broadPhase.Query(Aabb.FromCenterSize(Vector2.Zero, new Vector2(20.0f, 20.0f)), found);
        Assert.AreEqual(1, found.Count);
        Assert.AreEqual(1, found[0].Id);
    }

    private void Update()
    {
        _broadPhase.Update(_bodies.All);
        _broadPhase.FindPairs(_pairs);
    }
}
