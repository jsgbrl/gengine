using System;
using GEngine.Core.Platform;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>
/// Covers <see cref="PlatformFactory"/>. All three platform paths are exercised from
/// whichever machine runs the suite, which is the entire reason the probe is injected
/// instead of being asked for directly.
/// </summary>
public sealed class PlatformFactoryTests
{
    private static readonly PlatformChoices<string> Choices =
        new(static () => "windows", static () => "macos", static () => "linux");

    [TestCase(PlatformKind.Windows, "windows")]
    [TestCase(PlatformKind.MacOs, "macos")]
    [TestCase(PlatformKind.Linux, "linux")]
    public void ItBuildsTheImplementationForTheProbedPlatform(PlatformKind kind, string expected)
    {
        Assert.AreEqual(expected, PlatformFactory.Create(new FixedPlatformProbe(kind), Choices));
    }

    [Test]
    public void AnUnknownPlatform_FailsWithAMessageNamingWhatItSaw()
    {
        var probe = new FixedPlatformProbe(PlatformKind.Unknown, "SomeOtherOS 1.0");
        PlatformNotSupportedException failure = Assert.Throws<PlatformNotSupportedException>(
            () => PlatformFactory.Create(probe, Choices));
        Assert.IsTrue(failure.Message.Contains("SomeOtherOS 1.0", StringComparison.Ordinal));
    }

    [Test]
    public void OnlyTheChosenFactoryRuns()
    {
        int windowsCalls = 0;
        int linuxCalls = 0;
        var counted = new PlatformChoices<string>(
            () => { windowsCalls++; return "windows"; },
            () => "macos",
            () => { linuxCalls++; return "linux"; });

        PlatformFactory.Create(new FixedPlatformProbe(PlatformKind.Linux), counted);
        Assert.AreEqual(0, windowsCalls);
        Assert.AreEqual(1, linuxCalls);
    }
}
