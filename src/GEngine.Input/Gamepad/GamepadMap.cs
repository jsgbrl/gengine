// Which button asks for which action. The same idea as InputMap and deliberately the same
// shape: the game asks for Jump, and only this table knows that Jump is the cross.

using System.Collections.Generic;
using GEngine.Core.Contracts;

namespace GEngine.Input.Gamepad;

/// <summary>Which gamepad buttons ask for which actions.</summary>
public sealed class GamepadMap
{
    private readonly Dictionary<GamepadButtons, InputAction> _bindings = [];

    /// <summary>The bindings the game starts with: cross jumps, square runs, options pauses.</summary>
    /// <returns>A fresh map, so a caller may rebind it without disturbing anyone else.</returns>
    public static GamepadMap CreateDefault()
    {
        var map = new GamepadMap();
        map.Bind(GamepadButtons.Cross, InputAction.Jump);
        map.Bind(GamepadButtons.Square, InputAction.Run);
        map.Bind(GamepadButtons.RightTrigger, InputAction.Run);
        map.Bind(GamepadButtons.Options, InputAction.Pause);
        map.Bind(GamepadButtons.Triangle, InputAction.Confirm);
        map.Bind(GamepadButtons.Circle, InputAction.Cancel);
        return map;
    }

    /// <summary>How many buttons are bound.</summary>
    public int Count => _bindings.Count;

    /// <summary>Binds one button to an action, replacing whatever it did before.</summary>
    /// <param name="button">A single button, not a combination.</param>
    /// <param name="action">What it should ask for.</param>
    public void Bind(GamepadButtons button, InputAction action) => _bindings[button] = action;

    /// <summary>Removes a binding.</summary>
    /// <param name="button">The button.</param>
    /// <returns>True when it was bound.</returns>
    public bool Unbind(GamepadButtons button) => _bindings.Remove(button);

    /// <summary>Every binding, so the gamepad can walk them once per frame.</summary>
    public IReadOnlyDictionary<GamepadButtons, InputAction> Bindings => _bindings;

    /// <summary>Which buttons ask for an action, sorted so a listing is reproducible.</summary>
    /// <param name="action">The action.</param>
    /// <returns>The buttons bound to it.</returns>
    public IReadOnlyList<GamepadButtons> ButtonsFor(InputAction action)
    {
        List<GamepadButtons> buttons = [];
        foreach (KeyValuePair<GamepadButtons, InputAction> binding in _bindings)
        {
            if (binding.Value == action)
            {
                buttons.Add(binding.Key);
            }
        }

        buttons.Sort();
        return buttons;
    }

    /// <summary>The buttons for an action, as something a title screen can print.</summary>
    /// <param name="action">The action.</param>
    /// <returns>Text such as "CROSS", or the D-pad for the movement actions.</returns>
    public string Describe(InputAction action)
    {
        if (action is InputAction.MoveLeft or InputAction.MoveRight or InputAction.MoveUp or InputAction.MoveDown)
        {
            return "STICK OR D-PAD";
        }

        IReadOnlyList<GamepadButtons> buttons = ButtonsFor(action);
        if (buttons.Count == 0)
        {
            return "UNBOUND";
        }

        List<string> names = [];
        foreach (GamepadButtons button in buttons)
        {
            names.Add(button.ToString().ToUpperInvariant());
        }

        return string.Join(" OR ", names);
    }
}
