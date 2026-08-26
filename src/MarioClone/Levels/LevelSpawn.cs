// One thing to create, and where. Stored in tile coordinates because that is what the level
// file is written in; the loader converts to pixels once, at the edge.

using System;

namespace MarioClone.Levels;

/// <summary>One entity the level asks for, at one grid position.</summary>
public readonly struct LevelSpawn : IEquatable<LevelSpawn>
{
    /// <summary>Creates a spawn.</summary>
    /// <param name="kind">What to create.</param>
    /// <param name="column">Zero-based column of the grid.</param>
    /// <param name="row">Zero-based row of the grid.</param>
    public LevelSpawn(SpawnKind kind, int column, int row)
    {
        Kind = kind;
        Column = column;
        Row = row;
    }

    /// <summary>What to create.</summary>
    public SpawnKind Kind { get; }

    /// <summary>Zero-based column of the grid.</summary>
    public int Column { get; }

    /// <summary>Zero-based row of the grid.</summary>
    public int Row { get; }

    /// <summary>Compares two spawns.</summary>
    /// <param name="other">The spawn to compare with.</param>
    /// <returns>True when they ask for the same thing in the same place.</returns>
    public bool Equals(LevelSpawn other) => Kind == other.Kind && Column == other.Column && Row == other.Row;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is LevelSpawn other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, Column, Row);

    /// <inheritdoc/>
    public override string ToString() => Kind + " at " + Column + "," + Row;

    /// <summary>Equality.</summary>
    /// <param name="left">First spawn.</param>
    /// <param name="right">Second spawn.</param>
    /// <returns>True when they match.</returns>
    public static bool operator ==(LevelSpawn left, LevelSpawn right) => left.Equals(right);

    /// <summary>Inequality.</summary>
    /// <param name="left">First spawn.</param>
    /// <param name="right">Second spawn.</param>
    /// <returns>True when they differ.</returns>
    public static bool operator !=(LevelSpawn left, LevelSpawn right) => !left.Equals(right);
}
