using GEngine.Core;
using GEngine.Physics.BroadPhase;
using GEngine.Physics.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="BroadPhaseFilter"/>, the rule both broad phases share.</summary>
public sealed class BroadPhaseFilterTests
{
    private BodySet _bodies = new();

    [Setup]
    public void Setup() => _bodies = new BodySet();

    [Test]
    public void TwoOverlappingDynamicBodies_AreAPair()
    {
        RigidBody2D first = _bodies.AddBoxAt(0.0f, 0.0f);
        RigidBody2D second = _bodies.AddBoxAt(5.0f, 0.0f);
        Assert.IsTrue(BroadPhaseFilter.ShouldPair(first, second));
    }

    [Test]
    public void TwoStaticBodies_AreNeverAPair_HoweverMuchTheyOverlap()
    {
        RigidBody2D first = _bodies.Add(BodyType.Static, Vector2.Zero, new Vector2(10.0f, 10.0f));
        RigidBody2D second = _bodies.Add(BodyType.Static, Vector2.Zero, new Vector2(10.0f, 10.0f));
        Assert.IsFalse(BroadPhaseFilter.ShouldPair(first, second));
    }

    [Test]
    public void BodiesThatOnlyTouch_AreNotAPair()
    {
        RigidBody2D first = _bodies.AddBoxAt(0.0f, 0.0f);
        RigidBody2D second = _bodies.AddBoxAt(10.0f, 0.0f);
        Assert.IsFalse(BroadPhaseFilter.ShouldPair(first, second));
    }

    [Test]
    public void BodiesOnLayersThatIgnoreEachOther_AreNotAPair()
    {
        RigidBody2D first = _bodies.AddBoxAt(0.0f, 0.0f);
        RigidBody2D second = _bodies.AddBoxAt(5.0f, 0.0f);
        first.Layer = 0b01;
        first.Mask = 0b01;
        second.Layer = 0b10;
        second.Mask = 0b10;
        Assert.IsFalse(BroadPhaseFilter.ShouldPair(first, second));
    }

    [Test]
    public void OneSidedMasking_IsStillNotAPair_BecauseCollisionIsMutual()
    {
        RigidBody2D first = _bodies.AddBoxAt(0.0f, 0.0f);
        RigidBody2D second = _bodies.AddBoxAt(5.0f, 0.0f);
        second.Mask = 0;
        Assert.IsFalse(BroadPhaseFilter.ShouldPair(first, second));
    }

    [Test]
    public void Compare_OrdersPairsByTheirKey()
    {
        RigidBody2D first = _bodies.AddBoxAt(0.0f, 0.0f);
        RigidBody2D second = _bodies.AddBoxAt(1.0f, 0.0f);
        RigidBody2D third = _bodies.AddBoxAt(2.0f, 0.0f);
        var low = BroadPhasePair.Ordered(first, second);
        var high = BroadPhasePair.Ordered(second, third);
        Assert.IsTrue(BroadPhaseFilter.Compare(low, high) < 0);
        Assert.IsTrue(BroadPhaseFilter.Compare(high, low) > 0);
        Assert.AreEqual(0, BroadPhaseFilter.Compare(low, low));
    }
}
