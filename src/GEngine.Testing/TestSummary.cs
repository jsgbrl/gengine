// What a whole run adds up to. Exit code comes from FailedCount and nothing else.

using System;
using System.Collections.Generic;

namespace GEngine.Testing;

/// <summary>The results of a whole run, plus the counts the exit code is derived from.</summary>
public sealed class TestSummary
{
    private readonly List<TestResult> _results;

    /// <summary>Creates a summary over the given results.</summary>
    /// <param name="results">Every case that was considered, in report order.</param>
    /// <param name="elapsedMilliseconds">Wall time of the whole run.</param>
    public TestSummary(IEnumerable<TestResult> results, double elapsedMilliseconds)
    {
        ArgumentNullException.ThrowIfNull(results);
        _results = [.. results];
        ElapsedMilliseconds = elapsedMilliseconds;
        PassedCount = Count(TestOutcome.Passed);
        FailedCount = Count(TestOutcome.Failed);
        SkippedCount = Count(TestOutcome.Skipped);
    }

    /// <summary>Every case that was considered, in report order.</summary>
    public IReadOnlyList<TestResult> Results => _results;

    /// <summary>Wall time of the whole run, in milliseconds.</summary>
    public double ElapsedMilliseconds { get; }

    /// <summary>How many cases passed.</summary>
    public int PassedCount { get; }

    /// <summary>How many cases failed.</summary>
    public int FailedCount { get; }

    /// <summary>How many cases were skipped.</summary>
    public int SkippedCount { get; }

    /// <summary>The process exit code this run should produce.</summary>
    public int ExitCode => FailedCount == 0 ? 0 : 1;

    private int Count(TestOutcome outcome)
    {
        int total = 0;
        foreach (TestResult result in _results)
        {
            if (result.Outcome == outcome)
            {
                total++;
            }
        }

        return total;
    }
}
