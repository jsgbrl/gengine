using System.Collections.Generic;
using GEngine.Rendering.Assets;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="MemoryAssetSource"/>, the one every other test builds on.</summary>
public sealed class MemoryAssetSourceTests
{
    [Test]
    public void WhatWasPutInComesBackOut()
    {
        MemoryAssetSource source = new MemoryAssetSource().With("assets/coin.sprite", "pixels:\nY\n");
        Assert.IsTrue(source.Exists("assets/coin.sprite"));
        Assert.AreEqual("pixels:\nY\n", source.ReadText("assets/coin.sprite"));
    }

    [Test]
    public void AMissingAssetIsNamedInTheFailure()
    {
        var source = new MemoryAssetSource();
        Assert.IsFalse(source.Exists("nothing"));
        KeyNotFoundException failure = Assert.Throws<KeyNotFoundException>(() => source.ReadText("nothing"));
        Assert.IsTrue(failure.Message.Contains("nothing", System.StringComparison.Ordinal));
    }

    [Test]
    public void ListPaths_FiltersByPrefixAndSortsSoAListingIsReproducible()
    {
        MemoryAssetSource source = new MemoryAssetSource()
            .With("assets/b.sprite", "b")
            .With("assets/a.sprite", "a")
            .With("levels/1-1.txt", "x");

        IReadOnlyList<string> found = source.ListPaths("assets/");
        Assert.AreEqual(2, found.Count);
        Assert.AreEqual("assets/a.sprite", found[0]);
        Assert.AreEqual("assets/b.sprite", found[1]);
    }

    [Test]
    public void AnEmptyPrefixListsEverything()
    {
        MemoryAssetSource source = new MemoryAssetSource().With("a", "1").With("b", "2");
        Assert.AreEqual(2, source.ListPaths(string.Empty).Count);
    }

    [Test]
    public void TheSourceNamesItselfForErrorMessages()
    {
        Assert.AreEqual("memory", new MemoryAssetSource().Name);
    }
}
