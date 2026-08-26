// What stopped a moving body this axis: where along the movement, which face, and - when
// it was a body rather than a tile - which body. Tiles have no RigidBody2D, so the body is
// nullable and that null is the whole reason tile collisions raise no contact callbacks.

using GEngine.Core;
using GEngine.Physics.NarrowPhase;

namespace GEngine.Physics;

internal readonly struct Obstruction
{
    public Obstruction(SweepResult result, Aabb bounds, RigidBody2D? body)
    {
        Result = result;
        Bounds = bounds;
        Body = body;
    }

    public static Obstruction None => new(SweepResult.Miss, default, null);

    public SweepResult Result { get; }

    public Aabb Bounds { get; }

    public RigidBody2D? Body { get; }

    public static Obstruction Closer(Obstruction current, Obstruction candidate)
    {
        if (!candidate.Result.IsHit)
        {
            return current;
        }

        return current.Result.IsHit && current.Result.Time <= candidate.Result.Time ? current : candidate;
    }
}
