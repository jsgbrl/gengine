using GEngine.Testing;
using MarioClone.Actors;
using MarioClone.Audio;
using MarioClone.Tests.Doubles;

namespace MarioClone.Tests;

/// <summary>
/// The mushroom: the one pickup that changes the player rather than the score. Growing changes
/// the size of the body, which is the interesting part - a bigger box can be standing in a wall
/// the smaller one fitted through.
/// </summary>
public sealed class MushroomTests
{
    [Test]
    public void AMushroomMakesThePlayerBig()
    {
        var test = new TestWorld("""
            tiles:
            ............
            ............
            ..M.........
            ############
            """);

        test.Step(5);
        Assert.AreEqual(PlayerSize.Small, test.Player.Size);
        float feetBefore = test.Player.Bounds.Bottom;

        test.Player.Grow();

        Assert.AreEqual(PlayerSize.Big, test.Player.Size);
        Assert.ApproximatelyEqual(feetBefore, test.Player.Bounds.Bottom, 0.01f, "it grew upwards, not into the floor");
        Assert.ApproximatelyEqual(Player.BigHeightPixels, test.Player.Bounds.Size.Y, 0.01f);
        Assert.AreEqual(1, test.Audio.CountOf(GameSound.PowerUp));
    }

    [Test]
    public void GrowingTwiceChangesNothingTheSecondTime()
    {
        var test = new TestWorld("""
            tiles:
            ............
            ............
            ..M.........
            ############
            """);

        test.Step(5);
        test.Player.Grow();
        test.Player.Grow();
        Assert.AreEqual(1, test.Audio.CountOf(GameSound.PowerUp));
    }
}
