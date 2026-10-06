using System;
using GEngine.Core;
using GEngine.Rendering.Text;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="PixelFont"/>.</summary>
public sealed class PixelFontTests
{
    [Test]
    public void TheFontIsFiveBySevenWithOneColumnBetweenGlyphs()
    {
        Assert.AreEqual(5, PixelFont.GlyphWidth);
        Assert.AreEqual(7, PixelFont.GlyphHeight);
        Assert.AreEqual(6, PixelFont.Advance);
    }

    [Test]
    public void MeasureWidth_CountsTheGapsBetweenGlyphsButNotAfterTheLast()
    {
        Assert.AreEqual(0, PixelFont.MeasureWidth(string.Empty));
        Assert.AreEqual(5, PixelFont.MeasureWidth("A"));
        Assert.AreEqual(11, PixelFont.MeasureWidth("AB"));
    }

    [Test]
    public void TheFontHasDigitsLettersAndTheSymbolsAHudNeeds()
    {
        foreach (char character in "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ -.,:!?*x/+<>%=")
        {
            Assert.IsTrue(PixelFont.Contains(character), "the font has " + character);
        }
    }

    [Test]
    public void ALowerCaseLetterIsDrawnAsItsCapital()
    {
        Assert.IsTrue(PixelFont.Contains('a'));
        Assert.AreEqual(PixelFont.IsSet('A', 2, 0), PixelFont.IsSet('a', 2, 0));
    }

    [Test]
    public void AnUnknownCharacterIsBlankRatherThanAnError()
    {
        Assert.IsFalse(PixelFont.Contains('~'));
        Assert.IsFalse(PixelFont.IsSet('~', 0, 0));
    }

    [Test]
    public void IsSet_OutsideTheGlyphIsFalse()
    {
        Assert.IsFalse(PixelFont.IsSet('A', -1, 0));
        Assert.IsFalse(PixelFont.IsSet('A', 5, 0));
        Assert.IsFalse(PixelFont.IsSet('A', 0, 7));
    }

    [Test]
    public void ASpaceIsEntirelyBlank()
    {
        for (int row = 0; row < PixelFont.GlyphHeight; row++)
        {
            for (int column = 0; column < PixelFont.GlyphWidth; column++)
            {
                Assert.IsFalse(PixelFont.IsSet(' ', column, row));
            }
        }
    }

    [Test]
    public void DrawTo_RendersALegibleGlyph()
    {
        var buffer = new FrameBuffer(5, 7);
        PixelFont.DrawTo(buffer, "H", Vector2.Zero, Palette.White);
        Assert.MatchesSnapshot(AsciiSnapshot.Capture(buffer, ".@"), string.Join("\n",
            "@...@",
            "@...@",
            "@...@",
            "@@@@@",
            "@...@",
            "@...@",
            "@...@"));
    }

    [Test]
    public void DrawTo_AdvancesOneGlyphPlusTheGapPerCharacter()
    {
        var buffer = new FrameBuffer(12, 7);
        PixelFont.DrawTo(buffer, "II", Vector2.Zero, Palette.White);
        Assert.AreEqual(Palette.White, buffer.GetPixel(2, 1), "the stem of the first I");
        Assert.AreEqual(Palette.White, buffer.GetPixel(8, 1), "the stem of the second I");
    }

    [Test]
    public void DrawTo_RefusesMissingArguments()
    {
        var buffer = new FrameBuffer(5, 7);
        Assert.Throws<ArgumentNullException>(() => PixelFont.DrawTo(buffer, null!, Vector2.Zero, Palette.White));
        Assert.Throws<ArgumentNullException>(static () => PixelFont.DrawTo(null!, "A", Vector2.Zero, Palette.White));
    }
}
