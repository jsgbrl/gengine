// One source of input: a keyboard, a gamepad, a recorded script. Every source answers the
// same two questions about the same actions, so the router can mix them and the game
// cannot tell which one answered.

using System;

namespace GEngine.Core.Contracts;

/// <summary>A polled source of player input, expressed in actions rather than keys.</summary>
public interface IInputBackend : IDisposable
{
    /// <summary>Human-readable name, shown on the title screen and in diagnostics.</summary>
    string Name { get; }

    /// <summary>False when the device is unplugged or the script has run out.</summary>
    bool IsConnected { get; }

    /// <summary>Reads the device once. Called exactly once per frame, before anything asks.</summary>
    /// <param name="deltaSeconds">Seconds since the previous poll.</param>
    void Poll(float deltaSeconds);

    /// <summary>Whether an action is being asked for right now.</summary>
    /// <param name="action">The action.</param>
    /// <returns>True while it is held.</returns>
    bool IsDown(InputAction action);

    /// <summary>
    /// How strongly an action is being asked for, from zero to one. A key gives one while
    /// it is down; a stick gives how far it is pushed. Writing the player controller
    /// against this instead of against <see cref="IsDown"/> is what makes analogue and
    /// digital movement feel like the same game.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <returns>A value from zero to one.</returns>
    float AxisValue(InputAction action);
}
