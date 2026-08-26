// A goomba: walks, turns around when it meets a wall or runs out of floor, dies when it is
// stomped, and kills whatever walks into its side.
//
// The stomp test is one comparison, and it is worth understanding. The solver separates two
// overlapping bodies along whichever axis they overlap least, and hands the contact a normal
// pointing along that axis. A player who landed on the goomba overlaps it vertically; a player
// who walked into it overlaps it horizontally. So the normal already knows which happened,
// with no velocities, no timers and no guessing.

using System;
using GEngine.Core;
using GEngine.Physics;
using MarioClone.Audio;
using MarioClone.Game;

namespace MarioClone.Actors;

/// <summary>A goomba.</summary>
public sealed class Goomba : Actor
{
    /// <summary>How wide and tall a goomba is, in pixels.</summary>
    public const float SizePixels = 7.0f;

    /// <summary>How fast it patrols, in pixels per second.</summary>
    public const float WalkSpeedPixelsPerSecond = 22.0f;

    /// <summary>How far below its feet it looks for floor before deciding to turn.</summary>
    public const float LedgeProbePixels = 3.0f;

    private float _direction = -1.0f;

    /// <summary>Creates a goomba walking left.</summary>
    /// <param name="body">Its body.</param>
    public Goomba(RigidBody2D body)
        : base(body)
    {
        Body.Layer = CollisionLayers.Enemy;
        Body.Mask = CollisionLayers.EnemyMask;
        SpriteName = "goomba";
        IsFacingLeft = true;
    }

    /// <summary>Which way it is walking: minus one for left, one for right.</summary>
    public float Direction => _direction;

    /// <inheritdoc/>
    protected override void OnFixedUpdate(float fixedDeltaSeconds)
    {
        if (ShouldTurn())
        {
            Turn();
        }

        Body.Velocity = Body.Velocity.WithX(_direction * WalkSpeedPixelsPerSecond);
    }

    /// <inheritdoc/>
    public override void OnCollisionEnter(Contact contact)
    {
        if (ActorOf(contact.Other) is not Player player)
        {
            TurnOnWall(contact);
            return;
        }

        if (contact.Normal.Y > 0.5f)
        {
            Stomped(player);
            return;
        }

        player.Hurt();
    }

    private void Stomped(Player player)
    {
        Retire();
        player.Bounce();
        World.Session.AddScore(GameSession.StompScore);
        World.Audio.Play(GameSound.Stomp);
    }

    // A contact whose normal is mostly sideways is something solid in the way that the probe
    // below cannot see - another goomba, which is on its own layer rather than the level's.
    private void TurnOnWall(Contact contact)
    {
        if (MathF.Abs(contact.Normal.X) > 0.5f && MathF.Sign(contact.Normal.X) != MathF.Sign(_direction))
        {
            Turn();
        }
    }

    // Two probes, one step ahead: is there a wall at chest height, and is there still floor
    // under the next step? The second is what stops a goomba walking off every ledge in the
    // level within about four seconds. The first exists because most walls in this game are
    // tiles, and a tile has no body, so no contact ever arrives to say the goomba hit one.
    private bool ShouldTurn()
    {
        if (!Body.IsGrounded)
        {
            return false;
        }

        float ahead = _direction < 0.0f ? Body.Bounds.Left - 1.0f : Body.Bounds.Right + 1.0f;
        bool wallAhead = IsSolidAt(new Vector2(ahead, Body.Bounds.Center.Y));
        bool floorAhead = IsSolidAt(new Vector2(ahead, Body.Bounds.Bottom + LedgeProbePixels));
        return wallAhead || !floorAhead;
    }

    private bool IsSolidAt(Vector2 point)
    {
        if (World.Physics.QueryPoint(point, CollisionLayers.Level) is not null)
        {
            return true;
        }

        int column = MathG.FloorToInt(point.X / World.Level.TileSize);
        int row = MathG.FloorToInt(point.Y / World.Level.TileSize);
        return World.Level.TileAt(column, row) != Levels.TileKind.Empty;
    }

    private void Turn()
    {
        _direction = -_direction;
        IsFacingLeft = _direction < 0.0f;
    }
}
