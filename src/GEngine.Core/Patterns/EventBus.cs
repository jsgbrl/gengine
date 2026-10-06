// Observer, typed by the message. A goomba does not know the HUD exists; it publishes
// GoombaStomped and whoever cares has already subscribed.

using System;
using System.Collections.Generic;

namespace GEngine.Core.Patterns;

/// <summary>
/// Publishes messages to whoever subscribed to that message type. Handlers per type are
/// kept in a single multicast delegate, which gives the engine C# event semantics for
/// free: publishing walks the invocation list captured when the publish began, so a
/// handler that unsubscribes - or subscribes - from inside a handler cannot corrupt the
/// walk. It also means an unsubscribe made during a publish takes effect on the next one.
/// </summary>
public sealed class EventBus
{
    private readonly Dictionary<Type, Delegate> _handlers = [];

    /// <summary>Adds a handler for a message type.</summary>
    /// <typeparam name="TEvent">Type of the message.</typeparam>
    /// <param name="handler">What to call when one is published.</param>
    public void Subscribe<TEvent>(Action<TEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        Type key = typeof(TEvent);
        _handlers[key] = _handlers.TryGetValue(key, out Delegate? existing)
            ? Delegate.Combine(existing, handler)
            : handler;
    }

    /// <summary>Removes a handler for a message type.</summary>
    /// <typeparam name="TEvent">Type of the message.</typeparam>
    /// <param name="handler">The handler previously passed to <see cref="Subscribe"/>.</param>
    /// <returns>True when the handler was subscribed.</returns>
    public bool Unsubscribe<TEvent>(Action<TEvent> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        Type key = typeof(TEvent);
        if (!_handlers.TryGetValue(key, out Delegate? existing))
        {
            return false;
        }

        var remaining = Delegate.Remove(existing, handler);
        return remaining is null ? _handlers.Remove(key) : Replace(key, existing, remaining);
    }

    /// <summary>Delivers a message to every subscriber of its type.</summary>
    /// <typeparam name="TEvent">Type of the message.</typeparam>
    /// <param name="message">The message; its runtime type decides who hears it.</param>
    public void Publish<TEvent>(TEvent message)
    {
        if (_handlers.TryGetValue(typeof(TEvent), out Delegate? handler))
        {
            ((Action<TEvent>)handler).Invoke(message);
        }
    }

    /// <summary>How many handlers are subscribed to a message type.</summary>
    /// <typeparam name="TEvent">Type of the message.</typeparam>
    /// <returns>The number of handlers.</returns>
    /// <remarks>This walks the invocation list and allocates; it is for tests and diagnostics.</remarks>
    public int SubscriberCount<TEvent>()
    {
        return _handlers.TryGetValue(typeof(TEvent), out Delegate? handler)
            ? handler.GetInvocationList().Length
            : 0;
    }

    /// <summary>Forgets every subscription.</summary>
    public void Clear() => _handlers.Clear();

    private bool Replace(Type key, Delegate existing, Delegate remaining)
    {
        _handlers[key] = remaining;
        return !ReferenceEquals(existing, remaining);
    }
}
