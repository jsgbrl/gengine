// Linux. Escape sequences have worked here since before there was a Windows to enable them
// on, so there is nothing to turn on - but the depth is still checked and still announced,
// because "it just works" is not the same as "it works, and here is what you got".
//
// The Linux virtual console - TERM=linux, the one on a machine with no desktop - really does
// only have sixteen colours, so the check is not a formality.

using System.IO;
using GEngine.Core.Contracts;

namespace GEngine.Rendering;

/// <summary>The console driver for Linux.</summary>
public sealed class LinuxConsoleDriver : ConsoleDriver
{
    /// <summary>Creates the driver.</summary>
    /// <param name="logger">Where it explains what it found.</param>
    /// <param name="output">Where to write, or null to open the real standard output.</param>
    public LinuxConsoleDriver(ILogger logger, TextWriter? output = null)
        : base(logger, output)
    {
    }

    /// <inheritdoc/>
    public override string Name => "linux terminal";

    /// <inheritdoc/>
    protected override ColorDepth Negotiate() => ConsoleCapabilities.DetectFromEnvironment();
}
