// A scene is a list of entities plus one rule about when that list may change.
//
// The bug this exists to prevent is the one every engine author writes once: an entity is
// removed from inside its own update, the list mutates while the foreach is walking it,
// and either the collection throws or - worse, because it is silent - the entity after
// the removed one is skipped for that frame. Every add and remove is therefore queued and
// applied between iterations, never during one.

using System;
using System.Collections.Generic;

namespace GEngine.Core.Scenes;

/// <summary>A named collection of entities with deferred insertion and removal.</summary>
/// <remarks>
/// <see cref="Add"/> and <see cref="Remove"/> queue their work. The queue is drained at
/// the start of <see cref="Update"/> and <see cref="FixedUpdate"/>, so an entity added
/// during a frame first runs on the next one, and an entity removed during a frame still
/// finishes the frame it is in. Call <see cref="ApplyPendingChanges"/> to drain it early.
/// </remarks>
public sealed class Scene
{
    private readonly List<Entity> _entities = [];
    private readonly List<Entity> _pendingAdditions = [];
    private readonly List<Entity> _pendingRemovals = [];

    /// <summary>Creates a scene.</summary>
    /// <param name="name">Name of the scene, for logs and debugging.</param>
    public Scene(string name)
    {
        Name = name;
    }

    /// <summary>Name of the scene.</summary>
    public string Name { get; }

    /// <summary>The entities currently in the scene, in insertion order.</summary>
    public IReadOnlyList<Entity> Entities => _entities;

    /// <summary>How many additions and removals are waiting to be applied.</summary>
    public int PendingCount => _pendingAdditions.Count + _pendingRemovals.Count;

    /// <summary>Queues an entity for insertion.</summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The same entity, so a caller can keep the reference.</returns>
    public Entity Add(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _pendingAdditions.Add(entity);
        return entity;
    }

    /// <summary>Queues an entity for removal.</summary>
    /// <param name="entity">The entity to remove.</param>
    public void Remove(Entity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _pendingRemovals.Add(entity);
    }

    /// <summary>Applies every queued addition and removal, additions first.</summary>
    public void ApplyPendingChanges()
    {
        foreach (Entity entity in _pendingAdditions)
        {
            entity.Scene = this;
            _entities.Add(entity);
        }

        foreach (Entity entity in _pendingRemovals)
        {
            if (_entities.Remove(entity))
            {
                entity.Scene = null;
            }
        }

        _pendingAdditions.Clear();
        _pendingRemovals.Clear();
    }

    /// <summary>Runs one variable-time update over every active entity.</summary>
    /// <param name="deltaSeconds">Seconds since the previous frame.</param>
    public void Update(float deltaSeconds)
    {
        ApplyPendingChanges();
        foreach (Entity entity in _entities)
        {
            if (entity.IsActive)
            {
                entity.Update(deltaSeconds);
            }
        }
    }

    /// <summary>Runs one fixed step over every active entity.</summary>
    /// <param name="fixedDeltaSeconds">The fixed step, in seconds.</param>
    public void FixedUpdate(float fixedDeltaSeconds)
    {
        ApplyPendingChanges();
        foreach (Entity entity in _entities)
        {
            if (entity.IsActive)
            {
                entity.FixedUpdate(fixedDeltaSeconds);
            }
        }
    }

    /// <summary>The first entity with a given name, or null when there is none.</summary>
    /// <param name="name">Name to look for.</param>
    /// <returns>The entity, or null.</returns>
    public Entity? Find(string name)
    {
        foreach (Entity entity in _entities)
        {
            if (string.Equals(entity.Name, name, StringComparison.Ordinal))
            {
                return entity;
            }
        }

        return null;
    }
}
