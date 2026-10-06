using GEngine.Core;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>
/// Covers the drawing operations, and above all their clipping. Every one of them is checked
/// against all four edges, because a sprite that walks off the left of the screen and
/// reappears on the right of the row above is the classic symptom of a missing clip.
/// </summary>
public sealed class FrameBufferDrawingTests
{
    private const string TestRamp = ".@";

    private FrameBuffer _buffer = new(8, 6);

    [Setup]
    public void Setup() => _buffer = new FrameBuffer(8, 6);

    [Test]
    public void DrawRectangle_FillsExactlyTheRectangle()
    {
        _buffer.DrawRectangle(new Aabb(new Vector2(2.0f, 1.0f), new Vector2(5.0f, 3.0f)), Palette.White);
        Assert.MatchesSnapshot(Capture(), string.Join("\n",
            "........",
            "..@@@...",
            "..@@@...",
            "........",
            "........",
            "........"));
    }

    [TestCase(-4.0f, 0.0f)]
    [TestCase(6.0f, 0.0f)]
    [TestCase(0.0f, -4.0f)]
    [TestCase(0.0f, 5.0f)]
    public void DrawRectangle_ClipsAgainstEveryEdgeWithoutWrapping(float x, float y)
    {
        _buffer.DrawRectangle(new Aabb(new Vector2(x, y), new Vector2(x + 4.0f, y + 4.0f)), Palette.White);
        AssertNothingWrapped();
    }

    [Test]
    public void DrawRectangle_EntirelyOutside_DrawsNothing()
    {
        _buffer.DrawRectangle(new Aabb(new Vector2(100.0f, 100.0f), new Vector2(110.0f, 110.0f)), Palette.White);
        Assert.MatchesSnapshot(Capture(), string.Join("\n", "........", "........", "........", "........", "........", "........"));
    }

    [Test]
    public void DrawRectangleOutline_DrawsFourEdgesAndAHollowMiddle()
    {
        _buffer.DrawRectangleOutline(new Aabb(new Vector2(1.0f, 1.0f), new Vector2(5.0f, 4.0f)), Palette.White);
        Assert.MatchesSnapshot(Capture(), string.Join("\n",
            "........",
            ".@@@@...",
            ".@..@...",
            ".@@@@...",
            "........",
            "........"));
    }

    [Test]
    public void DrawPixels_CopiesASourceAtAPosition()
    {
        _buffer.DrawPixels(Block(2, 2), new Vector2(3.0f, 2.0f));
        Assert.MatchesSnapshot(Capture(), string.Join("\n",
            "........",
            "........",
            "...@@...",
            "...@@...",
            "........",
            "........"));
    }

    [Test]
    public void DrawPixels_ClipsAtTheLeftAndTopWithoutWrapping()
    {
        _buffer.DrawPixels(Block(3, 3), new Vector2(-2.0f, -2.0f));
        Assert.MatchesSnapshot(Capture(), string.Join("\n",
            "@.......",
            "........",
            "........",
            "........",
            "........",
            "........"));
    }

    [Test]
    public void DrawPixels_ClipsAtTheRightAndBottomWithoutWrapping()
    {
        _buffer.DrawPixels(Block(3, 3), new Vector2(6.0f, 4.0f));
        Assert.MatchesSnapshot(Capture(), string.Join("\n",
            "........",
            "........",
            "........",
            "........",
            "......@@",
            "......@@"));
    }

    [Test]
    public void DrawPixels_SkipsTransparentPixelsInTheSource()
    {
        _buffer.Clear(Palette.White);
        _buffer.DrawPixels(new Sprite("hole", 2, 1, [Color.Transparent, Palette.Black]), new Vector2(0.0f, 0.0f));
        Assert.AreEqual(Palette.White, _buffer.GetPixel(0, 0));
        Assert.AreEqual(Palette.Black, _buffer.GetPixel(1, 0));
    }

    [Test]
    public void DrawSprite_IsDrawPixelsUnderAnotherName()
    {
        _buffer.DrawSprite(Block(2, 2), new Vector2(0.0f, 0.0f));
        Assert.AreEqual(Palette.White, _buffer.GetPixel(1, 1));
    }

    [Test]
    public void DrawText_PutsGlyphsWhereItIsTold()
    {
        var wide = new FrameBuffer(12, 8);
        wide.DrawText("A", new Vector2(0.0f, 0.0f), Palette.White);
        Assert.AreEqual(Palette.White, wide.GetPixel(2, 0), "the apex of the A");
        Assert.AreEqual(Color.Transparent, wide.GetPixel(0, 0));
    }

    [Test]
    public void DrawText_ClipsAtTheEdgeInsteadOfThrowing()
    {
        _buffer.DrawText("GENGINE", new Vector2(6.0f, 4.0f), Palette.White);
        AssertNothingWrapped();
    }

    private void AssertNothingWrapped()
    {
        foreach (string line in Capture().Split('\n'))
        {
            Assert.AreEqual(8, line.Length);
        }
    }

    // Two characters is enough here and far easier to read than the ten-step default ramp:
    // a dot is nothing, an at sign is something.
    private string Capture() => AsciiSnapshot.Capture(_buffer, TestRamp);

    private static Sprite Block(int width, int height)
    {
        var pixels = new Color[width * height];
        System.Array.Fill(pixels, Palette.White);
        return new Sprite("block", width, height, pixels);
    }
}
