using System;
using GEngine.Physics.BroadPhase;
using GEngine.Physics.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Physics.Tests;

/// <summary>Covers <see cref="BroadPhasePair"/>.</summary>
public sealed class BroadPhasePairTests
{
    [Test]
    public void Ordered_PutsTheLowerIdentityFirst_WhicheverWayRoundItWasGiven()
    {
        var bodies = new BodySet();
        RigidBody2D first = bodies.AddBoxAt(0.0f, 0.0f);
        RigidBody2D second = bodies.AddBoxAt(1.0f, 0.0f);
        Assert.AreSame(first, BroadPhasePair.Ordered(second, first).First);
        Assert.AreSame(second, BroadPhasePair.Ordered(second, first).Second);
    }

    [Test]
    public void TheSamePairFoundTwice_HasTheSameKey()
    {
        var bodies = new BodySet();
        RigidBody2D first = bodies.AddBoxAt(0.0f, 0.0f);
        RigidBody2D second = bodies.AddBoxAt(1.0f, 0.0f);
        Assert.AreEqual(BroadPhasePair.Ordered(first, second).Key, BroadPhasePair.Ordered(second, first).Key);
    }

    [Test]
    public void TwoDifferentPairs_HaveDifferentKeys()
    {
        var bodies = new BodySet();
        RigidBody2D first = bodies.AddBoxAt(0.0f, 0.0f);
        RigidBody2D second = bodies.AddBoxAt(1.0f, 0.0f);
        RigidBody2D third = bodies.AddBoxAt(2.0f, 0.0f);
        Assert.AreNotEqual(BroadPhasePair.Ordered(first, second).Key, BroadPhasePair.Ordered(first, third).Key);
    }

    [Test]
    public void Equality_ComparesTheTwoBodies()
    {
        var bodies = new BodySet();
        RigidBody2D first = bodies.AddBoxAt(0.0f, 0.0f);
        RigidBody2D second = bodies.AddBoxAt(1.0f, 0.0f);
        var pair = BroadPhasePair.Ordered(first, second);
        Assert.IsTrue(pair == BroadPhasePair.Ordered(second, first));
        Assert.IsTrue(pair != BroadPhasePair.Ordered(first, first));
        Assert.IsTrue(pair.Equals((object)BroadPhasePair.Ordered(first, second)));
        Assert.IsFalse(pair.Equals("not a pair"));
        Assert.AreEqual(pair.GetHashCode(), BroadPhasePair.Ordered(second, first).GetHashCode());
        Assert.AreEqual("1 with 2", pair.ToString());
    }

    [Test]
    public void AMissingBody_IsRefused()
    {
        var bodies = new BodySet();
        RigidBody2D body = bodies.AddBoxAt(0.0f, 0.0f);
        Assert.Throws<ArgumentNullException>(() => BroadPhasePair.Ordered(body, null!));
    }
}
