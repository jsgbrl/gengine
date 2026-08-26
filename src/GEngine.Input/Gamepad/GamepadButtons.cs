// The buttons of a DualSense, as bits. One int carries the whole state of every button,
// which is what lets GamepadState be a value type small enough to hand around by copy.

using System;

namespace GEngine.Input.Gamepad;

/// <summary>Every button of a DualSense, as a bitmask.</summary>
[Flags]
public enum GamepadButtons
{
    /// <summary>Nothing is pressed.</summary>
    None = 0,

    /// <summary>The square, on the left of the face.</summary>
    Square = 1 << 0,

    /// <summary>The cross, at the bottom of the face. This is jump.</summary>
    Cross = 1 << 1,

    /// <summary>The circle, on the right of the face.</summary>
    Circle = 1 << 2,

    /// <summary>The triangle, at the top of the face.</summary>
    Triangle = 1 << 3,

    /// <summary>The upper left shoulder button.</summary>
    LeftShoulder = 1 << 4,

    /// <summary>The upper right shoulder button.</summary>
    RightShoulder = 1 << 5,

    /// <summary>The left trigger, as a button: pressed once it passes its threshold.</summary>
    LeftTrigger = 1 << 6,

    /// <summary>The right trigger, as a button.</summary>
    RightTrigger = 1 << 7,

    /// <summary>Create, where Select used to be.</summary>
    Create = 1 << 8,

    /// <summary>Options, where Start used to be. This is pause.</summary>
    Options = 1 << 9,

    /// <summary>Clicking the left stick.</summary>
    LeftStick = 1 << 10,

    /// <summary>Clicking the right stick.</summary>
    RightStick = 1 << 11,

    /// <summary>The PlayStation button.</summary>
    PlayStation = 1 << 12,

    /// <summary>Clicking the touchpad.</summary>
    Touchpad = 1 << 13,

    /// <summary>The microphone mute button.</summary>
    Mute = 1 << 14,
}
