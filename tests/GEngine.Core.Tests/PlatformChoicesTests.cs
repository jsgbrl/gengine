using System;
using GEngine.Core.Platform;
using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="PlatformChoices{TImplementation}"/>.</summary>
public sealed class PlatformChoicesTests
{
    [Test]
    public void EachFactoryIsKeptUnderItsOwnPlatform()
    {
        var choices = new PlatformChoices<string>(() => "windows", () => "macos", () => "linux");
        Assert.AreEqual("windows", choices.Windows());
        Assert.AreEqual("macos", choices.MacOs());
        Assert.AreEqual("linux", choices.Linux());
    }

    [Test]
    public void TheFactoriesAreNotCalledAtConstruction()
    {
        int calls = 0;
        _ = new PlatformChoices<string>(Count, Count, Count);
        Assert.AreEqual(0, calls);

        string Count()
        {
            calls++;
            return "called";
        }
    }

    [Test]
    public void AMissingFactory_IsRefusedImmediately()
    {
        Assert.Throws<ArgumentNullException>(() => new PlatformChoices<string>(null!, () => "b", () => "c"));
        Assert.Throws<ArgumentNullException>(() => new PlatformChoices<string>(() => "a", null!, () => "c"));
        Assert.Throws<ArgumentNullException>(() => new PlatformChoices<string>(() => "a", () => "b", null!));
    }
}
