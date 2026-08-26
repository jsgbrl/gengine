// The two numbers everything else is built out of: a position, a velocity, a size, an
// offset. Kept as a readonly struct of two floats so an array of them is an array of
// numbers, with no indirection and nothing for the collector to trace.

using System;
using System.Globalization;

namespace GEngine.Core;

/// <summary>
/// A two-dimensional vector of single-precision floats. Axes follow the screen: X grows
/// to the right and Y grows <em>downwards</em>, which is why gravity is a positive Y.
/// </summary>
public readonly partial struct Vector2 : IEquatable<Vector2>
{
    /// <summary>Both components zero.</summary>
    public static readonly Vector2 Zero = new(0.0f, 0.0f);

    /// <summary>Both components one.</summary>
    public static readonly Vector2 One = new(1.0f, 1.0f);

    /// <summary>One unit to the right.</summary>
    public static readonly Vector2 UnitX = new(1.0f, 0.0f);

    /// <summary>One unit down the screen.</summary>
    public static readonly Vector2 UnitY = new(0.0f, 1.0f);

    /// <summary>Horizontal component, growing to the right.</summary>
    public readonly float X;

    /// <summary>Vertical component, growing downwards.</summary>
    public readonly float Y;

    /// <summary>Creates a vector from its components.</summary>
    /// <param name="x">Horizontal component.</param>
    /// <param name="y">Vertical component.</param>
    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    /// <summary>Squared length. Prefer it to <see cref="Length"/> when comparing distances: no square root.</summary>
    public float LengthSquared => (X * X) + (Y * Y);

    /// <summary>Length of the vector.</summary>
    public float Length => MathF.Sqrt(LengthSquared);

    /// <summary>A copy with a different horizontal component.</summary>
    /// <param name="x">The new horizontal component.</param>
    /// <returns>The new vector.</returns>
    public Vector2 WithX(float x) => new(x, Y);

    /// <summary>A copy with a different vertical component.</summary>
    /// <param name="y">The new vertical component.</param>
    /// <returns>The new vector.</returns>
    public Vector2 WithY(float y) => new(X, y);

    /// <summary>
    /// A vector in the same direction with length one. Normalising the zero vector has no
    /// meaningful answer, so this returns <see cref="Zero"/> rather than a pair of NaNs
    /// that would silently poison every later frame.
    /// </summary>
    /// <returns>The unit vector, or zero.</returns>
    public Vector2 Normalized()
    {
        float lengthSquared = LengthSquared;
        if (lengthSquared <= MathG.Epsilon * MathG.Epsilon)
        {
            return Zero;
        }

        float length = MathF.Sqrt(lengthSquared);
        return new Vector2(X / length, Y / length);
    }

    /// <summary>Compares two vectors allowing for accumulated floating-point error.</summary>
    /// <param name="other">The vector to compare with.</param>
    /// <param name="epsilon">Largest accepted difference per axis.</param>
    /// <returns>True when both axes agree within the epsilon.</returns>
    public bool ApproximatelyEquals(Vector2 other, float epsilon = MathG.Epsilon) =>
        MathG.Approximately(X, other.X, epsilon) && MathG.Approximately(Y, other.Y, epsilon);

    /// <summary>Exact component-wise equality.</summary>
    /// <param name="other">The vector to compare with.</param>
    /// <returns>True when both components match bit for bit.</returns>
    public bool Equals(Vector2 other) => X.Equals(other.X) && Y.Equals(other.Y);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Vector2 other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(X, Y);

    /// <inheritdoc/>
    public override string ToString() =>
        string.Format(CultureInfo.InvariantCulture, "({0:0.###}, {1:0.###})", X, Y);
}
