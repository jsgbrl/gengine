using GEngine.Testing;

namespace GEngine.Core.Tests;

/// <summary>Covers <see cref="Color"/>.</summary>
public sealed class ColorTests
{
    [Test]
    public void Constructor_DefaultsToOpaque()
    {
        var red = new Color(255, 0, 0);
        Assert.AreEqual(255, red.A);
        Assert.IsTrue(red.IsOpaque);
        Assert.IsFalse(red.IsTransparent);
    }

    [Test]
    public void Transparent_IsInvisible()
    {
        Assert.IsTrue(Color.Transparent.IsTransparent);
        Assert.IsFalse(Color.Transparent.IsOpaque);
    }

    [Test]
    public void WithAlpha_ChangesOnlyTheAlpha()
    {
        Color faded = Color.White.WithAlpha(128);
        Assert.AreEqual(255, faded.R);
        Assert.AreEqual(128, faded.A);
    }

    [Test]
    public void Lerp_MovesEveryChannelIncludingAlpha()
    {
        var half = Color.Lerp(Color.Black, Color.White, 0.5f);
        Assert.AreEqual(128, half.R);
        Assert.AreEqual(128, half.G);
        Assert.AreEqual(128, half.B);
        Assert.AreEqual(255, half.A);
    }

    [Test]
    public void Over_WithAnOpaqueSource_KeepsTheSource()
    {
        Assert.AreEqual(Color.White, Color.Over(Color.White, Color.Black));
    }

    [Test]
    public void Over_WithATransparentSource_KeepsTheDestination()
    {
        Assert.AreEqual(Color.Black, Color.Over(Color.Transparent, Color.Black));
    }

    [Test]
    public void Over_WithAHalfTransparentSource_MixesTheTwo()
    {
        var blended = Color.Over(Color.White.WithAlpha(128), Color.Black);
        Assert.AreEqual(128, blended.R);
        Assert.AreEqual(255, blended.A);
    }

    [Test]
    public void Equality_ComparesAllFourChannels()
    {
        var opaque = new Color(1, 2, 3);
        var faded = new Color(1, 2, 3, 4);
        Assert.IsTrue(opaque == new Color(1, 2, 3));
        Assert.IsTrue(opaque != faded);
        Assert.IsTrue(opaque.Equals((object)new Color(1, 2, 3)));
        Assert.IsFalse(opaque.Equals("not a colour"));
        Assert.AreEqual(opaque.GetHashCode(), new Color(1, 2, 3).GetHashCode());
    }

    [Test]
    public void ToString_IsTheHexTheTerminalWouldGet()
    {
        Assert.AreEqual("#0A0B0CFF", new Color(10, 11, 12).ToString());
    }
}
