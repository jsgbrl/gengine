// Three outcomes, no fourth. "Inconclusive" is how a suite starts lying to you.

namespace GEngine.Testing;

/// <summary>How a single test case ended.</summary>
public enum TestOutcome
{
    /// <summary>The test ran and every assertion held.</summary>
    Passed,

    /// <summary>An assertion failed, or the test threw.</summary>
    Failed,

    /// <summary>The test was not run, and said why.</summary>
    Skipped,
}
