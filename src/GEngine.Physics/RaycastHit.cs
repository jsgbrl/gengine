// What a ray found. A miss is a value too, with a null body, so a caller never has to
// remember which of the out parameters is meaningful.

using System;
using GEngine.Core;

namespace GEngine.Physics;

/// <summary>Where a ray first met something, and what it met.</summary>
public readonly struct RaycastHit : IEquatable<RaycastHit>
{
    /// <summary>Creates a hit.</summary>
    /// <param name="body">What was hit.</param>
    /// <param name="point">Where the ray touched it, in world pixels.</param>
    /// <param name="normal">Unit vector pointing from the surface back along the ray.</param>
    /// <param name="distance">How far along the ray the touch happened, in pixels.</param>
    public RaycastHit(RigidBody2D body, Vector2 point, Vector2 normal, float distance)
    {
        Body = body;
        Point = point;
        Normal = normal;
        Distance = distance;
    }

    /// <summary>A ray that hit nothing.</summary>
    public static RaycastHit Miss => default;

    /// <summary>What was hit, or null when nothing was.</summary>
    public RigidBody2D? Body { get; }

    /// <summary>Where the ray touched, in world pixels.</summary>
    public Vector2 Point { get; }

    /// <summary>Unit vector pointing from the surface back along the ray.</summary>
    public Vector2 Normal { get; }

    /// <summary>How far along the ray the touch happened, in pixels.</summary>
    public float Distance { get; }

    /// <summary>True when the ray hit something.</summary>
    public bool IsHit => Body is not null;

    /// <summary>Compares two hits field by field.</summary>
    /// <param name="other">The hit to compare with.</param>
    /// <returns>True when they describe the same result.</returns>
    public bool Equals(RaycastHit other) =>
        ReferenceEquals(Body, other.Body)
        && Point.Equals(other.Point)
        && Normal.Equals(other.Normal)
        && Distance.Equals(other.Distance);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is RaycastHit other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Body, Point, Normal, Distance);

    /// <inheritdoc/>
    public override string ToString() => IsHit ? "hit " + Body!.Id + " at " + Point : "miss";

    /// <summary>Equality.</summary>
    /// <param name="left">First hit.</param>
    /// <param name="right">Second hit.</param>
    /// <returns>True when they match.</returns>
    public static bool operator ==(RaycastHit left, RaycastHit right) => left.Equals(right);

    /// <summary>Inequality.</summary>
    /// <param name="left">First hit.</param>
    /// <param name="right">Second hit.</param>
    /// <returns>True when they differ.</returns>
    public static bool operator !=(RaycastHit left, RaycastHit right) => !left.Equals(right);
}
