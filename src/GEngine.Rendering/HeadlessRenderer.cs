// A renderer that presents to nowhere and remembers what it was given. Every rendering test
// in the repository goes through this: a frame is drawn, presented, and then compared -
// which is how the drawing code is tested without a terminal, and how the allocation test
// can measure a frame without the console driver in the way.

using System;
using GEngine.Core;
using GEngine.Core.Contracts;

namespace GEngine.Rendering;

/// <summary>A renderer that keeps the last frame instead of showing it.</summary>
public sealed class HeadlessRenderer : IRenderer
{
    private readonly FrameBuffer _lastFrame;

    /// <summary>Creates a renderer with a fixed surface size.</summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    public HeadlessRenderer(int width, int height)
    {
        _lastFrame = new FrameBuffer(width, height);
    }

    /// <inheritdoc/>
    public int Width => _lastFrame.Width;

    /// <inheritdoc/>
    public int Height => _lastFrame.Height;

    /// <summary>The last frame presented, copied. Empty until the first present.</summary>
    public IPixelSource LastFrame => _lastFrame;

    /// <summary>How many frames have been presented.</summary>
    public int PresentCount { get; private set; }

    /// <inheritdoc/>
    public void Present(IPixelSource frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        _lastFrame.Clear(Color.Transparent);
        _lastFrame.Blit(frame, Vector2.Zero);
        PresentCount++;
    }

    /// <summary>Renders the last frame as text, for a snapshot assertion.</summary>
    /// <returns>One line per pixel row.</returns>
    public string Capture() => AsciiSnapshot.Capture(_lastFrame);

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
