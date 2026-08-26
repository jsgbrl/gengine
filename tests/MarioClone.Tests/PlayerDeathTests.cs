using GEngine.Core.Contracts;
using GEngine.Testing;
using MarioClone.Actors;
using MarioClone.Audio;
using MarioClone.Tests.Doubles;

namespace MarioClone.Tests;

/// <summary>The ways a run ends: a pit, a goomba, and the flag.</summary>
public sealed class PlayerDeathTests
{
    private const string PitAhead = """
        tiles:
        ............
        ............
        ..M.........
        ####........
        ............
        ............
        ............
        ............
        """;

    [Test]
    public void FallingDownAPitKillsThePlayer()
    {
        var test = new TestWorld(PitAhead);
        test.Hold(InputAction.MoveRight);
        for (int step = 0; step < 240 && !test.Player.IsDead; step++)
        {
            test.Step();
        }

        Assert.IsTrue(test.Player.IsDead);
        Assert.AreEqual(1, test.Audio.CountOf(GameSound.Death));
    }

    [Test]
    public void APitKillsABigPlayerJustAsDead()
    {
        var test = new TestWorld(PitAhead);
        test.Step(5);
        test.Player.Grow();
        test.Hold(InputAction.MoveRight);
        for (int step = 0; step < 240 && !test.Player.IsDead; step++)
        {
            test.Step();
        }

        Assert.IsTrue(test.Player.IsDead, "being big is not a parachute");
    }

    [Test]
    public void ADeadPlayerStopsRespondingToInput()
    {
        var test = new TestWorld(PitAhead);
        test.Player.Kill();
        test.Hold(InputAction.MoveRight);
        test.Step(10);
        Assert.ApproximatelyEqual(0.0f, test.Player.Body.Velocity.X);
    }

    [Test]
    public void KillingAPlayerTwiceOnlyCountsOnce()
    {
        var test = new TestWorld(PitAhead);
        test.Player.Kill();
        test.Player.Kill();
        Assert.AreEqual(1, test.Audio.CountOf(GameSound.Death));
    }

    [Test]
    public void AnInvulnerablePlayerCannotBeHurt()
    {
        var test = new TestWorld(PitAhead);
        test.Step(5);
        test.Player.Grow();
        test.Player.Hurt();
        Assert.IsTrue(test.Player.IsInvulnerable);
        Assert.AreEqual(PlayerSize.Small, test.Player.Size);

        test.Player.Hurt();
        Assert.IsFalse(test.Player.IsDead, "the second hit landed inside the invulnerability");
    }

    [Test]
    public void InvulnerabilityWearsOff()
    {
        var test = new TestWorld(PitAhead);
        test.Step(5);
        test.Player.Grow();
        test.Player.Hurt();
        test.Step((int)(PlayerTuning.Default.InvulnerabilitySeconds * 60.0f) + 5);
        Assert.IsFalse(test.Player.IsInvulnerable);
    }

    [Test]
    public void TouchingTheFlagEndsTheLevelRatherThanBlockingThePlayer()
    {
        var test = new TestWorld("""
            tiles:
            ............
            ............
            ..M...F.....
            ############
            """);

        test.Hold(InputAction.MoveRight);
        for (int step = 0; step < 240 && !test.Player.HasReachedGoal; step++)
        {
            test.Step();
        }

        Assert.IsTrue(test.Player.HasReachedGoal);
        Assert.AreEqual(Game.GameSession.GoalScore, test.Session.Score);
        Assert.AreEqual(1, test.Audio.CountOf(GameSound.LevelComplete));
    }

    [Test]
    public void TheFlagIsOnlyReachedOnce()
    {
        var test = new TestWorld("""
            tiles:
            ............
            ............
            ..M...F.....
            ############
            """);

        test.Hold(InputAction.MoveRight);
        test.Step(240);
        Assert.AreEqual(Game.GameSession.GoalScore, test.Session.Score);
    }
}
