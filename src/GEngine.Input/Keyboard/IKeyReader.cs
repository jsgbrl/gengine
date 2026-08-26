// Where key presses come from. The interface exists for one reason: System.Console reads the
// real keyboard, and a test needs to press keys without one.

using System;

namespace GEngine.Input.Keyboard;

/// <summary>A non-blocking source of key presses.</summary>
public interface IKeyReader
{
    /// <summary>Takes the next key press, if one is waiting.</summary>
    /// <param name="key">The key that was pressed.</param>
    /// <returns>False when nothing is waiting; the caller must not block.</returns>
    bool TryReadKey(out ConsoleKey key);
}
