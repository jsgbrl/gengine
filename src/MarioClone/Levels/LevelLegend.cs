// The legend of the level file, in one table. A new kind of thing is one line here, one case
// in MarioFactory, and nothing else - which is what "a new level is a text file" costs.

using System.Collections.Generic;

namespace MarioClone.Levels;

/// <summary>What each character of a level file means.</summary>
public static class LevelLegend
{
    /// <summary>Sky.</summary>
    public const char Empty = '.';

    /// <summary>Solid ground.</summary>
    public const char Ground = '#';

    /// <summary>A pipe.</summary>
    public const char Pipe = 'P';

    /// <summary>A breakable brick.</summary>
    public const char Brick = 'B';

    /// <summary>A question block.</summary>
    public const char QuestionBlock = '?';

    /// <summary>A coin.</summary>
    public const char Coin = 'o';

    /// <summary>A goomba.</summary>
    public const char Goomba = 'G';

    /// <summary>The flag at the end.</summary>
    public const char Goal = 'F';

    /// <summary>Where the player starts.</summary>
    public const char Player = 'M';

    /// <summary>The characters that pave the grid, and what they pave it with.</summary>
    public static IReadOnlyDictionary<char, TileKind> Tiles { get; } = new Dictionary<char, TileKind>
    {
        [Empty] = TileKind.Empty,
        [Ground] = TileKind.Ground,
        [Pipe] = TileKind.Pipe,
    };

    /// <summary>The characters that place a thing, and what they place.</summary>
    public static IReadOnlyDictionary<char, SpawnKind> Spawns { get; } = new Dictionary<char, SpawnKind>
    {
        [Brick] = SpawnKind.Brick,
        [QuestionBlock] = SpawnKind.QuestionBlock,
        [Coin] = SpawnKind.Coin,
        [Goomba] = SpawnKind.Goomba,
        [Goal] = SpawnKind.Goal,
        [Player] = SpawnKind.Player,
    };

    /// <summary>The legend as text, for the level file's own header and for the README.</summary>
    /// <returns>One line per character.</returns>
    public static string Describe() => string.Join(
        "\n",
        ".  sky",
        "#  ground",
        "P  pipe",
        "B  brick, breakable when the player is big",
        "?  question block, gives once",
        "o  coin",
        "G  goomba",
        "F  the flag at the end",
        "M  where the player starts");
}
