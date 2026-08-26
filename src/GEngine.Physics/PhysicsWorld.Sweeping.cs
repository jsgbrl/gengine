// What is in the way, and what to do about it. Only non-dynamic bodies and tiles are swept
// against here: two dynamic bodies have no privileged axis, so they are resolved discretely
// in PhysicsWorld.Contacts.cs instead.

using System;
using GEngine.Core;
using GEngine.Physics.NarrowPhase;
using GEngine.Physics.Tiles;

namespace GEngine.Physics;

/// <content>Swept collision against the level.</content>
public sealed partial class PhysicsWorld
{
    private Obstruction FindNearestObstruction(RigidBody2D body, Vector2 delta)
    {
        Aabb start = body.Bounds;
        BroadPhase.Query(start.Union(start.Translated(delta)), _candidates);
        Obstruction nearest = Obstruction.None;
        foreach (RigidBody2D other in _candidates)
        {
            nearest = Obstruction.Closer(nearest, SweepAgainst(body, delta, other));
        }

        return Obstruction.Closer(nearest, SweepAgainstTiles(body, delta));
    }

    private static Obstruction SweepAgainst(RigidBody2D body, Vector2 delta, RigidBody2D other)
    {
        if (!IsSolidFor(body, other, delta))
        {
            return Obstruction.None;
        }

        SweepResult result = SweptAabb.Sweep(body.Bounds, delta, other.Bounds);
        return result.IsHit ? new Obstruction(result, other.Bounds, other) : Obstruction.None;
    }

    private Obstruction SweepAgainstTiles(RigidBody2D body, Vector2 delta)
    {
        if (Tiles is null)
        {
            return Obstruction.None;
        }

        SweepResult result = TileSweep.FindNearest(Tiles, body.Bounds, delta, out Aabb tileBounds);
        return result.IsHit ? new Obstruction(result, tileBounds, null) : Obstruction.None;
    }

    private static bool IsSolidFor(RigidBody2D body, RigidBody2D other, Vector2 delta)
    {
        if (ReferenceEquals(body, other) || other.Type == BodyType.Dynamic || other.IsTrigger)
        {
            return false;
        }

        if (!body.CollidesWith(other))
        {
            return false;
        }

        return !other.IsOneWay || (delta.Y > 0.0f && body.Bounds.Bottom <= other.Bounds.Top + MathG.Epsilon);
    }

    // Snapping to the exact face, rather than moving by delta times the hit time. The two
    // agree to within a rounding error, and a rounding error on the wrong side leaves the
    // body a fraction inside the floor - which the next step reads as a penetration and
    // corrects, and the step after that undoes. That is what a jittering platformer is.
    private static void SnapToSurface(RigidBody2D body, Obstruction obstruction)
    {
        Vector2 normal = obstruction.Result.Normal;
        Vector2 half = body.HalfSize;
        Aabb face = obstruction.Bounds;
        if (normal.X > 0.0f)
        {
            body.Position = body.Position.WithX(face.Right + half.X);
        }
        else if (normal.X < 0.0f)
        {
            body.Position = body.Position.WithX(face.Left - half.X);
        }
        else if (normal.Y > 0.0f)
        {
            body.Position = body.Position.WithY(face.Bottom + half.Y);
        }
        else
        {
            body.Position = body.Position.WithY(face.Top - half.Y);
        }
    }

    private void RespondToSurface(RigidBody2D body, Obstruction obstruction)
    {
        Vector2 normal = obstruction.Result.Normal;
        ApplyBounceAndFriction(body, normal, obstruction.Body);
        if (normal.Y < 0.0f)
        {
            body.IsGrounded = true;
            body.Ground = obstruction.Body;
        }

        if (obstruction.Body is not null)
        {
            AddContact(body, obstruction.Body, normal, 0.0f);
        }
    }

    // Below the resting speed the bounce is dropped entirely. Without that line a ball with
    // any restitution at all keeps taking ever smaller bounces for ever, and what the player
    // sees is not a bounce but a shiver.
    private void ApplyBounceAndFriction(RigidBody2D body, Vector2 normal, RigidBody2D? other)
    {
        float approach = Vector2.Dot(body.Velocity, normal);
        if (approach >= 0.0f)
        {
            return;
        }

        Vector2 into = normal * approach;
        Vector2 across = body.Velocity - into;
        float restitution = MathF.Max(body.Restitution, other?.Restitution ?? 0.0f);
        float bounce = -approach > _settings.RestingSpeedPixelsPerSecond ? restitution : 0.0f;
        float friction = MathF.Sqrt(body.Friction * (other?.Friction ?? _settings.TileFriction));
        body.Velocity = (across * (1.0f - friction)) - (into * bounce);
    }

    private bool PushOutOnce(RigidBody2D body)
    {
        BroadPhase.Query(body.Bounds, _candidates);
        foreach (RigidBody2D other in _candidates)
        {
            if (!IsSolidFor(body, other, Vector2.Zero) || !Separate(body, other.Bounds, out Vector2 push))
            {
                continue;
            }

            // Being pushed out of something counts as touching it, and a game that only
            // ever overlaps - a body spawned inside a door - would otherwise hear nothing.
            AddContact(body, other, push.Normalized(), push.Length);
            return true;
        }

        return PushOutOfTiles(body);
    }

    private bool PushOutOfTiles(RigidBody2D body)
    {
        if (Tiles is null)
        {
            return false;
        }

        return TileSweep.TryFindOverlap(Tiles, body.Bounds, out Aabb tile) && Separate(body, tile, out _);
    }

    private static bool Separate(RigidBody2D body, Aabb obstacle, out Vector2 push)
    {
        if (!AabbCollision.TryGetMinimumTranslation(body.Bounds, obstacle, out push))
        {
            return false;
        }

        body.Position += push;
        return true;
    }
}
