// From a controller state to the actions the game asks about.
//
// The stick and the D-pad feed the same four movement actions, and the stronger of the two
// wins: a player may hold left on the D-pad while nudging the stick and get the D-pad's full
// left, which is what they meant. The buttons come straight from the map.

using System;
using System.Collections.Generic;
using GEngine.Core;
using GEngine.Core.Contracts;

namespace GEngine.Input.Gamepad;

/// <content>Turning a gamepad state into action values.</content>
public sealed partial class DualSenseGamepad
{
    /// <inheritdoc/>
    public bool IsDown(InputAction action) => _values[(int)action] >= 0.5f;

    /// <inheritdoc/>
    public float AxisValue(InputAction action) => _values[(int)action];

    private GamepadState ApplyDeadZones(GamepadState raw) => new(
        DeadZone.ApplyRadial(raw.LeftStick, DeadZoneRadius),
        DeadZone.ApplyRadial(raw.RightStick, DeadZoneRadius),
        new Vector2(DeadZone.ApplyLinear(raw.LeftTrigger), DeadZone.ApplyLinear(raw.RightTrigger)),
        raw.Buttons)
    {
        Hat = raw.Hat,
    };

    private void FillActions()
    {
        Array.Clear(_values);
        AddMovement(HatDirections.ToVector(State.Hat));
        AddMovement(State.LeftStick);
        foreach (KeyValuePair<GamepadButtons, InputAction> binding in Map.Bindings)
        {
            if (State.IsDown(binding.Key))
            {
                Report(binding.Value, 1.0f);
            }
        }
    }

    // One vector becomes up to two actions: the negative half of an axis is one action and
    // the positive half is another, which is what lets the player controller ask "how far
    // right?" without ever seeing a negative number.
    private void AddMovement(Vector2 direction)
    {
        Report(InputAction.MoveLeft, -direction.X);
        Report(InputAction.MoveRight, direction.X);
        Report(InputAction.MoveUp, -direction.Y);
        Report(InputAction.MoveDown, direction.Y);
    }

    private void Report(InputAction action, float value)
    {
        int index = (int)action;
        _values[index] = MathF.Max(_values[index], MathG.Clamp01(value));
    }
}
