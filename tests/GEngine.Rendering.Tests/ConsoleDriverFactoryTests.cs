using System;
using GEngine.Core.Contracts;
using GEngine.Core.Platform;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>
/// The one place that decides which terminal this is. It is a factory rather than three `if`s
/// scattered through the renderer because rule 5 says the three systems are peers - and a peer
/// relationship is much easier to keep when the choice is made exactly once, in a file whose
/// whole content is the choice.
///
/// Only the Windows branch was verified by running it. macOS and Linux are checked here through
/// a fixed probe, which proves the wiring and not the terminal; docs/platforms.md says so.
/// </summary>
public sealed class ConsoleDriverFactoryTests
{
    [TestCase(PlatformKind.Windows, "WindowsConsoleDriver")]
    [TestCase(PlatformKind.MacOs, "MacOsConsoleDriver")]
    [TestCase(PlatformKind.Linux, "LinuxConsoleDriver")]
    public void EachPlatformGetsItsOwnDriver(PlatformKind kind, string expected)
    {
        using IConsoleDriver driver = Create(kind);
        Assert.AreEqual(expected, driver.GetType().Name);
    }

    // A system nobody wrote a driver for is refused by name rather than guessed at. Guessing
    // would mean a fourth system silently getting whichever driver happened to be last in the
    // list, and finding out during a game; this way it fails at startup and says what it saw.
    [Test]
    public void AnUnknownSystemIsRefusedByName()
    {
        PlatformNotSupportedException refused = Assert.Throws<PlatformNotSupportedException>(
            () => Create(PlatformKind.Unknown).Dispose());
        Assert.IsTrue(refused.Message.Contains("Windows", StringComparison.Ordinal), refused.Message);
        Assert.IsTrue(refused.Message.Contains("a curious system", StringComparison.Ordinal), refused.Message);
    }

    [Test]
    public void EveryDriverSaysWhatItIs()
    {
        foreach (PlatformKind kind in new[] { PlatformKind.Windows, PlatformKind.MacOs, PlatformKind.Linux })
        {
            CheckDescribes(kind);
        }
    }

    // The factory hands its logger to whichever driver it builds, so that what a terminal
    // managed to turn on is reported by the terminal rather than guessed at by the caller.
    // What the driver then says is PlatformConsoleDriverTests' business - it builds the drivers
    // itself, with a StringWriter, because a driver given the real console enables the alternate
    // screen and the test run disappears.
    [Test]
    public void TheFactoryHandsItsLoggerToTheDriverItBuilds()
    {
        var logger = new MemoryLogger();
        using IConsoleDriver driver = ConsoleDriverFactory.Create(new FixedPlatformProbe(PlatformKind.Linux), logger);
        Assert.IsNotNull(driver);
        Assert.AreEqual(0, logger.Messages.Count, "building one says nothing; enabling one does");
    }

    [Test]
    public void ItRefusesToChooseWithoutAProbe()
    {
        Assert.Throws<ArgumentNullException>(() => ConsoleDriverFactory.Create(null!, NullLogger.Instance));
    }

    private static void CheckDescribes(PlatformKind kind)
    {
        using IConsoleDriver driver = Create(kind);
        Assert.IsFalse(string.IsNullOrWhiteSpace(driver.Name), kind + " has a driver with no name");
    }

    private static IConsoleDriver Create(PlatformKind kind) =>
        ConsoleDriverFactory.Create(new FixedPlatformProbe(kind, "a curious system"), NullLogger.Instance);
}
