// The answer to "if this box moves along this vector, what does it hit first?"

using System;
using System.Globalization;
using GEngine.Core;

namespace GEngine.Physics.NarrowPhase;

/// <summary>Where along a movement a box first touches an obstacle, and from which side.</summary>
public readonly struct SweepResult : IEquatable<SweepResult>
{
    private SweepResult(bool isHit, float time, Vector2 normal)
    {
        IsHit = isHit;
        Time = time;
        Normal = normal;
    }

    /// <summary>Nothing was in the way.</summary>
    public static SweepResult Miss => new(false, 1.0f, Vector2.Zero);

    /// <summary>True when the movement was interrupted.</summary>
    public bool IsHit { get; }

    /// <summary>How far along the movement the touch happened, from zero to one.</summary>
    public float Time { get; }

    /// <summary>Unit vector pointing from the obstacle back towards the moving box.</summary>
    public Vector2 Normal { get; }

    /// <summary>Builds a hit.</summary>
    /// <param name="time">How far along the movement, from zero to one.</param>
    /// <param name="normal">Unit vector pointing back towards the moving box.</param>
    /// <returns>The hit.</returns>
    public static SweepResult Hit(float time, Vector2 normal) => new(true, time, normal);

    /// <summary>Compares two results field by field.</summary>
    /// <param name="other">The result to compare with.</param>
    /// <returns>True when they describe the same outcome.</returns>
    public bool Equals(SweepResult other) =>
        IsHit == other.IsHit && Time.Equals(other.Time) && Normal.Equals(other.Normal);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is SweepResult other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(IsHit, Time, Normal);

    /// <inheritdoc/>
    public override string ToString() => IsHit
        ? string.Format(CultureInfo.InvariantCulture, "hit at {0:0.###} along {1}", Time, Normal)
        : "miss";

    /// <summary>Equality.</summary>
    /// <param name="left">First result.</param>
    /// <param name="right">Second result.</param>
    /// <returns>True when they match.</returns>
    public static bool operator ==(SweepResult left, SweepResult right) => left.Equals(right);

    /// <summary>Inequality.</summary>
    /// <param name="left">First result.</param>
    /// <param name="right">Second result.</param>
    /// <returns>True when they differ.</returns>
    public static bool operator !=(SweepResult left, SweepResult right) => !left.Equals(right);
}
