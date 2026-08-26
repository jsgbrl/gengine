using System;

namespace GEngine.Testing.Tests;

/// <summary>Covers <see cref="Assert.MatchesSnapshot"/>.</summary>
public sealed class AssertSnapshotTests
{
    [Test]
    public void MatchesSnapshot_AcceptsIdenticalText()
    {
        Assert.MatchesSnapshot("ab\ncd", "ab\ncd");
    }

    [Test]
    public void MatchesSnapshot_IgnoresTheLineEndingConvention()
    {
        Assert.MatchesSnapshot("ab\r\ncd", "ab\ncd");
    }

    [Test]
    public void MatchesSnapshot_ReportsTheFirstDifferingLineNumber()
    {
        AssertionException failure = Assert.Throws<AssertionException>(
            static () => Assert.MatchesSnapshot("ab\nXX\nef", "ab\ncd\nef"));
        Assert.IsTrue(failure.Message.Contains("line 2", StringComparison.Ordinal));
        Assert.IsTrue(failure.Message.Contains("|cd|", StringComparison.Ordinal));
        Assert.IsTrue(failure.Message.Contains("|XX|", StringComparison.Ordinal));
    }

    [Test]
    public void MatchesSnapshot_ReportsAMissingTrailingLine()
    {
        AssertionException failure = Assert.Throws<AssertionException>(
            static () => Assert.MatchesSnapshot("ab", "ab\ncd"));
        Assert.IsTrue(failure.Message.Contains("<end of capture>", StringComparison.Ordinal));
    }
}
