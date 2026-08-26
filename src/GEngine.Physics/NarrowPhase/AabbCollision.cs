// Discrete overlap between two boxes, and the shortest push that separates them. This is
// what resolves a body that is already inside another one - a spawn on top of a wall, or a
// platform that moved into a passenger - before anything is allowed to move.

using System;
using GEngine.Core;

namespace GEngine.Physics.NarrowPhase;

/// <summary>Overlap tests between two axis-aligned boxes.</summary>
public static class AabbCollision
{
    /// <summary>
    /// Finds the shortest vector that pushes one box out of another: the minimum
    /// translation vector. Both axes are measured and the smaller one wins, which is why a
    /// player who has sunk one pixel into the floor is lifted one pixel rather than being
    /// thrown sideways across the level.
    /// </summary>
    /// <param name="moving">The box to push out.</param>
    /// <param name="obstacle">The box it is inside.</param>
    /// <param name="translation">What to add to the moving box to separate them.</param>
    /// <returns>False when the boxes do not overlap, in which case the translation is zero.</returns>
    public static bool TryGetMinimumTranslation(Aabb moving, Aabb obstacle, out Vector2 translation)
    {
        float overlapX = MathF.Min(moving.Max.X, obstacle.Max.X) - MathF.Max(moving.Min.X, obstacle.Min.X);
        float overlapY = MathF.Min(moving.Max.Y, obstacle.Max.Y) - MathF.Max(moving.Min.Y, obstacle.Min.Y);
        if (overlapX <= 0.0f || overlapY <= 0.0f)
        {
            translation = Vector2.Zero;
            return false;
        }

        translation = overlapX < overlapY
            ? new Vector2(SignedOverlap(moving.Center.X, obstacle.Center.X, overlapX), 0.0f)
            : new Vector2(0.0f, SignedOverlap(moving.Center.Y, obstacle.Center.Y, overlapY));
        return true;
    }

    /// <summary>How deep two boxes overlap on each axis, or zero where they do not.</summary>
    /// <param name="first">First box.</param>
    /// <param name="second">Second box.</param>
    /// <returns>Overlap per axis, never negative.</returns>
    public static Vector2 OverlapDepth(Aabb first, Aabb second)
    {
        float overlapX = MathF.Min(first.Max.X, second.Max.X) - MathF.Max(first.Min.X, second.Min.X);
        float overlapY = MathF.Min(first.Max.Y, second.Max.Y) - MathF.Max(first.Min.Y, second.Min.Y);
        return new Vector2(MathF.Max(overlapX, 0.0f), MathF.Max(overlapY, 0.0f));
    }

    private static float SignedOverlap(float movingCenter, float obstacleCenter, float overlap) =>
        movingCenter < obstacleCenter ? -overlap : overlap;
}
