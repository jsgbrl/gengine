// The canvas. One flat array of colours, row-major, and every drawing operation clips
// itself against all four edges before it writes - because the alternative is a sprite that
// walks off the left of the screen and reappears on the right of the row above.

using System;
using GEngine.Core;
using GEngine.Core.Contracts;

namespace GEngine.Rendering;

/// <summary>A rectangle of pixels that can be drawn into and then presented.</summary>
public sealed partial class FrameBuffer : IPixelSource
{
    private Color[] _pixels;

    /// <summary>Creates a frame buffer.</summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public FrameBuffer(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        Width = width;
        Height = height;
        _pixels = new Color[width * height];
    }

    /// <inheritdoc/>
    public int Width { get; private set; }

    /// <inheritdoc/>
    public int Height { get; private set; }

    /// <inheritdoc/>
    public ReadOnlySpan<Color> Pixels => _pixels;

    /// <summary>The whole buffer as a box, which is what every clip is measured against.</summary>
    public Aabb Bounds => new(Vector2.Zero, new Vector2(Width, Height));

    /// <summary>Overwrites every pixel, ignoring alpha.</summary>
    /// <param name="color">The colour to fill with.</param>
    public void Clear(Color color) => Array.Fill(_pixels, color);

    /// <summary>
    /// Draws one pixel, blended over what is already there, and does nothing at all when the
    /// position is outside the buffer. Silently ignoring the outside is the whole clipping
    /// strategy: every other operation is written in terms of this one.
    /// </summary>
    /// <param name="x">Column, from the left.</param>
    /// <param name="y">Row, from the top.</param>
    /// <param name="color">The colour to draw.</param>
    public void SetPixel(int x, int y, Color color)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height || color.IsTransparent)
        {
            return;
        }

        int index = (y * Width) + x;
        _pixels[index] = color.IsOpaque ? color : Color.Over(color, _pixels[index]);
    }

    /// <summary>The colour at a position, or transparent outside the buffer.</summary>
    /// <param name="x">Column, from the left.</param>
    /// <param name="y">Row, from the top.</param>
    /// <returns>The colour.</returns>
    public Color GetPixel(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return Color.Transparent;
        }

        return _pixels[(y * Width) + x];
    }

    /// <summary>Changes the size, discarding the contents.</summary>
    /// <param name="width">New width in pixels.</param>
    /// <param name="height">New height in pixels.</param>
    public void Resize(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        if (width == Width && height == Height)
        {
            return;
        }

        Width = width;
        Height = height;
        _pixels = new Color[width * height];
    }
}
