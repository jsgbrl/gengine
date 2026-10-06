// The assertion vocabulary.
//
// Every failure message says what was expected and what actually happened, in that order,
// because that is the order a reader debugs in. Assert is partial only so each part stays
// inside the 150-line type budget: the numeric and snapshot assertions live in
// Assert.Approximate.cs and Assert.Snapshot.cs.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GEngine.Testing;

/// <summary>
/// Assertions available to tests. A failing assertion throws
/// <see cref="AssertionException"/>; anything else that escapes a test is reported as an
/// error in the test rather than a failure of the code under test.
/// </summary>
public static partial class Assert
{
    /// <summary>Fails unconditionally.</summary>
    /// <param name="message">Why the test cannot continue.</param>
    public static void Fail(string message)
    {
        throw new AssertionException(message);
    }

    /// <summary>Fails unless the condition holds.</summary>
    /// <param name="condition">The condition under test.</param>
    /// <param name="message">Context added to the failure message.</param>
    public static void IsTrue(bool condition, string message = "")
    {
        if (condition)
        {
            return;
        }

        throw new AssertionException(Describe("true", "false", message));
    }

    /// <summary>Fails unless the condition is false.</summary>
    /// <param name="condition">The condition under test.</param>
    /// <param name="message">Context added to the failure message.</param>
    public static void IsFalse(bool condition, string message = "")
    {
        if (!condition)
        {
            return;
        }

        throw new AssertionException(Describe("false", "true", message));
    }

    /// <summary>Fails unless the two values are equal.</summary>
    /// <typeparam name="TValue">Type of the compared values.</typeparam>
    /// <param name="expected">The value the test expects.</param>
    /// <param name="actual">The value the code produced.</param>
    /// <param name="message">Context added to the failure message.</param>
    public static void AreEqual<TValue>(TValue expected, TValue actual, string message = "")
    {
        if (EqualityComparer<TValue>.Default.Equals(expected, actual))
        {
            return;
        }

        throw new AssertionException(Describe(Format(expected), Format(actual), message));
    }

    /// <summary>Fails when the two values are equal.</summary>
    /// <typeparam name="TValue">Type of the compared values.</typeparam>
    /// <param name="unexpected">The value the test rules out.</param>
    /// <param name="actual">The value the code produced.</param>
    /// <param name="message">Context added to the failure message.</param>
    public static void AreNotEqual<TValue>(TValue unexpected, TValue actual, string message = "")
    {
        if (!EqualityComparer<TValue>.Default.Equals(unexpected, actual))
        {
            return;
        }

        throw new AssertionException(Describe("anything else", Format(actual), message));
    }

    /// <summary>Fails unless both references point at the same object.</summary>
    /// <param name="expected">The instance the test expects.</param>
    /// <param name="actual">The instance the code produced.</param>
    /// <param name="message">Context added to the failure message.</param>
    public static void AreSame(object? expected, object? actual, string message = "")
    {
        if (ReferenceEquals(expected, actual))
        {
            return;
        }

        throw new AssertionException(Describe("the same instance", "a different instance", message));
    }

    /// <summary>Fails unless the value is null.</summary>
    /// <param name="value">The value under test.</param>
    /// <param name="message">Context added to the failure message.</param>
    public static void IsNull(object? value, string message = "")
    {
        if (value is null)
        {
            return;
        }

        throw new AssertionException(Describe("null", Format(value), message));
    }

    /// <summary>Fails when the value is null.</summary>
    /// <param name="value">The value under test.</param>
    /// <param name="message">Context added to the failure message.</param>
    public static void IsNotNull(object? value, string message = "")
    {
        if (value is not null)
        {
            return;
        }

        throw new AssertionException(Describe("not null", "null", message));
    }

    /// <summary>Fails unless the action throws the expected exception type.</summary>
    /// <typeparam name="TException">The exception type the test expects.</typeparam>
    /// <param name="action">The code that should throw.</param>
    /// <param name="message">Context added to the failure message.</param>
    /// <returns>The thrown exception, so the test can assert on its contents.</returns>
    public static TException Throws<TException>(Action action, string message = "")
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(action);
        try
        {
            action();
        }
        catch (TException expected)
        {
            return expected;
        }

        throw new AssertionException(Describe(typeof(TException).Name, "no exception", message));
    }

    private static string Describe(string expected, string actual, string message)
    {
        string headline = string.Format(
            CultureInfo.InvariantCulture,
            "expected: {0}{1}actual:   {2}",
            expected,
            Environment.NewLine,
            actual);

        return message.Length == 0 ? headline : message + Environment.NewLine + headline;
    }

    private static string Format<TValue>(TValue value)
    {
        return value switch
        {
            null => "null",
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? "null",
        };
    }
}
