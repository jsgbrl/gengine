// A five by seven font, in code. There is no font file to ship, no parser to write and no
// licence to check - and at this size a glyph is legible at one console pixel per dot,
// which after the half-block trick is half a character cell.
//
// It is a static class because there is exactly one font. A second one would be a second
// table of glyphs, and the day that exists is the day this grows an interface.

using System;
using GEngine.Core;

namespace GEngine.Rendering.Text;

/// <summary>The built-in bitmap font, and how to draw with it.</summary>
public static class BitmapFont
{
    /// <summary>Width of one glyph, in pixels.</summary>
    public const int GlyphWidth = BitmapFontGlyphs.Width;

    /// <summary>Height of one glyph, in pixels.</summary>
    public const int GlyphHeight = BitmapFontGlyphs.Height;

    /// <summary>Blank columns between two glyphs.</summary>
    public const int Spacing = 1;

    /// <summary>How much one character advances the pen, in pixels.</summary>
    public const int Advance = GlyphWidth + Spacing;

    /// <summary>How wide a string will be, in pixels.</summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>The width, including the gaps between glyphs but not a trailing one.</returns>
    public static int MeasureWidth(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return text.Length == 0 ? 0 : (text.Length * Advance) - Spacing;
    }

    /// <summary>True when the font has a glyph for a character.</summary>
    /// <param name="character">The character.</param>
    /// <returns>True when it can be drawn.</returns>
    public static bool Contains(char character) => BitmapFontGlyphs.All.ContainsKey(Normalise(character));

    /// <summary>True when one dot of a glyph is filled.</summary>
    /// <param name="character">The character.</param>
    /// <param name="column">Column inside the glyph, from the left.</param>
    /// <param name="row">Row inside the glyph, from the top.</param>
    /// <returns>True when the dot is part of the letter.</returns>
    public static bool IsSet(char character, int column, int row)
    {
        if (column < 0 || column >= GlyphWidth || row < 0 || row >= GlyphHeight)
        {
            return false;
        }

        return BitmapFontGlyphs.All.TryGetValue(Normalise(character), out string? glyph)
            && glyph[(row * GlyphWidth) + column] == '#';
    }

    /// <summary>Draws a line of text into a frame buffer.</summary>
    /// <param name="target">Where to draw.</param>
    /// <param name="text">What to write. Unknown characters are drawn as blanks.</param>
    /// <param name="position">Where the top-left corner of the first glyph goes.</param>
    /// <param name="color">The colour to draw with.</param>
    public static void DrawTo(FrameBuffer target, string text, Vector2 position, Color color)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(text);
        int originX = MathG.RoundToInt(position.X);
        int originY = MathG.RoundToInt(position.Y);
        for (int index = 0; index < text.Length; index++)
        {
            DrawGlyph(target, text[index], new Vector2(originX + (index * Advance), originY), color);
        }
    }

    private static void DrawGlyph(FrameBuffer target, char character, Vector2 origin, Color color)
    {
        int originX = (int)origin.X;
        int originY = (int)origin.Y;
        for (int row = 0; row < GlyphHeight; row++)
        {
            for (int column = 0; column < GlyphWidth; column++)
            {
                if (IsSet(character, column, row))
                {
                    target.SetPixel(originX + column, originY + row, color);
                }
            }
        }
    }

    // The font is upper case, so a lower-case letter is drawn as its capital rather than as
    // a blank. Only 'x' keeps its own glyph, because "3 x 100" reads badly in capitals.
    private static char Normalise(char character) =>
        character == 'x' ? character : char.ToUpperInvariant(character);
}
