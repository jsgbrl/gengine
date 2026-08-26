// Every number that decides how the game feels, in one place, so an experiment is a one-line
// change and a bad experiment is a one-line revert.
//
// These are not arbitrary. A platformer is judged almost entirely on the quarter of a second
// around a jump, and the four numbers that matter most are the last four in this table:
//
//   coyote time      you may still jump for a moment after walking off a ledge
//   jump buffer      a jump pressed just before landing is remembered and fires on landing
//   jump cut         letting go of the button shortens the ascent
//   skid             turning around at speed decelerates faster than it accelerates
//
// Without the first two, the game feels like it is ignoring you - and the player never blames
// the physics, they blame themselves, and then they stop playing.

namespace MarioClone.Actors;

/// <summary>Every constant that decides how the player moves.</summary>
public sealed class PlayerTuning
{
    /// <summary>The tuning the game ships with.</summary>
    public static PlayerTuning Default { get; } = new();

    /// <summary>Downward acceleration, in pixels per second squared.</summary>
    public float GravityPixelsPerSecondSquared { get; init; } = 460.0f;

    /// <summary>Fastest fall, in pixels per second. Terminal velocity.</summary>
    public float MaximumFallSpeedPixelsPerSecond { get; init; } = 190.0f;

    /// <summary>Top walking speed, in pixels per second.</summary>
    public float WalkSpeedPixelsPerSecond { get; init; } = 42.0f;

    /// <summary>Top running speed, in pixels per second, while Run is held.</summary>
    public float RunSpeedPixelsPerSecond { get; init; } = 72.0f;

    /// <summary>How quickly the player reaches top speed, in pixels per second squared.</summary>
    public float AccelerationPixelsPerSecondSquared { get; init; } = 260.0f;

    /// <summary>How quickly the player stops when nothing is held, in pixels per second squared.</summary>
    public float FrictionPixelsPerSecondSquared { get; init; } = 340.0f;

    /// <summary>
    /// How quickly the player turns around, in pixels per second squared. Higher than
    /// acceleration on purpose: that difference is the skid, and it is what makes a change of
    /// direction feel like weight rather than like a teleport.
    /// </summary>
    public float SkidPixelsPerSecondSquared { get; init; } = 640.0f;

    /// <summary>How much of the ground acceleration applies in the air, from zero to one.</summary>
    public float AirControl { get; init; } = 0.65f;

    /// <summary>Upward speed a jump starts with, in pixels per second. Negative is up.</summary>
    public float JumpVelocityPixelsPerSecond { get; init; } = -168.0f;

    /// <summary>Upward speed a stomp bounce gives, in pixels per second.</summary>
    public float StompBouncePixelsPerSecond { get; init; } = -130.0f;

    /// <summary>How long after leaving a ledge a jump still works, in seconds.</summary>
    public float CoyoteTimeSeconds { get; init; } = 0.10f;

    /// <summary>How long before landing a jump press is remembered, in seconds.</summary>
    public float JumpBufferSeconds { get; init; } = 0.10f;

    /// <summary>What the rising velocity is multiplied by when the button is let go early.</summary>
    public float JumpCutFactor { get; init; } = 0.4f;

    /// <summary>How long the player cannot be hurt after being hit, in seconds.</summary>
    public float InvulnerabilitySeconds { get; init; } = 1.5f;

    /// <summary>How far below the level the player has to fall to count as dead, in pixels.</summary>
    public float PitDepthPixels { get; init; } = 24.0f;
}
