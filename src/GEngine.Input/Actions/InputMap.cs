// Command, with the command reduced to an enum member.
//
// The game never asks "is the space bar down?". It asks "is Jump down?", and this table is
// the only place that knows the answer involves a space bar. Change the table and the
// controls change; nothing above this line moves.

using System;
using System.Collections.Generic;
using GEngine.Core.Contracts;

namespace GEngine.Input.Actions;

/// <summary>Which keys ask for which actions.</summary>
public sealed partial class InputMap
{
    private readonly Dictionary<ConsoleKey, InputAction> _bindings = [];

    /// <summary>The bindings the game starts with: arrows or WASD, space to jump.</summary>
    /// <returns>A fresh map, so a caller may rebind it without disturbing anyone else.</returns>
    public static InputMap CreateDefault()
    {
        var map = new InputMap();
        map.Bind(ConsoleKey.LeftArrow, InputAction.MoveLeft);
        map.Bind(ConsoleKey.A, InputAction.MoveLeft);
        map.Bind(ConsoleKey.RightArrow, InputAction.MoveRight);
        map.Bind(ConsoleKey.D, InputAction.MoveRight);
        map.Bind(ConsoleKey.UpArrow, InputAction.MoveUp);
        map.Bind(ConsoleKey.W, InputAction.MoveUp);
        map.Bind(ConsoleKey.DownArrow, InputAction.MoveDown);
        map.Bind(ConsoleKey.S, InputAction.MoveDown);
        map.Bind(ConsoleKey.Spacebar, InputAction.Jump);
        map.Bind(ConsoleKey.Z, InputAction.Jump);
        map.Bind(ConsoleKey.C, InputAction.Run);
        map.Bind(ConsoleKey.X, InputAction.Run);
        map.Bind(ConsoleKey.P, InputAction.Pause);
        map.Bind(ConsoleKey.Enter, InputAction.Confirm);
        map.Bind(ConsoleKey.Escape, InputAction.Cancel);
        return map;
    }

    /// <summary>How many keys are bound.</summary>
    public int Count => _bindings.Count;

    /// <summary>Binds a key to an action, replacing whatever that key did before.</summary>
    /// <param name="key">The key, as ConsoleKey reports it.</param>
    /// <param name="action">What it should ask for.</param>
    public void Bind(ConsoleKey key, InputAction action) => _bindings[key] = action;

    /// <summary>Removes a binding.</summary>
    /// <param name="key">The key to unbind; unbinding an unbound key does nothing.</param>
    /// <returns>True when the key was bound.</returns>
    public bool Unbind(ConsoleKey key) => _bindings.Remove(key);

    /// <summary>What a key asks for.</summary>
    /// <param name="key">The key to look up.</param>
    /// <param name="action">The action, when the key is bound.</param>
    /// <returns>True when the key is bound.</returns>
    public bool TryGetAction(ConsoleKey key, out InputAction action) => _bindings.TryGetValue(key, out action);

    /// <summary>Which keys ask for an action, sorted so a listing is reproducible.</summary>
    /// <param name="action">The action to look up.</param>
    /// <returns>The keys bound to it.</returns>
    public IReadOnlyList<ConsoleKey> KeysFor(InputAction action)
    {
        List<ConsoleKey> keys = [];
        foreach (KeyValuePair<ConsoleKey, InputAction> binding in _bindings)
        {
            if (binding.Value == action)
            {
                keys.Add(binding.Key);
            }
        }

        keys.Sort();
        return keys;
    }

    /// <summary>The keys for an action, as something to print on a title screen.</summary>
    /// <param name="action">The action to describe.</param>
    /// <returns>Text such as "LEFTARROW OR A", or "UNBOUND".</returns>
    public string Describe(InputAction action)
    {
        IReadOnlyList<ConsoleKey> keys = KeysFor(action);
        if (keys.Count == 0)
        {
            return "UNBOUND";
        }

        List<string> names = [];
        foreach (ConsoleKey key in keys)
        {
            names.Add(key.ToString().ToUpperInvariant());
        }

        return string.Join(" OR ", names);
    }
}
