// The heads-up display: five numbers along the top, in the font that lives in code. It is
// drawn last, after the level, so it is never behind anything.

using System;
using System.Globalization;
using GEngine.Core;
using GEngine.Rendering;
using GEngine.Rendering.Text;
using MarioClone.Game;

namespace MarioClone.View;

/// <summary>Draws the score, coins, world, time and lives.</summary>
public static class Hud
{
    /// <summary>How far from the top-left corner the display starts, in pixels.</summary>
    public const float MarginPixels = 2.0f;

    /// <summary>Draws the display across the top of the frame.</summary>
    /// <param name="frame">Where to draw.</param>
    /// <param name="session">The numbers to show.</param>
    public static void Draw(FrameBuffer frame, GameSession session)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(session);
        string left = string.Format(
            CultureInfo.InvariantCulture,
            "{0:000000} x{1:00}",
            session.Score,
            session.Coins);

        string right = string.Format(
            CultureInfo.InvariantCulture,
            "{0} {1:000} x{2}",
            session.World,
            (int)session.TimeLeftSeconds,
            session.Lives);

        PixelFont.DrawTo(frame, left, new Vector2(MarginPixels, MarginPixels), Palette.White);
        float rightX = frame.Width - MarginPixels - PixelFont.MeasureWidth(right);
        PixelFont.DrawTo(frame, right, new Vector2(rightX, MarginPixels), Palette.White);
    }

    /// <summary>Draws a line of text in the middle of the frame, as a banner.</summary>
    /// <param name="frame">Where to draw.</param>
    /// <param name="text">What to say.</param>
    /// <param name="color">What colour to say it in.</param>
    public static void DrawBanner(FrameBuffer frame, string text, Color color)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(text);
        float x = (frame.Width - PixelFont.MeasureWidth(text)) / 2.0f;
        float y = (frame.Height - PixelFont.GlyphHeight) / 2.0f;
        PixelFont.DrawTo(frame, text, new Vector2(x, y), color);
    }

    /// <summary>Draws a line of text centred, at a given height.</summary>
    /// <param name="frame">Where to draw.</param>
    /// <param name="text">What to say.</param>
    /// <param name="y">How far down the frame, in pixels.</param>
    /// <param name="color">What colour to say it in.</param>
    public static void DrawCentered(FrameBuffer frame, string text, float y, Color color)
    {
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(text);
        float x = (frame.Width - PixelFont.MeasureWidth(text)) / 2.0f;
        PixelFont.DrawTo(frame, text, new Vector2(x, y), color);
    }
}
