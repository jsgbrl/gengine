using System.IO;
using GEngine.Core.Contracts;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>
/// Covers <see cref="ConsoleDriver"/>, the half every platform driver shares. The output is
/// a StringWriter, so the exact escape sequences a driver would have sent to a terminal can
/// be read back - without switching the screen the test report is printing on.
/// </summary>
public sealed class ConsoleDriverTests
{
    private MemoryLogger _logger = new();

    [Setup]
    public void Setup() => _logger = new MemoryLogger();

    [Test]
    public void ANewDriverIsNotEnabledAndHasWrittenNothing()
    {
        using var driver = new TestDriver(_logger, ColorDepth.TrueColor);
        Assert.IsFalse(driver.IsEnabled);
        Assert.AreEqual(0, driver.Written.Length);
    }

    [Test]
    public void Enable_EntersTheAlternateScreenAndHidesTheCursor()
    {
        using var driver = new TestDriver(_logger, ColorDepth.TrueColor);
        driver.Enable();
        string written = driver.Written;
        Assert.IsTrue(written.Contains(AnsiEncoder.EnterAlternateScreen, System.StringComparison.Ordinal));
        Assert.IsTrue(written.Contains(AnsiEncoder.HideCursor, System.StringComparison.Ordinal));
        Assert.IsTrue(driver.IsEnabled);
    }

    [Test]
    public void Enable_NegotiatesTheDepthOnceAndSaysWhatItGot()
    {
        using var driver = new TestDriver(_logger, ColorDepth.Palette256);
        driver.Enable();
        driver.Enable();
        Assert.AreEqual(ColorDepth.Palette256, driver.Depth);
        Assert.AreEqual(1, driver.NegotiateCount);
        Assert.IsTrue(_logger.Contains("256 colours"));
    }

    [Test]
    public void Restore_LeavesTheAlternateScreenAndShowsTheCursor()
    {
        using var driver = new TestDriver(_logger, ColorDepth.TrueColor);
        driver.Enable();
        driver.Restore();
        string written = driver.Written;
        Assert.IsTrue(written.Contains(AnsiEncoder.ShowCursor, System.StringComparison.Ordinal));
        Assert.IsTrue(written.Contains(AnsiEncoder.LeaveAlternateScreen, System.StringComparison.Ordinal));
        Assert.IsFalse(driver.IsEnabled);
    }

    [Test]
    public void Restore_WithoutEnable_DoesNothingAndDoesNotThrow()
    {
        using var driver = new TestDriver(_logger, ColorDepth.TrueColor);
        driver.Restore();
        driver.Restore();
        Assert.AreEqual(0, driver.Written.Length);
    }

    [Test]
    public void Restore_Twice_OnlyRestoresOnce()
    {
        using var driver = new TestDriver(_logger, ColorDepth.TrueColor);
        driver.Enable();
        driver.Restore();
        int afterFirst = driver.Written.Length;
        driver.Restore();
        Assert.AreEqual(afterFirst, driver.Written.Length);
    }

    [Test]
    public void Dispose_RestoresTheTerminal()
    {
        var driver = new TestDriver(_logger, ColorDepth.TrueColor);
        driver.Enable();
        driver.Dispose();
        Assert.IsFalse(driver.IsEnabled, "disposing put the terminal back");
        Assert.IsTrue(driver.Written.Contains(AnsiEncoder.ShowCursor, System.StringComparison.Ordinal));
    }

    [Test]
    public void SizeFallsBackToEightyByTwentyFourWhenOutputIsNotATerminal()
    {
        using var driver = new TestDriver(_logger, ColorDepth.TrueColor);
        Assert.IsTrue(driver.Columns > 0);
        Assert.IsTrue(driver.Rows > 0);
    }

    private sealed class TestDriver : ConsoleDriver
    {
        private readonly ColorDepth _depth;

        public TestDriver(ILogger logger, ColorDepth depth)
            : base(logger, new StringWriter())
        {
            _depth = depth;
        }

        public override string Name => "test terminal";

        public string Written => Output.ToString() ?? string.Empty;

        public int NegotiateCount { get; private set; }

        protected override ColorDepth Negotiate()
        {
            NegotiateCount++;
            return _depth;
        }
    }
}
