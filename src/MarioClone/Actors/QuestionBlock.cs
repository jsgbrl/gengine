// A question block: solid, and bumps up once when hit from below, giving what is inside. The
// tween is four frames of the block rising and falling back, which is the whole animation
// budget a console game has and is enough to read as a bump.
//
// It is a body and not a tile for one reason: a tile has no identity, so a contact cannot
// point at it, so nothing can be told that this particular block was hit.

using GEngine.Core;
using GEngine.Physics;
using MarioClone.Audio;
using MarioClone.Game;

namespace MarioClone.Actors;

/// <summary>A question block that gives what is inside it, once.</summary>
public sealed class QuestionBlock : Actor
{
    /// <summary>How far the block rises when it is bumped, in pixels.</summary>
    public const float BumpHeightPixels = 3.0f;

    /// <summary>How long the bump takes, in seconds.</summary>
    public const float BumpSeconds = 0.16f;

    private readonly Vector2 _restingPosition;
    private float _bumpSecondsLeft;

    /// <summary>Creates a block.</summary>
    /// <param name="body">Its body, which is made static here.</param>
    /// <param name="gives">What comes out when it is hit.</param>
    public QuestionBlock(RigidBody2D body, BlockPrize gives)
        : base(body)
    {
        Body.Type = BodyType.Static;
        Body.Layer = CollisionLayers.Level;
        Body.Mask = CollisionLayers.LevelMask;
        _restingPosition = body.Position;
        Gives = gives;
        SpriteName = "question";
    }

    /// <summary>What comes out when it is hit.</summary>
    public BlockPrize Gives { get; }

    /// <summary>True once it has given what it had. It stays solid, and stays put.</summary>
    public bool IsSpent { get; private set; }

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
        float halfway = BumpSeconds / 2.0f;
        float rise = _bumpSecondsLeft > halfway
            ? (BumpSeconds - _bumpSecondsLeft) / halfway
            : _bumpSecondsLeft / halfway;

        Body.Position = _restingPosition.WithY(_restingPosition.Y - (rise * BumpHeightPixels));
    }

    /// <inheritdoc/>
    public override void OnCollisionEnter(Contact contact)
    {
        if (contact.Normal.Y < -0.5f && ActorOf(contact.Other) is Player player)
        {
            Bump();
        }
    }

    private void Bump()
    {
        if (IsSpent)
        {
            return;
        }

        IsSpent = true;
        _bumpSecondsLeft = BumpSeconds;
        SpriteName = "block-spent";
        World.Audio.Play(GameSound.BlockBump);
        Give();
    }

    private void Give()
    {
        if (Gives == BlockPrize.Coin)
        {
            World.Session.CollectCoin();
            World.Audio.Play(GameSound.Coin);
            return;
        }

        World.SpawnMushroomAbove(this);
    }
}
