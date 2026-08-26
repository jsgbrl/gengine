// Asking the world questions between steps: what is along this ray, what is in this box,
// what is under this point - plus the hash a determinism test compares two runs with.
//
// Every query re-files the bodies into the broad phase before it searches. That makes a
// query linear in the number of bodies rather than logarithmic, and it is the right trade:
// queries happen a handful of times a frame, the step happens once, and a query that
// silently used a stale index would be a bug that only shows up in the level nobody tested.

using System;
using System.Collections.Generic;
using GEngine.Core;
using GEngine.Physics.NarrowPhase;

namespace GEngine.Physics;

/// <content>Queries and the state hash.</content>
public sealed partial class PhysicsWorld
{
    /// <summary>Finds the nearest body along a ray.</summary>
    /// <param name="origin">Where the ray starts, in world pixels.</param>
    /// <param name="direction">Which way it points. Need not be a unit vector.</param>
    /// <param name="distance">How far to look, in pixels.</param>
    /// <param name="hit">What was found, or a miss.</param>
    /// <returns>True when the ray hit something.</returns>
    public bool Raycast(Vector2 origin, Vector2 direction, float distance, out RaycastHit hit)
    {
        Vector2 delta = direction.Normalized() * distance;
        BroadPhase.Update(_bodies);
        BroadPhase.Query(Aabb.FromCorners(origin, origin + delta), _candidates);
        hit = RaycastHit.Miss;
        float nearest = float.MaxValue;
        foreach (RigidBody2D body in _candidates)
        {
            SweepResult result = SweptAabb.Cast(origin, delta, body.Bounds);
            if (result.IsHit && result.Time * distance < nearest)
            {
                nearest = result.Time * distance;
                hit = new RaycastHit(body, origin + (delta * result.Time), result.Normal, nearest);
            }
        }

        return hit.IsHit;
    }

    /// <summary>Appends every body whose box overlaps an area, in identity order.</summary>
    /// <param name="area">The area to search.</param>
    /// <param name="layerMask">Which layers to accept, as a bitmask.</param>
    /// <param name="results">Buffer to append to. It is cleared first.</param>
    public void OverlapBox(Aabb area, int layerMask, List<RigidBody2D> results)
    {
        ArgumentNullException.ThrowIfNull(results);
        BroadPhase.Update(_bodies);
        BroadPhase.Query(area, results);
        for (int index = results.Count - 1; index >= 0; index--)
        {
            if ((results[index].Layer & layerMask) == 0)
            {
                results.RemoveAt(index);
            }
        }
    }

    /// <summary>The lowest-identity body containing a point, or null.</summary>
    /// <param name="point">The point, in world pixels.</param>
    /// <param name="layerMask">Which layers to accept, as a bitmask.</param>
    /// <returns>The body, or null when the point is in empty space.</returns>
    public RigidBody2D? QueryPoint(Vector2 point, int layerMask)
    {
        foreach (RigidBody2D body in _bodies)
        {
            if ((body.Layer & layerMask) != 0 && body.Bounds.Contains(point))
            {
                return body;
            }
        }

        return null;
    }

    /// <summary>Refiles every body into the broad phase without stepping the simulation.</summary>
    public void Refresh() => BroadPhase.Update(_bodies);

    /// <summary>
    /// A hash of every position and velocity in the world, in identity order. Two runs fed
    /// the same input must produce the same hash after the same number of steps; comparing
    /// one number is what makes that assertion writable at all.
    /// </summary>
    /// <returns>The hash.</returns>
    /// <remarks>
    /// FNV-1a over the raw float bits, not <see cref="HashCode"/>: the framework hash is
    /// seeded randomly at process start, so it would agree with itself inside one run and
    /// disagree with the same run tomorrow.
    /// </remarks>
    public long StateHash()
    {
        ulong hash = 14695981039346656037UL;
        foreach (RigidBody2D body in _bodies)
        {
            hash = Mix(hash, BitConverter.SingleToUInt32Bits(body.Position.X));
            hash = Mix(hash, BitConverter.SingleToUInt32Bits(body.Position.Y));
            hash = Mix(hash, BitConverter.SingleToUInt32Bits(body.Velocity.X));
            hash = Mix(hash, BitConverter.SingleToUInt32Bits(body.Velocity.Y));
        }

        return (long)hash;
    }

    private static ulong Mix(ulong hash, uint value)
    {
        for (int shift = 0; shift < 32; shift += 8)
        {
            hash = (hash ^ ((value >> shift) & 0xFF)) * 1099511628211UL;
        }

        return hash;
    }
}
