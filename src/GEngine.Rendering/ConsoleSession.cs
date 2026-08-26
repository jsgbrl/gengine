// The promise that the terminal is always left usable.
//
// There are four ways out of a game: it finishes, the player presses Esc, the player presses
// Ctrl+C, or something throws. Only the first two go through the code you wrote. A session
// hooks the other two - CancelKeyPress and ProcessExit - so the alternate screen is left and
// the cursor comes back whatever happens, and unhooks them again on the way out so a second
// session does not inherit the first one's handlers.

using System;
using GEngine.Core.Contracts;
using GEngine.Core.Platform;

namespace GEngine.Rendering;

/// <summary>A console put into drawing mode, and guaranteed to be put back.</summary>
public sealed class ConsoleSession : IDisposable
{
    private readonly ConsoleCancelEventHandler _onCancelKey;
    private readonly EventHandler _onProcessExit;
    private bool _isDisposed;

    /// <summary>Creates a session over a driver and takes ownership of it.</summary>
    /// <param name="driver">The terminal. The session disposes it.</param>
    public ConsoleSession(IConsoleDriver driver)
    {
        ArgumentNullException.ThrowIfNull(driver);
        Driver = driver;
        Renderer = new ConsoleRenderer(driver);
        _onCancelKey = (_, arguments) =>
        {
            arguments.Cancel = true;
            Cancel();
        };
        _onProcessExit = (_, _) => Driver.Restore();
        System.Console.CancelKeyPress += _onCancelKey;
        AppDomain.CurrentDomain.ProcessExit += _onProcessExit;
    }

    /// <summary>The terminal.</summary>
    public IConsoleDriver Driver { get; }

    /// <summary>The renderer drawing on it.</summary>
    public ConsoleRenderer Renderer { get; }

    /// <summary>True once the player asked to stop, by Ctrl+C or otherwise.</summary>
    public bool IsCancelled { get; private set; }

    /// <summary>Opens a session on whichever terminal this system has.</summary>
    /// <param name="probe">Says which platform this is.</param>
    /// <param name="logger">Where the driver explains what it managed to turn on.</param>
    /// <returns>The session. The caller must dispose it.</returns>
    public static ConsoleSession Start(IPlatformProbe probe, ILogger logger) =>
        new(ConsoleDriverFactory.Create(probe, logger));

    /// <summary>Asks the game to stop. This is what Ctrl+C does.</summary>
    public void Cancel() => IsCancelled = true;

    /// <summary>Unhooks the handlers and puts the terminal back.</summary>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        System.Console.CancelKeyPress -= _onCancelKey;
        AppDomain.CurrentDomain.ProcessExit -= _onProcessExit;
        Renderer.Dispose();
    }
}
