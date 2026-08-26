// Everything about the terminal that differs between operating systems, behind one
// interface - which is rule 4 of the build prompt, and the reason there is no conditional
// compilation anywhere in the repository.
//
// A driver is responsible for exactly three things: saying how much colour the terminal
// accepts, saying how big it is, and putting the terminal into a state the renderer can
// draw on - and then putting it back, whatever happened.

using System;
using System.IO;

namespace GEngine.Rendering;

/// <summary>The terminal, as the renderer needs it.</summary>
public interface IConsoleDriver : IDisposable
{
    /// <summary>Human-readable name, shown in diagnostics.</summary>
    string Name { get; }

    /// <summary>How much colour this terminal accepts. Only meaningful after <see cref="Enable"/>.</summary>
    ColorDepth Depth { get; }

    /// <summary>How many character cells across.</summary>
    int Columns { get; }

    /// <summary>How many character cells down.</summary>
    int Rows { get; }

    /// <summary>Where the renderer writes. Buffered, and flushed once per frame.</summary>
    TextWriter Output { get; }

    /// <summary>
    /// Puts the terminal into drawing mode: virtual terminal sequences on, alternate screen,
    /// cursor hidden, UTF-8 output. Safe to call twice.
    /// </summary>
    void Enable();

    /// <summary>
    /// Puts the terminal back exactly as it was found. Safe to call twice, and safe to call
    /// when <see cref="Enable"/> never ran or failed halfway.
    /// </summary>
    void Restore();
}
