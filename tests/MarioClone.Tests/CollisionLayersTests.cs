using GEngine.Testing;
using MarioClone.Game;

namespace MarioClone.Tests;

/// <summary>
/// Layers are the answer to "who can touch whom". They are worth a test because getting one bit
/// wrong produces a bug that looks like physics - a goomba walking through a coin, a player
/// falling through the floor - and is not.
/// </summary>
public sealed class CollisionLayersTests
{
    [Test]
    public void EveryLayerIsItsOwnBit()
    {
        int[] layers =
        [
            CollisionLayers.Level,
            CollisionLayers.Player,
            CollisionLayers.Enemy,
            CollisionLayers.Pickup,
            CollisionLayers.Goal,
        ];

        int union = 0;
        foreach (int layer in layers)
        {
            Assert.AreEqual(0, union & layer, "layers overlap, so one thing is two things");
            union |= layer;
        }
    }

    [Test]
    public void ThePlayerCollidesWithEverythingWorthCollidingWith()
    {
        Assert.AreEqual(CollisionLayers.Level, CollisionLayers.PlayerMask & CollisionLayers.Level);
        Assert.AreEqual(CollisionLayers.Enemy, CollisionLayers.PlayerMask & CollisionLayers.Enemy);
        Assert.AreEqual(CollisionLayers.Pickup, CollisionLayers.PlayerMask & CollisionLayers.Pickup);
        Assert.AreEqual(CollisionLayers.Goal, CollisionLayers.PlayerMask & CollisionLayers.Goal);
    }

    // Otherwise walking into a coin turns a goomba around, which looks like the coin is solid.
    [Test]
    public void EnemiesIgnorePickupsAndTheFlag()
    {
        Assert.AreEqual(0, CollisionLayers.EnemyMask & CollisionLayers.Pickup);
        Assert.AreEqual(0, CollisionLayers.EnemyMask & CollisionLayers.Goal);
    }

    [Test]
    public void APickupOnlyEverHearsFromThePlayer()
    {
        Assert.AreEqual(CollisionLayers.Player, CollisionLayers.PickupMask);
    }

    [Test]
    public void MasksAgreeWithEachOtherInBothDirections()
    {
        AssertMutual(CollisionLayers.Player, CollisionLayers.PlayerMask, CollisionLayers.Level, CollisionLayers.LevelMask);
        AssertMutual(CollisionLayers.Player, CollisionLayers.PlayerMask, CollisionLayers.Enemy, CollisionLayers.EnemyMask);
        AssertMutual(CollisionLayers.Enemy, CollisionLayers.EnemyMask, CollisionLayers.Level, CollisionLayers.LevelMask);
        AssertMutual(CollisionLayers.Player, CollisionLayers.PlayerMask, CollisionLayers.Pickup, CollisionLayers.PickupMask);
    }

    // A collision one side agrees to and the other does not is a filter that depends on which
    // body the broad phase happened to name first.
    private static void AssertMutual(int leftLayer, int leftMask, int rightLayer, int rightMask)
    {
        bool leftSeesRight = (leftMask & rightLayer) != 0;
        bool rightSeesLeft = (rightMask & leftLayer) != 0;
        Assert.AreEqual(leftSeesRight, rightSeesLeft, "one side agreed to a collision the other refused");
    }
}
