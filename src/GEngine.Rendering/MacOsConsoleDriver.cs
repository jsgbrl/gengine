// macOS. Escape sequences are interpreted out of the box here too, and the depth comes from
// the same two variables - but the default terminal is worth a word of its own.
//
// Terminal.app sets TERM=xterm-256color and does not set COLORTERM, because it genuinely
// stops at 256 colours; iTerm2, Ghostty and WezTerm set COLORTERM=truecolor and mean it. So
// on a stock Mac the game runs at 256 colours and says so, which is the honest answer rather
// than a screen of colours that are quietly wrong.

using System.IO;
using GEngine.Core.Contracts;

namespace GEngine.Rendering;

/// <summary>The console driver for macOS.</summary>
public sealed class MacOsConsoleDriver : ConsoleDriver
{
    /// <summary>Creates the driver.</summary>
    /// <param name="logger">Where it explains what it found.</param>
    /// <param name="output">Where to write, or null to open the real standard output.</param>
    public MacOsConsoleDriver(ILogger logger, TextWriter? output = null)
        : base(logger, output)
    {
    }

    /// <inheritdoc/>
    public override string Name => "macos terminal";

    /// <inheritdoc/>
    protected override ColorDepth Negotiate() => ConsoleCapabilities.DetectFromEnvironment();
}
