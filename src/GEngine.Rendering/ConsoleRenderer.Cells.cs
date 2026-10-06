// The inner loop: one character cell at a time, skipping the ones that have not changed and
// sending a colour only when it differs from the colour already in effect.
//
// A row of unchanged cells costs nothing; a row of same-coloured cells costs one escape and
// then one character each.

using GEngine.Core;
using GEngine.Core.Contracts;

namespace GEngine.Rendering;

/// <content>The per-cell diff and the escape sequences it emits.</content>
public sealed partial class ConsoleRenderer
{
    private void AppendRow(IPixelSource frame, int row, int columns)
    {
        for (int column = 0; column < columns; column++)
        {
            int index = (row * _cellColumns) + column;
            Color top = Flatten(PixelAt(frame, column, row * 2));
            Color bottom = Flatten(PixelAt(frame, column, (row * 2) + 1));
            if (_isPrimed && _previousTop[index] == top && _previousBottom[index] == bottom)
            {
                continue;
            }

            _previousTop[index] = top;
            _previousBottom[index] = bottom;
            AppendCell(column, row, top, bottom);
        }
    }

    private void AppendCell(int column, int row, Color top, Color bottom)
    {
        if (_cursorRow != row || _cursorColumn != column)
        {
            AnsiEncoder.AppendCursorTo(_buffer, column, row);
            _cursorRow = row;
            _cursorColumn = column;
            _hasColors = false;
        }

        AppendColors(top, bottom);
        _buffer.Append(HalfBlock);
        _cursorColumn++;
    }

    private void AppendColors(Color top, Color bottom)
    {
        if (!_hasColors || _lastForeground != top)
        {
            AnsiEncoder.AppendForeground(_buffer, top, _driver.Depth);
            _lastForeground = top;
        }

        if (!_hasColors || _lastBackground != bottom)
        {
            AnsiEncoder.AppendBackground(_buffer, bottom, _driver.Depth);
            _lastBackground = bottom;
        }

        _hasColors = true;
    }

    // A terminal has no alpha, so a partly transparent pixel is composited onto the
    // renderer's background before it is sent. Doing it here and not in the frame buffer
    // keeps the buffer honest about what was drawn into it.
    private Color Flatten(Color color) => color.IsOpaque ? color : Color.Over(color, Background);

    private static Color PixelAt(IPixelSource frame, int x, int y)
    {
        if (x < 0 || x >= frame.Width || y < 0 || y >= frame.Height)
        {
            return Color.Transparent;
        }

        return frame.Pixels[(y * frame.Width) + x];
    }
}
