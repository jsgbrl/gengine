// Windows.
//
// Escape sequences are not interpreted until the console mode says so, and asking takes three
// calls into kernel32: get the handle, read the mode, write it back with one more bit set.
// Windows Terminal has that bit on already; a plain conhost window does not, and without this a
// frame prints as a screenful of escape codes.

using System;
using System.IO;
using GEngine.Core.Contracts;
using GEngine.Rendering.Interop;

namespace GEngine.Rendering;

/// <summary>The console driver for Windows.</summary>
public sealed class WindowsConsoleDriver : ConsoleDriver
{
    /// <summary>Creates the driver.</summary>
    /// <param name="logger">Where it explains what it managed to turn on.</param>
    /// <param name="output">Where to write, or null to open the real standard output.</param>
    public WindowsConsoleDriver(ILogger logger, TextWriter? output = null)
        : base(logger, output)
    {
    }

    /// <inheritdoc/>
    public override string Name => "windows console";

    /// <summary>Turns on virtual terminal processing for the standard output handle.</summary>
    /// <param name="logger">Where to explain a failure.</param>
    /// <returns>True when escape sequences will now be interpreted.</returns>
    public static bool TryEnableVirtualTerminal(ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        if (!OperatingSystem.IsWindows())
        {
            return true;
        }

        IntPtr handle = NativeMethods.GetStdHandle(NativeMethods.StandardOutputHandle);
        if (!NativeMethods.GetConsoleMode(handle, out uint mode))
        {
            logger.Warning("standard output is not a console window, so colour stays at 16");
            return false;
        }

        if (NativeMethods.SetConsoleMode(handle, mode | NativeMethods.EnableVirtualTerminalProcessing))
        {
            return true;
        }

        logger.Warning("this console refuses virtual terminal sequences, so colour stays at 16");
        return false;
    }

    /// <inheritdoc/>
    protected override ColorDepth Negotiate()
    {
        if (!TryEnableVirtualTerminal(Logger))
        {
            return ColorDepth.Basic16;
        }

        // Windows Terminal sets neither COLORTERM nor TERM but does accept truecolor, so a
        // console that took the mode bit is credited with the colour as well.
        ColorDepth fromEnvironment = ConsoleCapabilities.DetectFromEnvironment();
        return fromEnvironment == ColorDepth.Basic16 ? ColorDepth.TrueColor : fromEnvironment;
    }
}
