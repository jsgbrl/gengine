using GEngine.Core.Contracts;
using GEngine.Testing;
using MarioClone.Actors;
using MarioClone.Audio;
using MarioClone.Game;
using MarioClone.Tests.Doubles;

namespace MarioClone.Tests;

/// <summary>
/// A goomba does four things, and each of them has a test: it patrols, it turns at a wall, it
/// turns at a ledge, and it decides whether it was stomped or walked into.
/// </summary>
public sealed class GoombaTests
{
    private const string Corridor = """
        tiles:
        ............
        ............
        ............
        ..M....G....
        ############
        """;

    private const string Shelf = """
        tiles:
        ............
        ............
        ............
        .....G......
        ..#######...
        ............
        """;

    private const string Boxed = """
        tiles:
        ............
        ............
        ...#..#.....
        ...#G.#.....
        ############
        """;

    [Test]
    public void AGoombaWalksLeftToBeginWith()
    {
        var test = new TestWorld(Corridor);
        Goomba goomba = test.Find<Goomba>()!;
        float start = goomba.Position.X;
        test.Step(30);
        Assert.IsTrue(goomba.Position.X < start);
        Assert.IsTrue(goomba.IsFacingLeft);
    }

    [Test]
    public void AGoombaTurnsAroundWhenItMeetsAWall()
    {
        var test = new TestWorld(Boxed);
        Goomba goomba = test.Find<Goomba>()!;
        test.Step(60);
        Assert.IsFalse(goomba.IsFacingLeft, "it hit the wall on its left and turned");
        Assert.IsTrue(goomba.Direction > 0.0f);
    }

    [Test]
    public void AGoombaTurnsAroundAtTheEdgeOfAShelfRatherThanWalkingOff()
    {
        var test = new TestWorld(Shelf);
        Goomba goomba = test.Find<Goomba>()!;
        test.Step(180);
        Assert.IsTrue(goomba.IsAlive, "it never fell");
        Assert.IsTrue(goomba.Bounds.Bottom <= 40.5f, "it is still on the shelf");
    }

    [Test]
    public void StompingAGoombaKillsItAndBouncesThePlayerBackUp()
    {
        TestWorld test = Stomp();
        Assert.AreEqual(0, test.CountOf<Goomba>(), "the goomba is gone");
        Assert.IsTrue(test.Player.Body.Velocity.Y < 0.0f, "and the player is on the way back up");
        Assert.IsFalse(test.Player.IsDead);
    }

    [Test]
    public void StompingAGoombaScoresAndMakesTheStompSound()
    {
        TestWorld test = Stomp();
        Assert.AreEqual(GameSession.StompScore, test.Session.Score);
        Assert.AreEqual(1, test.Audio.CountOf(GameSound.Stomp));
    }

    [Test]
    public void WalkingIntoAGoombaFromTheSideKillsThePlayer()
    {
        var test = new TestWorld(Corridor);
        test.Hold(InputAction.MoveRight);
        for (int step = 0; step < 240 && !test.Player.IsDead; step++)
        {
            test.Step();
        }

        Assert.IsTrue(test.Player.IsDead);
        Assert.AreEqual(1, test.CountOf<Goomba>(), "the goomba is still walking");
    }

    [Test]
    public void ABigPlayerWalkedIntoFromTheSideShrinksInsteadOfDying()
    {
        var test = new TestWorld(Corridor);
        test.Step(5);
        test.Player.Grow();
        Assert.AreEqual(PlayerSize.Big, test.Player.Size);

        test.Hold(InputAction.MoveRight);
        for (int step = 0; step < 240 && test.Player.Size == PlayerSize.Big; step++)
        {
            test.Step();
        }

        Assert.AreEqual(PlayerSize.Small, test.Player.Size);
        Assert.IsFalse(test.Player.IsDead);
        Assert.IsTrue(test.Player.IsInvulnerable, "and cannot be hit again straight away");
    }

    // Drops the player onto a goomba from directly above, which is the only way the solver can
    // read as a stomp.
    private static TestWorld Stomp()
    {
        var test = new TestWorld("""
            tiles:
            ............
            ............
            ..M.........
            ............
            ..G.........
            ############
            """);

        for (int step = 0; step < 120 && test.CountOf<Goomba>() > 0; step++)
        {
            test.Step();
        }

        return test;
    }
}
