using System;
using GEngine.Rendering;
using GEngine.Rendering.Text;
using GEngine.Testing;
using MarioClone.Game;
using MarioClone.View;

namespace MarioClone.Tests;

/// <summary>
/// The display is six numbers and a font. What can go wrong is arithmetic - a score wider than
/// the frame, a right-hand column that runs off the edge - so that is what these check.
/// </summary>
public sealed class HudTests
{
    [Test]
    public void ADisplayPutsInkOnBothEndsOfTheTopLine()
    {
        var frame = new FrameBuffer(160, 96);
        frame.Clear(Palette.Black);
        Hud.Draw(frame, new GameSession());

        Assert.IsTrue(InkIn(frame, 0, frame.Width / 2), "the score is on the left");
        Assert.IsTrue(InkIn(frame, frame.Width / 2, frame.Width), "the world and clock are on the right");
    }

    [Test]
    public void TheDisplayStaysOutOfTheWayOfTheLevel()
    {
        var frame = new FrameBuffer(160, 96);
        frame.Clear(Palette.Black);
        Hud.Draw(frame, new GameSession());

        for (int y = (int)Hud.MarginPixels + PixelFont.GlyphHeight + 1; y < frame.Height; y++)
        {
            for (int x = 0; x < frame.Width; x++)
            {
                Assert.AreEqual(Palette.Black, frame.GetPixel(x, y), "the display wrote below its line");
            }
        }
    }

    // Whatever the score, the right-hand column ends at the margin: it is laid out from the
    // right edge backwards, so a six-figure score cannot push the clock off the screen.
    [Test]
    public void TheRightHandColumnIsAlwaysInsideTheFrame()
    {
        var frame = new FrameBuffer(160, 96);
        var session = new GameSession();
        for (int coins = 0; coins < 40; coins++)
        {
            session.CollectCoin();
        }

        frame.Clear(Palette.Black);
        Hud.Draw(frame, session);
        Assert.IsFalse(InkIn(frame, frame.Width - (int)Hud.MarginPixels, frame.Width), "nothing in the margin");
    }

    [Test]
    public void ABannerLandsInTheMiddleOfTheFrame()
    {
        var frame = new FrameBuffer(160, 96);
        frame.Clear(Palette.Black);
        Hud.DrawBanner(frame, "PAUSED", Palette.White);

        int middle = (frame.Height - PixelFont.GlyphHeight) / 2;
        Assert.IsTrue(RowHasInk(frame, middle + 1), "the banner is on the middle row");
        Assert.IsFalse(RowHasInk(frame, 1), "and nowhere near the top");
    }

    [Test]
    public void CentredTextIsCentred()
    {
        var frame = new FrameBuffer(160, 96);
        frame.Clear(Palette.Black);
        Hud.DrawCentered(frame, "GENGINE", 40.0f, Palette.White);

        // A glyph's advance includes the gap that follows it, so the measured word is one gap
        // wider than the ink and the right-hand margin is that much larger. Anything beyond
        // that is a centring bug rather than a spacing one.
        int left = FirstInkColumn(frame);
        int right = LastInkColumnFromTheRight(frame);
        Assert.IsTrue(Math.Abs(left - right) <= PixelFont.Spacing, "the same gap on both sides");
    }

    [Test]
    public void DrawingWithNothingToDrawOnIsRefused()
    {
        var frame = new FrameBuffer(16, 16);
        Assert.Throws<ArgumentNullException>(() => Hud.Draw(null!, new GameSession()));
        Assert.Throws<ArgumentNullException>(() => Hud.Draw(frame, null!));
        Assert.Throws<ArgumentNullException>(() => Hud.DrawBanner(frame, null!, Palette.White));
        Assert.Throws<ArgumentNullException>(() => Hud.DrawCentered(frame, null!, 0.0f, Palette.White));
    }

    private static bool InkIn(FrameBuffer frame, int firstColumn, int lastColumn)
    {
        for (int x = firstColumn; x < lastColumn; x++)
        {
            for (int y = 0; y < frame.Height; y++)
            {
                if (frame.GetPixel(x, y) != Palette.Black)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool RowHasInk(FrameBuffer frame, int row) => InkIn(RowOf(frame, row), 0, frame.Width);

    private static FrameBuffer RowOf(FrameBuffer frame, int row)
    {
        var strip = new FrameBuffer(frame.Width, 1);
        for (int x = 0; x < frame.Width; x++)
        {
            strip.SetPixel(x, 0, frame.GetPixel(x, row));
        }

        return strip;
    }

    private static int FirstInkColumn(FrameBuffer frame)
    {
        for (int x = 0; x < frame.Width; x++)
        {
            if (InkIn(frame, x, x + 1))
            {
                return x;
            }
        }

        return frame.Width;
    }

    private static int LastInkColumnFromTheRight(FrameBuffer frame)
    {
        for (int x = frame.Width - 1; x >= 0; x--)
        {
            if (InkIn(frame, x, x + 1))
            {
                return frame.Width - 1 - x;
            }
        }

        return frame.Width;
    }
}
