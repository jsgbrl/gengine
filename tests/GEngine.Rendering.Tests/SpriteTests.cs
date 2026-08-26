using System;
using GEngine.Core;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="Sprite"/>.</summary>
public sealed class SpriteTests
{
    [Test]
    public void ASpriteKeepsItsNameSizeAndPixels()
    {
        Sprite sprite = Checker();
        Assert.AreEqual("checker", sprite.Name);
        Assert.AreEqual(2, sprite.Width);
        Assert.AreEqual(2, sprite.Height);
        Assert.AreEqual(4, sprite.Pixels.Length);
    }

    [Test]
    public void At_ReadsRowMajor()
    {
        Sprite sprite = Checker();
        Assert.AreEqual(Palette.White, sprite.At(0, 0));
        Assert.AreEqual(Palette.Black, sprite.At(1, 0));
        Assert.AreEqual(Palette.Black, sprite.At(0, 1));
        Assert.AreEqual(Palette.White, sprite.At(1, 1));
    }

    [Test]
    public void At_OutsideTheSprite_IsTransparent()
    {
        Sprite sprite = Checker();
        Assert.AreEqual(Color.Transparent, sprite.At(-1, 0));
        Assert.AreEqual(Color.Transparent, sprite.At(2, 0));
        Assert.AreEqual(Color.Transparent, sprite.At(0, -1));
        Assert.AreEqual(Color.Transparent, sprite.At(0, 2));
    }

    [Test]
    public void Mirrored_ReversesTheColumnsAndKeepsTheRows()
    {
        Sprite mirrored = Checker().Mirrored();
        Assert.AreEqual(Palette.Black, mirrored.At(0, 0));
        Assert.AreEqual(Palette.White, mirrored.At(1, 0));
        Assert.IsTrue(mirrored.Name.EndsWith("-mirrored", StringComparison.Ordinal));
    }

    [Test]
    public void AWrongNumberOfPixels_IsRefused()
    {
        Assert.Throws<ArgumentException>(static () => new Sprite("bad", 2, 2, [Palette.White]));
    }

    [Test]
    public void ADegenerateSize_IsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () => new Sprite("bad", 0, 2, []));
    }

    private static Sprite Checker() =>
        new("checker", 2, 2, [Palette.White, Palette.Black, Palette.Black, Palette.White]);
}
