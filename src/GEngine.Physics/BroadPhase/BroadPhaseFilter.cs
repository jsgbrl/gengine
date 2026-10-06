// The one place that decides whether two bodies are worth a closer look.
//
// Both broad phases call it, which is what lets a test assert that the fast one and the slow
// one return exactly the same set: if they disagreed, the disagreement could only be in the
// acceleration structure, which is precisely what the test is there to catch.

using System;

namespace GEngine.Physics.BroadPhase;

/// <summary>Decides which pairs of bodies a broad phase reports.</summary>
public static class BroadPhaseFilter
{
    /// <summary>
    /// Whether two bodies should be reported as a pair. In a world where every shape is an
    /// axis-aligned box, the exact overlap test costs four comparisons, so the broad phase
    /// can afford to run it and hand the solver pairs that really are touching.
    /// </summary>
    /// <param name="first">One body.</param>
    /// <param name="second">The other body.</param>
    /// <returns>True when the pair should be reported.</returns>
    public static bool ShouldPair(RigidBody2D first, RigidBody2D second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        if (first.Type != BodyType.Dynamic && second.Type != BodyType.Dynamic)
        {
            return false;
        }

        return first.CollidesWith(second) && first.Bounds.Intersects(second.Bounds);
    }

    /// <summary>Orders two pairs by identity, so a list of pairs sorts reproducibly.</summary>
    /// <param name="left">First pair.</param>
    /// <param name="right">Second pair.</param>
    /// <returns>Negative, zero or positive in the usual comparison sense.</returns>
    public static int Compare(BroadPhasePair left, BroadPhasePair right) => left.Key.CompareTo(right.Key);
}
