using System.Reflection;
using GEngine.Rendering.Assets;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="EmbeddedAssetSource"/> against this test assembly's own resources.</summary>
public sealed class EmbeddedAssetSourceTests
{
    [Test]
    public void AnAssemblyWithNoResourcesListsNothing()
    {
        var source = new EmbeddedAssetSource(typeof(EmbeddedAssetSourceTests).Assembly);
        Assert.AreEqual(0, source.ListPaths("assets/").Count);
    }

    [Test]
    public void TheSourceNamesItsAssembly()
    {
        Assembly assembly = typeof(EmbeddedAssetSourceTests).Assembly;
        Assert.IsTrue(new EmbeddedAssetSource(assembly).Name.Contains("Rendering.Tests", System.StringComparison.Ordinal));
    }
}
