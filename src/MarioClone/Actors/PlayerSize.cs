// Two sizes, and the whole difference between them: a big player is twice as tall, can break
// bricks, and survives one hit by shrinking instead of dying.

namespace MarioClone.Actors;

/// <summary>How big the player is.</summary>
public enum PlayerSize
{
    /// <summary>One tile tall. A hit is fatal.</summary>
    Small,

    /// <summary>Two tiles tall. Breaks bricks, and a hit shrinks rather than kills.</summary>
    Big,
}
