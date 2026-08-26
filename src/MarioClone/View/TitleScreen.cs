// The title, and the controls of the device actually in the player's hands.
//
// Showing keyboard keys to somebody holding a gamepad is a small thing that tells them the
// game was not written for them. The router already knows which source last reported anything,
// so this costs one comparison.

using System;
using GEngine.Core.Contracts;
using GEngine.Input.Actions;
using GEngine.Input.Gamepad;
using GEngine.Rendering;

namespace MarioClone.View;

/// <summary>Draws the title screen and the controls for whichever device is in use.</summary>
public sealed class TitleScreen
{
    private readonly InputMap _keyboard;
    private readonly GamepadMap _gamepad;

    /// <summary>Creates the screen.</summary>
    /// <param name="keyboard">The key bindings, so the screen can name the real keys.</param>
    /// <param name="gamepad">The button bindings, so it can name the real buttons.</param>
    public TitleScreen(InputMap keyboard, GamepadMap gamepad)
    {
        ArgumentNullException.ThrowIfNull(keyboard);
        ArgumentNullException.ThrowIfNull(gamepad);
        _keyboard = keyboard;
        _gamepad = gamepad;
    }

    /// <summary>Draws the title and the controls.</summary>
    /// <param name="frame">Where to draw.</param>
    /// <param name="active">The source that last reported anything, or null.</param>
    public void Draw(FrameBuffer frame, IInputBackend? active)
    {
        ArgumentNullException.ThrowIfNull(frame);
        frame.Clear(Palette.DeepBlue);
        float line = MathF.Max(4.0f, (frame.Height / 2.0f) - 22.0f);
        Hud.DrawCentered(frame, "GENGINE", line, Palette.White);
        Hud.DrawCentered(frame, "WORLD 1-1", line + 10.0f, Palette.Yellow);
        Hud.DrawCentered(frame, DescribeSource(active), line + 22.0f, Palette.LightGreen);
        Hud.DrawCentered(frame, Describe(InputAction.MoveLeft, active) + " MOVE", line + 32.0f, Palette.Grey);
        Hud.DrawCentered(frame, Describe(InputAction.Jump, active) + " JUMP", line + 40.0f, Palette.Grey);
        Hud.DrawCentered(frame, Describe(InputAction.Run, active) + " RUN", line + 48.0f, Palette.Grey);
        Hud.DrawCentered(frame, Describe(InputAction.Confirm, active) + " START", line + 56.0f, Palette.White);
    }

    private static bool IsGamepad(IInputBackend? active) => active is DualSenseGamepad;

    private static string DescribeSource(IInputBackend? active) =>
        active is null ? "PRESS ANYTHING" : active.Name.ToUpperInvariant();

    private string Describe(InputAction action, IInputBackend? active) =>
        IsGamepad(active) ? _gamepad.Describe(action) : _keyboard.Describe(action);
}
