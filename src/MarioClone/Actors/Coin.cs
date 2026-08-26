// A coin: a trigger that scores once and disappears. Being a trigger is the whole design - a
// coin that could be stood on would change how the level plays.

using GEngine.Physics;
using MarioClone.Audio;
using MarioClone.Game;

namespace MarioClone.Actors;

/// <summary>A coin waiting to be collected.</summary>
public sealed class Coin : Actor
{
    /// <summary>How wide and tall a coin is, in pixels.</summary>
    public const float SizePixels = 5.0f;

    /// <summary>Creates a coin.</summary>
    /// <param name="body">Its body, which is made a trigger here.</param>
    public Coin(RigidBody2D body)
        : base(body)
    {
        Body.Type = BodyType.Static;
        Body.IsTrigger = true;
        Body.Layer = CollisionLayers.Pickup;
        Body.Mask = CollisionLayers.PickupMask;
        SpriteName = "coin";
    }

    /// <inheritdoc/>
    public override void OnTriggerEnter(Contact contact)
    {
        if (!IsAlive || ActorOf(contact.Other) is not Player)
        {
            return;
        }

        Retire();
        World.Session.CollectCoin();
        World.Audio.Play(GameSound.Coin);
    }
}
