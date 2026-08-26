using System;
using GEngine.Core;
using GEngine.Rendering.Text;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="BitmapFont"/>.</summary>
public sealed class BitmapFontTests
{
    [Test]
    public void TheFontIsFiveBySevenWithOneColumnBetweenGlyphs()
    {
        Assert.AreEqual(5, BitmapFont.GlyphWidth);
        Assert.AreEqual(7, BitmapFont.GlyphHeight);
        Assert.AreEqual(6, BitmapFont.Advance);
    }

    [Test]
    public void MeasureWidth_CountsTheGapsBetweenGlyphsButNotAfterTheLast()
    {
        Assert.AreEqual(0, BitmapFont.MeasureWidth(string.Empty));
        Assert.AreEqual(5, BitmapFont.MeasureWidth("A"));
        Assert.AreEqual(11, BitmapFont.MeasureWidth("AB"));
    }

    [Test]
    public void TheFontHasDigitsLettersAndTheSymbolsAHudNeeds()
    {
        foreach (char character in "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ -.,:!?*x/+<>%=")
        {
            Assert.IsTrue(BitmapFont.Contains(character), "the font has " + character);
        }
    }

    [Test]
    public void ALowerCaseLetterIsDrawnAsItsCapital()
    {
        Assert.IsTrue(BitmapFont.Contains('a'));
        Assert.AreEqual(BitmapFont.IsSet('A', 2, 0), BitmapFont.IsSet('a', 2, 0));
    }

    [Test]
    public void AnUnknownCharacterIsBlankRatherThanAnError()
    {
        Assert.IsFalse(BitmapFont.Contains('~'));
        Assert.IsFalse(BitmapFont.IsSet('~', 0, 0));
    }

    [Test]
    public void IsSet_OutsideTheGlyphIsFalse()
    {
        Assert.IsFalse(BitmapFont.IsSet('A', -1, 0));
        Assert.IsFalse(BitmapFont.IsSet('A', 5, 0));
        Assert.IsFalse(BitmapFont.IsSet('A', 0, 7));
    }

    [Test]
    public void ASpaceIsEntirelyBlank()
    {
        for (int row = 0; row < BitmapFont.GlyphHeight; row++)
        {
            for (int column = 0; column < BitmapFont.GlyphWidth; column++)
            {
                Assert.IsFalse(BitmapFont.IsSet(' ', column, row));
            }
        }
    }

    [Test]
    public void DrawTo_RendersALegibleGlyph()
    {
        var buffer = new FrameBuffer(5, 7);
        BitmapFont.DrawTo(buffer, "H", Vector2.Zero, Palette.White);
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
        BitmapFont.DrawTo(buffer, "II", Vector2.Zero, Palette.White);
        Assert.AreEqual(Palette.White, buffer.GetPixel(2, 1), "the stem of the first I");
        Assert.AreEqual(Palette.White, buffer.GetPixel(8, 1), "the stem of the second I");
    }

    [Test]
    public void DrawTo_RefusesMissingArguments()
    {
        var buffer = new FrameBuffer(5, 7);
        Assert.Throws<ArgumentNullException>(() => BitmapFont.DrawTo(buffer, null!, Vector2.Zero, Palette.White));
        Assert.Throws<ArgumentNullException>(static () => BitmapFont.DrawTo(null!, "A", Vector2.Zero, Palette.White));
    }
}
