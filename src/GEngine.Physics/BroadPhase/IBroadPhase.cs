// Strategy. Two implementations, one interface, and a test that pins them to the same
// answer: the whole point of the pair is to show that an optimisation is only allowed to
// change how long something takes, never what it says.

using System.Collections.Generic;
using GEngine.Core;

namespace GEngine.Physics.BroadPhase;

/// <summary>Finds which pairs of bodies are worth testing, and answers area queries.</summary>
public interface IBroadPhase
{
    /// <summary>Name used in diagnostics and in the example that compares the two.</summary>
    string Name { get; }

    /// <summary>Rebuilds whatever index this implementation keeps. Called once per step.</summary>
    /// <param name="bodies">Every body in the world, in identity order.</param>
    void Update(IReadOnlyList<RigidBody2D> bodies);

    /// <summary>Appends every reportable pair, ordered by identity.</summary>
    /// <param name="results">Buffer to append to. It is cleared first.</param>
    void FindPairs(List<BroadPhasePair> results);

    /// <summary>Appends every body whose box overlaps an area, ordered by identity.</summary>
    /// <param name="area">The area to search.</param>
    /// <param name="results">Buffer to append to. It is cleared first.</param>
    void Query(Aabb area, List<RigidBody2D> results);
}
