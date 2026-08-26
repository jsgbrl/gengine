using GEngine.Core.Platform;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="FixedPlatformProbe"/>.</summary>
public sealed class FixedPlatformProbeTests
{
    [TestCase(PlatformKind.Windows)]
    [TestCase(PlatformKind.MacOs)]
    [TestCase(PlatformKind.Linux)]
    [TestCase(PlatformKind.Unknown)]
    public void ItReportsWhateverItWasTold(PlatformKind kind)
    {
        Assert.AreEqual(kind, new FixedPlatformProbe(kind).Kind);
    }

    [Test]
    public void TheDescriptionCanBeSetAndHasAReadableDefault()
    {
        Assert.AreEqual("a mac in a drawer", new FixedPlatformProbe(PlatformKind.MacOs, "a mac in a drawer").Description);
        Assert.IsTrue(new FixedPlatformProbe(PlatformKind.Linux).Description.Length > 0);
    }
}
