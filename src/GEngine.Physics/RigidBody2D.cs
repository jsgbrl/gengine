// A box with a velocity. Everything the solver needs to know about a thing in the world is
// here, and nothing else is: a body does not know it is a goomba.

using System;
using GEngine.Core;

namespace GEngine.Physics;

/// <summary>An axis-aligned box the physics world simulates.</summary>
public sealed class RigidBody2D
{
    private float _mass = 1.0f;

    /// <summary>Creates a body.</summary>
    /// <param name="type">What is allowed to move it.</param>
    /// <param name="position">Centre of the box, in world pixels.</param>
    /// <param name="size">Width and height of the box, in pixels.</param>
    public RigidBody2D(BodyType type, Vector2 position, Vector2 size)
    {
        Type = type;
        Position = position;
        Size = size;
    }

    /// <summary>
    /// Identity inside its world, assigned when the body is added. The solver iterates in
    /// Id order and never in insertion or hash order, which is where determinism comes
    /// from: the same bodies resolved in the same sequence give the same floats.
    /// </summary>
    public int Id { get; internal set; }

    /// <summary>What is allowed to move this body.</summary>
    public BodyType Type { get; set; }

    /// <summary>Centre of the box, in world pixels.</summary>
    public Vector2 Position { get; set; }

    /// <summary>Width and height of the box, in pixels.</summary>
    public Vector2 Size { get; set; }

    /// <summary>Pixels per second.</summary>
    public Vector2 Velocity { get; set; }

    /// <summary>How strongly gravity pulls this body. Zero for a floating coin, one for the player.</summary>
    public float GravityScale { get; set; } = 1.0f;

    /// <summary>Fraction of the velocity lost per second, as air resistance.</summary>
    public float Drag { get; set; }

    /// <summary>How much of the approach speed comes back on a bounce, from zero to one.</summary>
    public float Restitution { get; set; }

    /// <summary>How strongly a contact slows movement along the surface, from zero to one.</summary>
    public float Friction { get; set; }

    /// <summary>Largest speed per axis, in pixels per second. Terminal velocity lives here.</summary>
    public Vector2 MaxVelocity { get; set; } = new(float.MaxValue, float.MaxValue);

    /// <summary>Which layer this body belongs to, as a single bit.</summary>
    public int Layer { get; set; } = 1;

    /// <summary>Which layers this body collides with, as a bitmask.</summary>
    public int Mask { get; set; } = ~0;

    /// <summary>When true the body reports overlaps but never stops anything.</summary>
    public bool IsTrigger { get; set; }

    /// <summary>When true the body is solid only to something falling onto it from above.</summary>
    public bool IsOneWay { get; set; }

    /// <summary>Whatever the game wants to hang off this body, usually its entity.</summary>
    public object? Owner { get; set; }

    /// <summary>Where the collision and trigger callbacks go, or null when nobody is listening.</summary>
    public ICollisionListener? Listener { get; set; }

    /// <summary>True when this body ended the last step standing on something.</summary>
    public bool IsGrounded { get; internal set; }

    /// <summary>What it is standing on, or null. A kinematic ground carries its passenger.</summary>
    public RigidBody2D? Ground { get; internal set; }

    /// <summary>
    /// Mass in arbitrary units. Setting it recomputes <see cref="InverseMass"/>, which is
    /// what the solver actually uses: an immovable body becomes the number zero instead of
    /// a branch in the middle of the impulse formula.
    /// </summary>
    public float Mass
    {
        get => _mass;
        set => _mass = MathG.Clamp(value, MathG.Epsilon, float.MaxValue);
    }

    /// <summary>One over the mass, or zero for anything that cannot be pushed.</summary>
    public float InverseMass => Type == BodyType.Dynamic ? 1.0f / _mass : 0.0f;

    /// <summary>The box this body occupies right now.</summary>
    public Aabb Bounds => Aabb.FromCenterSize(Position, Size);

    /// <summary>Half the width and half the height.</summary>
    public Vector2 HalfSize => Size * 0.5f;

    // How far a kinematic body moved during the current step. A dynamic body standing on
    // it adds the same offset, which is what makes a moving platform carry its passenger
    // instead of sliding out from under it.
    internal Vector2 StepDelta { get; set; }

    /// <summary>Adds an instantaneous change of momentum, divided by the mass.</summary>
    /// <param name="impulse">The impulse, in pixel-units per second.</param>
    public void AddImpulse(Vector2 impulse) => Velocity += impulse * InverseMass;

    /// <summary>True when this body and another are allowed to collide at all.</summary>
    /// <param name="other">The other body.</param>
    /// <returns>True when each mask admits the other layer.</returns>
    public bool CollidesWith(RigidBody2D other)
    {
        ArgumentNullException.ThrowIfNull(other);
        return (Mask & other.Layer) != 0 && (other.Mask & Layer) != 0;
    }
}
