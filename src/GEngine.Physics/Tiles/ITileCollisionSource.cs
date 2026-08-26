// A whole level of solid ground without one body per tile.
//
// A screen of Mario is a few thousand tiles. Giving each one a RigidBody2D would put a few
// thousand boxes through the broad phase every step to discover what a division already
// knows: which cell a point is in. The tilemap is queried by arithmetic instead, so a level
// costs the same as an empty world.

using GEngine.Core;

namespace GEngine.Physics.Tiles;

/// <summary>A grid of collidable tiles the physics world can test against directly.</summary>
public interface ITileCollisionSource
{
    /// <summary>Width and height of one tile, in pixels.</summary>
    float TileSize { get; }

    /// <summary>How many tiles across.</summary>
    int Columns { get; }

    /// <summary>How many tiles down.</summary>
    int Rows { get; }

    /// <summary>What the tile at a position does. Outside the grid is always empty.</summary>
    /// <param name="column">Zero-based column.</param>
    /// <param name="row">Zero-based row.</param>
    /// <returns>How that tile collides.</returns>
    TileCollision At(int column, int row);

    /// <summary>The box a tile occupies in world pixels.</summary>
    /// <param name="column">Zero-based column.</param>
    /// <param name="row">Zero-based row.</param>
    /// <returns>The tile box.</returns>
    Aabb BoundsOf(int column, int row);
}
