// How many colours the terminal will accept. Not every terminal takes twenty-four bits, and
// the ones that do not must still get a picture - the same picture, on all three systems,
// with a line in the log saying what happened.

namespace GEngine.Rendering;

/// <summary>How much colour a terminal can be given.</summary>
public enum ColorDepth
{
    /// <summary>The sixteen colours every terminal has had since the 1980s.</summary>
    Basic16,

    /// <summary>The xterm 256-colour cube plus its greyscale ramp.</summary>
    Palette256,

    /// <summary>Twenty-four bits: the colour asked for is the colour drawn.</summary>
    TrueColor,
}
