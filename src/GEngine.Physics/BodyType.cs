// Three kinds of body, and the difference between them is entirely about who is allowed
// to move them.

namespace GEngine.Physics;

/// <summary>What moves a body and what a collision does to it.</summary>
public enum BodyType
{
    /// <summary>
    /// Never moves and is never pushed. Walls, floors, pipes. The solver treats an
    /// infinite mass here, which is why a player pressed into a wall stops dead instead of
    /// pushing the level sideways.
    /// </summary>
    Static,

    /// <summary>
    /// Moves because something set its velocity, and is never pushed by a collision. A
    /// lift or a moving platform: it goes where the game says, and carries whatever is
    /// standing on it.
    /// </summary>
    Kinematic,

    /// <summary>Moved by gravity, forces and collisions. The player, the goombas, a coin in flight.</summary>
    Dynamic,
}
