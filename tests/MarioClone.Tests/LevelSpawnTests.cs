using GEngine.Testing;
using MarioClone.Levels;

namespace MarioClone.Tests;

/// <summary>
/// A spawn is a value: what to make and where. It is compared by value so that a test can say
/// what a level contains without caring which object holds it.
/// </summary>
public sealed class LevelSpawnTests
{
    [Test]
    public void ASpawnRemembersWhatAndWhere()
    {
        var spawn = new LevelSpawn(SpawnKind.Goomba, 12, 9);
        Assert.AreEqual(SpawnKind.Goomba, spawn.Kind);
        Assert.AreEqual(12, spawn.Column);
        Assert.AreEqual(9, spawn.Row);
    }

    [Test]
    public void TwoSpawnsWithTheSameFieldsAreEqual()
    {
        var left = new LevelSpawn(SpawnKind.Coin, 3, 4);
        var right = new LevelSpawn(SpawnKind.Coin, 3, 4);
        Assert.IsTrue(left == right);
        Assert.IsFalse(left != right);
        Assert.IsTrue(left.Equals(right));
        Assert.IsTrue(left.Equals((object)right));
        Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
    }

    [Test]
    public void ChangingAnyFieldMakesThemDifferent()
    {
        var spawn = new LevelSpawn(SpawnKind.Coin, 3, 4);
        Assert.IsTrue(spawn != new LevelSpawn(SpawnKind.Goomba, 3, 4));
        Assert.IsTrue(spawn != new LevelSpawn(SpawnKind.Coin, 4, 4));
        Assert.IsTrue(spawn != new LevelSpawn(SpawnKind.Coin, 3, 5));
        Assert.IsFalse(spawn.Equals("not a spawn"));
    }

    [Test]
    public void ItPrintsAsSomethingAFailureMessageCanShow()
    {
        var spawn = new LevelSpawn(SpawnKind.Player, 2, 7);
        Assert.AreEqual("Player at 2,7", spawn.ToString());
    }
}
