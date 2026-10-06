// Escape sequences, and the two fallbacks.
//
// A terminal that cannot take twenty-four bits still gets the picture: the colour is mapped
// into the 256-colour cube, or into the sixteen colours every terminal has had for forty years.
// The mapping is arithmetic, not a table, so it is the same on Windows, macOS and Linux.

using System;
using GEngine.Core;

namespace GEngine.Rendering;

/// <summary>Writes ANSI escape sequences into an <see cref="AnsiBuffer"/>.</summary>
public static class AnsiEncoder
{
    /// <summary>Returns every attribute to its default.</summary>
    public const string Reset = "\u001b[0m";

    /// <summary>Hides the caret, so it does not blink over the picture.</summary>
    public const string HideCursor = "\u001b[?25l";

    /// <summary>Shows the caret again.</summary>
    public const string ShowCursor = "\u001b[?25h";

    /// <summary>Switches to the alternate screen, leaving the user's scrollback untouched.</summary>
    public const string EnterAlternateScreen = "\u001b[?1049h";

    /// <summary>Switches back, restoring whatever was on screen before.</summary>
    public const string LeaveAlternateScreen = "\u001b[?1049l";

    /// <summary>Clears the whole screen.</summary>
    public const string ClearScreen = "\u001b[2J";

    /// <summary>Moves the caret. Both coordinates are zero-based; the escape is one-based.</summary>
    /// <param name="buffer">Where to write.</param>
    /// <param name="column">Column, from the left.</param>
    /// <param name="row">Row, from the top.</param>
    public static void AppendCursorTo(AnsiBuffer buffer, int column, int row)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        buffer.Append("\u001b[");
        buffer.AppendNumber(row + 1);
        buffer.Append(';');
        buffer.AppendNumber(column + 1);
        buffer.Append('H');
    }

    /// <summary>Sets the foreground colour, at whatever depth the terminal accepts.</summary>
    /// <param name="buffer">Where to write.</param>
    /// <param name="color">The colour.</param>
    /// <param name="depth">How much colour the terminal takes.</param>
    public static void AppendForeground(AnsiBuffer buffer, Color color, ColorDepth depth) =>
        AppendColor(buffer, color, depth, isForeground: true);

    /// <summary>Sets the background colour, at whatever depth the terminal accepts.</summary>
    /// <param name="buffer">Where to write.</param>
    /// <param name="color">The colour.</param>
    /// <param name="depth">How much colour the terminal takes.</param>
    public static void AppendBackground(AnsiBuffer buffer, Color color, ColorDepth depth) =>
        AppendColor(buffer, color, depth, isForeground: false);

    /// <summary>Maps a colour into the xterm 256-colour palette.</summary>
    /// <param name="color">The colour.</param>
    /// <returns>The palette index, from 16 to 255.</returns>
    public static int ToPalette256(Color color)
    {
        if (color.R == color.G && color.G == color.B)
        {
            return 232 + Math.Clamp((color.R * 24) / 256, 0, 23);
        }

        int red = Level(color.R);
        int green = Level(color.G);
        int blue = Level(color.B);
        return 16 + (36 * red) + (6 * green) + blue;
    }

    /// <summary>
    /// Maps a colour onto the sixteen colours every terminal has, by finding the nearest one.
    /// A cheaper rule - one threshold per channel plus a brightness bit - is off by a whole
    /// hue on any colour that is not a corner of the cube: a light blue comes out white, and
    /// a mid grey comes out black.
    /// </summary>
    /// <param name="color">The colour.</param>
    /// <returns>The index, from 0 to 15.</returns>
    public static int ToBasic16(Color color)
    {
        int nearest = 0;
        int nearestDistance = int.MaxValue;
        for (int index = 0; index < Basic16.Length; index++)
        {
            int distance = SquaredDistance(color, Basic16[index]);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = index;
            }
        }

        return nearest;
    }

    // The sixteen colours as xterm renders them. Terminals differ by a few units on the dark
    // half, which never changes which of the sixteen is nearest.
    private static readonly Color[] Basic16 =
    [
        new(0, 0, 0),
        new(205, 0, 0),
        new(0, 205, 0),
        new(205, 205, 0),
        new(0, 0, 238),
        new(205, 0, 205),
        new(0, 205, 205),
        new(229, 229, 229),
        new(127, 127, 127),
        new(255, 0, 0),
        new(0, 255, 0),
        new(255, 255, 0),
        new(92, 92, 255),
        new(255, 0, 255),
        new(0, 255, 255),
        new(255, 255, 255),
    ];

    private static int SquaredDistance(Color left, Color right)
    {
        int red = left.R - right.R;
        int green = left.G - right.G;
        int blue = left.B - right.B;
        return (red * red) + (green * green) + (blue * blue);
    }

    private static void AppendColor(AnsiBuffer buffer, Color color, ColorDepth depth, bool isForeground)
    {
        ArgumentNullException.ThrowIfNull(buffer);
        buffer.Append("\u001b[");
        switch (depth)
        {
            case ColorDepth.TrueColor:
                AppendTrueColor(buffer, color, isForeground);
                break;
            case ColorDepth.Palette256:
                AppendIndexed(buffer, ToPalette256(color), isForeground);
                break;
            default:
                AppendBasic(buffer, ToBasic16(color), isForeground);
                break;
        }

        buffer.Append('m');
    }

    private static void AppendTrueColor(AnsiBuffer buffer, Color color, bool isForeground)
    {
        buffer.Append(isForeground ? "38;2;" : "48;2;");
        buffer.AppendNumber(color.R);
        buffer.Append(';');
        buffer.AppendNumber(color.G);
        buffer.Append(';');
        buffer.AppendNumber(color.B);
    }

    private static void AppendIndexed(AnsiBuffer buffer, int index, bool isForeground)
    {
        buffer.Append(isForeground ? "38;5;" : "48;5;");
        buffer.AppendNumber(index);
    }

    private static void AppendBasic(AnsiBuffer buffer, int index, bool isForeground)
    {
        int baseCode = isForeground ? 30 : 40;
        int brightOffset = index >= 8 ? 60 : 0;
        buffer.AppendNumber(baseCode + brightOffset + (index % 8));
    }

    private static int Level(byte channel) => Math.Clamp((channel * 6) / 256, 0, 5);
}
