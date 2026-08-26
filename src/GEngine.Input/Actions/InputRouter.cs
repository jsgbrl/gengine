// Where several sources become one answer.
//
// Every connected backend is polled every frame and the strongest report per action wins, so
// a player may hold left on a stick and left on a keyboard at once, may put the gamepad down
// mid-level and reach for the arrow keys, and may unplug the gamepad without the game
// noticing anything except that one backend stopped reporting. There is no "current device"
// to switch, which is why switching costs nothing and cannot lose a frame.

using System;
using System.Collections.Generic;
using GEngine.Core.Contracts;

namespace GEngine.Input.Actions;

/// <summary>Merges every connected input source into one <see cref="InputState"/>.</summary>
public sealed class InputRouter : IDisposable
{
    private readonly List<IInputBackend> _backends = [];

    /// <summary>Creates a router writing into a state.</summary>
    /// <param name="state">Where the merged answer goes.</param>
    public InputRouter(InputState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        State = state;
    }

    /// <summary>The merged state the game reads.</summary>
    public InputState State { get; }

    /// <summary>Every source, in the order they were added.</summary>
    public IReadOnlyList<IInputBackend> Backends => _backends;

    /// <summary>
    /// The source that last reported anything. The title screen uses it to show the controls
    /// of the device actually in the player's hands.
    /// </summary>
    public IInputBackend? ActiveBackend { get; private set; }

    /// <summary>How many sources are connected right now.</summary>
    public int ConnectedCount => CountConnected();

    /// <summary>Adds a source.</summary>
    /// <param name="backend">The source.</param>
    /// <returns>The same source, so a caller can keep the reference.</returns>
    public IInputBackend Add(IInputBackend backend)
    {
        ArgumentNullException.ThrowIfNull(backend);
        _backends.Add(backend);
        return backend;
    }

    /// <summary>Removes a source and disposes it.</summary>
    /// <param name="backend">The source.</param>
    /// <returns>True when it was there.</returns>
    public bool Remove(IInputBackend backend)
    {
        if (!_backends.Remove(backend))
        {
            return false;
        }

        if (ReferenceEquals(ActiveBackend, backend))
        {
            ActiveBackend = null;
        }

        backend.Dispose();
        return true;
    }

    /// <summary>Polls every connected source and merges what they report.</summary>
    /// <param name="deltaSeconds">Seconds since the previous frame.</param>
    public void Poll(float deltaSeconds)
    {
        State.Begin();
        foreach (IInputBackend backend in _backends)
        {
            PollOne(backend, deltaSeconds);
        }

        State.End(deltaSeconds);
    }

    /// <summary>Disposes every source and forgets them.</summary>
    public void Dispose()
    {
        foreach (IInputBackend backend in _backends)
        {
            backend.Dispose();
        }

        _backends.Clear();
        ActiveBackend = null;
    }

    private void PollOne(IInputBackend backend, float deltaSeconds)
    {
        if (!backend.IsConnected)
        {
            return;
        }

        backend.Poll(deltaSeconds);
        bool reported = false;
        foreach (InputAction action in InputActions.All)
        {
            float value = backend.AxisValue(action);
            State.Report(action, value);
            reported |= value > 0.0f;
        }

        if (reported)
        {
            ActiveBackend = backend;
        }
    }

    private int CountConnected()
    {
        int connected = 0;
        foreach (IInputBackend backend in _backends)
        {
            connected += backend.IsConnected ? 1 : 0;
        }

        return connected;
    }
}
