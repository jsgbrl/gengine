// The one exception type that means "the code under test is wrong" rather than "the
// test itself blew up". The runner tells the two apart by this type.

using System;

namespace GEngine.Testing;

/// <summary>
/// Thrown by <see cref="Assert"/> when an expectation fails. Any other exception escaping
/// a test is reported as an error in the test itself, with its full stack trace.
/// </summary>
public sealed class AssertionException : Exception
{
    /// <summary>Creates an assertion failure with no message.</summary>
    public AssertionException()
    {
    }

    /// <summary>Creates an assertion failure.</summary>
    /// <param name="message">What was expected and what happened instead.</param>
    public AssertionException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an assertion failure that wraps another exception.</summary>
    /// <param name="message">What was expected and what happened instead.</param>
    /// <param name="innerException">The underlying failure.</param>
    public AssertionException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
