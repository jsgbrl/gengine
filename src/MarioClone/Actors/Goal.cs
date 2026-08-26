// The flag at the end. A trigger, because walking into the end of a level should not feel like
// walking into a wall.

using GEngine.Physics;
using MarioClone.Game;

namespace MarioClone.Actors;

/// <summary>The flag that ends the level.</summary>
public sealed class Goal : Actor
{
    /// <summary>How wide the flag is, in pixels.</summary>
    public const float WidthPixels = 4.0f;

    /// <summary>How tall the flag is, in pixels.</summary>
    public const float HeightPixels = 40.0f;

    /// <summary>Creates the flag.</summary>
    /// <param name="body">Its body, which is made a trigger here.</param>
    public Goal(RigidBody2D body)
        : base(body)
    {
        Body.Type = BodyType.Static;
        Body.IsTrigger = true;
        Body.Layer = CollisionLayers.Goal;
        Body.Mask = CollisionLayers.Player;
        SpriteName = "flag";
    }

    /// <inheritdoc/>
    public override void OnTriggerEnter(Contact contact)
    {
        if (ActorOf(contact.Other) is Player player)
        {
            player.ReachGoal();
        }
    }
}
