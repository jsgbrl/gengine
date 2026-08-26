// The verbs of the game. Nothing above this layer ever names a key or a button: the
// player controller asks whether Jump is down, and whether that came from the space bar
// or from Cross on a DualSense is the input layer's problem. That is the Command pattern
// with the command reduced to its smallest possible form, an enum member.

namespace GEngine.Core.Contracts;

/// <summary>Something the player can ask the game to do.</summary>
public enum InputAction
{
    /// <summary>Move left.</summary>
    MoveLeft,

    /// <summary>Move right.</summary>
    MoveRight,

    /// <summary>Move up: menus, and looking up.</summary>
    MoveUp,

    /// <summary>Move down: menus, and ducking.</summary>
    MoveDown,

    /// <summary>Jump. Held longer means higher.</summary>
    Jump,

    /// <summary>Run while held.</summary>
    Run,

    /// <summary>Pause and unpause.</summary>
    Pause,

    /// <summary>Accept a menu choice.</summary>
    Confirm,

    /// <summary>Go back, or quit from the title screen.</summary>
    Cancel,
}
