using System;
using System.Collections.Generic;
using GEngine.Testing;
using MarioClone.Levels;

namespace MarioClone.Tests;

/// <summary>
/// The legend is the whole vocabulary of a level file. If a character is in neither table the
/// loader refuses the file, so these two tables are the difference between a level that loads
/// and one that does not.
/// </summary>
public sealed class LevelLegendTests
{
    [TestCase(LevelLegend.Empty, TileKind.Empty)]
    [TestCase(LevelLegend.Ground, TileKind.Ground)]
    [TestCase(LevelLegend.Pipe, TileKind.Pipe)]
    public void EveryTileCharacterMapsToItsTile(char symbol, TileKind expected)
    {
        Assert.IsTrue(LevelLegend.Tiles.TryGetValue(symbol, out TileKind tile));
        Assert.AreEqual(expected, tile);
    }

    [TestCase(LevelLegend.Brick, SpawnKind.Brick)]
    [TestCase(LevelLegend.QuestionBlock, SpawnKind.QuestionBlock)]
    [TestCase(LevelLegend.Coin, SpawnKind.Coin)]
    [TestCase(LevelLegend.Goomba, SpawnKind.Goomba)]
    [TestCase(LevelLegend.Goal, SpawnKind.Goal)]
    [TestCase(LevelLegend.Player, SpawnKind.Player)]
    public void EverySpawnCharacterMapsToItsThing(char symbol, SpawnKind expected)
    {
        Assert.IsTrue(LevelLegend.Spawns.TryGetValue(symbol, out SpawnKind spawn));
        Assert.AreEqual(expected, spawn);
    }

    // A character that meant both would make the loader's answer depend on the order it asks in.
    [Test]
    public void NoCharacterIsBothATileAndAThing()
    {
        foreach (KeyValuePair<char, TileKind> tile in LevelLegend.Tiles)
        {
            Assert.IsFalse(LevelLegend.Spawns.ContainsKey(tile.Key), tile.Key + " means two things");
        }
    }

    [Test]
    public void EveryTileKindHasACharacter()
    {
        foreach (TileKind kind in Enum.GetValues<TileKind>())
        {
            Assert.IsTrue(Holds(LevelLegend.Tiles, kind), kind + " cannot be written in a level");
        }
    }

    [Test]
    public void EverySpawnKindHasACharacter()
    {
        foreach (SpawnKind kind in Enum.GetValues<SpawnKind>())
        {
            Assert.IsTrue(Holds(LevelLegend.Spawns, kind), kind + " cannot be written in a level");
        }
    }

    [Test]
    public void TheLegendCanDescribeItselfForAHeaderComment()
    {
        string described = LevelLegend.Describe();
        Assert.IsTrue(described.Contains(LevelLegend.Ground, StringComparison.Ordinal), "the description names the ground");
        Assert.IsTrue(described.Contains(LevelLegend.Goomba, StringComparison.Ordinal), "and the goomba");
    }

    // The read-only view of a dictionary has no ContainsValue, and asking the concrete type for
    // one would mean the legend exposing its dictionary just so a test could look inside it.
    private static bool Holds<TValue>(IReadOnlyDictionary<char, TValue> legend, TValue wanted)
    {
        foreach (KeyValuePair<char, TValue> entry in legend)
        {
            if (EqualityComparer<TValue>.Default.Equals(entry.Value, wanted))
            {
                return true;
            }
        }

        return false;
    }
}
