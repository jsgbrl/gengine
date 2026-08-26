// Sprites are text. A legend maps one character to one colour, and the rows below it are
// the picture - so a sprite is edited in a notepad and a code review of one is a diff a
// human can read.
//
//     # mario, small
//     legend:
//     . transparent
//     R red
//     S skin
//     pixels:
//     ..RRR..
//     .RSSSR.
//
// A hash starts a comment, blank lines are ignored, and every pixel row must be the same
// width.

using System;
using System.Collections.Generic;
using System.Globalization;
using GEngine.Core;

namespace GEngine.Rendering;

/// <summary>Reads the text sprite format.</summary>
public static class SpriteParser
{
    /// <summary>Parses a sprite.</summary>
    /// <param name="name">Name to give the sprite.</param>
    /// <param name="text">The file contents.</param>
    /// <returns>The parsed sprite.</returns>
    /// <exception cref="FormatException">The text is not a well-formed sprite.</exception>
    public static Sprite Parse(string name, string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        Dictionary<char, Color> legend = [];
        List<string> rows = [];
        bool inPixels = false;
        foreach (string line in Lines(text))
        {
            inPixels = ReadLine(line, inPixels, legend, rows);
        }

        return Build(name, legend, rows);
    }

    /// <summary>Parses a colour written as a palette name or as six or eight hex digits.</summary>
    /// <param name="text">The colour, such as sky or E45C10 or E45C1080.</param>
    /// <returns>The colour.</returns>
    /// <exception cref="FormatException">The text is not a colour.</exception>
    public static Color ParseColor(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (Palette.TryGetByName(text, out Color named))
        {
            return named;
        }

        string digits = text.StartsWith('#') ? text[1..] : text;
        if (digits.Length is not (6 or 8))
        {
            throw new FormatException("a colour is a palette name or six or eight hex digits: " + text);
        }

        byte alpha = digits.Length == 8 ? Hex(digits, 6) : (byte)255;
        return new Color(Hex(digits, 0), Hex(digits, 2), Hex(digits, 4), alpha);
    }

    private static bool ReadLine(string line, bool inPixels, Dictionary<char, Color> legend, List<string> rows)
    {
        if (line.Equals("legend:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (line.Equals("pixels:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (inPixels)
        {
            rows.Add(line);
            return true;
        }

        ReadLegendEntry(line, legend);
        return false;
    }

    private static void ReadLegendEntry(string line, Dictionary<char, Color> legend)
    {
        string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 || parts[0].Length != 1)
        {
            throw new FormatException("a legend line is one character, a space and a colour: " + line);
        }

        legend[parts[0][0]] = ParseColor(parts[1]);
    }

    private static byte Hex(string digits, int offset) =>
        byte.Parse(digits.AsSpan(offset, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

    private static Sprite Build(string name, Dictionary<char, Color> legend, List<string> rows)
    {
        if (rows.Count == 0)
        {
            throw new FormatException(name + " has no pixels section");
        }

        int width = rows[0].Length;
        var pixels = new Color[width * rows.Count];
        for (int y = 0; y < rows.Count; y++)
        {
            ReadPixelRow(name, legend, rows[y], pixels.AsSpan(y * width, width));
        }

        return new Sprite(name, width, rows.Count, pixels);
    }

    private static void ReadPixelRow(string name, Dictionary<char, Color> legend, string row, Span<Color> target)
    {
        if (row.Length != target.Length)
        {
            throw new FormatException(name + " has rows of different widths: " + row);
        }

        for (int x = 0; x < row.Length; x++)
        {
            if (!legend.TryGetValue(row[x], out Color color))
            {
                throw new FormatException(name + " uses '" + row[x] + "', which the legend does not define");
            }

            target[x] = color;
        }
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
