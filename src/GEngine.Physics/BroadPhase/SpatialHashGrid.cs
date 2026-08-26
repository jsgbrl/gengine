// The broad phase the game uses. Bodies are filed into square cells by their box, and only
// bodies sharing a cell are ever compared - so the cost stops depending on how many bodies
// exist and starts depending on how many are near each other.
//
// Two details make it work rather than merely look clever. A box that straddles a boundary
// goes into every cell it touches, so nothing is missed at the seams; and a pair found in
// two cells at once has to be recognised as one pair, which is what the seen set is for.

using System;
using System.Collections.Generic;
using GEngine.Core;

namespace GEngine.Physics.BroadPhase;

/// <summary>A uniform grid broad phase. Same answers as brute force, far fewer comparisons.</summary>
public sealed class SpatialHashGrid : IBroadPhase
{
    /// <summary>Cell size used when none is given: wide enough for a few tiles.</summary>
    public const float DefaultCellSize = 64.0f;

    private readonly Dictionary<long, List<RigidBody2D>> _cells = [];
    private readonly HashSet<long> _seen = [];
    private readonly float _cellSize;

    /// <summary>Creates a grid.</summary>
    /// <param name="cellSize">Width and height of one cell, in pixels.</param>
    /// <exception cref="ArgumentOutOfRangeException">The cell size is not positive.</exception>
    public SpatialHashGrid(float cellSize = DefaultCellSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(cellSize);
        _cellSize = cellSize;
    }

    /// <inheritdoc/>
    public string Name => "spatial hash";

    /// <summary>How many cells currently hold at least one body.</summary>
    public int OccupiedCellCount => CountOccupied();

    /// <summary>How many pair comparisons the last call to <see cref="FindPairs"/> made.</summary>
    public int LastComparisonCount { get; private set; }

    /// <inheritdoc/>
    public void Update(IReadOnlyList<RigidBody2D> bodies)
    {
        ArgumentNullException.ThrowIfNull(bodies);
        foreach (List<RigidBody2D> cell in _cells.Values)
        {
            cell.Clear();
        }

        foreach (RigidBody2D body in bodies)
        {
            Insert(body);
        }
    }

    /// <inheritdoc/>
    public void FindPairs(List<BroadPhasePair> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        results.Clear();
        _seen.Clear();
        LastComparisonCount = 0;
        foreach (List<RigidBody2D> cell in _cells.Values)
        {
            AddPairsIn(cell, results);
        }

        results.Sort(BroadPhaseFilter.Compare);
    }

    /// <inheritdoc/>
    public void Query(Aabb area, List<RigidBody2D> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        results.Clear();
        _seen.Clear();
        CellRange range = RangeOf(area);
        for (int column = range.FirstColumn; column <= range.LastColumn; column++)
        {
            QueryRows(column, range, area, results);
        }

        results.Sort(static (left, right) => left.Id.CompareTo(right.Id));
    }

    // Written as plain nested loops rather than a callback taking a lambda: a lambda that
    // captures the body would allocate a closure for every body, every step, and this is
    // the one place in the engine where that would happen sixty times a second.
    private void Insert(RigidBody2D body)
    {
        CellRange range = RangeOf(body.Bounds);
        for (int column = range.FirstColumn; column <= range.LastColumn; column++)
        {
            for (int row = range.FirstRow; row <= range.LastRow; row++)
            {
                CellAt(column, row).Add(body);
            }
        }
    }

    private void QueryRows(int column, CellRange range, Aabb area, List<RigidBody2D> results)
    {
        for (int row = range.FirstRow; row <= range.LastRow; row++)
        {
            AddOverlapping(CellAt(column, row), area, results);
        }
    }

    private CellRange RangeOf(Aabb area) => new(
        MathG.FloorToInt(area.Min.X / _cellSize),
        MathG.FloorToInt(area.Max.X / _cellSize),
        MathG.FloorToInt(area.Min.Y / _cellSize),
        MathG.FloorToInt(area.Max.Y / _cellSize));

    private List<RigidBody2D> CellAt(int column, int row)
    {
        long key = ((long)column << 32) | (uint)row;
        if (!_cells.TryGetValue(key, out List<RigidBody2D>? cell))
        {
            cell = [];
            _cells[key] = cell;
        }

        return cell;
    }

    private void AddPairsIn(List<RigidBody2D> cell, List<BroadPhasePair> results)
    {
        for (int first = 0; first < cell.Count; first++)
        {
            AddPairsWith(cell, first, results);
        }
    }

    private void AddPairsWith(List<RigidBody2D> cell, int first, List<BroadPhasePair> results)
    {
        for (int second = first + 1; second < cell.Count; second++)
        {
            LastComparisonCount++;
            TryAdd(cell[first], cell[second], results);
        }
    }

    private void TryAdd(RigidBody2D first, RigidBody2D second, List<BroadPhasePair> results)
    {
        if (!BroadPhaseFilter.ShouldPair(first, second))
        {
            return;
        }

        var pair = BroadPhasePair.Ordered(first, second);
        if (_seen.Add(pair.Key))
        {
            results.Add(pair);
        }
    }

    private void AddOverlapping(List<RigidBody2D> cell, Aabb area, List<RigidBody2D> results)
    {
        foreach (RigidBody2D body in cell)
        {
            if (body.Bounds.Intersects(area) && _seen.Add(body.Id))
            {
                results.Add(body);
            }
        }
    }

    private readonly struct CellRange
    {
        public CellRange(int firstColumn, int lastColumn, int firstRow, int lastRow)
        {
            FirstColumn = firstColumn;
            LastColumn = lastColumn;
            FirstRow = firstRow;
            LastRow = lastRow;
        }

        public int FirstColumn { get; }

        public int LastColumn { get; }

        public int FirstRow { get; }

        public int LastRow { get; }
    }

    private int CountOccupied()
    {
        int occupied = 0;
        foreach (List<RigidBody2D> cell in _cells.Values)
        {
            occupied += cell.Count > 0 ? 1 : 0;
        }

        return occupied;
    }
}
