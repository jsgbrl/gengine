// Phases one to three of a step: kinematic movement, integration, and the axis-by-axis
// walk of the dynamic bodies. Finding what is in the way is next door, in
// PhysicsWorld.Sweeping.cs.

using System;
using GEngine.Core;

namespace GEngine.Physics;

/// <content>Integration and movement.</content>
public sealed partial class PhysicsWorld
{
    private void MoveKinematicBodies(float fixedDeltaSeconds)
    {
        foreach (RigidBody2D body in _bodies)
        {
            if (body.Type == BodyType.Kinematic)
            {
                body.StepDelta = body.Velocity * fixedDeltaSeconds;
                body.Position += body.StepDelta;
            }
        }

        CarryPassengers();
    }

    private void CarryPassengers()
    {
        foreach (RigidBody2D body in _bodies)
        {
            if (body.Type == BodyType.Dynamic && body.Ground is { Type: BodyType.Kinematic } platform)
            {
                body.Position += platform.StepDelta;
            }
        }
    }

    private void ResetGroundFlags()
    {
        foreach (RigidBody2D body in _bodies)
        {
            body.IsGrounded = false;
            body.Ground = null;
        }
    }

    // Semi-implicit Euler: the new velocity is computed first and the position is then moved
    // with it. The explicit form - move with the old velocity, then update it - loses energy
    // on the way up and gains it on the way down, so a bouncing ball slowly climbs out of
    // the room. Verlet would be more accurate still, but it stores positions rather than
    // velocities, and the whole game reads and writes velocity: a jump sets it, a stomp
    // reverses it, a conveyor adds to it.
    private void IntegrateDynamicBodies(float fixedDeltaSeconds)
    {
        foreach (RigidBody2D body in _bodies)
        {
            if (body.Type == BodyType.Dynamic)
            {
                Integrate(body, fixedDeltaSeconds);
            }
        }
    }

    private void Integrate(RigidBody2D body, float fixedDeltaSeconds)
    {
        Vector2 velocity = body.Velocity + (Gravity * body.GravityScale * fixedDeltaSeconds);
        velocity *= MathF.Max(0.0f, 1.0f - (body.Drag * fixedDeltaSeconds));
        body.Velocity = new Vector2(
            MathG.Clamp(velocity.X, -body.MaxVelocity.X, body.MaxVelocity.X),
            MathG.Clamp(velocity.Y, -body.MaxVelocity.Y, body.MaxVelocity.Y));
    }

    // X before Y. Resolving both at once means choosing which of two overlaps to undo, and
    // undoing the wrong one snags a player on the seam between two floor tiles. Moving one
    // axis at a time never has to choose.
    private void MoveDynamicBodies(float fixedDeltaSeconds)
    {
        foreach (RigidBody2D body in _bodies)
        {
            if (body.Type != BodyType.Dynamic)
            {
                continue;
            }

            PushOutOfTheLevel(body);
            MoveAxis(body, new Vector2(body.Velocity.X * fixedDeltaSeconds, 0.0f));
            MoveAxis(body, new Vector2(0.0f, body.Velocity.Y * fixedDeltaSeconds));
        }
    }

    // A body that bounces a quarter of the way through a step still has three quarters of
    // the step to travel, now going the other way. Spending that remainder is what makes a
    // ball with restitution 0.95 return to nine tenths of its height instead of eight.
    private void MoveAxis(RigidBody2D body, Vector2 delta)
    {
        for (int bounce = 0; bounce <= _settings.MaximumBouncesPerStep; bounce++)
        {
            if (delta.LengthSquared <= 0.0f)
            {
                return;
            }

            Obstruction nearest = FindNearestObstruction(body, delta);
            if (!nearest.Result.IsHit)
            {
                body.Position += delta;
                return;
            }

            Vector2 before = body.Velocity;
            SnapToSurface(body, nearest);
            RespondToSurface(body, nearest);
            delta = RemainingTravel(delta, before, body.Velocity, nearest.Result.Time);
        }
    }

    // What is left of the movement after a bounce, expressed in the same units: the unspent
    // fraction of the step, scaled by how much the velocity changed along each axis.
    private static Vector2 RemainingTravel(Vector2 delta, Vector2 before, Vector2 after, float time)
    {
        float unspent = 1.0f - time;
        return new Vector2(
            Rescale(delta.X, before.X, after.X),
            Rescale(delta.Y, before.Y, after.Y)) * unspent;
    }

    private static float Rescale(float delta, float before, float after) =>
        MathF.Abs(before) <= MathG.Epsilon ? 0.0f : delta * (after / before);

    // A body can begin a step already inside the level: it was spawned there, a platform
    // moved into it, or a designer typed the wrong number. The swept test cannot help with
    // that - it answers "when does the movement first touch", and the answer is "before it
    // started" - so the overlap is undone first, shortest push wins, and only then does
    // anything move.
    private void PushOutOfTheLevel(RigidBody2D body)
    {
        for (int pass = 0; pass < _settings.DepenetrationPasses; pass++)
        {
            if (!PushOutOnce(body))
            {
                return;
            }
        }
    }
}
