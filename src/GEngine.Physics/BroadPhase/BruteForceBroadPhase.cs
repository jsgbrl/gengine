// Every pair, tested. Quadratic, obviously too slow for a real level, and kept anyway:
// it is the definition of the right answer that SpatialHashGrid is measured against, and
// with a handful of bodies it is genuinely the faster of the two.

using System;
using System.Collections.Generic;
using GEngine.Core;

namespace GEngine.Physics.BroadPhase;

/// <summary>The reference broad phase: it compares every body with every other body.</summary>
public sealed class BruteForceBroadPhase : IBroadPhase
{
    private IReadOnlyList<RigidBody2D> _bodies = [];

    /// <inheritdoc/>
    public string Name => "brute force";

    /// <summary>How many pair comparisons the last call to <see cref="FindPairs"/> made.</summary>
    public int LastComparisonCount { get; private set; }

    /// <inheritdoc/>
    public void Update(IReadOnlyList<RigidBody2D> bodies)
    {
        _bodies = bodies;
    }

    /// <inheritdoc/>
    public void FindPairs(List<BroadPhasePair> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        results.Clear();
        LastComparisonCount = 0;
        for (int first = 0; first < _bodies.Count; first++)
        {
            AddPairsWith(first, results);
        }

        results.Sort(BroadPhaseFilter.Compare);
    }

    /// <inheritdoc/>
    public void Query(Aabb area, List<RigidBody2D> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        results.Clear();
        foreach (RigidBody2D body in _bodies)
        {
            if (body.Bounds.Intersects(area))
            {
                results.Add(body);
            }
        }
    }

    private void AddPairsWith(int first, List<BroadPhasePair> results)
    {
        for (int second = first + 1; second < _bodies.Count; second++)
        {
            LastComparisonCount++;
            if (BroadPhaseFilter.ShouldPair(_bodies[first], _bodies[second]))
            {
                results.Add(BroadPhasePair.Ordered(_bodies[first], _bodies[second]));
            }
        }
    }
}
