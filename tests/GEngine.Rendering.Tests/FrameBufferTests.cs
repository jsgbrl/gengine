using System;
using GEngine.Core;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="FrameBuffer"/>: pixels, blending and the four edges.</summary>
public sealed class FrameBufferTests
{
    private FrameBuffer _buffer = new(4, 3);

    [Setup]
    public void Setup() => _buffer = new FrameBuffer(4, 3);

    [Test]
    public void ANewBuffer_IsTransparentEverywhere()
    {
        Assert.AreEqual(4, _buffer.Width);
        Assert.AreEqual(3, _buffer.Height);
        Assert.AreEqual(Color.Transparent, _buffer.GetPixel(0, 0));
        Assert.AreEqual(12, _buffer.Pixels.Length);
    }

    [Test]
    public void ADegenerateSize_IsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(static () => new FrameBuffer(0, 3));
        Assert.Throws<ArgumentOutOfRangeException>(static () => new FrameBuffer(4, -1));
    }

    [Test]
    public void Clear_OverwritesEveryPixelIgnoringAlpha()
    {
        _buffer.Clear(Palette.Red.WithAlpha(7));
        Assert.AreEqual(7, _buffer.GetPixel(2, 2).A);
    }

    [Test]
    public void SetPixel_WritesAnOpaqueColourStraightThrough()
    {
        _buffer.SetPixel(1, 1, Palette.Red);
        Assert.AreEqual(Palette.Red, _buffer.GetPixel(1, 1));
    }

    [Test]
    public void SetPixel_BlendsAPartlyTransparentColourOverWhatIsThere()
    {
        _buffer.Clear(Palette.Black);
        _buffer.SetPixel(1, 1, Color.White.WithAlpha(128));
        Assert.AreEqual(128, _buffer.GetPixel(1, 1).R);
    }

    [Test]
    public void SetPixel_IgnoresAFullyTransparentColour()
    {
        _buffer.Clear(Palette.Red);
        _buffer.SetPixel(1, 1, Color.Transparent);
        Assert.AreEqual(Palette.Red, _buffer.GetPixel(1, 1));
    }

    [TestCase(-1, 1)]
    [TestCase(4, 1)]
    [TestCase(1, -1)]
    [TestCase(1, 3)]
    public void SetPixel_OutsideAnyEdge_DoesNothingAtAll(int x, int y)
    {
        _buffer.Clear(Palette.Black);
        _buffer.SetPixel(x, y, Palette.Red);
        Assert.AreEqual(Palette.Black, _buffer.GetPixel(0, 0));
        Assert.AreEqual(Palette.Black, _buffer.GetPixel(3, 2));
    }

    [TestCase(-1, 1)]
    [TestCase(4, 1)]
    [TestCase(1, -1)]
    [TestCase(1, 3)]
    public void GetPixel_OutsideAnyEdge_IsTransparent(int x, int y)
    {
        _buffer.Clear(Palette.Red);
        Assert.AreEqual(Color.Transparent, _buffer.GetPixel(x, y));
    }

    [Test]
    public void Bounds_CoverTheWholeBuffer()
    {
        Assert.AreEqual(Vector2.Zero, _buffer.Bounds.Min);
        Assert.AreEqual(new Vector2(4.0f, 3.0f), _buffer.Bounds.Max);
    }

    [Test]
    public void Resize_ChangesTheSizeAndClearsTheContents()
    {
        _buffer.Clear(Palette.Red);
        _buffer.Resize(2, 2);
        Assert.AreEqual(2, _buffer.Width);
        Assert.AreEqual(Color.Transparent, _buffer.GetPixel(0, 0));
    }

    [Test]
    public void Resize_ToTheSameSize_KeepsTheContents()
    {
        _buffer.Clear(Palette.Red);
        _buffer.Resize(4, 3);
        Assert.AreEqual(Palette.Red, _buffer.GetPixel(0, 0));
    }
}
