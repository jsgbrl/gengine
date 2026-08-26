// Two bodies that might be touching. Always stored with the lower Id first, so the same
// pair found twice - which is what happens when two boxes share more than one grid cell -
// is recognisably the same pair.

using System;

namespace GEngine.Physics.BroadPhase;

/// <summary>An ordered pair of bodies the narrow phase should look at.</summary>
public readonly struct BroadPhasePair : IEquatable<BroadPhasePair>
{
    private BroadPhasePair(RigidBody2D first, RigidBody2D second)
    {
        First = first;
        Second = second;
    }

    /// <summary>The body with the lower identity.</summary>
    public RigidBody2D First { get; }

    /// <summary>The body with the higher identity.</summary>
    public RigidBody2D Second { get; }

    /// <summary>Both identities packed into one number, for cheap duplicate detection.</summary>
    public long Key => ((long)First.Id << 32) | (uint)Second.Id;

    /// <summary>Builds a pair with the lower identity first, whichever way round it was given.</summary>
    /// <param name="first">One body.</param>
    /// <param name="second">The other body.</param>
    /// <returns>The ordered pair.</returns>
    public static BroadPhasePair Ordered(RigidBody2D first, RigidBody2D second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        return first.Id <= second.Id ? new BroadPhasePair(first, second) : new BroadPhasePair(second, first);
    }

    /// <summary>Compares two pairs by their bodies.</summary>
    /// <param name="other">The pair to compare with.</param>
    /// <returns>True when both hold the same two bodies.</returns>
    public bool Equals(BroadPhasePair other) =>
        ReferenceEquals(First, other.First) && ReferenceEquals(Second, other.Second);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is BroadPhasePair other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => Key.GetHashCode();

    /// <inheritdoc/>
    public override string ToString() => First.Id + " with " + Second.Id;

    /// <summary>Equality.</summary>
    /// <param name="left">First pair.</param>
    /// <param name="right">Second pair.</param>
    /// <returns>True when they hold the same two bodies.</returns>
    public static bool operator ==(BroadPhasePair left, BroadPhasePair right) => left.Equals(right);

    /// <summary>Inequality.</summary>
    /// <param name="left">First pair.</param>
    /// <param name="right">Second pair.</param>
    /// <returns>True when they differ.</returns>
    public static bool operator !=(BroadPhasePair left, BroadPhasePair right) => !left.Equals(right);
}
