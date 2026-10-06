// One frame of escape sequences, built in a reusable char array and written to the terminal
// in a single call.
//
// A StringBuilder would be the obvious choice and it is the wrong one: ToString allocates
// the whole frame again, sixty times a second, and the engine promises no allocations in a
// steady frame. Numbers are formatted digit by digit for the same reason - int.ToString
// allocates a string per colour channel, and there are two per character cell.

using System;
using System.IO;

namespace GEngine.Rendering;

/// <summary>A growable character buffer that never allocates once it is warm.</summary>
public sealed class AnsiBuffer
{
    private char[] _characters;

    /// <summary>Creates a buffer.</summary>
    /// <param name="capacity">How many characters to reserve up front.</param>
    public AnsiBuffer(int capacity = 1 << 16)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(capacity);
        _characters = new char[capacity];
    }

    /// <summary>How many characters are in the buffer.</summary>
    public int Length { get; private set; }

    /// <summary>How many characters fit before the buffer has to grow.</summary>
    public int Capacity => _characters.Length;

    /// <summary>Empties the buffer without giving up its memory.</summary>
    public void Clear() => Length = 0;

    /// <summary>Appends one character.</summary>
    /// <param name="value">The character to append.</param>
    public void Append(char value)
    {
        EnsureRoomFor(1);
        _characters[Length++] = value;
    }

    /// <summary>Appends a string.</summary>
    /// <param name="value">The text to append; it is copied, not kept.</param>
    public void Append(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        EnsureRoomFor(value.Length);
        value.CopyTo(0, _characters, Length, value.Length);
        Length += value.Length;
    }

    /// <summary>Appends a non-negative number in decimal, without allocating.</summary>
    /// <param name="value">The number to append; it must not be negative.</param>
    public void AppendNumber(int value)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(value);
        if (value >= 10)
        {
            AppendNumber(value / 10);
        }

        Append((char)('0' + (value % 10)));
    }

    /// <summary>Writes everything to a stream and empties the buffer.</summary>
    /// <param name="writer">Where to write.</param>
    public void FlushTo(TextWriter writer)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.Write(_characters, 0, Length);
        Clear();
    }

    /// <inheritdoc/>
    public override string ToString() => new(_characters, 0, Length);

    private void EnsureRoomFor(int extra)
    {
        if (Length + extra <= _characters.Length)
        {
            return;
        }

        int wanted = Math.Max(_characters.Length * 2, Length + extra);
        Array.Resize(ref _characters, wanted);
    }
}
