// The things the level places rather than paves: they become entities with bodies, not tiles.

namespace MarioClone.Levels;

/// <summary>Something the level file asks the game to create.</summary>
public enum SpawnKind
{
    /// <summary>Where the player starts.</summary>
    Player,

    /// <summary>A goomba, walking left.</summary>
    Goomba,

    /// <summary>A coin, floating, waiting to be collected.</summary>
    Coin,

    /// <summary>A brick: solid, and breakable when the player is big.</summary>
    Brick,

    /// <summary>A question block: bumps up once and gives what is inside.</summary>
    QuestionBlock,

    /// <summary>The flag at the end of the level.</summary>
    Goal,
}
