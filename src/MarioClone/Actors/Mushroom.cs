// A mushroom: walks like a goomba, is collected like a coin, and is what comes out of a
// question block.
//
// It is a solid body rather than a trigger so that it can walk along the floor instead of
// falling through it; the one frame in which it pushes the player is the frame in which it is
// collected.

using System;
using GEngine.Physics;
using MarioClone.Game;

namespace MarioClone.Actors;

/// <summary>A mushroom that makes the player big.</summary>
public sealed class Mushroom : Actor
{
    /// <summary>How wide and tall a mushroom is, in pixels.</summary>
    public const float SizePixels = 6.0f;

    /// <summary>How fast it walks, in pixels per second.</summary>
    public const float WalkSpeedPixelsPerSecond = 26.0f;

    private float _direction = 1.0f;

    /// <summary>Creates a mushroom walking right.</summary>
    /// <param name="body">Its body.</param>
    public Mushroom(RigidBody2D body)
        : base(body)
    {
        Body.Layer = CollisionLayers.Pickup;
        Body.Mask = CollisionLayers.Level | CollisionLayers.Player;
        SpriteName = "mushroom";
    }

    /// <inheritdoc/>
    protected override void OnFixedUpdate(float fixedDeltaSeconds)
    {
        Body.Velocity = Body.Velocity.WithX(_direction * WalkSpeedPixelsPerSecond);
    }

    /// <inheritdoc/>
    public override void OnCollisionEnter(Contact contact)
    {
        if (ActorOf(contact.Other) is Player player)
        {
            Retire();
            player.Grow();
            return;
        }

        if (MathF.Abs(contact.Normal.X) > 0.5f)
        {
            _direction = -_direction;
        }
    }
}
