// What every console driver does the same way: open the stream, enter the alternate screen,
// hide the cursor, and put it all back afterwards whatever happened.
//
// The three platform drivers add one thing each - how they work out the colour depth, and what
// they have to do to earn it.
//
// The output stream is a constructor parameter so a test can hand in a StringWriter and
// assert on the exact bytes a driver would have sent, without a terminal and without
// switching the screen the test report is printing on.

using System;
using System.IO;
using System.Text;
using GEngine.Core.Contracts;

namespace GEngine.Rendering;

/// <summary>The parts of a console driver that are the same on every system.</summary>
public abstract class ConsoleDriver : IConsoleDriver
{
    private const int OutputBufferBytes = 1 << 16;
    private const int FallbackColumns = 80;
    private const int FallbackRows = 24;

    private readonly bool _ownsOutput;
    private bool _isEnabled;

    /// <summary>Creates a driver.</summary>
    /// <param name="logger">Where the driver explains what it managed to turn on.</param>
    /// <param name="output">Where to write, or null to open the real standard output.</param>
    protected ConsoleDriver(ILogger logger, TextWriter? output = null)
    {
        ArgumentNullException.ThrowIfNull(logger);
        Logger = logger;
        _ownsOutput = output is null;
        Output = output ?? OpenStandardOutput();
    }

    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <inheritdoc/>
    public ColorDepth Depth { get; private set; } = ColorDepth.Basic16;

    /// <inheritdoc/>
    public TextWriter Output { get; }

    /// <inheritdoc/>
    public int Columns => Measure(static () => System.Console.WindowWidth, FallbackColumns);

    /// <inheritdoc/>
    public int Rows => Measure(static () => System.Console.WindowHeight, FallbackRows);

    /// <summary>True between <see cref="Enable"/> and <see cref="Restore"/>.</summary>
    public bool IsEnabled => _isEnabled;

    /// <summary>Where the driver explains what it managed to turn on.</summary>
    protected ILogger Logger { get; }

    /// <summary>Opens standard output with a large buffer and no automatic flushing.</summary>
    /// <returns>The writer.</returns>
    public static TextWriter OpenStandardOutput() =>
        new StreamWriter(System.Console.OpenStandardOutput(), new UTF8Encoding(false), OutputBufferBytes)
        {
            AutoFlush = false,
        };

    /// <inheritdoc/>
    public void Enable()
    {
        if (_isEnabled)
        {
            return;
        }

        Depth = Negotiate();
        Logger.Info(Name + " - " + ConsoleCapabilities.Describe(Depth));
        TrySetUtf8Output();
        Output.Write(AnsiEncoder.EnterAlternateScreen);
        Output.Write(AnsiEncoder.HideCursor);
        Output.Write(AnsiEncoder.ClearScreen);
        Output.Flush();
        _isEnabled = true;
    }

    // Called from a finally, from Ctrl+C and from ProcessExit, so it has to survive being
    // called twice and being called after something else has already gone wrong.
    /// <inheritdoc/>
    public void Restore()
    {
        if (!_isEnabled)
        {
            return;
        }

        _isEnabled = false;
        try
        {
            Output.Write(AnsiEncoder.Reset);
            Output.Write(AnsiEncoder.ShowCursor);
            Output.Write(AnsiEncoder.LeaveAlternateScreen);
            Output.Flush();
        }
        catch (IOException problem)
        {
            Logger.Warning("could not restore the terminal: " + problem.Message);
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases the terminal and, when this driver opened it, the output stream.</summary>
    /// <param name="disposing">
    /// True when called from <see cref="Dispose()"/>. The boolean parameter is the one the
    /// framework's disposal pattern requires, and the one place in the repository where a
    /// method takes a flag rather than an enum.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        Restore();
        if (_ownsOutput)
        {
            Output.Dispose();
        }
    }

    /// <summary>Works out the depth, doing whatever this platform needs first.</summary>
    /// <returns>The depth the terminal will accept.</returns>
    protected abstract ColorDepth Negotiate();

    private void TrySetUtf8Output()
    {
        try
        {
            System.Console.OutputEncoding = new UTF8Encoding(false);
        }
        catch (IOException)
        {
            Logger.Warning("output is redirected, so the half-block character may not render");
        }
    }

    // WindowWidth throws when standard output is a pipe rather than a terminal, which is
    // exactly what happens under a build server. A fixed size beats a crash.
    private static int Measure(Func<int> read, int fallback)
    {
        try
        {
            int measured = read();
            return measured > 0 ? measured : fallback;
        }
        catch (Exception)
        {
            return fallback;
        }
    }
}
