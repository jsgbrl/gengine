// Continuous collision, by slabs.
//
// Moving a box a whole frame at a time and then asking whether it overlaps anything is how
// a bullet passes through a wall: at ten thousand pixels a second the box is on one side
// before the step and the other side after it, and never overlaps at all. The fix is to
// ask a different question - along this movement, when is the first touch? - and it has a
// closed form. Grow the obstacle by half the moving box and the moving box becomes a
// point; the movement becomes a ray; and a ray against an axis-aligned box is the
// intersection of two intervals, one per axis. Those intervals are the slabs.

using System;
using GEngine.Core;

namespace GEngine.Physics.NarrowPhase;

/// <summary>Continuous collision of a moving box, or a ray, against a fixed box.</summary>
public static class SweptAabb
{
    /// <summary>Finds where a moving box first touches an obstacle.</summary>
    /// <param name="moving">The box before it moves.</param>
    /// <param name="delta">How far it wants to move this step, in pixels.</param>
    /// <param name="obstacle">The box in its way.</param>
    /// <returns>The first touch, or a miss.</returns>
    public static SweepResult Sweep(Aabb moving, Vector2 delta, Aabb obstacle) =>
        Cast(moving.Center, delta, obstacle.Expanded(moving.HalfSize));

    /// <summary>Finds where a ray first enters a box.</summary>
    /// <param name="origin">Where the ray starts.</param>
    /// <param name="delta">Direction and length of the ray; the answer is a fraction of it.</param>
    /// <param name="box">The box to test against.</param>
    /// <returns>The first entry, or a miss.</returns>
    public static SweepResult Cast(Vector2 origin, Vector2 delta, Aabb box)
    {
        Slab horizontal = SlabOf(origin.X, delta.X, box.Min.X, box.Max.X);
        Slab vertical = SlabOf(origin.Y, delta.Y, box.Min.Y, box.Max.Y);
        if (horizontal.IsMiss || vertical.IsMiss)
        {
            return SweepResult.Miss;
        }

        float entry = MathF.Max(horizontal.Entry, vertical.Entry);
        float exit = MathF.Min(horizontal.Exit, vertical.Exit);
        if (entry > exit || entry < 0.0f || entry > 1.0f)
        {
            return SweepResult.Miss;
        }

        return SweepResult.Hit(entry, NormalOf(horizontal, vertical, delta));
    }

    // The interval of the movement during which one axis is inside the box. A movement with
    // no component on that axis is either always inside it or never, which is the branch
    // that turns into a division by zero if it is forgotten.
    private static Slab SlabOf(float origin, float delta, float minimum, float maximum)
    {
        if (MathF.Abs(delta) < MathG.Epsilon)
        {
            return origin <= minimum || origin >= maximum
                ? Slab.Miss
                : new Slab(float.NegativeInfinity, float.PositiveInfinity);
        }

        float first = (minimum - origin) / delta;
        float second = (maximum - origin) / delta;
        return first <= second ? new Slab(first, second) : new Slab(second, first);
    }

    // Whichever axis entered last is the axis that was blocking, so that is the face the
    // moving box landed on. The normal points back along the movement.
    private static Vector2 NormalOf(Slab horizontal, Slab vertical, Vector2 delta)
    {
        if (horizontal.Entry > vertical.Entry)
        {
            return delta.X > 0.0f ? new Vector2(-1.0f, 0.0f) : new Vector2(1.0f, 0.0f);
        }

        return delta.Y > 0.0f ? new Vector2(0.0f, -1.0f) : new Vector2(0.0f, 1.0f);
    }

    private readonly struct Slab
    {
        public Slab(float entry, float exit)
        {
            Entry = entry;
            Exit = exit;
        }

        public static Slab Miss => new(float.NaN, float.NaN);

        public float Entry { get; }

        public float Exit { get; }

        public bool IsMiss => float.IsNaN(Entry);
    }
}
