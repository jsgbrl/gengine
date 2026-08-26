// The player. Movement is next door in Player.Movement.cs; this half is state - how big, how
// hurt, how dead - and what happens when something touches it.

using System;
using GEngine.Core;
using GEngine.Input.Actions;
using GEngine.Physics;
using MarioClone.Audio;
using MarioClone.Game;

namespace MarioClone.Actors;

/// <summary>The player character.</summary>
public sealed partial class Player : Actor
{
    /// <summary>How wide the player is, in pixels. The same at both sizes.</summary>
    public const float WidthPixels = 7.0f;

    /// <summary>How tall a small player is, in pixels.</summary>
    public const float SmallHeightPixels = 7.0f;

    /// <summary>How tall a big player is, in pixels.</summary>
    public const float BigHeightPixels = 14.0f;

    private readonly InputState _input;
    private float _coyoteSeconds;
    private float _jumpBufferSeconds;
    private float _invulnerableSeconds;
    private bool _isRising;

    /// <summary>Creates a player.</summary>
    /// <param name="body">Its body, already sized for a small player.</param>
    /// <param name="input">Where its decisions come from.</param>
    /// <param name="tuning">Every number that decides how it feels.</param>
    public Player(RigidBody2D body, InputState input, PlayerTuning tuning)
        : base(body)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(tuning);
        _input = input;
        Tuning = tuning;
        Body.MaxVelocity = new Vector2(400.0f, tuning.MaximumFallSpeedPixelsPerSecond);
        Body.Layer = CollisionLayers.Player;
        Body.Mask = CollisionLayers.PlayerMask;
        SpriteName = "mario-small";
    }

    /// <summary>Every number that decides how the player feels.</summary>
    public PlayerTuning Tuning { get; }

    /// <summary>How big the player is.</summary>
    public PlayerSize Size { get; private set; } = PlayerSize.Small;

    /// <summary>True while the player is standing on something.</summary>
    public bool IsGrounded => Body.IsGrounded;

    /// <summary>True while a hit cannot land.</summary>
    public bool IsInvulnerable => _invulnerableSeconds > 0.0f;

    /// <summary>True once the player has died. The game decides what happens next.</summary>
    public bool IsDead { get; private set; }

    /// <summary>True once the player has touched the flag.</summary>
    public bool HasReachedGoal { get; private set; }

    /// <summary>How long the player has been able to jump without being on the ground.</summary>
    public float CoyoteSecondsLeft => _coyoteSeconds;

    /// <summary>How long a jump press is still remembered for.</summary>
    public float JumpBufferSecondsLeft => _jumpBufferSeconds;

    /// <summary>Makes the player big, and taller, keeping their feet where they are.</summary>
    public void Grow()
    {
        if (Size == PlayerSize.Big)
        {
            return;
        }

        Resize(PlayerSize.Big);
        World.Audio.Play(GameSound.PowerUp);
    }

    /// <summary>
    /// Hurts the player: a big one shrinks and is briefly invulnerable, a small one dies.
    /// Doing nothing while already invulnerable is what stops a single goomba from killing a
    /// player twice in two consecutive steps.
    /// </summary>
    public void Hurt()
    {
        if (IsInvulnerable || IsDead || HasReachedGoal)
        {
            return;
        }

        if (Size == PlayerSize.Big)
        {
            Resize(PlayerSize.Small);
            _invulnerableSeconds = Tuning.InvulnerabilitySeconds;
            World.Audio.Play(GameSound.Hurt);
            return;
        }

        Kill();
    }

    /// <summary>Kills the player outright, however big they are. This is what a pit does.</summary>
    public void Kill()
    {
        if (IsDead)
        {
            return;
        }

        IsDead = true;
        Body.Velocity = new Vector2(0.0f, Tuning.JumpVelocityPixelsPerSecond * 0.6f);
        World.Audio.Play(GameSound.Death);
    }

    /// <summary>Throws the player back up, which is what stomping something does.</summary>
    public void Bounce()
    {
        Body.Velocity = Body.Velocity.WithY(Tuning.StompBouncePixelsPerSecond);
        _isRising = true;
    }

    /// <summary>Records that the flag has been touched.</summary>
    public void ReachGoal()
    {
        if (HasReachedGoal)
        {
            return;
        }

        HasReachedGoal = true;
        World.Session.AddScore(GameSession.GoalScore);
        World.Audio.Play(GameSound.LevelComplete);
    }

    private void Resize(PlayerSize size)
    {
        float bottom = Body.Bounds.Bottom;
        float height = size == PlayerSize.Big ? BigHeightPixels : SmallHeightPixels;
        Size = size;
        Body.Size = new Vector2(WidthPixels, height);
        Body.Position = new Vector2(Body.Position.X, bottom - (height / 2.0f));
        SpriteName = size == PlayerSize.Big ? "mario-big" : "mario-small";
    }
}
