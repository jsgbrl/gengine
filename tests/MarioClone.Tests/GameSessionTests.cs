using GEngine.Testing;
using MarioClone.Game;

namespace MarioClone.Tests;

/// <summary>
/// Score, coins, lives and the clock. Nothing here knows what a level is: the session is the
/// part of the game that survives a level, which is why it is a separate object from the world.
/// </summary>
public sealed class GameSessionTests
{
    [Test]
    public void ANewSessionStartsWithThreeLivesAndNothingElse()
    {
        var session = new GameSession();
        Assert.AreEqual(GameSession.StartingLives, session.Lives);
        Assert.AreEqual(0, session.Score);
        Assert.AreEqual(0, session.Coins);
        Assert.ApproximatelyEqual(GameSession.LevelSeconds, session.TimeLeftSeconds, 0.001f);
        Assert.IsFalse(session.IsGameOver);
        Assert.IsFalse(session.IsOutOfTime);
    }

    [Test]
    public void ACoinIsWorthACoinAndSomePoints()
    {
        var session = new GameSession();
        session.CollectCoin();
        Assert.AreEqual(1, session.Coins);
        Assert.AreEqual(GameSession.CoinScore, session.Score);
    }

    [Test]
    public void PointsAddUpAndNegativePointsDoNothing()
    {
        var session = new GameSession();
        session.AddScore(GameSession.StompScore);
        session.AddScore(GameSession.BrickScore);
        Assert.AreEqual(GameSession.StompScore + GameSession.BrickScore, session.Score);

        session.AddScore(-500);
        Assert.AreEqual(GameSession.StompScore + GameSession.BrickScore, session.Score, "a score never falls");
    }

    [Test]
    public void LivesRunOutAndStopAtZero()
    {
        var session = new GameSession();
        for (int life = 0; life < GameSession.StartingLives; life++)
        {
            Assert.IsFalse(session.IsGameOver, "still alive with " + session.Lives + " lives");
            session.LoseLife();
        }

        Assert.AreEqual(0, session.Lives);
        Assert.IsTrue(session.IsGameOver);

        session.LoseLife();
        Assert.AreEqual(0, session.Lives, "a life count never goes negative");
    }

    [Test]
    public void TheClockCountsDownAndStopsAtZero()
    {
        var session = new GameSession();
        session.Tick(1.0f);
        Assert.ApproximatelyEqual(GameSession.LevelSeconds - 1.0f, session.TimeLeftSeconds, 0.001f);

        session.Tick(GameSession.LevelSeconds);
        Assert.ApproximatelyEqual(0.0f, session.TimeLeftSeconds, 0.001f, "time never goes negative");
        Assert.IsTrue(session.IsOutOfTime);
    }

    [Test]
    public void RestartingALevelResetsTheClockAndKeepsTheScore()
    {
        var session = new GameSession();
        session.CollectCoin();
        session.Tick(120.0f);
        session.RestartLevel();

        Assert.ApproximatelyEqual(GameSession.LevelSeconds, session.TimeLeftSeconds, 0.001f);
        Assert.AreEqual(GameSession.CoinScore, session.Score, "what you earned is yours");
        Assert.AreEqual(1, session.Coins);
    }

    [Test]
    public void TheWorldNameIsWhateverTheGameWasStartedWith()
    {
        var session = new GameSession { World = "1-2" };
        Assert.AreEqual("1-2", session.World);
        Assert.AreEqual("1-1", new GameSession().World, "and one-one by default");
    }
}
