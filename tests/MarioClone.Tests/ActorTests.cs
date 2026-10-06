using System;
using GEngine.Core;
using GEngine.Core.Contracts;
using GEngine.Physics;
using GEngine.Testing;
using MarioClone.Actors;
using MarioClone.Tests.Doubles;

namespace MarioClone.Tests;

/// <summary>
/// What every actor has in common: a body, a sprite name, a facing, and a life that ends. The
/// base class is deliberately thin - an actor decides what it does, and MarioFactory decides
/// what it is - so what there is to check is the join between an actor and its world.
/// </summary>
public sealed class ActorTests
{
    [Test]
    public void AnActorIsAliveUntilSomethingRetiresIt()
    {
        var test = new TestWorld("tiles:\n.M..o.\n######\n");
        Coin coin = test.Find<Coin>()!;
        Assert.IsTrue(coin.IsAlive);

        test.Hold(InputAction.MoveRight);
        test.Step(60);
        Assert.IsFalse(coin.IsAlive, "collecting it retired it");
    }

    [Test]
    public void AnActorsPositionAndBoundsAreItsBodys()
    {
        var test = new TestWorld("tiles:\n.M.\n###\n");
        Player player = test.Player;
        Assert.ApproximatelyEqual(player.Body.Position.X, player.Position.X, 0.0f);
        Assert.ApproximatelyEqual(player.Body.Bounds.Top, player.Bounds.Top, 0.0f);
    }

    // An actor with no world is a programming mistake, not a runtime condition, so asking is an
    // exception rather than a null - a null would be checked in ten places and forgotten in one.
    [Test]
    public void AnActorOutsideAWorldSaysSoRatherThanReturningNothing()
    {
        var orphan = new Coin(new RigidBody2D(BodyType.Static, Vector2.Zero, new Vector2(5.0f, 5.0f)));
        Assert.Throws<InvalidOperationException>(() => _ = orphan.World);
    }

    [Test]
    public void AnActorInAWorldKnowsWhichWorld()
    {
        var test = new TestWorld("tiles:\n.M.\n###\n");
        Assert.AreSame(test.World, test.Player.World);
    }

    [Test]
    public void EveryActorHasASpriteToDrawItWith()
    {
        var test = new TestWorld("tiles:\n.M.o.G.B.?.F.\n#############\n");
        foreach (Actor actor in test.World.Actors)
        {
            Assert.IsFalse(string.IsNullOrEmpty(actor.SpriteName), actor.GetType().Name + " has no sprite");
        }
    }

    // Facing is what a mirrored sprite is chosen by, so it has to change with the walking and
    // not with the pushing: a goomba shoved backwards is still facing the way it walks.
    [Test]
    public void AGoombaFacesTheWayItWalks()
    {
        var test = new TestWorld("tiles:\n.M......G.\n##########\n");
        Goomba goomba = test.Find<Goomba>()!;
        test.Step(1);
        bool facing = goomba.IsFacingLeft;

        test.Step(120);
        Assert.AreEqual(facing, goomba.IsFacingLeft, "nothing turned it around on open ground");
    }

    [Test]
    public void AnActorHearsAboutWhatItTouches()
    {
        var test = new TestWorld("tiles:\n.M..o.\n######\n");
        Coin coin = test.Find<Coin>()!;
        int before = test.Session.Coins;

        test.Hold(InputAction.MoveRight);
        test.Step(60);

        Assert.AreEqual(before + 1, test.Session.Coins, "the trigger reached the coin");
        Assert.IsFalse(coin.IsAlive);
    }
}
