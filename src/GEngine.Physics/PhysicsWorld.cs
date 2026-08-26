// The world. One step is six phases, in this order and for these reasons:
//
//   1. kinematic bodies move, carrying whatever was standing on them last step
//   2. dynamic velocities are integrated - semi-implicit Euler, velocity before position
//   3. dynamic bodies move against the world: swept, X first and then Y
//   4. dynamic bodies that ended up overlapping each other are pushed apart and bounced
//   5. triggers are collected
//   6. enter, stay and exit callbacks are dispatched, each exactly once
//
// The split in the middle is the design decision worth arguing with. Against the level -
// static bodies, kinematic platforms, tiles - collision is continuous and resolved one axis
// at a time, because that is what stops a fast body tunnelling and what makes a platformer
// feel right. Against other dynamic bodies it is discrete, minimum translation plus an
// impulse, because two moving boxes have no privileged axis.

using System;
using System.Collections.Generic;
using GEngine.Core;
using GEngine.Core.Contracts;
using GEngine.Physics.BroadPhase;
using GEngine.Physics.Tiles;

namespace GEngine.Physics;

/// <summary>A world of axis-aligned bodies, stepped at a fixed rate.</summary>
public sealed partial class PhysicsWorld : IPhysicsWorld
{
    private readonly List<RigidBody2D> _bodies = [];
    private readonly List<BroadPhasePair> _pairs = [];
    private readonly List<RigidBody2D> _candidates = [];
    private readonly PhysicsSettings _settings;
    private int _nextId = 1;

    /// <summary>Creates a world.</summary>
    /// <param name="settings">Gravity and the solver constants.</param>
    /// <param name="broadPhase">Which broad phase to use, usually a <see cref="SpatialHashGrid"/>.</param>
    public PhysicsWorld(PhysicsSettings settings, IBroadPhase broadPhase)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(broadPhase);
        _settings = settings;
        BroadPhase = broadPhase;
        Gravity = settings.Gravity;
    }

    /// <inheritdoc/>
    public Vector2 Gravity { get; set; }

    /// <summary>Every body in the world, in identity order. That order is where determinism comes from.</summary>
    public IReadOnlyList<RigidBody2D> Bodies => _bodies;

    /// <summary>The broad phase in use.</summary>
    public IBroadPhase BroadPhase { get; }

    /// <summary>The level geometry, or null when the world is only bodies.</summary>
    public ITileCollisionSource? Tiles { get; set; }

    /// <summary>How many steps this world has run.</summary>
    public long StepCount { get; private set; }

    /// <summary>Adds a body and gives it its identity.</summary>
    /// <param name="body">The body to add.</param>
    /// <returns>The same body, so a caller can keep the reference.</returns>
    public RigidBody2D Add(RigidBody2D body)
    {
        ArgumentNullException.ThrowIfNull(body);
        body.Id = _nextId++;
        _bodies.Add(body);
        return body;
    }

    /// <summary>Removes a body and forgets every contact it was part of.</summary>
    /// <param name="body">The body to remove.</param>
    /// <returns>True when it was in the world.</returns>
    public bool Remove(RigidBody2D body)
    {
        ArgumentNullException.ThrowIfNull(body);
        if (!_bodies.Remove(body))
        {
            return false;
        }

        ForgetContactsOf(body);
        return true;
    }

    /// <inheritdoc/>
    public void Step(float fixedDeltaSeconds)
    {
        SwapContactBuffers();
        MoveKinematicBodies(fixedDeltaSeconds);
        ResetGroundFlags();
        IntegrateDynamicBodies(fixedDeltaSeconds);
        BroadPhase.Update(_bodies);
        MoveDynamicBodies(fixedDeltaSeconds);
        BroadPhase.Update(_bodies);
        BroadPhase.FindPairs(_pairs);
        ResolvePairs();
        DispatchContacts();
        StepCount++;
    }
}
