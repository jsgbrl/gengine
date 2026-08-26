// Every constant the solver uses, in one place and with its unit in its name.

using GEngine.Core;

namespace GEngine.Physics;

/// <summary>How a <see cref="PhysicsWorld"/> behaves.</summary>
public sealed class PhysicsSettings
{
    /// <summary>The settings the game and the examples use.</summary>
    public static PhysicsSettings Default { get; } = new();

    /// <summary>Acceleration applied to dynamic bodies, in pixels per second squared, pointing down.</summary>
    public Vector2 Gravity { get; init; } = new(0.0f, 900.0f);

    /// <summary>How many passes are made to push overlapping bodies apart at the start of a step.</summary>
    public int DepenetrationPasses { get; init; } = 4;

    /// <summary>
    /// Below this approach speed, in pixels per second, a bounce is dropped and the body is
    /// simply stopped. Without it a ball with any restitution at all keeps taking ever
    /// smaller bounces for ever, and a stack of them shivers instead of resting.
    /// </summary>
    public float RestingSpeedPixelsPerSecond { get; init; } = 25.0f;

    /// <summary>
    /// How many times one axis of one body may bounce inside a single step. A bounce that
    /// happens a quarter of the way through a step leaves three quarters of the step still
    /// to travel, at the new velocity; throwing that remainder away is a quiet energy leak
    /// that makes restitution mean less than the number says.
    /// </summary>
    public int MaximumBouncesPerStep { get; init; } = 4;

    /// <summary>Friction the tilemap applies, since a tile has no body to carry its own.</summary>
    public float TileFriction { get; init; }
}
