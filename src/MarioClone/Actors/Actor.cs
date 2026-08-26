// Everything in the level that moves or reacts. An actor is a Component, so the scene updates
// it, and an ICollisionListener, so the physics world tells it what it touched - and it owns
// the body that does the touching.
//
// Position lives in the body and nowhere else. There is no copy in the transform to keep in
// sync, because a second copy of the truth is a second thing to get wrong.

using System;
using GEngine.Core;
using GEngine.Core.Scenes;
using GEngine.Physics;
using MarioClone.Game;

namespace MarioClone.Actors;

/// <summary>Something in the level with a body and an opinion about what it touches.</summary>
public abstract class Actor : Component, ICollisionListener
{
    /// <summary>Creates an actor around a body.</summary>
    /// <param name="body">The body it moves with. The world takes ownership when it is added.</param>
    protected Actor(RigidBody2D body)
    {
        ArgumentNullException.ThrowIfNull(body);
        Body = body;
        body.Listener = this;
    }

    /// <summary>The body this actor moves with.</summary>
    public RigidBody2D Body { get; }

    /// <summary>Where it is, in world pixels.</summary>
    public Vector2 Position => Body.Position;

    /// <summary>The box it occupies, in world pixels.</summary>
    public Aabb Bounds => Body.Bounds;

    /// <summary>Which sprite draws it right now.</summary>
    public string SpriteName { get; protected set; } = string.Empty;

    /// <summary>True when it should be drawn mirrored.</summary>
    public bool IsFacingLeft { get; protected set; }

    /// <summary>False once it has been used up, stomped or collected. The world removes it.</summary>
    public bool IsAlive { get; protected set; } = true;

    /// <summary>The game this actor is part of. Set when the world adds it.</summary>
    public MarioWorld World => _world ?? throw new InvalidOperationException("this actor is not in a world yet");

    private MarioWorld? _world;

    /// <inheritdoc/>
    public virtual void OnCollisionEnter(Contact contact)
    {
    }

    /// <inheritdoc/>
    public virtual void OnCollisionStay(Contact contact)
    {
    }

    /// <inheritdoc/>
    public virtual void OnCollisionExit(Contact contact)
    {
    }

    /// <inheritdoc/>
    public virtual void OnTriggerEnter(Contact contact)
    {
    }

    /// <inheritdoc/>
    public virtual void OnTriggerStay(Contact contact)
    {
    }

    /// <inheritdoc/>
    public virtual void OnTriggerExit(Contact contact)
    {
    }

    /// <summary>Marks this actor as used up. The world takes it out at the end of the step.</summary>
    protected void Retire() => IsAlive = false;

    internal void JoinWorld(MarioWorld world) => _world = world;

    /// <summary>The actor a body belongs to, or null when the body is level geometry.</summary>
    /// <param name="body">The body.</param>
    /// <returns>The actor.</returns>
    protected static Actor? ActorOf(RigidBody2D body) => body.Owner as Actor;
}
