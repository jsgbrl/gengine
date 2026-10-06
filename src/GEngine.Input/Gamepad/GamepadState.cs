// A snapshot of the controller. It is a readonly struct of about thirty bytes, which is why
// the reader thread can hand one to the game thread by copy and nothing has to be locked,
// pooled or allocated.

using System;
using System.Globalization;
using GEngine.Core;

namespace GEngine.Input.Gamepad;

/// <summary>Everything a gamepad is doing at one instant.</summary>
public readonly struct GamepadState : IEquatable<GamepadState>
{
    /// <summary>Creates a state.</summary>
    /// <param name="leftStick">Left stick, each axis from minus one to one.</param>
    /// <param name="rightStick">Right stick, each axis from minus one to one.</param>
    /// <param name="triggers">Left trigger in X and right trigger in Y, each from zero to one.</param>
    /// <param name="buttons">Which buttons are pressed.</param>
    public GamepadState(Vector2 leftStick, Vector2 rightStick, Vector2 triggers, GamepadButtons buttons)
    {
        LeftStick = leftStick;
        RightStick = rightStick;
        Triggers = triggers;
        Buttons = buttons;
    }

    /// <summary>
    /// Nothing pressed and both sticks centred. The hat has to be set by hand: the hardware
    /// numbers north as zero, so the default value of the enum is up, not neutral.
    /// </summary>
    public static GamepadState Neutral =>
        new(Vector2.Zero, Vector2.Zero, Vector2.Zero, GamepadButtons.None) { Hat = HatDirection.Neutral };

    /// <summary>Left stick. Positive Y is down, matching the screen.</summary>
    public Vector2 LeftStick { get; }

    /// <summary>Right stick. Positive Y is down, matching the screen.</summary>
    public Vector2 RightStick { get; }

    /// <summary>Left trigger in X and right trigger in Y, each from zero to one.</summary>
    public Vector2 Triggers { get; }

    /// <summary>Which buttons are pressed.</summary>
    public GamepadButtons Buttons { get; }

    /// <summary>Which way the D-pad is pointing.</summary>
    public HatDirection Hat { get; init; }

    /// <summary>Left trigger, from zero to one.</summary>
    public float LeftTrigger => Triggers.X;

    /// <summary>Right trigger, from zero to one.</summary>
    public float RightTrigger => Triggers.Y;

    /// <summary>Whether a button is pressed.</summary>
    /// <param name="button">The button, or several of them.</param>
    /// <returns>True when every named button is pressed.</returns>
    public bool IsDown(GamepadButtons button) => (Buttons & button) == button;

    /// <summary>Whether any of several buttons is pressed.</summary>
    /// <param name="buttons">One or more button flags, combined with |.</param>
    /// <returns>True when at least one of them is pressed.</returns>
    public bool IsAnyDown(GamepadButtons buttons) => (Buttons & buttons) != GamepadButtons.None;

    /// <summary>Compares two snapshots field by field.</summary>
    /// <param name="other">The snapshot to compare with.</param>
    /// <returns>True when the controller is in the same state.</returns>
    public bool Equals(GamepadState other) =>
        LeftStick.Equals(other.LeftStick)
        && RightStick.Equals(other.RightStick)
        && Triggers.Equals(other.Triggers)
        && Buttons == other.Buttons
        && Hat == other.Hat;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is GamepadState other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(LeftStick, RightStick, Triggers, Buttons, Hat);

    /// <inheritdoc/>
    public override string ToString() => string.Format(
        CultureInfo.InvariantCulture,
        "left {0} right {1} triggers {2} hat {3} buttons {4}",
        LeftStick,
        RightStick,
        Triggers,
        Hat,
        Buttons);

    /// <summary>Equality.</summary>
    /// <param name="left">First snapshot.</param>
    /// <param name="right">Second snapshot.</param>
    /// <returns>True when they match.</returns>
    public static bool operator ==(GamepadState left, GamepadState right) => left.Equals(right);

    /// <summary>Inequality.</summary>
    /// <param name="left">First snapshot.</param>
    /// <param name="right">Second snapshot.</param>
    /// <returns>True when they differ.</returns>
    public static bool operator !=(GamepadState left, GamepadState right) => !left.Equals(right);
}
