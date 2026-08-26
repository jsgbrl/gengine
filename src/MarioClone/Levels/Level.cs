// A loaded level: the collision grid, the things to place, and how big it all is. It knows
// nothing about physics or drawing - LevelLoader fills it, MarioFactory turns it into a world.

using System;
using System.Collections.Generic;
using GEngine.Core;

namespace MarioClone.Levels;

/// <summary>A level as data: a grid of tiles and a list of things to place in it.</summary>
public sealed class Level
{
    private readonly TileKind[] _tiles;
    private readonly List<LevelSpawn> _spawns = [];

    /// <summary>Creates an empty level.</summary>
    /// <param name="columns">How many tiles across.</param>
    /// <param name="rows">How many tiles down.</param>
    /// <param name="tileSize">Width and height of one tile, in pixels.</param>
    public Level(int columns, int rows, float tileSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileSize);
        Columns = columns;
        Rows = rows;
        TileSize = tileSize;
        _tiles = new TileKind[columns * rows];
    }

    /// <summary>How many tiles across.</summary>
    public int Columns { get; }

    /// <summary>How many tiles down.</summary>
    public int Rows { get; }

    /// <summary>Width and height of one tile, in pixels.</summary>
    public float TileSize { get; }

    /// <summary>The things to place, in the order the file listed them.</summary>
    public IReadOnlyList<LevelSpawn> Spawns => _spawns;

    /// <summary>The whole level in pixels.</summary>
    public Aabb Bounds => new(Vector2.Zero, new Vector2(Columns * TileSize, Rows * TileSize));

    /// <summary>Where the player starts, in pixels. The centre of the level if the file forgot to say.</summary>
    public Vector2 PlayerStart { get; internal set; }

    /// <summary>Sets one cell of the grid.</summary>
    /// <param name="column">Zero-based column.</param>
    /// <param name="row">Zero-based row.</param>
    /// <param name="kind">What that cell holds.</param>
    public void SetTile(int column, int row, TileKind kind)
    {
        if (Contains(column, row))
        {
            _tiles[(row * Columns) + column] = kind;
        }
    }

    /// <summary>What one cell holds. Outside the grid is empty.</summary>
    /// <param name="column">Zero-based column.</param>
    /// <param name="row">Zero-based row.</param>
    /// <returns>The tile.</returns>
    public TileKind TileAt(int column, int row) =>
        Contains(column, row) ? _tiles[(row * Columns) + column] : TileKind.Empty;

    /// <summary>Adds a thing to place.</summary>
    /// <param name="spawn">What to place, and where.</param>
    public void AddSpawn(LevelSpawn spawn) => _spawns.Add(spawn);

    /// <summary>The box one tile occupies, in pixels.</summary>
    /// <param name="column">Zero-based column.</param>
    /// <param name="row">Zero-based row.</param>
    /// <returns>The box.</returns>
    public Aabb TileBounds(int column, int row)
    {
        var corner = new Vector2(column * TileSize, row * TileSize);
        return new Aabb(corner, corner + new Vector2(TileSize, TileSize));
    }

    /// <summary>The centre of a tile, in pixels, which is where a thing placed there goes.</summary>
    /// <param name="column">Zero-based column.</param>
    /// <param name="row">Zero-based row.</param>
    /// <returns>The centre.</returns>
    public Vector2 TileCenter(int column, int row) => TileBounds(column, row).Center;

    /// <summary>True when a position names a tile that exists.</summary>
    /// <param name="column">Zero-based column.</param>
    /// <param name="row">Zero-based row.</param>
    /// <returns>True when it is inside the grid.</returns>
    public bool Contains(int column, int row) =>
        column >= 0 && column < Columns && row >= 0 && row < Rows;
}
