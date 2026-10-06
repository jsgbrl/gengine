// A source that reports whatever a test says is held.
//
// The router owns the frame: it calls Begin, asks every source, and calls End. So a test cannot
// report actions into the state directly - the next Poll would wipe them. It has to be a source,
// which is the same shape a keyboard and a DualSense are, and is the point of the interface.

using System;
using System.Collections.Generic;
using GEngine.Core.Contracts;

namespace MarioClone.Tests.Doubles;

internal sealed class HeldInput : IInputBackend
{
    private readonly HashSet<InputAction> _held = [];

    public string Name => "held";

    public bool IsConnected => true;

    public void Hold(params InputAction[] actions)
    {
        ArgumentNullException.ThrowIfNull(actions);
        _held.Clear();
        foreach (InputAction action in actions)
        {
            _held.Add(action);
        }
    }

    public void Release() => _held.Clear();

    public void Poll(float deltaSeconds)
    {
    }

    public bool IsDown(InputAction action) => _held.Contains(action);

    public float AxisValue(InputAction action) => IsDown(action) ? 1.0f : 0.0f;

    public void Dispose() => Release();
}
