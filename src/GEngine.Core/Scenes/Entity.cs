// A named thing in a scene: a transform plus the components that give it behaviour.
// Composition instead of inheritance - a goomba is not a subclass of enemy, it is an
// entity carrying a body, a sprite and a patrol.

using System;
using System.Collections.Generic;

namespace GEngine.Core.Scenes;

/// <summary>A named object in a <see cref="Scene"/>, built out of <see cref="Component"/>s.</summary>
public sealed class Entity
{
    private readonly List<Component> _components = [];

    /// <summary>Creates an entity.</summary>
    /// <param name="name">Name used to find it later. Not required to be unique.</param>
    public Entity(string name)
    {
        Name = name;
    }

    /// <summary>Name used to find the entity in its scene.</summary>
    public string Name { get; }

    /// <summary>Where the entity is.</summary>
    public Transform2D Transform { get; } = new();

    /// <summary>When false, the scene skips this entity entirely.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>The scene holding this entity, or null while it is detached.</summary>
    public Scene? Scene { get; internal set; }

    /// <summary>The components attached, in the order they were added.</summary>
    public IReadOnlyList<Component> Components => _components;

    /// <summary>Attaches a component and returns it, so a caller can keep the reference.</summary>
    /// <typeparam name="TComponent">Type of the component.</typeparam>
    /// <param name="component">The component to attach.</param>
    /// <returns>The same component.</returns>
    public TComponent Add<TComponent>(TComponent component)
        where TComponent : Component
    {
        ArgumentNullException.ThrowIfNull(component);
        _components.Add(component);
        component.Attach(this);
        return component;
    }

    /// <summary>Detaches the first component of a type.</summary>
    /// <typeparam name="TComponent">Type of the component.</typeparam>
    /// <returns>True when one was attached and has now been detached.</returns>
    public bool Remove<TComponent>()
        where TComponent : Component
    {
        TComponent? found = Get<TComponent>();
        if (found is null)
        {
            return false;
        }

        _components.Remove(found);
        found.Detach();
        return true;
    }

    /// <summary>The first component of a type, or null when there is none.</summary>
    /// <typeparam name="TComponent">Type of the component.</typeparam>
    /// <returns>The component, or null.</returns>
    public TComponent? Get<TComponent>()
        where TComponent : Component
    {
        foreach (Component component in _components)
        {
            if (component is TComponent match)
            {
                return match;
            }
        }

        return null;
    }

    /// <summary>The first component of a type, or a clear failure when there is none.</summary>
    /// <typeparam name="TComponent">Type of the component.</typeparam>
    /// <returns>The component.</returns>
    /// <exception cref="InvalidOperationException">No component of that type is attached.</exception>
    public TComponent Require<TComponent>()
        where TComponent : Component
    {
        return Get<TComponent>()
            ?? throw new InvalidOperationException(Name + " has no " + typeof(TComponent).Name);
    }

    internal void Update(float deltaSeconds)
    {
        foreach (Component component in _components)
        {
            component.Update(deltaSeconds);
        }
    }

    internal void FixedUpdate(float fixedDeltaSeconds)
    {
        foreach (Component component in _components)
        {
            component.FixedUpdate(fixedDeltaSeconds);
        }
    }
}
