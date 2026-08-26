// Drawing a picture in a terminal, with two tricks.
//
// The first is the half block. A character cell is about twice as tall as it is wide, so a
// picture drawn one pixel per cell comes out squashed. Printing U+2580 - the upper half
// block - and giving it a foreground colour and a background colour puts two square pixels
// in one cell: the foreground is the top one, the background is the bottom one. Square
// pixels, and twice the vertical resolution, for free.
//
// The second is the diff. Console.Clear() every frame is where the flicker comes from, and
// redrawing eighty by fifty cells is four thousand escape sequences. Only the cells that
// changed are sent, in one write, at the end of the frame - and a frame identical to the
// last one sends nothing at all.

using System;
using GEngine.Core;
using GEngine.Core.Contracts;

namespace GEngine.Rendering;

/// <summary>Presents frames in a terminal using half-block characters and truecolor ANSI.</summary>
public sealed partial class ConsoleRenderer : IRenderer
{
    /// <summary>U+2580, the upper half block: foreground on top, background underneath.</summary>
    public const char HalfBlock = '\u2580';

    private readonly IConsoleDriver _driver;
    private readonly AnsiBuffer _buffer = new();
    private Color[] _previousTop = [];
    private Color[] _previousBottom = [];
    private Color _lastForeground;
    private Color _lastBackground;
    private int _cellColumns;
    private int _cellRows;
    private int _cursorColumn = -1;
    private int _cursorRow = -1;
    private bool _hasColors;
    private bool _isPrimed;

    /// <summary>Creates a renderer over a console driver, and enables the driver.</summary>
    /// <param name="driver">The terminal to draw on.</param>
    public ConsoleRenderer(IConsoleDriver driver)
    {
        ArgumentNullException.ThrowIfNull(driver);
        _driver = driver;
        _driver.Enable();
    }

    /// <summary>Width of the surface in pixels, which is one per character cell.</summary>
    public int Width => _driver.Columns;

    /// <summary>Height of the surface in pixels, which is two per character cell.</summary>
    public int Height => _driver.Rows * 2;

    /// <summary>What a transparent pixel is composited onto, since a terminal has no alpha.</summary>
    public Color Background { get; set; } = Palette.Black;

    /// <summary>How many characters the last frame sent. Zero when nothing changed.</summary>
    public int CharactersWrittenLastFrame { get; private set; }

    /// <summary>How many times the terminal has been resized under this renderer.</summary>
    public int ResizeCount { get; private set; }

    /// <inheritdoc/>
    public void Present(IPixelSource frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        _buffer.Clear();
        AdaptToTerminalSize();
        ForgetCursor();

        int columns = Math.Min(_cellColumns, frame.Width);
        int rows = Math.Min(_cellRows, frame.Height / 2);
        for (int row = 0; row < rows; row++)
        {
            AppendRow(frame, row, columns);
        }

        Flush();
        _isPrimed = true;
    }

    /// <inheritdoc/>
    public void Dispose() => _driver.Dispose();

    private void Flush()
    {
        CharactersWrittenLastFrame = _buffer.Length;
        if (_buffer.Length == 0)
        {
            return;
        }

        _buffer.Append(AnsiEncoder.Reset);
        CharactersWrittenLastFrame = _buffer.Length;
        _buffer.FlushTo(_driver.Output);
        _driver.Output.Flush();
    }

    // A terminal that changed size invalidates every remembered cell, so the next frame is a
    // full redraw. Detecting it here rather than with a signal handler keeps the whole thing
    // to one comparison per frame and works the same on all three systems.
    private void AdaptToTerminalSize()
    {
        int columns = _driver.Columns;
        int rows = _driver.Rows;
        if (columns == _cellColumns && rows == _cellRows)
        {
            return;
        }

        if (_cellColumns != 0)
        {
            ResizeCount++;
        }

        _cellColumns = columns;
        _cellRows = rows;
        _previousTop = new Color[columns * rows];
        _previousBottom = new Color[columns * rows];
        _isPrimed = false;
        _buffer.Append(AnsiEncoder.ClearScreen);
    }

    private void ForgetCursor()
    {
        _cursorColumn = -1;
        _cursorRow = -1;
        _hasColors = false;
    }
}
