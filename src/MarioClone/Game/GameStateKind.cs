// The six states a run passes through. Written as an enum driven by a StateMachine rather than
// a pile of booleans, because "is paused and not dead and not finished" is how a game ends up
// paused inside a death animation.

namespace MarioClone.Game;

/// <summary>Where the game is.</summary>
public enum GameStateKind
{
    /// <summary>The title screen, waiting for a button.</summary>
    Title,

    /// <summary>Being played.</summary>
    Playing,

    /// <summary>Paused. The world is frozen and the display says so.</summary>
    Paused,

    /// <summary>The player died and the game is waiting before restarting.</summary>
    Death,

    /// <summary>The flag was reached.</summary>
    LevelComplete,

    /// <summary>No lives left.</summary>
    GameOver,
}
