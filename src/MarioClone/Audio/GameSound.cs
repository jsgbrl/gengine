// The sounds the game would make. They are named here even though nothing plays them yet,
// because the alternative - adding the names later - means changing every place that would
// have asked for one.

namespace MarioClone.Audio;

/// <summary>A sound the game asks for.</summary>
public enum GameSound
{
    /// <summary>The player jumped.</summary>
    Jump,

    /// <summary>A coin was collected.</summary>
    Coin,

    /// <summary>A goomba was stomped.</summary>
    Stomp,

    /// <summary>A brick broke.</summary>
    BrickBreak,

    /// <summary>A block was bumped and gave what was inside.</summary>
    BlockBump,

    /// <summary>The player grew.</summary>
    PowerUp,

    /// <summary>The player was hit.</summary>
    Hurt,

    /// <summary>The player died.</summary>
    Death,

    /// <summary>The flag was reached.</summary>
    LevelComplete,
}
