using System;
using GEngine.Core;
using GEngine.Rendering.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>
/// Covers <see cref="ConsoleRenderer"/>: the half-block trick, the diff, and the two things
/// a terminal renderer has to survive - a resize and a frame that has not changed.
/// </summary>
public sealed class ConsoleRendererTests
{
    private FakeConsoleDriver _driver = new();

    [Setup]
    public void Setup() => _driver = new FakeConsoleDriver { Columns = 4, Rows = 2 };

    [Test]
    public void TheSurfaceIsOnePixelPerColumnAndTwoPerRow()
    {
        using var renderer = new ConsoleRenderer(_driver);
        Assert.AreEqual(4, renderer.Width);
        Assert.AreEqual(4, renderer.Height);
    }

    [Test]
    public void ConstructingItEnablesTheDriver()
    {
        using var renderer = new ConsoleRenderer(_driver);
        Assert.AreEqual(1, _driver.EnableCount);
    }

    [Test]
    public void OneCellCarriesTwoPixels_TheTopAsForegroundAndTheBottomAsBackground()
    {
        using var renderer = new ConsoleRenderer(_driver);
        var frame = new FrameBuffer(1, 2);
        frame.SetPixel(0, 0, new Color(10, 20, 30));
        frame.SetPixel(0, 1, new Color(40, 50, 60));
        _driver.ClearWritten();

        renderer.Present(frame);

        string written = _driver.Written;
        Assert.IsTrue(written.Contains("38;2;10;20;30", StringComparison.Ordinal), "the top pixel is the foreground");
        Assert.IsTrue(written.Contains("48;2;40;50;60", StringComparison.Ordinal), "the bottom pixel is the background");
        Assert.IsTrue(written.Contains(ConsoleRenderer.HalfBlock, StringComparison.Ordinal), "half block");
    }

    [Test]
    public void APresentedFrameThatHasNotChanged_SendsNothingAtAll()
    {
        using var renderer = new ConsoleRenderer(_driver);
        var frame = new FrameBuffer(4, 4);
        frame.Clear(Palette.Sky);

        renderer.Present(frame);
        Assert.IsTrue(renderer.CharactersWrittenLastFrame > 0);

        _driver.ClearWritten();
        renderer.Present(frame);
        Assert.AreEqual(0, renderer.CharactersWrittenLastFrame);
        Assert.AreEqual(0, _driver.Written.Length, "no bytes reach the terminal either");
    }

    [Test]
    public void OnlyTheCellsThatChangedAreSent()
    {
        using var renderer = new ConsoleRenderer(_driver);
        var frame = new FrameBuffer(4, 4);
        frame.Clear(Palette.Sky);
        renderer.Present(frame);
        int fullFrame = renderer.CharactersWrittenLastFrame;

        frame.SetPixel(0, 0, Palette.Red);
        renderer.Present(frame);

        Assert.IsTrue(renderer.CharactersWrittenLastFrame > 0);
        Assert.IsTrue(renderer.CharactersWrittenLastFrame < fullFrame / 2, "one cell is far cheaper than sixteen");
    }

    [Test]
    public void AResizedTerminal_ForcesAFullRedraw()
    {
        using var renderer = new ConsoleRenderer(_driver);
        var frame = new FrameBuffer(4, 4);
        frame.Clear(Palette.Sky);
        renderer.Present(frame);
        renderer.Present(frame);
        Assert.AreEqual(0, renderer.CharactersWrittenLastFrame);

        _driver.Columns = 3;
        renderer.Present(frame);
        Assert.IsTrue(renderer.CharactersWrittenLastFrame > 0);
        Assert.AreEqual(1, renderer.ResizeCount);
    }

    [Test]
    public void AFrameLargerThanTheTerminal_IsClippedRatherThanRefused()
    {
        using var renderer = new ConsoleRenderer(_driver);
        var frame = new FrameBuffer(100, 100);
        frame.Clear(Palette.Sky);
        renderer.Present(frame);
        Assert.IsTrue(renderer.CharactersWrittenLastFrame > 0);
    }

    [Test]
    public void ATransparentPixelIsCompositedOntoTheRendererBackground()
    {
        using var renderer = new ConsoleRenderer(_driver) { Background = new Color(1, 2, 3) };
        var frame = new FrameBuffer(1, 2);
        _driver.ClearWritten();
        renderer.Present(frame);
        Assert.IsTrue(_driver.Written.Contains("38;2;1;2;3", StringComparison.Ordinal));
    }

    [Test]
    public void ADriverAtSixteenColours_SendsPlainColourCodes()
    {
        _driver.Depth = ColorDepth.Basic16;
        using var renderer = new ConsoleRenderer(_driver);
        var frame = new FrameBuffer(1, 2);
        frame.Clear(Color.White);
        _driver.ClearWritten();

        renderer.Present(frame);

        Assert.IsTrue(_driver.Written.Contains("\u001b[97m", StringComparison.Ordinal));
        Assert.IsFalse(_driver.Written.Contains("38;2;", StringComparison.Ordinal));
    }

    [Test]
    public void DisposingTheRendererDisposesTheDriver()
    {
        var renderer = new ConsoleRenderer(_driver);
        renderer.Dispose();
        Assert.AreEqual(1, _driver.RestoreCount);
    }

    [Test]
    public void AMissingFrameIsRefused()
    {
        using var renderer = new ConsoleRenderer(_driver);
        Assert.Throws<ArgumentNullException>(() => renderer.Present(null!));
    }
}
