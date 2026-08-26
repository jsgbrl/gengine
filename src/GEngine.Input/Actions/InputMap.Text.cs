// Remapping from a text file. One line per binding, key first, action second - the same
// shape as the sprite legend, because a repository with two file formats is a repository
// with two parsers to keep working.
//
//     # controls
//     LeftArrow  MoveLeft
//     Spacebar   Jump

using System;
using System.Collections.Generic;
using GEngine.Core.Contracts;

namespace GEngine.Input.Actions;

/// <content>Reading and writing the text form of a map.</content>
public sealed partial class InputMap
{
    /// <summary>Reads a map from text.</summary>
    /// <param name="text">The file contents.</param>
    /// <returns>The map.</returns>
    /// <exception cref="FormatException">A line is not a key and an action.</exception>
    public static InputMap Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var map = new InputMap();
        foreach (string line in Lines(text))
        {
            ReadBinding(map, line);
        }

        return map;
    }

    /// <summary>Writes the map back out, sorted, so saving and reloading is a no-op.</summary>
    /// <returns>The text form.</returns>
    public string ToText()
    {
        List<string> lines = [];
        foreach (InputAction action in InputActions.All)
        {
            foreach (ConsoleKey key in KeysFor(action))
            {
                lines.Add(key + " " + action);
            }
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static void ReadBinding(InputMap map, string line)
    {
        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
        {
            throw new FormatException("a binding is a key, a space and an action: " + line);
        }

        if (!Enum.TryParse(parts[0], ignoreCase: true, out ConsoleKey key))
        {
            throw new FormatException("no key is called " + parts[0]);
        }

        if (!Enum.TryParse(parts[1], ignoreCase: true, out InputAction action))
        {
            throw new FormatException("no action is called " + parts[1]);
        }

        map.Bind(key, action);
    }

    private static IEnumerable<string> Lines(string text)
    {
        foreach (string raw in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            string line = raw.Trim();
            if (line.Length > 0 && !line.StartsWith('#'))
            {
                yield return line;
            }
        }
    }
}
