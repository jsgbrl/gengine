// The numbers on the heads-up display, and the rules that change them. Kept apart from the
// actors so that a test can assert "collecting a coin scores 200" without a physics world.

using System;

namespace MarioClone.Game;

/// <summary>Score, coins, lives and the clock: one run through the game.</summary>
public sealed class GameSession
{
    /// <summary>What a coin is worth.</summary>
    public const int CoinScore = 200;

    /// <summary>What stomping a goomba is worth.</summary>
    public const int StompScore = 100;

    /// <summary>What breaking a brick is worth.</summary>
    public const int BrickScore = 50;

    /// <summary>What reaching the flag is worth.</summary>
    public const int GoalScore = 1000;

    /// <summary>How many lives a run starts with.</summary>
    public const int StartingLives = 3;

    /// <summary>How long a level lasts, in seconds of game time.</summary>
    public const float LevelSeconds = 300.0f;

    /// <summary>Points collected so far.</summary>
    public int Score { get; private set; }

    /// <summary>Coins collected so far.</summary>
    public int Coins { get; private set; }

    /// <summary>Lives left. The run ends when this reaches zero.</summary>
    public int Lives { get; private set; } = StartingLives;

    /// <summary>Which world this is, for the display.</summary>
    public string World { get; init; } = "1-1";

    /// <summary>How much time is left, in seconds.</summary>
    public float TimeLeftSeconds { get; private set; } = LevelSeconds;

    /// <summary>True when the clock has run out.</summary>
    public bool IsOutOfTime => TimeLeftSeconds <= 0.0f;

    /// <summary>True when there are no lives left.</summary>
    public bool IsGameOver => Lives <= 0;

    /// <summary>Adds points.</summary>
    /// <param name="points">How many.</param>
    public void AddScore(int points) => Score += Math.Max(0, points);

    /// <summary>Collects a coin, and its points with it.</summary>
    public void CollectCoin()
    {
        Coins++;
        AddScore(CoinScore);
    }

    /// <summary>Takes a life. The caller decides what happens next.</summary>
    public void LoseLife() => Lives = Math.Max(0, Lives - 1);

    /// <summary>Runs the clock down.</summary>
    /// <param name="deltaSeconds">Seconds of game time that passed.</param>
    public void Tick(float deltaSeconds) => TimeLeftSeconds = Math.Max(0.0f, TimeLeftSeconds - deltaSeconds);

    /// <summary>Puts the clock back to the start of a level, keeping score, coins and lives.</summary>
    public void RestartLevel() => TimeLeftSeconds = LevelSeconds;
}
