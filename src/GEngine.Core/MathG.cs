// Scalar helpers. The name is MathG and not Math because a file that says
// "using GEngine.Core;" must still be able to reach System.Math without ceremony.

using System;

namespace GEngine.Core;

/// <summary>Scalar maths the engine needs and <see cref="MathF"/> does not provide.</summary>
public static class MathG
{
    /// <summary>
    /// The tolerance used when the engine compares floats. One part in a hundred thousand
    /// is far below one console pixel and far above the error a few hundred additions
    /// accumulate, which is exactly the band a game needs.
    /// </summary>
    public const float Epsilon = 1e-5f;

    /// <summary>Compares two floats allowing for accumulated error.</summary>
    /// <param name="left">First value.</param>
    /// <param name="right">Second value.</param>
    /// <param name="epsilon">Largest accepted difference.</param>
    /// <returns>True when the two agree within the epsilon.</returns>
    public static bool Approximately(float left, float right, float epsilon = Epsilon) =>
        MathF.Abs(left - right) <= epsilon;

    /// <summary>Keeps a value inside a range.</summary>
    /// <param name="value">The value.</param>
    /// <param name="minimum">Lower bound.</param>
    /// <param name="maximum">Upper bound.</param>
    /// <returns>The value, pulled to the nearest bound if it was outside.</returns>
    public static float Clamp(float value, float minimum, float maximum) =>
        MathF.Min(MathF.Max(value, minimum), maximum);

    /// <summary>Keeps a value between zero and one.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The clamped value.</returns>
    public static float Clamp01(float value) => Clamp(value, 0.0f, 1.0f);

    /// <summary>Straight-line interpolation, with the amount clamped to zero and one.</summary>
    /// <param name="from">Value at amount zero.</param>
    /// <param name="to">Value at amount one.</param>
    /// <param name="amount">Position between the two.</param>
    /// <returns>The interpolated value.</returns>
    public static float Lerp(float from, float to, float amount) =>
        from + ((to - from) * Clamp01(amount));

    /// <summary>Straight-line interpolation that is allowed to overshoot.</summary>
    /// <param name="from">Value at amount zero.</param>
    /// <param name="to">Value at amount one.</param>
    /// <param name="amount">Position between the two, unclamped.</param>
    /// <returns>The interpolated value.</returns>
    public static float LerpUnclamped(float from, float to, float amount) =>
        from + ((to - from) * amount);

    /// <summary>
    /// Steps a value towards a target by at most one increment. This is how friction and
    /// acceleration are written in the player controller: the target is a speed, the step
    /// is acceleration times delta time, and the value can never overshoot the target.
    /// </summary>
    /// <param name="current">Where the value is now.</param>
    /// <param name="target">Where it is heading.</param>
    /// <param name="maximumStep">Largest change allowed this call, as a positive number.</param>
    /// <returns>The stepped value.</returns>
    public static float MoveTowards(float current, float target, float maximumStep)
    {
        float difference = target - current;
        if (MathF.Abs(difference) <= maximumStep)
        {
            return target;
        }

        return current + (MathF.Sign(difference) * maximumStep);
    }

    /// <summary>The sign of a value, with zero mapping to zero.</summary>
    /// <param name="value">The value.</param>
    /// <returns>Minus one, zero or one.</returns>
    public static float Sign(float value)
    {
        if (Approximately(value, 0.0f))
        {
            return 0.0f;
        }

        return value < 0.0f ? -1.0f : 1.0f;
    }

    /// <summary>Largest integer not greater than the value.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The floor, as an integer.</returns>
    public static int FloorToInt(float value) => (int)MathF.Floor(value);

    /// <summary>Smallest integer not less than the value.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The ceiling, as an integer.</returns>
    public static int CeilToInt(float value) => (int)MathF.Ceiling(value);

    /// <summary>Nearest integer, with halves going away from zero.</summary>
    /// <param name="value">The value.</param>
    /// <returns>The rounded value, as an integer.</returns>
    public static int RoundToInt(float value) => (int)MathF.Round(value, MidpointRounding.AwayFromZero);
}
