using System;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="SpriteParser"/>.</summary>
public sealed class SpriteParserTests
{
    private const string Coin = """
        # a two by two coin
        legend:
        . transparent
        Y yellow
        pixels:
        .Y
        Y.
        """;

    [Test]
    public void AWellFormedSprite_ParsesToTheRightSizeAndColours()
    {
        Sprite sprite = SpriteParser.Parse("coin", Coin);
        Assert.AreEqual(2, sprite.Width);
        Assert.AreEqual(2, sprite.Height);
        Assert.AreEqual(Palette.Yellow, sprite.At(1, 0));
        Assert.IsTrue(sprite.At(0, 0).IsTransparent);
    }

    [Test]
    public void CommentsAndBlankLinesAreIgnored()
    {
        Sprite sprite = SpriteParser.Parse("coin", "\n# one\n\nlegend:\n# two\nY yellow\npixels:\nY\n");
        Assert.AreEqual(1, sprite.Width);
    }

    [Test]
    public void ARowOfADifferentWidth_IsAnError()
    {
        FormatException failure = Assert.Throws<FormatException>(
            static () => SpriteParser.Parse("ragged", "legend:\nY yellow\npixels:\nYY\nY\n"));
        Assert.IsTrue(failure.Message.Contains("different widths", StringComparison.Ordinal));
    }

    [Test]
    public void ACharacterMissingFromTheLegend_IsAnError()
    {
        FormatException failure = Assert.Throws<FormatException>(
            static () => SpriteParser.Parse("mystery", "legend:\nY yellow\npixels:\nYZ\n"));
        Assert.IsTrue(failure.Message.Contains("legend does not define", StringComparison.Ordinal));
    }

    [Test]
    public void ASpriteWithNoPixels_IsAnError()
    {
        Assert.Throws<FormatException>(static () => SpriteParser.Parse("empty", "legend:\nY yellow\n"));
    }

    [Test]
    public void AMalformedLegendLine_IsAnError()
    {
        Assert.Throws<FormatException>(static () => SpriteParser.Parse("bad", "legend:\nYY yellow\npixels:\nY\n"));
    }

    [Test]
    public void ParseColor_ReadsPaletteNamesCaseInsensitively()
    {
        Assert.AreEqual(Palette.Sky, SpriteParser.ParseColor("SKY"));
        Assert.IsTrue(SpriteParser.ParseColor("transparent").IsTransparent);
    }

    [Test]
    public void ParseColor_ReadsSixAndEightHexDigits_WithOrWithoutAHash()
    {
        Assert.AreEqual(new Core.Color(18, 52, 86), SpriteParser.ParseColor("123456"));
        Assert.AreEqual(new Core.Color(18, 52, 86), SpriteParser.ParseColor("#123456"));
        Assert.AreEqual(new Core.Color(18, 52, 86, 120), SpriteParser.ParseColor("12345678"));
    }

    [Test]
    public void ParseColor_RejectsAnythingElse()
    {
        Assert.Throws<FormatException>(static () => SpriteParser.ParseColor("beige"));
        Assert.Throws<FormatException>(static () => SpriteParser.ParseColor("12345"));
    }
}
