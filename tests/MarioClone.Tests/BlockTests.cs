using GEngine.Core.Contracts;
using GEngine.Testing;
using MarioClone.Actors;
using MarioClone.Audio;
using MarioClone.Game;
using MarioClone.Tests.Doubles;

namespace MarioClone.Tests;

/// <summary>Question blocks give once; bricks break only for a big player.</summary>
public sealed class BlockTests
{
    private const string BlockOverhead = """
        tiles:
        ............
        ..?.........
        ............
        ..M.........
        ############
        """;

    private const string BrickOverhead = """
        tiles:
        ............
        ..B.........
        ............
        ..M.........
        ############
        """;

    [Test]
    public void HittingAQuestionBlockFromBelowGivesWhatIsInsideIt()
    {
        var test = new TestWorld(BlockOverhead);
        QuestionBlock block = test.Find<QuestionBlock>()!;
        Assert.IsFalse(block.IsSpent);

        JumpIntoIt(test);

        Assert.IsTrue(block.IsSpent);
        Assert.IsTrue(test.Session.Coins > 0 || test.CountOf<Mushroom>() > 0, "something came out");
        Assert.AreEqual(1, test.Audio.CountOf(GameSound.BlockBump));
    }

    [Test]
    public void AQuestionBlockGivesExactlyOnce()
    {
        var test = new TestWorld(BlockOverhead);
        JumpIntoIt(test);
        int afterFirst = test.Session.Score;

        test.Release();
        test.Step(40);
        JumpIntoIt(test);

        Assert.AreEqual(afterFirst, test.Session.Score, "the second hit gives nothing");
        Assert.AreEqual(1, test.Audio.CountOf(GameSound.BlockBump));
    }

    [Test]
    public void ASpentBlockIsStillSolidAndStillThere()
    {
        var test = new TestWorld(BlockOverhead);
        JumpIntoIt(test);
        Assert.AreEqual(1, test.CountOf<QuestionBlock>());
    }

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

    [Test]
    public void LandingOnTopOfABlockDoesNotBumpIt()
    {
        var test = new TestWorld("""
            tiles:
            ..M.........
            ............
            ..?.........
            ############
            """);

        test.Step(60);
        Assert.IsFalse(test.Find<QuestionBlock>()!.IsSpent, "a block is only hit from below");
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
