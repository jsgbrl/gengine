// What a tile does to a body that touches it. Three values, because a platformer needs
// exactly one thing beyond solid and empty.

namespace GEngine.Physics.Tiles;

/// <summary>How a tile of a tilemap collides.</summary>
public enum TileCollision
{
    /// <summary>Nothing there. Bodies pass straight through.</summary>
    None,

    /// <summary>Solid on all four sides.</summary>
    Solid,

    /// <summary>
    /// Solid only from above, and only for a body that is falling. This is what lets the
    /// player jump up through a ledge and then land on it.
    /// </summary>
    OneWay,
}
