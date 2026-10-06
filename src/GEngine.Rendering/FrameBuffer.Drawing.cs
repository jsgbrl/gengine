// Everything that draws more than one pixel. Each operation works out the clipped range
// first and then writes without a bounds check per pixel, which is the difference between
// a copy that costs one comparison per pixel and one that costs five.

using System;
using GEngine.Core;
using GEngine.Core.Contracts;
using GEngine.Rendering.Text;

namespace GEngine.Rendering;

/// <content>Filled shapes, sprites, text, and copying one pixel source over another.</content>
public sealed partial class FrameBuffer
{
    /// <summary>Fills a rectangle.</summary>
    /// <param name="rectangle">The rectangle in buffer pixels. Fractions are floored.</param>
    /// <param name="color">The colour to fill with.</param>
    public void DrawRectangle(Aabb rectangle, Color color)
    {
        int left = Math.Max(MathG.FloorToInt(rectangle.Left), 0);
        int right = Math.Min(MathG.CeilToInt(rectangle.Right), Width);
        int top = Math.Max(MathG.FloorToInt(rectangle.Top), 0);
        int bottom = Math.Min(MathG.CeilToInt(rectangle.Bottom), Height);
        for (int y = top; y < bottom; y++)
        {
            FillRow(y, left, right, color);
        }
    }

    /// <summary>Draws the four edges of a rectangle, one pixel thick.</summary>
    /// <param name="rectangle">The rectangle in buffer pixels.</param>
    /// <param name="color">The colour to draw with.</param>
    public void DrawRectangleOutline(Aabb rectangle, Color color)
    {
        int left = MathG.FloorToInt(rectangle.Left);
        int right = MathG.CeilToInt(rectangle.Right) - 1;
        int top = MathG.FloorToInt(rectangle.Top);
        int bottom = MathG.CeilToInt(rectangle.Bottom) - 1;
        for (int x = left; x <= right; x++)
        {
            SetPixel(x, top, color);
            SetPixel(x, bottom, color);
        }

        for (int y = top; y <= bottom; y++)
        {
            SetPixel(left, y, color);
            SetPixel(right, y, color);
        }
    }

    /// <summary>Draws a sprite with its top-left corner at a position.</summary>
    /// <param name="sprite">The sprite to draw; its transparent pixels are skipped.</param>
    /// <param name="position">Where the top-left corner goes, in buffer pixels.</param>
    public void DrawSprite(Sprite sprite, Vector2 position) => DrawPixels(sprite, position);

    /// <summary>Copies any pixel source over this one, blending as it goes.</summary>
    /// <param name="source">What to copy.</param>
    /// <param name="position">Where the top-left corner goes, in buffer pixels.</param>
    public void DrawPixels(IPixelSource source, Vector2 position)
    {
        ArgumentNullException.ThrowIfNull(source);
        int originX = MathG.RoundToInt(position.X);
        int originY = MathG.RoundToInt(position.Y);
        int firstX = Math.Max(0, -originX);
        int lastX = Math.Min(source.Width, Width - originX);
        int lastY = Math.Min(source.Height, Height - originY);
        for (int y = Math.Max(0, -originY); y < lastY; y++)
        {
            ReadOnlySpan<Color> row = source.Pixels.Slice(y * source.Width, source.Width);
            for (int x = firstX; x < lastX; x++)
            {
                SetPixel(originX + x, originY + y, row[x]);
            }
        }
    }

    /// <summary>Draws a line of text in the built-in font.</summary>
    /// <param name="text">What to write. Characters the font does not have become blanks.</param>
    /// <param name="position">Where the top-left corner of the first glyph goes.</param>
    /// <param name="color">The colour to draw with.</param>
    public void DrawText(string text, Vector2 position, Color color) =>
        PixelFont.DrawTo(this, text, position, color);

    private void FillRow(int y, int left, int right, Color color)
    {
        for (int x = left; x < right; x++)
        {
            SetPixel(x, y, color);
        }
    }
}
