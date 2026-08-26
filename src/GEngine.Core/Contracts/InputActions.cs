// The list of actions, written out by hand.
//
// Enum.GetValues would produce the same list with one line of reflection, and rule 6 of the
// build prompt keeps reflection to the test runner and the resource loader. Writing them out
// also means an action added to the enum and forgotten here is caught by a test rather than
// by a control that silently does nothing.

using System.Collections.Generic;

namespace GEngine.Core.Contracts;

/// <summary>Every <see cref="InputAction"/>, in declaration order.</summary>
public static class InputActions
{
    /// <summary>How many actions there are. Array sizes come from here.</summary>
    public const int Count = 9;

    /// <summary>Every action, in declaration order.</summary>
    public static IReadOnlyList<InputAction> All { get; } =
    [
        InputAction.MoveLeft,
        InputAction.MoveRight,
        InputAction.MoveUp,
        InputAction.MoveDown,
        InputAction.Jump,
        InputAction.Run,
        InputAction.Pause,
        InputAction.Confirm,
        InputAction.Cancel,
    ];
}
