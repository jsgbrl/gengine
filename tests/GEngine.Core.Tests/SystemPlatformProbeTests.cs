using System;
using GEngine.Core.Platform;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>
/// Covers <see cref="SystemPlatformProbe"/>. The expected answer is computed the same way
/// the test suite would have to compute it anyway, so the test says something true on all
/// three systems rather than only on the one it was written on.
/// </summary>
public sealed class SystemPlatformProbeTests
{
    [Test]
    public void ItAgreesWithTheRuntime()
    {
        PlatformKind expected = PlatformKind.Unknown;
        if (OperatingSystem.IsWindows())
        {
            expected = PlatformKind.Windows;
        }
        else if (OperatingSystem.IsMacOS())
        {
            expected = PlatformKind.MacOs;
        }
        else if (OperatingSystem.IsLinux())
        {
            expected = PlatformKind.Linux;
        }

        Assert.AreEqual(expected, SystemPlatformProbe.Instance.Kind);
    }

    [Test]
    public void ItReportsOneOfTheThreeSystemsTheEngineSupports()
    {
        Assert.AreNotEqual(PlatformKind.Unknown, SystemPlatformProbe.Instance.Kind);
    }

    [Test]
    public void TheDescriptionNamesTheSystemAndTheArchitecture()
    {
        string description = SystemPlatformProbe.Instance.Description;
        Assert.IsTrue(description.Length > 0);
        Assert.IsTrue(description.Contains('/', StringComparison.Ordinal));
    }

    [Test]
    public void Instance_IsShared_BecauseTheAnswerCannotChange()
    {
        Assert.AreSame(SystemPlatformProbe.Instance, SystemPlatformProbe.Instance);
    }
}
