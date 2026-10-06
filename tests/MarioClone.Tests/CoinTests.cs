using GEngine.Core.Contracts;
using GEngine.Testing;
using MarioClone.Actors;
using MarioClone.Audio;
using MarioClone.Game;
using MarioClone.Tests.Doubles;

namespace MarioClone.Tests;

/// <summary>Coins and mushrooms: collected once, and never twice.</summary>
public sealed class CoinTests
{
    private const string CoinOnTheWay = """
        tiles:
        ............
        ............
        ..M.o.......
        ############
        """;

    [Test]
    public void WalkingIntoACoinCollectsItAndItDisappears()
    {
        var test = new TestWorld(CoinOnTheWay);
        Assert.AreEqual(1, test.CountOf<Coin>());

        test.Hold(InputAction.MoveRight);
        RunUntilCollected(test);

        Assert.AreEqual(0, test.CountOf<Coin>(), "the coin is gone");
        Assert.AreEqual(1, test.Session.Coins);
        Assert.AreEqual(GameSession.CoinScore, test.Session.Score);
        Assert.AreEqual(1, test.Audio.CountOf(GameSound.Coin));
    }

    [Test]
    public void ACoinIsCollectedOnceAndNotOncePerStep()
    {
        var test = new TestWorld(CoinOnTheWay);
        test.Hold(InputAction.MoveRight);
        RunUntilCollected(test);
        test.Step(30);
        Assert.AreEqual(1, test.Session.Coins, "walking over where it was does not score again");
    }

    [Test]
    public void ACoinDoesNotBlockThePlayer()
    {
        var test = new TestWorld(CoinOnTheWay);
        test.Hold(InputAction.MoveRight);
        test.Step(90);
        Assert.IsTrue(test.Player.Position.X > 40.0f, "the player walked straight through where it was");
    }

    private static void RunUntilCollected(TestWorld test)
    {
        for (int step = 0; step < 120 && test.CountOf<Coin>() > 0; step++)
        {
            test.Step();
        }
    }
}
