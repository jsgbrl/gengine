// The real keyboard. Two guards matter here.
//
// KeyAvailable throws outright when standard input is a file rather than a terminal, which
// is what happens the moment anyone pipes the game anywhere - so it is asked only when input
// is not redirected. And ReadKey is given intercept: true so the key does not echo into the
// middle of the picture being drawn.

using System;

namespace GEngine.Input.Keyboard;

/// <summary>Reads key presses from the terminal.</summary>
public sealed class ConsoleKeyReader : IKeyReader
{
    /// <summary>The shared instance. It holds no state.</summary>
    public static ConsoleKeyReader Instance { get; } = new();

    /// <summary>False when input is a pipe or a file, in which case no key ever arrives.</summary>
    public static bool IsAvailable => !Console.IsInputRedirected;

    /// <inheritdoc/>
    public bool TryReadKey(out ConsoleKey key)
    {
        key = default;
        if (!IsAvailable || !Console.KeyAvailable)
        {
            return false;
        }

        key = Console.ReadKey(intercept: true).Key;
        return true;
    }
}
