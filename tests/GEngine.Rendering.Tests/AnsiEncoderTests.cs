using GEngine.Core;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="AnsiEncoder"/>, including the two fallbacks.</summary>
public sealed class AnsiEncoderTests
{
    private AnsiBuffer _buffer = new(256);

    [Setup]
    public void Setup() => _buffer = new AnsiBuffer(256);

    [Test]
    public void AppendCursorTo_IsOneBasedEvenThoughTheApiIsZeroBased()
    {
        AnsiEncoder.AppendCursorTo(_buffer, 0, 0);
        Assert.AreEqual("\u001b[1;1H", _buffer.ToString());
    }

    [Test]
    public void AppendCursorTo_PutsTheRowFirst()
    {
        AnsiEncoder.AppendCursorTo(_buffer, 4, 9);
        Assert.AreEqual("\u001b[10;5H", _buffer.ToString());
    }

    [Test]
    public void TrueColor_SendsTheChannelsUnchanged()
    {
        AnsiEncoder.AppendForeground(_buffer, new Color(1, 2, 3), ColorDepth.TrueColor);
        Assert.AreEqual("\u001b[38;2;1;2;3m", _buffer.ToString());
    }

    [Test]
    public void TrueColorBackground_UsesTheOtherIntroducer()
    {
        AnsiEncoder.AppendBackground(_buffer, new Color(1, 2, 3), ColorDepth.TrueColor);
        Assert.AreEqual("\u001b[48;2;1;2;3m", _buffer.ToString());
    }

    [Test]
    public void Palette256_SendsAnIndexFromTheCube()
    {
        AnsiEncoder.AppendForeground(_buffer, Color.White, ColorDepth.Palette256);
        Assert.AreEqual("\u001b[38;5;255m", _buffer.ToString());
    }

    [Test]
    public void Basic16_SendsAPlainColourCode()
    {
        AnsiEncoder.AppendForeground(_buffer, Color.White, ColorDepth.Basic16);
        Assert.AreEqual("\u001b[97m", _buffer.ToString());
        _buffer.Clear();
        AnsiEncoder.AppendBackground(_buffer, Color.Black, ColorDepth.Basic16);
        Assert.AreEqual("\u001b[40m", _buffer.ToString());
    }

    [Test]
    public void ToPalette256_MapsGreysOntoTheGreyscaleRamp()
    {
        Assert.IsTrue(AnsiEncoder.ToPalette256(new Color(128, 128, 128)) >= 232);
        Assert.IsTrue(AnsiEncoder.ToPalette256(new Color(128, 128, 128)) <= 255);
    }

    [Test]
    public void ToPalette256_MapsColoursIntoTheSixBySixBySixCube()
    {
        int index = AnsiEncoder.ToPalette256(new Color(255, 0, 0));
        Assert.AreEqual(196, index, "the far corner of the red axis");
        Assert.IsTrue(AnsiEncoder.ToPalette256(new Color(0, 255, 0)) is >= 16 and <= 231);
    }

    [Test]
    public void ToBasic16_PicksTheNearestOfTheSixteen()
    {
        Assert.AreEqual(0, AnsiEncoder.ToBasic16(Color.Black));
        Assert.AreEqual(15, AnsiEncoder.ToBasic16(Color.White));
        Assert.AreEqual(1, AnsiEncoder.ToBasic16(new Color(160, 0, 0)));
        Assert.AreEqual(12, AnsiEncoder.ToBasic16(new Color(120, 120, 255)));
    }

    [Test]
    public void ToBasic16_KeepsADarkGreyVisibleAsBrightBlack()
    {
        Assert.AreEqual(8, AnsiEncoder.ToBasic16(new Color(90, 90, 90)));
    }
}
