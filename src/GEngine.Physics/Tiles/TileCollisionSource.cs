// The tilemap the game uses: one array of enum values and the arithmetic to turn a pixel
// into a cell. Origin is the top-left corner of tile (0, 0) at world (0, 0).

using System;
using GEngine.Core;

namespace GEngine.Physics.Tiles;

/// <summary>A rectangular grid of tiles, stored as one flat array.</summary>
public sealed class TileCollisionSource : ITileCollisionSource
{
    private readonly TileCollision[] _tiles;

    /// <summary>Creates an empty tilemap.</summary>
    /// <param name="columns">How many tiles across.</param>
    /// <param name="rows">How many tiles down.</param>
    /// <param name="tileSize">Width and height of one tile, in pixels.</param>
    public TileCollisionSource(int columns, int rows, float tileSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(columns);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(rows);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(tileSize);
        Columns = columns;
        Rows = rows;
        TileSize = tileSize;
        _tiles = new TileCollision[columns * rows];
    }

    /// <inheritdoc/>
    public float TileSize { get; }

    /// <inheritdoc/>
    public int Columns { get; }

    /// <inheritdoc/>
    public int Rows { get; }

    /// <summary>The box the whole tilemap occupies, in world pixels.</summary>
    public Aabb Bounds => new(Vector2.Zero, new Vector2(Columns * TileSize, Rows * TileSize));

    /// <summary>Sets what one tile does.</summary>
    /// <param name="column">Zero-based column.</param>
    /// <param name="row">Zero-based row.</param>
    /// <param name="collision">How that tile should collide.</param>
    /// <exception cref="ArgumentOutOfRangeException">The position is outside the grid.</exception>
    public void Set(int column, int row, TileCollision collision)
    {
        if (!Contains(column, row))
        {
            throw new ArgumentOutOfRangeException(nameof(column), "tile position is outside the map");
        }

        _tiles[(row * Columns) + column] = collision;
    }

    /// <inheritdoc/>
    public TileCollision At(int column, int row) =>
        Contains(column, row) ? _tiles[(row * Columns) + column] : TileCollision.None;

    /// <inheritdoc/>
    public Aabb BoundsOf(int column, int row)
    {
        var corner = new Vector2(column * TileSize, row * TileSize);
        return new Aabb(corner, corner + new Vector2(TileSize, TileSize));
    }

    /// <summary>The column holding a world X coordinate, which may be outside the grid.</summary>
    /// <param name="worldX">Horizontal world coordinate, in pixels.</param>
    /// <returns>The column index.</returns>
    public int ColumnAt(float worldX) => MathG.FloorToInt(worldX / TileSize);

    /// <summary>The row holding a world Y coordinate, which may be outside the grid.</summary>
    /// <param name="worldY">Vertical world coordinate, in pixels.</param>
    /// <returns>The row index.</returns>
    public int RowAt(float worldY) => MathG.FloorToInt(worldY / TileSize);

    /// <summary>True when a position names a tile that exists.</summary>
    /// <param name="column">Zero-based column.</param>
    /// <param name="row">Zero-based row.</param>
    /// <returns>True when the position is inside the grid.</returns>
    public bool Contains(int column, int row) =>
        column >= 0 && column < Columns && row >= 0 && row < Rows;
}
