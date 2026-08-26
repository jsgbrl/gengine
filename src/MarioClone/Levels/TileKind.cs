// What a cell of the level's collision grid is. Only the inert geometry lives here: anything
// the game has to hear about - a brick that breaks, a block that gives a coin - is a body with
// a listener, not a tile, because a tile has no identity for a contact to point at.

namespace MarioClone.Levels;

/// <summary>What one cell of the level grid holds.</summary>
public enum TileKind
{
    /// <summary>Sky. Nothing to stand on.</summary>
    Empty,

    /// <summary>The ground, and the blocks that make up the scenery. Solid on all sides.</summary>
    Ground,

    /// <summary>A pipe. Solid, and drawn differently.</summary>
    Pipe,
}
