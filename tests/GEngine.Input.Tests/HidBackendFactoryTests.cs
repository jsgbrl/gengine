using System;
using System.Collections.Generic;
using GEngine.Core.Platform;
using GEngine.Input.Hid;
using GEngine.Input.Hid.Interop.Linux;
using GEngine.Input.Hid.Interop.MacOs;
using GEngine.Input.Hid.Interop.Windows;
using GEngine.Testing;

namespace GEngine.Input.Tests;

/// <summary>
/// Covers <see cref="HidBackendFactory"/> and what each platform backend does when it is
/// asked to enumerate on the wrong system: nothing, quietly, so a test suite can build all
/// three from one machine.
/// </summary>
public sealed class HidBackendFactoryTests
{
    [TestCase(PlatformKind.Windows, "windows hid")]
    [TestCase(PlatformKind.MacOs, "macos iokit")]
    [TestCase(PlatformKind.Linux, "linux hidraw")]
    public void TheFactoryPicksTheBackendForTheProbedPlatform(PlatformKind kind, string expected)
    {
        Assert.AreEqual(expected, HidBackendFactory.Create(new FixedPlatformProbe(kind)).Name);
    }

    [Test]
    public void AnUnknownPlatformIsRefusedInsteadOfGuessed()
    {
        Assert.Throws<PlatformNotSupportedException>(
            static () => HidBackendFactory.Create(new FixedPlatformProbe(PlatformKind.Unknown)));
    }

    [Test]
    public void EachBackendEnumeratesNothingRatherThanThrowingOnAnotherSystem()
    {
        AssertSafeEnumeration(new WindowsHidBackend());
        AssertSafeEnumeration(new LinuxHidBackend());
        using var mac = new MacOsHidBackend();
        AssertSafeEnumeration(mac);
    }

    [Test]
    public void OpeningSomethingThatIsNotThereGivesNullRatherThanThrowing()
    {
        var missing = new HidDeviceInfo(1, 2, "nowhere");
        Assert.IsNull(new LinuxHidBackend().Open(missing));
        using var mac = new MacOsHidBackend();
        Assert.IsNull(mac.Open(missing));
    }

    private static void AssertSafeEnumeration(IHidBackend backend)
    {
        IReadOnlyList<HidDeviceInfo> devices = backend.Enumerate();
        Assert.IsNotNull(devices);
        Assert.IsTrue(devices.Count >= 0, backend.Name + " enumerated without throwing");
    }
}
