using System.IO;
using GEngine.Rendering.Assets;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="FileSystemAssetSource"/>.</summary>
public sealed class FileSystemAssetSourceTests
{
    [Test]
    public void AMissingDirectoryListsNothingRatherThanThrowing()
    {
        var source = new FileSystemAssetSource(Path.Combine(Path.GetTempPath(), "gengine-not-here"));
        Assert.AreEqual(0, source.ListPaths(string.Empty).Count);
        Assert.IsFalse(source.Exists("anything"));
    }

    [Test]
    public void ForwardSlashPathsWorkWhateverTheSystemSeparatorIs()
    {
        string root = Path.Combine(Path.GetTempPath(), "gengine-assets-test");
        Directory.CreateDirectory(Path.Combine(root, "assets"));
        File.WriteAllText(Path.Combine(root, "assets", "coin.sprite"), "Y");
        try
        {
            var source = new FileSystemAssetSource(root);
            Assert.IsTrue(source.Exists("assets/coin.sprite"));
            Assert.AreEqual("Y", source.ReadText("assets/coin.sprite"));
            Assert.AreEqual("assets/coin.sprite", source.ListPaths("assets/")[0]);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Test]
    public void TheSourceNamesItsRoot()
    {
        Assert.IsTrue(new FileSystemAssetSource("somewhere").Name.Contains("somewhere", System.StringComparison.Ordinal));
    }
}
