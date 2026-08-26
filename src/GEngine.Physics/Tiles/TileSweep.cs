// Swept collision against a tilemap. The cells the movement can possibly touch are found
// by dividing, not by searching, so the cost depends on how far the body moved and not at
// all on how big the level is.

using System;
using GEngine.Core;
using GEngine.Physics.NarrowPhase;

namespace GEngine.Physics.Tiles;

/// <summary>Finds the first tile a moving box would hit.</summary>
public static class TileSweep
{
    /// <summary>Sweeps a box against every tile its movement could reach.</summary>
    /// <param name="tiles">The tilemap.</param>
    /// <param name="moving">The box before it moves.</param>
    /// <param name="delta">How far it wants to move, in pixels.</param>
    /// <param name="tileBounds">The box of the tile that was hit, when there was one.</param>
    /// <returns>The first touch, or a miss.</returns>
    public static SweepResult FindNearest(ITileCollisionSource tiles, Aabb moving, Vector2 delta, out Aabb tileBounds)
    {
        ArgumentNullException.ThrowIfNull(tiles);
        Aabb area = moving.Union(moving.Translated(delta));
        SweepResult nearest = SweepResult.Miss;
        tileBounds = default;
        int lastColumn = MathG.FloorToInt(area.Max.X / tiles.TileSize);
        int lastRow = MathG.FloorToInt(area.Max.Y / tiles.TileSize);
        for (int column = MathG.FloorToInt(area.Min.X / tiles.TileSize); column <= lastColumn; column++)
        {
            for (int row = MathG.FloorToInt(area.Min.Y / tiles.TileSize); row <= lastRow; row++)
            {
                Nearer(tiles, moving, delta, new TileAt(column, row)).Apply(ref nearest, ref tileBounds);
            }
        }

        return nearest;
    }

    /// <summary>Finds a solid tile a box is already inside, when there is one.</summary>
    /// <param name="tiles">The tilemap.</param>
    /// <param name="box">The box to test.</param>
    /// <param name="tileBounds">The box of the overlapping tile, when there was one.</param>
    /// <returns>True when the box overlaps a solid tile.</returns>
    /// <remarks>
    /// One-way tiles are never reported: being inside one is a legal state, and pushing a
    /// body out of the ledge it is jumping through is exactly what a one-way tile exists to
    /// avoid.
    /// </remarks>
    public static bool TryFindOverlap(ITileCollisionSource tiles, Aabb box, out Aabb tileBounds)
    {
        ArgumentNullException.ThrowIfNull(tiles);
        int lastColumn = MathG.FloorToInt(box.Max.X / tiles.TileSize);
        int lastRow = MathG.FloorToInt(box.Max.Y / tiles.TileSize);
        for (int column = MathG.FloorToInt(box.Min.X / tiles.TileSize); column <= lastColumn; column++)
        {
            for (int row = MathG.FloorToInt(box.Min.Y / tiles.TileSize); row <= lastRow; row++)
            {
                if (IsOverlappingSolid(tiles, box, new TileAt(column, row), out tileBounds))
                {
                    return true;
                }
            }
        }

        tileBounds = default;
        return false;
    }

    private static bool IsOverlappingSolid(ITileCollisionSource tiles, Aabb box, TileAt at, out Aabb tileBounds)
    {
        tileBounds = tiles.BoundsOf(at.Column, at.Row);
        return tiles.At(at.Column, at.Row) == TileCollision.Solid && box.Intersects(tileBounds);
    }

    private static Candidate Nearer(ITileCollisionSource tiles, Aabb moving, Vector2 delta, TileAt at)
    {
        TileCollision collision = tiles.At(at.Column, at.Row);
        Aabb bounds = tiles.BoundsOf(at.Column, at.Row);
        if (!IsSolidFor(collision, moving, delta, bounds))
        {
            return Candidate.None;
        }

        SweepResult result = SweptAabb.Sweep(moving, delta, bounds);
        return result.IsHit ? new Candidate(result, bounds) : Candidate.None;
    }

    // A one-way tile is solid only to a body that is on its way down and whose feet are
    // still at or above the tile's top edge. That is the whole rule behind jumping up
    // through a ledge and then landing on it.
    private static bool IsSolidFor(TileCollision collision, Aabb moving, Vector2 delta, Aabb tile)
    {
        if (collision == TileCollision.Solid)
        {
            return true;
        }

        return collision == TileCollision.OneWay
            && delta.Y > 0.0f
            && moving.Bottom <= tile.Top + MathG.Epsilon;
    }

    private readonly struct TileAt
    {
        public TileAt(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public int Column { get; }

        public int Row { get; }
    }

    private readonly struct Candidate
    {
        private readonly Aabb _bounds;
        private readonly SweepResult _result;

        public Candidate(SweepResult result, Aabb bounds)
        {
            _result = result;
            _bounds = bounds;
        }

        public static Candidate None => new(SweepResult.Miss, default);

        public void Apply(ref SweepResult nearest, ref Aabb nearestBounds)
        {
            if (!_result.IsHit || (nearest.IsHit && nearest.Time <= _result.Time))
            {
                return;
            }

            nearest = _result;
            nearestBounds = _bounds;
        }
    }
}
