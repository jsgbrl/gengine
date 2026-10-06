using System;
using GEngine.Rendering.Assets;
using GEngine.Testing;
using MarioClone.Game;
using MarioClone.Levels;

namespace MarioClone.Tests;

/// <summary>
/// The loader is thirty lines and one hard-won rule: the grid starts after `tiles:`. The first
/// version treated `#` as a comment, and since `#` is also the ground, it quietly deleted every
/// floor in the game. The test named TheGroundIsNotAComment is the one that would have caught it.
/// </summary>
public sealed class LevelLoaderTests
{
    private const float TileSize = 8.0f;

    [Test]
    public void TheGroundIsNotAComment()
    {
        Level level = Parse("# this line is a header\ntiles:\n####\n");
        Assert.AreEqual(TileKind.Ground, level.TileAt(0, 0), "a row of '#' is floor, not a comment");
        Assert.AreEqual(4, level.Columns);
        Assert.AreEqual(1, level.Rows);
    }

    [Test]
    public void EverythingBeforeTheMarkerIsAHeader()
    {
        Level level = Parse("name: world 1-1\nlegend: whatever we like\ntiles:\n..\n##\n");
        Assert.AreEqual(2, level.Rows, "the header contributed no rows");
        Assert.AreEqual(TileKind.Empty, level.TileAt(0, 0));
        Assert.AreEqual(TileKind.Ground, level.TileAt(0, 1));
    }

    [Test]
    public void ALevelWithoutTheMarkerIsRejected()
    {
        Assert.Throws<FormatException>(() => Parse("####\n####\n"));
    }

    [Test]
    public void ALevelWithNoRowsIsRejected()
    {
        Assert.Throws<FormatException>(() => Parse("tiles:\n"));
    }

    [Test]
    public void RaggedRowsAreRejected()
    {
        FormatException error = Assert.Throws<FormatException>(() => Parse("tiles:\n####\n###\n"));
        Assert.IsTrue(error.Message.Contains("row 1", StringComparison.Ordinal), "the message says which row is wrong");
    }

    [Test]
    public void ACharacterOutsideTheLegendIsRejected()
    {
        FormatException error = Assert.Throws<FormatException>(() => Parse("tiles:\n#Z#\n"));
        Assert.IsTrue(error.Message.Contains('Z', StringComparison.Ordinal), "the message says which character");
    }

    [Test]
    public void CarriageReturnsAreNotPartOfARow()
    {
        Level level = Parse("tiles:\r\n####\r\n####\r\n");
        Assert.AreEqual(4, level.Columns, "\r never counted as a column");
    }

    [Test]
    public void BlankLinesInsideTheGridAreSkipped()
    {
        Level level = Parse("tiles:\n##\n\n##\n\n");
        Assert.AreEqual(2, level.Rows);
    }

    // A thing is not a tile: you cannot stand on a coin, so the cell it came from stays empty.
    [Test]
    public void ASpawnLeavesTheCellEmptyBehindIt()
    {
        Level level = Parse("tiles:\n.o.\n###\n");
        Assert.AreEqual(TileKind.Empty, level.TileAt(1, 0));
        Assert.AreEqual(1, level.Spawns.Count);
        Assert.AreEqual(SpawnKind.Coin, level.Spawns[0].Kind);
        Assert.AreEqual(1, level.Spawns[0].Column);
    }

    [Test]
    public void TheStartMarkerBecomesThePlayerStart()
    {
        Level level = Parse("tiles:\n.M.\n###\n");
        Assert.ApproximatelyEqual(TileSize * 1.5f, level.PlayerStart.X, 0.001f, "centre of column one");
        Assert.ApproximatelyEqual(TileSize * 0.5f, level.PlayerStart.Y, 0.001f, "centre of row zero");
    }

    [Test]
    public void TheMarkerIsCaseInsensitiveAndMayBeIndented()
    {
        Level level = Parse("  TILES:  \n##\n");
        Assert.AreEqual(2, level.Columns);
    }

    [Test]
    public void ParsingNothingIsAnArgumentError()
    {
        Assert.Throws<ArgumentNullException>(() => LevelLoader.Parse(null!, TileSize));
    }

    [Test]
    public void TheShippedLevelLoadsAndHasOneOfEverything()
    {
        Level level = Parse(new EmbeddedAssetSource(typeof(MarioGame).Assembly).ReadText("levels/1-1.txt"));
        Assert.IsTrue(level.Columns > 100, "world 1-1 is a long level");
        Assert.IsTrue(Count(level, SpawnKind.Player) == 1, "exactly one start");
        Assert.IsTrue(Count(level, SpawnKind.Goal) == 1, "exactly one flag");
        Assert.IsTrue(Count(level, SpawnKind.Goomba) > 0, "somebody to jump on");
        Assert.IsTrue(Count(level, SpawnKind.Coin) > 0, "something to collect");
    }

    private static int Count(Level level, SpawnKind kind)
    {
        int found = 0;
        foreach (LevelSpawn spawn in level.Spawns)
        {
            found += spawn.Kind == kind ? 1 : 0;
        }

        return found;
    }

    private static Level Parse(string text) => LevelLoader.Parse(text, TileSize);
}
