// A colour as four bytes. It lives in Core and not in Rendering because IRenderer is
// declared here, and an interface cannot talk about pixels without a word for one.

using System;
using System.Globalization;

namespace GEngine.Core;

/// <summary>
/// A colour with eight bits per channel and straight (non-premultiplied) alpha. Four
/// bytes is exactly what a truecolor ANSI escape needs, so no conversion happens on the
/// way to the terminal.
/// </summary>
public readonly struct Color : IEquatable<Color>
{
    /// <summary>Fully transparent, and the colour a frame buffer clears to by default.</summary>
    public static readonly Color Transparent = new(0, 0, 0, 0);

    /// <summary>Opaque black.</summary>
    public static readonly Color Black = new(0, 0, 0);

    /// <summary>Opaque white.</summary>
    public static readonly Color White = new(255, 255, 255);

    /// <summary>Red channel.</summary>
    public readonly byte R;

    /// <summary>Green channel.</summary>
    public readonly byte G;

    /// <summary>Blue channel.</summary>
    public readonly byte B;

    /// <summary>Alpha channel: 0 is invisible, 255 fully covers what is behind it.</summary>
    public readonly byte A;

    /// <summary>Creates a colour.</summary>
    /// <param name="red">Red channel.</param>
    /// <param name="green">Green channel.</param>
    /// <param name="blue">Blue channel.</param>
    /// <param name="alpha">Alpha channel, opaque by default.</param>
    public Color(byte red, byte green, byte blue, byte alpha = 255)
    {
        R = red;
        G = green;
        B = blue;
        A = alpha;
    }

    /// <summary>True when nothing behind this colour can show through.</summary>
    public bool IsOpaque => A == 255;

    /// <summary>True when this colour covers nothing at all.</summary>
    public bool IsTransparent => A == 0;

    /// <summary>The same colour with a different alpha.</summary>
    /// <param name="alpha">The new alpha channel.</param>
    /// <returns>The new colour.</returns>
    public Color WithAlpha(byte alpha) => new(R, G, B, alpha);

    /// <summary>Mixes two colours channel by channel, alpha included.</summary>
    /// <param name="from">Colour at amount zero.</param>
    /// <param name="to">Colour at amount one.</param>
    /// <param name="amount">Position between the two, clamped to zero and one.</param>
    /// <returns>The mixed colour.</returns>
    public static Color Lerp(Color from, Color to, float amount) => new(
        Mix(from.R, to.R, amount),
        Mix(from.G, to.G, amount),
        Mix(from.B, to.B, amount),
        Mix(from.A, to.A, amount));

    /// <summary>
    /// Composites a colour on top of another. The two common cases - fully opaque and
    /// fully transparent - return immediately, which is what almost every sprite pixel
    /// is; the general blend is there so a fade does not have to be a special case.
    /// </summary>
    /// <param name="source">The colour on top.</param>
    /// <param name="destination">The colour underneath.</param>
    /// <returns>The result of drawing source over destination.</returns>
    public static Color Over(Color source, Color destination)
    {
        if (source.IsOpaque || destination.IsTransparent)
        {
            return source;
        }

        if (source.IsTransparent)
        {
            return destination;
        }

        float coverage = source.A / 255.0f;
        return new Color(
            Mix(destination.R, source.R, coverage),
            Mix(destination.G, source.G, coverage),
            Mix(destination.B, source.B, coverage),
            (byte)Math.Max(source.A, destination.A));
    }

    /// <summary>Exact equality of all four channels.</summary>
    /// <param name="other">The colour to compare with.</param>
    /// <returns>True when every channel matches.</returns>
    public bool Equals(Color other) => R == other.R && G == other.G && B == other.B && A == other.A;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is Color other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(R, G, B, A);

    /// <inheritdoc/>
    public override string ToString() =>
        string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}{3:X2}", R, G, B, A);

    /// <summary>Exact equality.</summary>
    /// <param name="left">First colour.</param>
    /// <param name="right">Second colour.</param>
    /// <returns>True when every channel matches.</returns>
    public static bool operator ==(Color left, Color right) => left.Equals(right);

    /// <summary>Exact inequality.</summary>
    /// <param name="left">First colour.</param>
    /// <param name="right">Second colour.</param>
    /// <returns>True when a channel differs.</returns>
    public static bool operator !=(Color left, Color right) => !left.Equals(right);

    private static byte Mix(byte from, byte to, float amount) =>
        (byte)MathG.RoundToInt(MathG.Lerp(from, to, amount));
}
