// A renderer takes a finished frame and puts it somewhere: a terminal, a string, nowhere.
// It never draws. Drawing is the frame buffer's job, and keeping the two apart is why the
// same game code can be tested headless and played on a console.

using System;

namespace GEngine.Core.Contracts;

/// <summary>Somewhere a finished frame can be presented.</summary>
public interface IRenderer : IDisposable
{
    /// <summary>Width of the surface, in pixels. May change when the terminal is resized.</summary>
    int Width { get; }

    /// <summary>Height of the surface, in pixels. May change when the terminal is resized.</summary>
    int Height { get; }

    /// <summary>Puts a frame on the surface.</summary>
    /// <param name="frame">The pixels to present.</param>
    void Present(IPixelSource frame);
}
