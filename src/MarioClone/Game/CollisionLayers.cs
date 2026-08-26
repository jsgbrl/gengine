// Who can touch whom, as bits. Layers are what stop a coin from blocking a goomba and a
// goomba from collecting a coin, without a single "if it is a goomba" anywhere in the solver.

namespace MarioClone.Game;

/// <summary>The collision layers of the game, and the masks that go with them.</summary>
public static class CollisionLayers
{
    /// <summary>Bricks, question blocks and anything else solid that is a body.</summary>
    public const int Level = 1 << 0;

    /// <summary>The player.</summary>
    public const int Player = 1 << 1;

    /// <summary>Goombas.</summary>
    public const int Enemy = 1 << 2;

    /// <summary>Coins and mushrooms.</summary>
    public const int Pickup = 1 << 3;

    /// <summary>The flag at the end.</summary>
    public const int Goal = 1 << 4;

    /// <summary>What the player collides with: everything.</summary>
    public const int PlayerMask = Level | Enemy | Pickup | Goal;

    /// <summary>What an enemy collides with: the level, the player, and each other.</summary>
    public const int EnemyMask = Level | Player | Enemy;

    /// <summary>What a pickup notices: only the player.</summary>
    public const int PickupMask = Player;

    /// <summary>What level geometry notices: whatever runs into it.</summary>
    public const int LevelMask = Player | Enemy;
}
