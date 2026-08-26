// Why a stick needs a dead zone, and why the obvious dead zone is the wrong one.
//
// A stick at rest does not report zero. It reports something near zero that wanders, so a
// character left alone drifts across the screen. The fix is to ignore small values - but
// ignoring them per axis carves a square hole out of a round stick: push it exactly
// diagonally at 20% and both axes are inside their dead zones, so nothing happens at all,
// while pushing straight right at 20% works. The player feels a controller with corners.
//
// A radial dead zone measures the distance from the centre instead, so the ignored region is
// the circle the stick actually rests in. The remainder is then rescaled, so the first
// movement past the edge of the dead zone is a small movement and not a jump to 20%.

using GEngine.Core;

namespace GEngine.Input.Gamepad;

/// <summary>Removes the wobble a stick has at rest.</summary>
public static class DeadZone
{
    /// <summary>The dead zone a DualSense needs: fifteen per cent of full travel.</summary>
    public const float DefaultRadius = 0.15f;

    /// <summary>Applies a radial dead zone to a stick and rescales what is left.</summary>
    /// <param name="stick">The stick, with each axis from minus one to one.</param>
    /// <param name="radius">How far from the centre counts as not moved, from zero to one.</param>
    /// <returns>Zero inside the dead zone, and a smooth ramp to full travel outside it.</returns>
    public static Vector2 ApplyRadial(Vector2 stick, float radius = DefaultRadius)
    {
        float magnitude = stick.Length;
        if (magnitude <= radius)
        {
            return Vector2.Zero;
        }

        float rescaled = MathG.Clamp01((magnitude - radius) / (1.0f - radius));
        return stick.Normalized() * rescaled;
    }

    /// <summary>Applies a dead zone to a single value, such as a trigger.</summary>
    /// <param name="value">The value, from zero to one.</param>
    /// <param name="threshold">Below this the value reads as zero.</param>
    /// <returns>Zero below the threshold, and a smooth ramp to one above it.</returns>
    public static float ApplyLinear(float value, float threshold = DefaultRadius)
    {
        if (value <= threshold)
        {
            return 0.0f;
        }

        return MathG.Clamp01((value - threshold) / (1.0f - threshold));
    }

    /// <summary>
    /// Turns a byte axis from a report into a signed float. The centre of a DualSense axis is
    /// 128, not 127.5, so the two halves are scaled separately: without that, a stick at rest
    /// reports a small constant push in one direction for ever.
    /// </summary>
    /// <param name="raw">The raw byte, 0 to 255.</param>
    /// <returns>Minus one to one, with 128 mapping to exactly zero.</returns>
    public static float FromAxisByte(byte raw)
    {
        const float Center = 128.0f;
        float offset = raw - Center;
        return offset >= 0.0f ? offset / (255.0f - Center) : offset / Center;
    }
}
