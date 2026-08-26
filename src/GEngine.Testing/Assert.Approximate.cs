// Float assertions. Comparing floats with == is the single most common way a physics
// test becomes flaky, so the tolerance is a first-class, visible argument.

using System;
using System.Globalization;

namespace GEngine.Testing;

/// <content>Assertions for floating-point values.</content>
public static partial class Assert
{
    /// <summary>Default tolerance: about one part in a hundred thousand.</summary>
    public const float DefaultTolerance = 1e-5f;

    /// <summary>Fails unless two floats agree to within a tolerance.</summary>
    /// <param name="expected">The value the test expects.</param>
    /// <param name="actual">The value the code produced.</param>
    /// <param name="tolerance">Largest accepted absolute difference.</param>
    /// <param name="message">Context added to the failure message.</param>
    public static void ApproximatelyEqual(
        float expected,
        float actual,
        float tolerance = DefaultTolerance,
        string message = "")
    {
        float difference = Math.Abs(expected - actual);
        if (difference <= tolerance && !float.IsNaN(difference))
        {
            return;
        }

        throw new AssertionException(Describe(
            Number(expected),
            Number(actual) + " (off by " + Number(difference) + ", tolerance " + Number(tolerance) + ")",
            message));
    }

    /// <summary>Fails unless the value lies inside an inclusive range.</summary>
    /// <param name="value">The value under test.</param>
    /// <param name="minimum">Smallest accepted value.</param>
    /// <param name="maximum">Largest accepted value.</param>
    /// <param name="message">Context added to the failure message.</param>
    public static void IsInRange(float value, float minimum, float maximum, string message = "")
    {
        if (value >= minimum && value <= maximum)
        {
            return;
        }

        throw new AssertionException(Describe(
            "between " + Number(minimum) + " and " + Number(maximum),
            Number(value),
            message));
    }

    /// <summary>Fails when the value is NaN or infinite.</summary>
    /// <param name="value">The value under test.</param>
    /// <param name="message">Context added to the failure message.</param>
    public static void IsFinite(float value, string message = "")
    {
        if (float.IsFinite(value))
        {
            return;
        }

        throw new AssertionException(Describe("a finite number", Number(value), message));
    }

    private static string Number(float value)
    {
        return value.ToString("0.######", CultureInfo.InvariantCulture);
    }
}
