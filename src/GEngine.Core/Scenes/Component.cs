// Template Method, in its smallest useful form: the base class owns the sequence - attach,
// update, fixed update, detach - and a subclass fills in the steps it cares about. The
// public side is sealed so a subclass cannot accidentally skip the enabled check.

using System;

namespace GEngine.Core.Scenes;

/// <summary>
/// A behaviour attached to an <see cref="Entity"/>. Override the On-methods; the entity
/// calls them in order and only while <see cref="IsEnabled"/> holds.
/// </summary>
public abstract class Component
{
    private Entity? _entity;

    /// <summary>The entity this component is attached to.</summary>
    /// <exception cref="InvalidOperationException">The component is not attached yet.</exception>
    public Entity Entity =>
        _entity ?? throw new InvalidOperationException("this component is not attached to an entity");

    /// <summary>True once the component has been attached to an entity.</summary>
    public bool IsAttached => _entity is not null;

    /// <summary>When false, the entity skips this component's updates but keeps it attached.</summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>The transform of the entity this component is attached to.</summary>
    public Transform2D Transform => Entity.Transform;

    /// <summary>Called once, right after the component is attached.</summary>
    protected virtual void OnAttach()
    {
    }

    /// <summary>Called once per frame with the real frame time.</summary>
    /// <param name="deltaSeconds">Seconds since the previous frame.</param>
    protected virtual void OnUpdate(float deltaSeconds)
    {
    }

    /// <summary>Called once per fixed step, always with the same delta.</summary>
    /// <param name="fixedDeltaSeconds">The fixed step, in seconds.</param>
    protected virtual void OnFixedUpdate(float fixedDeltaSeconds)
    {
    }

    /// <summary>Called once, right before the component is detached.</summary>
    protected virtual void OnDetach()
    {
    }

    internal void Attach(Entity entity)
    {
        _entity = entity;
        OnAttach();
    }

    internal void Detach()
    {
        OnDetach();
        _entity = null;
    }

    internal void Update(float deltaSeconds)
    {
        if (IsEnabled)
        {
            OnUpdate(deltaSeconds);
        }
    }

    internal void FixedUpdate(float fixedDeltaSeconds)
    {
        if (IsEnabled)
        {
            OnFixedUpdate(fixedDeltaSeconds);
        }
    }
}
