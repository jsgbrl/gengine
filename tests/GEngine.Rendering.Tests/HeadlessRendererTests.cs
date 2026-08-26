using System;
using GEngine.Core;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="HeadlessRenderer"/>.</summary>
public sealed class HeadlessRendererTests
{
    [Test]
    public void ANewRendererHasPresentedNothing()
    {
        using var renderer = new HeadlessRenderer(4, 4);
        Assert.AreEqual(4, renderer.Width);
        Assert.AreEqual(4, renderer.Height);
        Assert.AreEqual(0, renderer.PresentCount);
    }

    [Test]
    public void PresentKeepsACopyOfTheFrame()
    {
        using var renderer = new HeadlessRenderer(2, 2);
        var frame = new FrameBuffer(2, 2);
        frame.SetPixel(1, 1, Palette.Red);
        renderer.Present(frame);

        Assert.AreEqual(1, renderer.PresentCount);
        Assert.AreEqual(Palette.Red, renderer.LastFrame.Pixels[3]);
    }

    [Test]
    public void ItIsACopyAndNotAReference()
    {
        using var renderer = new HeadlessRenderer(2, 2);
        var frame = new FrameBuffer(2, 2);
        renderer.Present(frame);
        frame.Clear(Palette.Red);
        Assert.IsTrue(renderer.LastFrame.Pixels[0].IsTransparent);
    }

    [Test]
    public void EachPresentReplacesTheLastRatherThanDrawingOverIt()
    {
        using var renderer = new HeadlessRenderer(2, 2);
        var first = new FrameBuffer(2, 2);
        first.Clear(Palette.Red);
        renderer.Present(first);
        renderer.Present(new FrameBuffer(2, 2));
        Assert.IsTrue(renderer.LastFrame.Pixels[0].IsTransparent);
    }

    [Test]
    public void Capture_RendersTheLastFrameAsText()
    {
        using var renderer = new HeadlessRenderer(2, 2);
        var frame = new FrameBuffer(2, 2);
        frame.Clear(Color.White);
        renderer.Present(frame);
        Assert.MatchesSnapshot(renderer.Capture(), "@@\n@@");
    }

    [Test]
    public void AMissingFrameIsRefused()
    {
        using var renderer = new HeadlessRenderer(2, 2);
        Assert.Throws<ArgumentNullException>(() => renderer.Present(null!));
    }

    [Test]
    public void ASteadyFrameAllocatesNothing()
    {
        using var renderer = new HeadlessRenderer(64, 32);
        var frame = new FrameBuffer(64, 32);
        for (int warmup = 0; warmup < 5; warmup++)
        {
            DrawAndPresent(renderer, frame);
        }

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int steady = 0; steady < 10; steady++)
        {
            DrawAndPresent(renderer, frame);
        }

        Assert.AreEqual(0L, GC.GetAllocatedBytesForCurrentThread() - before, "ten frames, no allocation");
    }

    private static void DrawAndPresent(HeadlessRenderer renderer, FrameBuffer frame)
    {
        frame.Clear(Palette.Sky);
        frame.DrawRect(new Aabb(new Vector2(4.0f, 4.0f), new Vector2(20.0f, 20.0f)), Palette.Brown);
        frame.DrawText("SCORE 000100", new Vector2(2.0f, 1.0f), Palette.White);
        renderer.Present(frame);
    }
}
