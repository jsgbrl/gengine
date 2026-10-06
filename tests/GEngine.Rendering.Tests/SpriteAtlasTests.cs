using System;
using GEngine.Rendering.Assets;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>
/// The flyweight. A sprite is immutable, so the hundred coins in a level can be the same object
/// a hundred times - and the point of the atlas is that they are, which is what these check
/// rather than merely that loading works.
/// </summary>
public sealed class SpriteAtlasTests
{
    private const string Dot = "legend:\n. transparent\nX red\npixels:\nX.\n.X\n";

    [Test]
    public void AnAtlasStartsEmptyAndLoadsOnDemand()
    {
        SpriteAtlas atlas = Atlas();
        Assert.AreEqual(0, atlas.LoadedCount);

        atlas.Get("dot");
        Assert.AreEqual(1, atlas.LoadedCount, "asking for it loaded it");
    }

    // The same name gives the same object, not an equal one. A hundred coins on screen are one
    // sprite in memory, which is the entire reason this class exists.
    [Test]
    public void TheSameNameGivesTheSameObjectEveryTime()
    {
        SpriteAtlas atlas = Atlas();
        Assert.AreSame(atlas.Get("dot"), atlas.Get("dot"));
        Assert.AreEqual(1, atlas.LoadedCount, "and it was only read once");
    }

    [Test]
    public void ASpriteThatIsNotThereIsAnErrorThatNamesTheFile()
    {
        SpriteAtlas atlas = Atlas();
        Exception missing = Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => atlas.Get("nothing"));
        Assert.IsTrue(missing.Message.Contains("nothing", StringComparison.Ordinal), missing.Message);
    }

    [Test]
    public void AskingWhetherASpriteExistsNeverThrows()
    {
        SpriteAtlas atlas = Atlas();
        Assert.IsTrue(atlas.TryGet("dot", out Sprite? found));
        Assert.IsNotNull(found);

        Assert.IsFalse(atlas.TryGet("nothing", out Sprite? missing));
        Assert.IsNull(missing);
    }

    // Mirroring is cached too, and separately: a sprite and its mirror are two objects, and
    // asking twice must not build a third.
    [Test]
    public void AMirroredSpriteIsBuiltOnceAndIsNotTheOriginal()
    {
        SpriteAtlas atlas = Atlas();
        Sprite mirrored = atlas.GetMirrored("dot");

        Assert.AreSame(mirrored, atlas.GetMirrored("dot"));
        Assert.AreNotEqual(atlas.Get("dot").At(0, 0), mirrored.At(0, 0), "it really is flipped");
    }

    [Test]
    public void LoadingEverythingLoadsEverythingOnce()
    {
        MemoryAssetSource source = new MemoryAssetSource()
            .With("assets/one.sprite", Dot)
            .With("assets/two.sprite", Dot)
            .With("assets/notes.txt", "not a sprite");
        var atlas = new SpriteAtlas(source);

        Assert.AreEqual(2, atlas.LoadAll(), "the text file was not a sprite");
        Assert.AreEqual(2, atlas.LoadedCount);
        Assert.AreEqual(2, atlas.LoadAll(), "loading again loads nothing new");
    }

    [Test]
    public void ThePathOfASpriteIsTheFolderPlusTheNamePlusTheExtension()
    {
        Assert.AreEqual("assets/coin.sprite", Atlas().PathOf("coin"));
        Assert.AreEqual("art/coin.sprite", new SpriteAtlas(new MemoryAssetSource(), "art").PathOf("coin"));
    }

    [Test]
    public void ItRefusesToBeBuiltWithNowhereToReadFrom()
    {
        Assert.Throws<ArgumentNullException>(() => new SpriteAtlas(null!));
    }

    private static SpriteAtlas Atlas()
    {
        return new SpriteAtlas(new MemoryAssetSource().With("assets/dot.sprite", Dot));
    }
}
