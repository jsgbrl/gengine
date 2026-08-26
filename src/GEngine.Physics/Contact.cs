// One touch between two bodies, as a value. It is a struct and not a class because the
// solver produces one per contact per step and the engine promises no allocations in a
// steady frame.

using System;
using GEngine.Core;

namespace GEngine.Physics;

/// <summary>A collision or overlap between two bodies, seen from one of them.</summary>
public readonly struct Contact : IEquatable<Contact>
{
    /// <summary>Creates a contact.</summary>
    /// <param name="self">The body the callback belongs to.</param>
    /// <param name="other">The body it touched.</param>
    /// <param name="normal">Unit vector pointing from <paramref name="other"/> towards <paramref name="self"/>.</param>
    /// <param name="penetration">How deep the overlap was, in pixels, before it was resolved.</param>
    public Contact(RigidBody2D self, RigidBody2D other, Vector2 normal, float penetration)
    {
        Self = self;
        Other = other;
        Normal = normal;
        Penetration = penetration;
    }

    /// <summary>The body whose callback is running.</summary>
    public RigidBody2D Self { get; }

    /// <summary>The body it touched.</summary>
    public RigidBody2D Other { get; }

    /// <summary>
    /// Unit vector pointing from the other body towards this one, so landing on a floor
    /// gives (0, -1) on the screen-space Y-down axis. Stomping a goomba is exactly the
    /// test "the normal points up and I was falling".
    /// </summary>
    public Vector2 Normal { get; }

    /// <summary>How deep the overlap was, in pixels, before the solver separated them.</summary>
    public float Penetration { get; }

    /// <summary>The same contact seen from the other body.</summary>
    /// <returns>A contact with the two bodies swapped and the normal reversed.</returns>
    public Contact Flipped() => new(Other, Self, -Normal, Penetration);

    /// <summary>Compares two contacts by their bodies, normal and depth.</summary>
    /// <param name="other">The contact to compare with.</param>
    /// <returns>True when both describe the same touch.</returns>
    public bool Equals(Contact other) =>
        ReferenceEquals(Self, other.Self)
        && ReferenceEquals(Other, other.Other)
        && Normal.Equals(other.Normal)
        && Penetration.Equals(other.Penetration);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Contact other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Self, Other, Normal, Penetration);

    /// <inheritdoc/>
    public override string ToString() => Self.Id + " touched " + Other.Id + " along " + Normal;

    /// <summary>Equality.</summary>
    /// <param name="left">First contact.</param>
    /// <param name="right">Second contact.</param>
    /// <returns>True when both describe the same touch.</returns>
    public static bool operator ==(Contact left, Contact right) => left.Equals(right);

    /// <summary>Inequality.</summary>
    /// <param name="left">First contact.</param>
    /// <param name="right">Second contact.</param>
    /// <returns>True when they differ.</returns>
    public static bool operator !=(Contact left, Contact right) => !left.Equals(right);
}
