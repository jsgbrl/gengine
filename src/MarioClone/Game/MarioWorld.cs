// What a running level is made of: a scene of entities, a physics world under them, the level
// they are standing on, the numbers on the display, and somewhere for sound to go.
//
// It is the game's own composition object rather than a service locator: everything in it is
// named, typed and passed in, and an actor reaches it through a property rather than looking
// it up.

using System;
using System.Collections.Generic;
using GEngine.Core;
using GEngine.Core.Scenes;
using GEngine.Physics;
using MarioClone.Actors;
using MarioClone.Audio;
using MarioClone.Levels;

namespace MarioClone.Game;

/// <summary>One level, running.</summary>
public sealed class MarioWorld
{
    private readonly List<Actor> _actors = [];
    private readonly List<Actor> _retiring = [];

    /// <summary>Creates a world.</summary>
    /// <param name="level">The level being played.</param>
    /// <param name="physics">The physics world, already holding the level's tiles.</param>
    /// <param name="session">The score, coins and lives.</param>
    /// <param name="audio">Where sound goes.</param>
    public MarioWorld(Level level, PhysicsWorld physics, GameSession session, IAudioBackend audio)
    {
        ArgumentNullException.ThrowIfNull(level);
        ArgumentNullException.ThrowIfNull(physics);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentNullException.ThrowIfNull(audio);
        Level = level;
        Physics = physics;
        Session = session;
        Audio = audio;
    }

    /// <summary>The level being played.</summary>
    public Level Level { get; }

    /// <summary>The physics world.</summary>
    public PhysicsWorld Physics { get; }

    /// <summary>The score, coins and lives.</summary>
    public GameSession Session { get; }

    /// <summary>Where sound goes.</summary>
    public IAudioBackend Audio { get; }

    /// <summary>The scene holding every actor.</summary>
    public Scene Scene { get; } = new("level");

    /// <summary>Every actor still in the level, in the order they were added.</summary>
    public IReadOnlyList<Actor> Actors => _actors;

    /// <summary>The player, once one has been added.</summary>
    public Player? Player { get; private set; }

    /// <summary>Adds an actor: an entity in the scene and a body in the physics world.</summary>
    /// <param name="actor">The actor.</param>
    /// <param name="name">What to call its entity.</param>
    /// <returns>The same actor, so a caller can keep the reference.</returns>
    public Actor Add(Actor actor, string name)
    {
        ArgumentNullException.ThrowIfNull(actor);
        actor.JoinWorld(this);
        var entity = new Entity(name);
        entity.Add(actor);
        Physics.Add(actor.Body);
        actor.Body.Owner = actor;
        Scene.Add(entity);
        _actors.Add(actor);
        if (actor is Player player)
        {
            Player = player;
        }

        return actor;
    }

    /// <summary>
    /// Runs one fixed step: actors decide, physics moves them and reports what they touched,
    /// and then whatever was used up during the step is taken out. Removal is last on purpose -
    /// taking a body out of the world in the middle of the solver is the classic way to lose a
    /// contact that was about to fire.
    /// </summary>
    /// <param name="fixedDeltaSeconds">The fixed step, in seconds.</param>
    public void Step(float fixedDeltaSeconds)
    {
        Scene.FixedUpdate(fixedDeltaSeconds);
        Physics.Step(fixedDeltaSeconds);
        RemoveRetired();
    }

    /// <summary>Puts a mushroom on top of a block, walking right.</summary>
    /// <param name="block">The block it came out of.</param>
    /// <returns>The mushroom.</returns>
    public Mushroom SpawnMushroomAbove(Actor block)
    {
        ArgumentNullException.ThrowIfNull(block);
        float half = Mushroom.SizePixels / 2.0f;
        var position = new Vector2(block.Position.X, block.Bounds.Top - half - 0.5f);
        var body = new RigidBody2D(BodyType.Dynamic, position, new Vector2(Mushroom.SizePixels, Mushroom.SizePixels));
        return (Mushroom)Add(new Mushroom(body), "mushroom");
    }

    private void RemoveRetired()
    {
        _retiring.Clear();
        foreach (Actor actor in _actors)
        {
            if (!actor.IsAlive)
            {
                _retiring.Add(actor);
            }
        }

        foreach (Actor actor in _retiring)
        {
            Physics.Remove(actor.Body);
            _actors.Remove(actor);
            if (actor.Entity.Scene is not null)
            {
                Scene.Remove(actor.Entity);
            }
        }
    }
}
