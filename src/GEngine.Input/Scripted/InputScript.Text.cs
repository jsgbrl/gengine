// Reading and writing the text form of a script. Same shape as the input map and the sprite
// legend: a hash starts a comment, blank lines are ignored, and everything else is one line
// of data.

using System;
using System.Collections.Generic;
using System.Globalization;
using GEngine.Core.Contracts;

namespace GEngine.Input.Scripted;

/// <content>The text form of a script.</content>
public sealed partial class InputScript
{
    /// <summary>Reads a script from text.</summary>
    /// <param name="text">The file contents.</param>
    /// <returns>The script.</returns>
    /// <exception cref="FormatException">A line is not a frame range and one or more actions.</exception>
    public static InputScript Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        var script = new InputScript();
        foreach (string line in Lines(text))
        {
            ReadHold(script, line);
        }

        return script;
    }

    private static void ReadHold(InputScript script, string line)
    {
        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            throw new FormatException("a hold is a frame range and at least one action: " + line);
        }

        (int first, int last) = ReadRange(parts[0]);
        for (int index = 1; index < parts.Length; index++)
        {
            if (!Enum.TryParse(parts[index], ignoreCase: true, out InputAction action))
            {
                throw new FormatException("no action is called " + parts[index]);
            }

            script.Hold(action, first, last);
        }
    }

    private static (int First, int Last) ReadRange(string text)
    {
        string[] ends = text.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (ends.Length is not (1 or 2) || !int.TryParse(ends[0], CultureInfo.InvariantCulture, out int first))
        {
            throw new FormatException("a frame range is 12 or 12-34: " + text);
        }

        if (ends.Length == 1)
        {
            return (first, first);
        }

        if (!int.TryParse(ends[1], CultureInfo.InvariantCulture, out int last))
        {
            throw new FormatException("a frame range is 12 or 12-34: " + text);
        }

        return (first, last);
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
