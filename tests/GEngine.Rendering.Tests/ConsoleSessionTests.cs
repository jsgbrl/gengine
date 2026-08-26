using System;
using GEngine.Rendering.Tests.Doubles;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="ConsoleSession"/>.</summary>
public sealed class ConsoleSessionTests
{
    [Test]
    public void StartingASessionEnablesTheTerminal()
    {
        var driver = new FakeConsoleDriver();
        using var session = new ConsoleSession(driver);
        Assert.AreEqual(1, driver.EnableCount);
        Assert.AreSame(driver, session.Driver);
        Assert.IsNotNull(session.Renderer);
    }

    [Test]
    public void ANewSessionHasNotBeenCancelled()
    {
        var driver = new FakeConsoleDriver();
        using var session = new ConsoleSession(driver);
        Assert.IsFalse(session.IsCancelled);
    }

    [Test]
    public void Cancel_IsWhatTheGameLoopWatches()
    {
        var driver = new FakeConsoleDriver();
        using var session = new ConsoleSession(driver);
        session.Cancel();
        Assert.IsTrue(session.IsCancelled);
    }

    [Test]
    public void Dispose_PutsTheTerminalBack()
    {
        var driver = new FakeConsoleDriver();
        var session = new ConsoleSession(driver);
        session.Dispose();
        Assert.AreEqual(1, driver.RestoreCount);
    }

    [Test]
    public void Dispose_Twice_IsHarmless()
    {
        var driver = new FakeConsoleDriver();
        var session = new ConsoleSession(driver);
        session.Dispose();
        session.Dispose();
        Assert.AreEqual(1, driver.RestoreCount);
    }

    // ConsoleSession.Start has no test of its own on purpose. It is two calls - build the
    // driver for the platform, wrap it in a session - and both are covered:
    // PlatformConsoleDriverTests checks the first, everything above checks the second. A test
    // of Start itself would have to build a real driver on the real terminal and enable it,
    // which would switch the screen the test report is being printed on.

    [Test]
    public void AMissingDriverIsRefused()
    {
        Assert.Throws<ArgumentNullException>(static () => new ConsoleSession(null!));
    }
}
