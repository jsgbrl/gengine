// What each hat direction means as movement. Kept apart from the enum because a file holds
// one public type, and because the mapping is a decision the enum itself has no opinion on.

using GEngine.Core;

namespace GEngine.Input.Gamepad;

/// <summary>Turns a hat direction into a movement vector.</summary>
public static class HatDirections
{
    /// <summary>
    /// The direction as a unit-ish vector on the screen-space Y-down axis: north is negative
    /// Y. Diagonals are the two axes at full strength rather than normalised, which is what a
    /// D-pad means and what the analogue stick is for when it is not.
    /// </summary>
    /// <param name="direction">The hat direction.</param>
    /// <returns>The movement it asks for.</returns>
    public static Vector2 ToVector(HatDirection direction) => direction switch
    {
        HatDirection.North => new Vector2(0.0f, -1.0f),
        HatDirection.NorthEast => new Vector2(1.0f, -1.0f),
        HatDirection.East => new Vector2(1.0f, 0.0f),
        HatDirection.SouthEast => new Vector2(1.0f, 1.0f),
        HatDirection.South => new Vector2(0.0f, 1.0f),
        HatDirection.SouthWest => new Vector2(-1.0f, 1.0f),
        HatDirection.West => new Vector2(-1.0f, 0.0f),
        HatDirection.NorthWest => new Vector2(-1.0f, -1.0f),
        _ => Vector2.Zero,
    };
}
