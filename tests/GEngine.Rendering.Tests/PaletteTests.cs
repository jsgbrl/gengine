using GEngine.Core;
using GEngine.Testing;

namespace GEngine.Rendering.Tests;

/// <summary>Covers <see cref="Palette"/>.</summary>
public sealed class PaletteTests
{
    [Test]
    public void EveryNamedColourIsInTheList()
    {
        Assert.IsTrue(Palette.All.Count >= 15);
        Assert.IsTrue(Holds(Palette.Sky));
        Assert.IsTrue(Holds(Palette.Yellow));
    }

    [Test]
    public void TransparentIsNotOneOfTheDrawableColours()
    {
        Assert.IsFalse(Holds(Color.Transparent));
        Assert.IsTrue(Palette.Transparent.IsTransparent);
    }

    [Test]
    public void EveryDrawableColourIsOpaque()
    {
        foreach (Color color in Palette.All)
        {
            Assert.IsTrue(color.IsOpaque);
        }
    }

    [TestCase("sky")]
    [TestCase("SKY")]
    [TestCase("Sky")]
    public void TryGetByName_IgnoresCase(string name)
    {
        Assert.IsTrue(Palette.TryGetByName(name, out Color color));
        Assert.AreEqual(Palette.Sky, color);
    }

    [Test]
    public void TryGetByName_ReportsAnUnknownName()
    {
        Assert.IsFalse(Palette.TryGetByName("beige", out Color color));
        Assert.IsTrue(color.IsTransparent);
    }

    private static bool Holds(Color wanted)
    {
        foreach (Color color in Palette.All)
        {
            if (color == wanted)
            {
                return true;
            }
        }

        return false;
    }

    [Test]
    public void TryGetByName_KnowsTransparent()
    {
        Assert.IsTrue(Palette.TryGetByName("transparent", out Color color));
        Assert.IsTrue(color.IsTransparent);
    }
}
