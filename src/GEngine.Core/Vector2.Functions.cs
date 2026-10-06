// Free functions over vectors. They are static because none of them belongs to one of
// the two operands: Distance(a, b) reads better than a.DistanceTo(b) and says out loud
// that the operation is symmetric.

using System;

namespace GEngine.Core;

/// <content>Operations that combine two vectors rather than modify one.</content>
public readonly partial struct Vector2
{
    /// <summary>Dot product: the length of one vector projected onto the other.</summary>
    /// <param name="left">First vector.</param>
    /// <param name="right">Second vector.</param>
    /// <returns>The dot product.</returns>
    public static float Dot(Vector2 left, Vector2 right) => (left.X * right.X) + (left.Y * right.Y);

    /// <summary>Two-dimensional cross product: the signed area of the parallelogram.</summary>
    /// <param name="left">First vector.</param>
    /// <param name="right">Second vector.</param>
    /// <returns>Positive when right turns clockwise from left on a screen-space Y-down axis.</returns>
    public static float Cross(Vector2 left, Vector2 right) => (left.X * right.Y) - (left.Y * right.X);

    /// <summary>Distance between two points.</summary>
    /// <param name="from">First point.</param>
    /// <param name="to">Second point.</param>
    /// <returns>The distance.</returns>
    public static float Distance(Vector2 from, Vector2 to) => (to - from).Length;

    /// <summary>Squared distance between two points, without the square root.</summary>
    /// <param name="from">First point.</param>
    /// <param name="to">Second point.</param>
    /// <returns>The squared distance.</returns>
    public static float DistanceSquared(Vector2 from, Vector2 to) => (to - from).LengthSquared;

    /// <summary>Straight-line interpolation between two vectors.</summary>
    /// <param name="from">Value at amount zero.</param>
    /// <param name="to">Value at amount one.</param>
    /// <param name="amount">Position between the two, clamped to zero and one.</param>
    /// <returns>The interpolated vector.</returns>
    public static Vector2 Lerp(Vector2 from, Vector2 to, float amount) =>
        new(MathG.Lerp(from.X, to.X, amount), MathG.Lerp(from.Y, to.Y, amount));

    /// <summary>Component-wise minimum.</summary>
    /// <param name="left">First vector.</param>
    /// <param name="right">Second vector.</param>
    /// <returns>The smaller component of each axis.</returns>
    public static Vector2 Min(Vector2 left, Vector2 right) =>
        new(MathF.Min(left.X, right.X), MathF.Min(left.Y, right.Y));

    /// <summary>Component-wise maximum.</summary>
    /// <param name="left">First vector.</param>
    /// <param name="right">Second vector.</param>
    /// <returns>The larger component of each axis.</returns>
    public static Vector2 Max(Vector2 left, Vector2 right) =>
        new(MathF.Max(left.X, right.X), MathF.Max(left.Y, right.Y));

    /// <summary>Component-wise absolute value.</summary>
    /// <param name="value">The vector to take the absolute value of.</param>
    /// <returns>Both components made positive.</returns>
    public static Vector2 Abs(Vector2 value) => new(MathF.Abs(value.X), MathF.Abs(value.Y));

    /// <summary>Shortens the vector to a maximum length, leaving shorter ones alone.</summary>
    /// <param name="value">The vector to shorten.</param>
    /// <param name="maximumLength">Largest length allowed.</param>
    /// <returns>The clamped vector.</returns>
    public static Vector2 ClampLength(Vector2 value, float maximumLength)
    {
        float lengthSquared = value.LengthSquared;
        if (lengthSquared <= maximumLength * maximumLength)
        {
            return value;
        }

        return value.Normalized() * maximumLength;
    }
}
