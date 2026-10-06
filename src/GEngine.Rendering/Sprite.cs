// A sprite is a rectangle of colours with a name. It implements IPixelSource, which is the
// same contract a frame buffer offers a renderer, so drawing a sprite and presenting a
// frame are the same operation at two different scales.

using System;
using GEngine.Core;
using GEngine.Core.Contracts;

namespace GEngine.Rendering;

/// <summary>A named rectangle of pixels, loaded from a text file.</summary>
public sealed class Sprite : IPixelSource
{
    private readonly Color[] _pixels;

    /// <summary>Creates a sprite from its pixels, row by row.</summary>
    /// <param name="name">Name of the sprite, used in error messages.</param>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="pixels">Width times height colours, row-major.</param>
    /// <exception cref="ArgumentException">The pixel count does not match the size.</exception>
    public Sprite(string name, int width, int height, Color[] pixels)
    {
        ArgumentNullException.ThrowIfNull(pixels);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        if (pixels.Length != width * height)
        {
            throw new ArgumentException(
                "a " + width + " by " + height + " sprite needs " + (width * height) + " pixels",
                nameof(pixels));
        }

        Name = name;
        Width = width;
        Height = height;
        _pixels = pixels;
    }

    /// <summary>Name of the sprite.</summary>
    public string Name { get; }

    /// <inheritdoc/>
    public int Width { get; }

    /// <inheritdoc/>
    public int Height { get; }

    /// <inheritdoc/>
    public ReadOnlySpan<Color> Pixels => _pixels;

    /// <summary>The colour at a position, or transparent outside the sprite.</summary>
    /// <param name="x">Column, from the left.</param>
    /// <param name="y">Row, from the top.</param>
    /// <returns>The colour.</returns>
    public Color At(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return Color.Transparent;
        }

        return _pixels[(y * Width) + x];
    }

    /// <summary>A copy with the columns reversed, for a character that walks both ways.</summary>
    /// <returns>The mirrored sprite.</returns>
    public Sprite Mirrored()
    {
        var mirrored = new Color[_pixels.Length];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                mirrored[(y * Width) + x] = _pixels[(y * Width) + (Width - 1 - x)];
            }
        }

        return new Sprite(Name + "-mirrored", Width, Height, mirrored);
    }
}
