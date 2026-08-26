// A skip always carries a reason and the reason is always printed. A silently skipped
// test is worse than no test: it reads green and covers nothing.

using System;

namespace GEngine.Testing;

/// <summary>
/// Skips a test method, or every test in a class, and reports why. The summary counts
/// skips separately so they cannot hide inside a passing run.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SkipAttribute : Attribute
{
    /// <summary>Creates a skip marker.</summary>
    /// <param name="reason">Why this test cannot run. Printed with the result.</param>
    public SkipAttribute(string reason)
    {
        Reason = reason;
    }

    /// <summary>Why the test is skipped.</summary>
    public string Reason { get; }
}
