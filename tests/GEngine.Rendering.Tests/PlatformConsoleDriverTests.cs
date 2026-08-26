using System;
using System.IO;
using GEngine.Core.Contracts;
using GEngine.Core.Platform;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>
/// Covers the three platform drivers and <see cref="ConsoleDriverFactory"/>. All three are
/// built and inspected from whichever machine runs the suite: only the Windows one calls
/// into an operating system, and it checks first.
/// </summary>
public sealed class PlatformConsoleDriverTests
{
    private MemoryLogger _logger = new();

    [Setup]
    public void Setup() => _logger = new MemoryLogger();

    [Test]
    public void EachDriverNamesItsSystem()
    {
        using var windows = new WindowsConsoleDriver(_logger, new StringWriter());
        using var mac = new MacOsConsoleDriver(_logger, new StringWriter());
        using var linux = new LinuxConsoleDriver(_logger, new StringWriter());
        Assert.AreEqual("windows console", windows.Name);
        Assert.AreEqual("macos terminal", mac.Name);
        Assert.AreEqual("linux terminal", linux.Name);
    }

    [Test]
    public void TheFactoryPicksTheDriverForTheProbedPlatform()
    {
        AssertDriver(PlatformKind.Windows, typeof(WindowsConsoleDriver));
        AssertDriver(PlatformKind.MacOs, typeof(MacOsConsoleDriver));
        AssertDriver(PlatformKind.Linux, typeof(LinuxConsoleDriver));
    }

    [Test]
    public void TheFactoryRefusesAnUnknownPlatformInsteadOfGuessing()
    {
        Assert.Throws<PlatformNotSupportedException>(
            () => ConsoleDriverFactory.Create(new FixedPlatformProbe(PlatformKind.Unknown), _logger));
    }

    [Test]
    public void EveryDriverStartsAtTheSafeDepthBeforeItNegotiates()
    {
        using var linux = new LinuxConsoleDriver(_logger, new StringWriter());
        Assert.AreEqual(ColorDepth.Basic16, linux.Depth);
    }

    [Test]
    public void EnablingADriverNegotiatesADepthAndSaysSo()
    {
        using var linux = new LinuxConsoleDriver(_logger, new StringWriter());
        linux.Enable();
        Assert.IsTrue(linux.Depth is ColorDepth.Basic16 or ColorDepth.Palette256 or ColorDepth.TrueColor);
        Assert.IsTrue(_logger.Contains("linux terminal"));
    }

    [Test]
    public void TryEnableVirtualTerminal_IsHarmlessOffWindows_AndReportsOnIt()
    {
        bool enabled = WindowsConsoleDriver.TryEnableVirtualTerminal(_logger);
        if (!OperatingSystem.IsWindows())
        {
            Assert.IsTrue(enabled, "there is nothing to enable, so nothing failed");
            return;
        }

        Assert.IsTrue(enabled || _logger.Contains("colour stays at 16"), "either it worked or it said why");
    }

    private void AssertDriver(PlatformKind kind, Type expected)
    {
        using IConsoleDriver driver = ConsoleDriverFactory.Create(new FixedPlatformProbe(kind), _logger);
        Assert.AreEqual(expected.Name, driver.GetType().Name);
    }
}
