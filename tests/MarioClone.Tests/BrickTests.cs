using GEngine.Core.Contracts;
using GEngine.Testing;
using MarioClone.Actors;
using MarioClone.Audio;
using MarioClone.Game;
using MarioClone.Tests.Doubles;

namespace MarioClone.Tests;

/// <summary>
/// A brick holds for a small player and breaks for a big one, which is the only place in the
/// game where what the player is changes what the level does.
/// </summary>
public sealed class BrickTests
{
    private const string BrickOverhead = """
        tiles:
        ............
        ..B.........
        ............
        ..M.........
        ############
        """;

    [Test]
    public void ASmallPlayerBumpsABrickAndItHolds()
    {
        var test = new TestWorld(BrickOverhead);
        JumpIntoIt(test);
        Assert.AreEqual(1, test.CountOf<Brick>(), "a small player cannot break it");
        Assert.AreEqual(1, test.Audio.CountOf(GameSound.BlockBump));
        Assert.AreEqual(0, test.Audio.CountOf(GameSound.BrickBreak));
    }

    [Test]
    public void ABigPlayerBreaksABrick()
    {
        var test = new TestWorld("""
            tiles:
            ............
            ............
            ..B.........
            ............
            ..M.........
            ############
            """);

        test.Step(5);
        test.Player.Grow();
        JumpIntoIt(test);

        Assert.AreEqual(0, test.CountOf<Brick>(), "it broke");
        Assert.AreEqual(GameSession.BrickScore, test.Session.Score);
        Assert.AreEqual(1, test.Audio.CountOf(GameSound.BrickBreak));
    }

    private static void JumpIntoIt(TestWorld test)
    {
        test.Step(5);
        test.Hold(InputAction.Jump);
        test.Step(40);
        test.Release();
        test.Step(20);
    }
}
