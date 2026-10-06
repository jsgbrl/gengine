// What the terminal will accept, worked out from two environment variables. It is a pure
// function of its inputs so that all three systems - and every terminal on them - can be
// tested from one machine without a terminal at all.

using System;

namespace GEngine.Rendering;

/// <summary>Works out how much colour a terminal accepts.</summary>
public static class ConsoleCapabilities
{
    /// <summary>
    /// Reads the two variables terminals agree on. COLORTERM is set to truecolor or 24bit by
    /// terminals that mean it; TERM carries 256color on the rest. Anything else gets the
    /// sixteen colours that have worked everywhere since the 1980s.
    /// </summary>
    /// <param name="colorTerm">The COLORTERM variable, or null when it is unset.</param>
    /// <param name="term">The TERM variable, or null when it is unset.</param>
    /// <returns>The depth to use.</returns>
    public static ColorDepth Detect(string? colorTerm, string? term)
    {
        if (Mentions(colorTerm, "truecolor") || Mentions(colorTerm, "24bit"))
        {
            return ColorDepth.TrueColor;
        }

        if (Mentions(term, "256color") || Mentions(colorTerm, "256"))
        {
            return ColorDepth.Palette256;
        }

        return ColorDepth.Basic16;
    }

    /// <summary>Reads the two variables from the current process.</summary>
    /// <returns>The depth to use.</returns>
    public static ColorDepth DetectFromEnvironment() =>
        Detect(Environment.GetEnvironmentVariable("COLORTERM"), Environment.GetEnvironmentVariable("TERM"));

    /// <summary>A sentence explaining a depth, for the log and for the title screen.</summary>
    /// <param name="depth">The colour depth to describe.</param>
    /// <returns>The explanation.</returns>
    public static string Describe(ColorDepth depth) => depth switch
    {
        ColorDepth.TrueColor => "truecolor: 24 bits, the colour asked for is the colour drawn",
        ColorDepth.Palette256 => "256 colours: mapped into the xterm cube, set COLORTERM=truecolor for more",
        _ => "16 colours: the safe fallback, set TERM to a 256color variant for more",
    };

    private static bool Mentions(string? value, string what) =>
        value is not null && value.Contains(what, StringComparison.OrdinalIgnoreCase);
}
