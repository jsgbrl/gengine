using System;
using GEngine.Core;
using GEngine.Core.Contracts;
using GEngine.Input.Actions;
using GEngine.Input.Gamepad;
using GEngine.Input.Hid;
using GEngine.Rendering;
using GEngine.Rendering.Text;
using GEngine.Testing;
using MarioClone.Tests.Doubles;
using MarioClone.View;

namespace MarioClone.Tests;

/// <summary>
/// Showing keyboard keys to somebody holding a gamepad tells them the game was not written for
/// them. The screen asks the router which device last reported anything and names that device's
/// buttons - so what these tests check is that the naming actually changes with the device.
/// </summary>
public sealed class TitleScreenTests
{
    [Test]
    public void WithNoDeviceItAsksForOne()
    {
        Assert.IsTrue(Says(null, "PRESS ANYTHING"));
    }

    [Test]
    public void WithAKeyboardItNamesKeys()
    {
        using var keyboard = new HeldInput();
        Assert.IsTrue(Says(keyboard, Keyboard().Describe(InputAction.Jump)), "it named the jump key");
    }

    [Test]
    public void WithAGamepadItNamesButtons()
    {
        using var pad = new DualSenseGamepad(new FakeHidBackend(), Gamepad(), NullLogger.Instance);
        string button = Gamepad().Describe(InputAction.Jump);
        Assert.IsTrue(Says(pad, button), "it named the button, not the key");
        Assert.IsFalse(Says(pad, Keyboard().Describe(InputAction.Jump)), "and not both");
    }

    [Test]
    public void ItAlwaysSaysWhatTheGameIs()
    {
        Assert.IsTrue(Says(null, "GENGINE"));
        Assert.IsTrue(Says(null, "WORLD 1-1"));
        Assert.IsTrue(Says(null, "JUMP"));
        Assert.IsTrue(Says(null, "START"));
    }

    // The screen is drawn before anything else, so it has to clear what was there.
    [Test]
    public void ItCoversTheWholeFrame()
    {
        var frame = new FrameBuffer(160, 96);
        frame.Clear(Palette.Orange);
        Screen().Draw(frame, null);

        Assert.AreNotEqual(Palette.Orange, frame.GetPixel(0, 0), "the corner was repainted");
        Assert.AreNotEqual(Palette.Orange, frame.GetPixel(frame.Width - 1, frame.Height - 1));
    }

    [Test]
    public void ItFitsInASmallFrame()
    {
        var frame = new FrameBuffer(40, 24);
        Screen().Draw(frame, null);
        Assert.AreEqual(40, frame.Width, "a narrow terminal is a small frame, not an exception");
    }

    [Test]
    public void ItRefusesToBeBuiltOrDrawnWithoutWhatItNeeds()
    {
        Assert.Throws<ArgumentNullException>(() => new TitleScreen(null!, Gamepad()));
        Assert.Throws<ArgumentNullException>(() => new TitleScreen(Keyboard(), null!));
        Assert.Throws<ArgumentNullException>(() => Screen().Draw(null!, null));
    }

    private static InputMap Keyboard() => InputMap.CreateDefault();

    private static GamepadMap Gamepad() => GamepadMap.CreateDefault();

    private static TitleScreen Screen() => new(Keyboard(), Gamepad());

    // The screen writes with the pixel font, so reading it back is a snapshot of the frame.
    private static bool Says(IInputBackend? active, string text)
    {
        var frame = new FrameBuffer(200, 120);
        Screen().Draw(frame, active);
        var reference = new FrameBuffer(PixelFont.MeasureWidth(text), PixelFont.GlyphHeight);
        reference.Clear(Palette.DeepBlue);
        PixelFont.DrawTo(reference, text, Vector2.Zero, Palette.White);
        return Contains(frame, reference);
    }

    private static bool Contains(FrameBuffer frame, FrameBuffer wanted)
    {
        for (int y = 0; y + wanted.Height <= frame.Height; y++)
        {
            for (int x = 0; x + wanted.Width <= frame.Width; x++)
            {
                if (MatchesAt(frame, wanted, x, y))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool MatchesAt(FrameBuffer frame, FrameBuffer wanted, int atX, int atY)
    {
        for (int y = 0; y < wanted.Height; y++)
        {
            for (int x = 0; x < wanted.Width; x++)
            {
                bool ink = wanted.GetPixel(x, y) != Palette.DeepBlue;
                bool drawn = frame.GetPixel(atX + x, atY + y) != Palette.DeepBlue;
                if (ink != drawn)
                {
                    return false;
                }
            }
        }

        return true;
    }
}
