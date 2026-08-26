using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>
/// Covers <see cref="ConsoleCapabilities"/>. Every terminal on every one of the three systems
/// is exercised from whichever machine runs the suite, because the detection is a pure
/// function of two strings and nothing else.
/// </summary>
public sealed class ConsoleCapabilitiesTests
{
    [TestCase("truecolor", "xterm-256color")]
    [TestCase("24bit", "xterm")]
    [TestCase("TRUECOLOR", null)]
    public void ATerminalThatClaimsTwentyFourBits_GetsThem(string? colorTerm, string? term)
    {
        Assert.AreEqual(ColorDepth.TrueColor, ConsoleCapabilities.Detect(colorTerm, term));
    }

    [TestCase(null, "xterm-256color")]
    [TestCase(null, "screen-256color")]
    [TestCase("256", "xterm")]
    public void ATerminalThatClaims256_GetsThePalette(string? colorTerm, string? term)
    {
        Assert.AreEqual(ColorDepth.Palette256, ConsoleCapabilities.Detect(colorTerm, term));
    }

    [TestCase(null, null)]
    [TestCase(null, "linux")]
    [TestCase(null, "dumb")]
    [TestCase("", "vt100")]
    public void EverythingElse_GetsTheSixteenThatAlwaysWork(string? colorTerm, string? term)
    {
        Assert.AreEqual(ColorDepth.Basic16, ConsoleCapabilities.Detect(colorTerm, term));
    }

    [Test]
    public void MacTerminalDotAppLooksLike256_AndITermLooksLikeTruecolor()
    {
        Assert.AreEqual(ColorDepth.Palette256, ConsoleCapabilities.Detect(null, "xterm-256color"));
        Assert.AreEqual(ColorDepth.TrueColor, ConsoleCapabilities.Detect("truecolor", "xterm-256color"));
    }

    [Test]
    public void DetectFromEnvironment_ReturnsOneOfTheThreeDepths()
    {
        ColorDepth depth = ConsoleCapabilities.DetectFromEnvironment();
        Assert.IsTrue(depth is ColorDepth.Basic16 or ColorDepth.Palette256 or ColorDepth.TrueColor);
    }

    [Test]
    public void EveryDepthHasAnExplanationThatNamesTheWayOut()
    {
        Assert.IsTrue(ConsoleCapabilities.Describe(ColorDepth.TrueColor).Length > 0);
        Assert.IsTrue(ConsoleCapabilities.Describe(ColorDepth.Palette256).Contains("COLORTERM", System.StringComparison.Ordinal));
        Assert.IsTrue(ConsoleCapabilities.Describe(ColorDepth.Basic16).Contains("TERM", System.StringComparison.Ordinal));
    }
}
