// What a renderer is allowed to know about a frame: how big it is and where the pixels
// are. Nothing about how the frame was drawn, which is what keeps Core independent of
// GEngine.Rendering while still describing the contract between them.

using System;

namespace GEngine.Core.Contracts;

/// <summary>A rectangle of colours a renderer can read, laid out row by row.</summary>
public interface IPixelSource
{
    /// <summary>Width in pixels.</summary>
    int Width { get; }

    /// <summary>Height in pixels.</summary>
    int Height { get; }

    /// <summary>
    /// The pixels, row-major: the pixel at (x, y) is at index (y * Width) + x. Handed out
    /// as a span so presenting a frame costs one call and copies nothing.
    /// </summary>
    ReadOnlySpan<Color> Pixels { get; }
}
