// A brick: solid, and breakable only by a big player hitting it from below. A small player
// bumps it and it holds - which is the original's rule, and the reason growing changes how the
// level is played rather than just how long you survive.

using GEngine.Core;
using GEngine.Physics;
using MarioClone.Audio;
using MarioClone.Game;

namespace MarioClone.Actors;

/// <summary>A breakable brick.</summary>
public sealed class Brick : Actor
{
    /// <summary>How far the brick jumps when a small player bumps it, in pixels.</summary>
    public const float BumpHeightPixels = 2.0f;

    /// <summary>How long that bump takes, in seconds.</summary>
    public const float BumpSeconds = 0.12f;

    private readonly Vector2 _restingPosition;
    private float _bumpSecondsLeft;

    /// <summary>Creates a brick.</summary>
    /// <param name="body">Its body, which is made static here.</param>
    public Brick(RigidBody2D body)
        : base(body)
    {
        Body.Type = BodyType.Static;
        Body.Layer = CollisionLayers.Level;
        Body.Mask = CollisionLayers.LevelMask;
        _restingPosition = body.Position;
        SpriteName = "brick";
    }

    /// <summary>True while the bump animation is playing.</summary>
    public bool IsBumping => _bumpSecondsLeft > 0.0f;

    /// <inheritdoc/>
    protected override void OnFixedUpdate(float fixedDeltaSeconds)
    {
        if (_bumpSecondsLeft <= 0.0f)
        {
            return;
        }

        _bumpSecondsLeft = MathG.Clamp(_bumpSecondsLeft - fixedDeltaSeconds, 0.0f, BumpSeconds);
        float rise = _bumpSecondsLeft / BumpSeconds;
        Body.Position = _restingPosition.WithY(_restingPosition.Y - (rise * BumpHeightPixels));
    }

    /// <inheritdoc/>
    public override void OnCollisionEnter(Contact contact)
    {
        if (contact.Normal.Y >= -0.5f || ActorOf(contact.Other) is not Player player)
        {
            return;
        }

        if (player.Size == PlayerSize.Big)
        {
            Break();
            return;
        }

        _bumpSecondsLeft = BumpSeconds;
        World.Audio.Play(GameSound.BlockBump);
    }

    private void Break()
    {
        Retire();
        World.Session.AddScore(GameSession.BrickScore);
        World.Audio.Play(GameSound.BrickBreak);
    }
}
