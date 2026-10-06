// A frame as text, so a rendering test can assert on a picture a human can read in the
// failure message. Brightness is mapped onto a ramp of characters: a test that says the
// player is at the left of the screen looks like the player being at the left of the screen.

using System;
using System.Text;
using GEngine.Core;
using GEngine.Core.Contracts;

namespace GEngine.Rendering;

/// <summary>Turns a frame into readable text.</summary>
public static class AsciiSnapshot
{
    /// <summary>Dark to light. A transparent pixel is the first character.</summary>
    public const string DefaultRamp = " .:-=+*#%@";

    /// <summary>Captures a frame using the default ramp.</summary>
    /// <param name="frame">The pixels to read.</param>
    /// <returns>One line per pixel row, joined with newlines.</returns>
    public static string Capture(IPixelSource frame) => Capture(frame, DefaultRamp);

    /// <summary>Captures a frame using a ramp of characters, darkest first.</summary>
    /// <param name="frame">The pixels to read.</param>
    /// <param name="ramp">The characters to map brightness onto.</param>
    /// <returns>One line per pixel row, joined with newlines.</returns>
    public static string Capture(IPixelSource frame, string ramp)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentException.ThrowIfNullOrEmpty(ramp);
        var text = new StringBuilder(frame.Width * frame.Height);
        for (int y = 0; y < frame.Height; y++)
        {
            AppendRow(frame, y, ramp, text);
        }

        return text.ToString().TrimEnd('\n');
    }

    /// <summary>The character a colour maps to.</summary>
    /// <param name="color">The colour.</param>
    /// <param name="ramp">The characters to map brightness onto, darkest first.</param>
    /// <returns>One character from the ramp.</returns>
    public static char CharacterFor(Color color, string ramp)
    {
        ArgumentException.ThrowIfNullOrEmpty(ramp);
        if (color.IsTransparent)
        {
            return ramp[0];
        }

        int index = MathG.RoundToInt(Brightness(color) * (ramp.Length - 1));
        return ramp[Math.Clamp(index, 0, ramp.Length - 1)];
    }

    /// <summary>How bright a colour is, from zero to one, weighted the way an eye sees.</summary>
    /// <param name="color">The colour.</param>
    /// <returns>The brightness.</returns>
    public static float Brightness(Color color) =>
        ((0.299f * color.R) + (0.587f * color.G) + (0.114f * color.B)) / 255.0f;

    private static void AppendRow(IPixelSource frame, int y, string ramp, StringBuilder text)
    {
        ReadOnlySpan<Color> row = frame.Pixels.Slice(y * frame.Width, frame.Width);
        for (int x = 0; x < row.Length; x++)
        {
            text.Append(CharacterFor(row[x], ramp));
        }

        text.Append('\n');
    }
}
