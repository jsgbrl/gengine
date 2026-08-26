// The D-pad, as one of nine values rather than four bits. The controller reports it that
// way - a number from 0 to 8 in half a byte - because the four directions are not
// independent: a hat cannot be up and down at once, and it can be up and right at once.

namespace GEngine.Input.Gamepad;

/// <summary>
/// Which way the D-pad is pointing. The numbers are the ones the controller sends, which
/// means the default value of this enum is <see cref="North"/> and not <see cref="Neutral"/> -
/// anything that builds a state by hand has to say so.
/// </summary>
public enum HatDirection
{
    /// <summary>Up.</summary>
    North = 0,

    /// <summary>Up and to the right.</summary>
    NorthEast = 1,

    /// <summary>To the right.</summary>
    East = 2,

    /// <summary>Down and to the right.</summary>
    SouthEast = 3,

    /// <summary>Down.</summary>
    South = 4,

    /// <summary>Down and to the left.</summary>
    SouthWest = 5,

    /// <summary>To the left.</summary>
    West = 6,

    /// <summary>Up and to the left.</summary>
    NorthWest = 7,

    /// <summary>Not pressed.</summary>
    Neutral = 8,
}
