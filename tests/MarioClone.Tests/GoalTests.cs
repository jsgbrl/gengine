using GEngine.Core.Contracts;
using GEngine.Testing;
using MarioClone.Actors;
using MarioClone.Audio;
using MarioClone.Game;
using MarioClone.Tests.Doubles;

namespace MarioClone.Tests;

/// <summary>
/// The flag: the one actor whose whole job is to end the level. It is a tall thin trigger
/// rather than a tile, because reaching it has to be an event the game hears rather than a
/// position the game keeps checking for.
/// </summary>
public sealed class GoalTests
{
    private const string ShortRun = "tiles:\n.M......F.\n##########\n";

    [Test]
    public void TouchingTheFlagEndsTheLevel()
    {
        var test = new TestWorld(ShortRun);
        Assert.IsFalse(test.Player.HasReachedGoal, "not yet");

        test.Hold(InputAction.MoveRight);
        test.Step(200);

        Assert.IsTrue(test.Player.HasReachedGoal, "he ran into it");
        Assert.AreEqual(1, test.Audio.CountOf(GameSound.LevelComplete));
    }

    [Test]
    public void TheFlagIsWorthPoints()
    {
        var test = new TestWorld(ShortRun);
        test.Hold(InputAction.MoveRight);
        test.Step(200);
        Assert.IsTrue(test.Session.Score >= GameSession.GoalScore);
    }

    // A flag that is solid is a wall you cannot get past, and a flag that counts twice is a
    // level you finish twice.
    [Test]
    public void TheFlagIsWalkedThroughAndOnlyCountsOnce()
    {
        var test = new TestWorld(ShortRun);
        test.Hold(InputAction.MoveRight);
        test.Step(200);
        int scored = test.Session.Score;

        test.Step(100);
        Assert.AreEqual(scored, test.Session.Score, "standing in it changes nothing");
        Assert.AreEqual(1, test.Audio.CountOf(GameSound.LevelComplete));
    }

    [Test]
    public void TheFlagIsTallerThanItIsWideAndStandsOnTheGround()
    {
        var test = new TestWorld(ShortRun);
        Goal flag = test.Find<Goal>()!;
        Assert.IsTrue(Goal.HeightPixels > Goal.WidthPixels, "you can see it from a long way off");
        Assert.IsTrue(flag.Body.IsTrigger, "and run into it rather than stop at it");
        Assert.IsTrue(flag.Bounds.Bottom >= test.Player.Bounds.Bottom - 1.0f, "its foot is on the floor");
    }

    [Test]
    public void AGoombaCannotFinishTheLevel()
    {
        var test = new TestWorld("tiles:\n.M....G.F.\n##########\n");
        test.Step(300);
        Assert.IsFalse(test.Player.HasReachedGoal, "only the player reaches the flag");
        Assert.AreEqual(0, test.Audio.CountOf(GameSound.LevelComplete));
    }
}
