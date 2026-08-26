// An axis-aligned bounding box: the only collision shape in the engine. Everything in a
// tile-based platformer is a rectangle, and a rectangle-only world buys an exact overlap
// test that a general shape library cannot.

using System;
using System.Globalization;

namespace GEngine.Core;

/// <summary>
/// A rectangle with sides parallel to the axes, stored by its two corners. With the
/// screen-space Y-down axis, <see cref="Min"/> is the top-left corner and
/// <see cref="Max"/> the bottom-right one.
/// </summary>
public readonly struct Aabb : IEquatable<Aabb>
{
    /// <summary>Top-left corner: the smallest X and the smallest Y.</summary>
    public readonly Vector2 Min;

    /// <summary>Bottom-right corner: the largest X and the largest Y.</summary>
    public readonly Vector2 Max;

    /// <summary>Creates a box from its two corners.</summary>
    /// <param name="min">Top-left corner.</param>
    /// <param name="max">Bottom-right corner.</param>
    public Aabb(Vector2 min, Vector2 max)
    {
        Min = min;
        Max = max;
    }

    /// <summary>Creates a box from a centre and a full size.</summary>
    /// <param name="center">Centre of the box.</param>
    /// <param name="size">Width and height.</param>
    /// <returns>The box.</returns>
    public static Aabb FromCenterSize(Vector2 center, Vector2 size)
    {
        Vector2 half = size * 0.5f;
        return new Aabb(center - half, center + half);
    }

    /// <summary>Creates a box from any two opposite corners, in any order.</summary>
    /// <param name="first">One corner.</param>
    /// <param name="second">The opposite corner.</param>
    /// <returns>The box, with its corners sorted.</returns>
    public static Aabb FromCorners(Vector2 first, Vector2 second) =>
        new(Vector2.Min(first, second), Vector2.Max(first, second));

    /// <summary>Centre of the box.</summary>
    public Vector2 Center => (Min + Max) * 0.5f;

    /// <summary>Width and height.</summary>
    public Vector2 Size => Max - Min;

    /// <summary>Half the width and half the height.</summary>
    public Vector2 HalfSize => Size * 0.5f;

    /// <summary>Smallest X.</summary>
    public float Left => Min.X;

    /// <summary>Largest X.</summary>
    public float Right => Max.X;

    /// <summary>Smallest Y, which on a Y-down axis is the top edge.</summary>
    public float Top => Min.Y;

    /// <summary>Largest Y, which on a Y-down axis is the bottom edge.</summary>
    public float Bottom => Max.Y;

    /// <summary>
    /// True when the two boxes share an area. Boxes that merely touch along an edge do
    /// not intersect: a body resting exactly on the floor has to read as on it, not as
    /// inside it, or the solver would push it out again every single frame.
    /// </summary>
    /// <param name="other">The other box.</param>
    /// <returns>True when the overlap has a positive area.</returns>
    public bool Intersects(Aabb other) =>
        Min.X < other.Max.X && Max.X > other.Min.X && Min.Y < other.Max.Y && Max.Y > other.Min.Y;

    /// <summary>True when the two boxes overlap or share an edge.</summary>
    /// <param name="other">The other box.</param>
    /// <returns>True when they are not strictly apart.</returns>
    public bool Touches(Aabb other) =>
        Min.X <= other.Max.X && Max.X >= other.Min.X && Min.Y <= other.Max.Y && Max.Y >= other.Min.Y;

    /// <summary>True when the point is inside the box or on its edge.</summary>
    /// <param name="point">The point.</param>
    /// <returns>True when the point is not outside.</returns>
    public bool Contains(Vector2 point) =>
        point.X >= Min.X && point.X <= Max.X && point.Y >= Min.Y && point.Y <= Max.Y;

    /// <summary>The same box moved by an offset.</summary>
    /// <param name="offset">How far to move it.</param>
    /// <returns>The moved box.</returns>
    public Aabb Translated(Vector2 offset) => new(Min + offset, Max + offset);

    /// <summary>The same box grown by an amount on every side.</summary>
    /// <param name="amount">How much to add to each side, per axis.</param>
    /// <returns>The grown box.</returns>
    public Aabb Expanded(Vector2 amount) => new(Min - amount, Max + amount);

    /// <summary>The smallest box containing both.</summary>
    /// <param name="other">The other box.</param>
    /// <returns>The union.</returns>
    public Aabb Union(Aabb other) => new(Vector2.Min(Min, other.Min), Vector2.Max(Max, other.Max));

    /// <summary>The point of the box nearest a given point, which is the point itself when inside.</summary>
    /// <param name="point">The point.</param>
    /// <returns>The nearest point on or in the box.</returns>
    public Vector2 ClosestPoint(Vector2 point) => Vector2.Min(Vector2.Max(point, Min), Max);

    /// <summary>Exact equality of both corners.</summary>
    /// <param name="other">The box to compare with.</param>
    /// <returns>True when both corners match bit for bit.</returns>
    public bool Equals(Aabb other) => Min.Equals(other.Min) && Max.Equals(other.Max);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Aabb other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Min, Max);

    /// <inheritdoc/>
    public override string ToString() =>
        string.Format(CultureInfo.InvariantCulture, "[{0} .. {1}]", Min, Max);

    /// <summary>Exact equality.</summary>
    /// <param name="left">First box.</param>
    /// <param name="right">Second box.</param>
    /// <returns>True when both corners match.</returns>
    public static bool operator ==(Aabb left, Aabb right) => left.Equals(right);

    /// <summary>Exact inequality.</summary>
    /// <param name="left">First box.</param>
    /// <param name="right">Second box.</param>
    /// <returns>True when a corner differs.</returns>
    public static bool operator !=(Aabb left, Aabb right) => !left.Equals(right);
}
