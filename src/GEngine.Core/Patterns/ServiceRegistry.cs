// Service Locator, with the warning attached rather than filed away.
//
// A locator turns every dependency into a runtime lookup, which means a type no longer
// declares what it needs and the compiler can no longer check it. Use it for the handful
// of engine-wide services a composition root wires once - the clock, the logger, the
// renderer - and pass everything else through constructors, where a missing dependency is
// a build error instead of a crash three minutes into a level.

using System;
using System.Collections.Generic;

namespace GEngine.Core.Patterns;

/// <summary>A small map from service type to instance, filled once by the composition root.</summary>
public sealed class ServiceRegistry
{
    private readonly Dictionary<Type, object> _services = [];

    /// <summary>How many services are registered.</summary>
    public int Count => _services.Count;

    /// <summary>Registers an instance under a service type, replacing any previous one.</summary>
    /// <typeparam name="TService">The service type callers will ask for.</typeparam>
    /// <param name="service">The instance.</param>
    public void Register<TService>(TService service)
        where TService : class
    {
        ArgumentNullException.ThrowIfNull(service);
        _services[typeof(TService)] = service;
    }

    /// <summary>Removes a service.</summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <returns>True when one was registered.</returns>
    public bool Unregister<TService>()
        where TService : class
    {
        return _services.Remove(typeof(TService));
    }

    /// <summary>True when a service of this type is registered.</summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <returns>True when it can be resolved.</returns>
    public bool Contains<TService>()
        where TService : class
    {
        return _services.ContainsKey(typeof(TService));
    }

    /// <summary>Looks a service up, or fails loudly naming the missing type.</summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <returns>The registered instance.</returns>
    /// <exception cref="InvalidOperationException">No instance is registered for that type.</exception>
    public TService Resolve<TService>()
        where TService : class
    {
        if (_services.TryGetValue(typeof(TService), out object? service))
        {
            return (TService)service;
        }

        throw new InvalidOperationException("no service registered for " + typeof(TService).Name);
    }

    /// <summary>Looks a service up without failing when it is absent.</summary>
    /// <typeparam name="TService">The service type.</typeparam>
    /// <param name="service">The instance, or null.</param>
    /// <returns>True when one was registered.</returns>
    public bool TryResolve<TService>(out TService? service)
        where TService : class
    {
        if (_services.TryGetValue(typeof(TService), out object? found))
        {
            service = (TService)found;
            return true;
        }

        service = null;
        return false;
    }

    /// <summary>Forgets every registration.</summary>
    public void Clear() => _services.Clear();
}
