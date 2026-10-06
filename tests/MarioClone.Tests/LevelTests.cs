using GEngine.Core;
using GEngine.Testing;
using MarioClone.Levels;

namespace MarioClone.Tests;

/// <summary>
/// A level is a grid of tiles plus a list of things to spawn. Everything here is about the
/// translation between the two coordinate systems in the game: columns and rows, and pixels.
/// </summary>
public sealed class LevelTests
{
    private const float TileSize = 8.0f;

    [Test]
    public void ANewLevelIsEmpty()
    {
        var level = new Level(4, 3, TileSize);
        Assert.AreEqual(4, level.Columns);
        Assert.AreEqual(3, level.Rows);
        Assert.AreEqual(TileKind.Empty, level.TileAt(2, 1));
        Assert.AreEqual(0, level.Spawns.Count);
    }

    [Test]
    public void ATileGoesInAndComesBackOut()
    {
        var level = new Level(4, 3, TileSize);
        level.SetTile(2, 1, TileKind.Pipe);
        Assert.AreEqual(TileKind.Pipe, level.TileAt(2, 1));
        Assert.AreEqual(TileKind.Empty, level.TileAt(1, 1), "its neighbour was left alone");
    }

    // Outside the grid is empty rather than an exception, because the physics asks about tiles
    // it has no reason to believe exist - one column past the edge, on every step.
    [Test]
    public void OutsideTheGridIsEmptyRatherThanAnError()
    {
        var level = new Level(4, 3, TileSize);
        Assert.AreEqual(TileKind.Empty, level.TileAt(-1, 0));
        Assert.AreEqual(TileKind.Empty, level.TileAt(0, -1));
        Assert.AreEqual(TileKind.Empty, level.TileAt(4, 0));
        Assert.AreEqual(TileKind.Empty, level.TileAt(0, 3));
    }

    [Test]
    public void WritingOutsideTheGridIsIgnored()
    {
        var level = new Level(2, 2, TileSize);
        level.SetTile(9, 9, TileKind.Ground);
        Assert.AreEqual(TileKind.Empty, level.TileAt(9, 9), "it went nowhere and threw nothing");
    }

    [Test]
    public void ContainsKnowsWhereTheGridEnds()
    {
        var level = new Level(2, 2, TileSize);
        Assert.IsTrue(level.Contains(0, 0));
        Assert.IsTrue(level.Contains(1, 1));
        Assert.IsFalse(level.Contains(2, 1), "columns are zero based, so two is past the end");
        Assert.IsFalse(level.Contains(-1, 0));
    }

    [Test]
    public void TileBoundsAreTheSquareThatTileOccupies()
    {
        var level = new Level(4, 3, TileSize);
        Aabb bounds = level.TileBounds(2, 1);
        Assert.ApproximatelyEqual(16.0f, bounds.Left, 0.001f);
        Assert.ApproximatelyEqual(8.0f, bounds.Top, 0.001f, "row one starts eight pixels down");
        Assert.ApproximatelyEqual(TileSize, bounds.Size.X, 0.001f);
        Assert.ApproximatelyEqual(TileSize, bounds.Size.Y, 0.001f);
    }

    [Test]
    public void TileCenterIsTheMiddleOfThatSquare()
    {
        var level = new Level(4, 3, TileSize);
        Vector2 center = level.TileCenter(0, 0);
        Assert.ApproximatelyEqual(4.0f, center.X, 0.001f);
        Assert.ApproximatelyEqual(4.0f, center.Y, 0.001f);
    }

    [Test]
    public void TheBoundsOfTheLevelCoverEveryTile()
    {
        var level = new Level(4, 3, TileSize);
        Assert.ApproximatelyEqual(32.0f, level.Bounds.Size.X, 0.001f);
        Assert.ApproximatelyEqual(24.0f, level.Bounds.Size.Y, 0.001f);
        Assert.ApproximatelyEqual(0.0f, level.Bounds.Left, 0.001f, "a level starts at the origin");
    }

    [Test]
    public void SpawnsAreKeptInTheOrderTheyWereAdded()
    {
        var level = new Level(4, 3, TileSize);
        level.AddSpawn(new LevelSpawn(SpawnKind.Coin, 1, 1));
        level.AddSpawn(new LevelSpawn(SpawnKind.Goomba, 2, 1));
        Assert.AreEqual(2, level.Spawns.Count);
        Assert.AreEqual(SpawnKind.Coin, level.Spawns[0].Kind, "order decides who is entity zero");
        Assert.AreEqual(SpawnKind.Goomba, level.Spawns[1].Kind);
    }
}
