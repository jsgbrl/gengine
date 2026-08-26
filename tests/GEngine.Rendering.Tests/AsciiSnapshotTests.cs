using System;
using GEngine.Core;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="AsciiSnapshot"/>.</summary>
public sealed class AsciiSnapshotTests
{
    [Test]
    public void ATransparentFrameIsAllSpaces()
    {
        Assert.MatchesSnapshot(AsciiSnapshot.Capture(new FrameBuffer(3, 2)), "   \n   ");
    }

    [Test]
    public void BrightnessPicksTheCharacterFromTheRamp()
    {
        var buffer = new FrameBuffer(3, 1);
        buffer.SetPixel(0, 0, Color.Black);
        buffer.SetPixel(1, 0, new Color(128, 128, 128));
        buffer.SetPixel(2, 0, Color.White);
        Assert.AreEqual(" +@", AsciiSnapshot.Capture(buffer));
    }

    [Test]
    public void ACustomRampIsHonoured()
    {
        var buffer = new FrameBuffer(2, 1);
        buffer.SetPixel(1, 0, Color.White);
        Assert.AreEqual(".@", AsciiSnapshot.Capture(buffer, ".@"));
    }

    [Test]
    public void Brightness_WeightsGreenMostAndBlueLeast()
    {
        Assert.IsTrue(AsciiSnapshot.Brightness(new Color(0, 255, 0)) > AsciiSnapshot.Brightness(new Color(255, 0, 0)));
        Assert.IsTrue(AsciiSnapshot.Brightness(new Color(255, 0, 0)) > AsciiSnapshot.Brightness(new Color(0, 0, 255)));
        Assert.ApproximatelyEqual(1.0f, AsciiSnapshot.Brightness(Color.White), 0.01f);
        Assert.ApproximatelyEqual(0.0f, AsciiSnapshot.Brightness(Color.Black));
    }

    [Test]
    public void CharacterFor_MapsTransparentToTheFirstCharacter()
    {
        Assert.AreEqual('.', AsciiSnapshot.CharacterFor(Color.Transparent, ".@"));
    }

    [Test]
    public void CharacterFor_ClampsIntoTheRamp()
    {
        Assert.AreEqual('@', AsciiSnapshot.CharacterFor(Color.White, ".@"));
        Assert.AreEqual('#', AsciiSnapshot.CharacterFor(Color.White, "#"));
    }

    [Test]
    public void AnEmptyRampIsRefused()
    {
        Assert.Throws<ArgumentException>(static () => AsciiSnapshot.Capture(new FrameBuffer(1, 1), string.Empty));
    }

    [Test]
    public void EveryLineHasTheSameWidthAsTheFrame()
    {
        var buffer = new FrameBuffer(5, 3);
        foreach (string line in AsciiSnapshot.Capture(buffer).Split('\n'))
        {
            Assert.AreEqual(5, line.Length);
        }
    }
}
