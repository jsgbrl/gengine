namespace GEngine.Testing.Tests;

/// <summary>Covers <see cref="TestSummary"/>.</summary>
public sealed class TestSummaryTests
{
    [Test]
    public void Counts_AddUpPerOutcome()
    {
        TestSummary summary = Summary(TestOutcome.Passed, TestOutcome.Failed, TestOutcome.Skipped, TestOutcome.Passed);
        Assert.AreEqual(2, summary.PassedCount);
        Assert.AreEqual(1, summary.FailedCount);
        Assert.AreEqual(1, summary.SkippedCount);
    }

    [Test]
    public void ExitCode_IsZeroWhenNothingFailed_EvenWithSkips()
    {
        Assert.AreEqual(0, Summary(TestOutcome.Passed, TestOutcome.Skipped).ExitCode);
    }

    [Test]
    public void ExitCode_IsOneWhenSomethingFailed()
    {
        Assert.AreEqual(1, Summary(TestOutcome.Passed, TestOutcome.Failed).ExitCode);
    }

    [Test]
    public void Results_KeepTheOrderTheyWereGivenIn()
    {
        TestSummary summary = Summary(TestOutcome.Failed, TestOutcome.Passed);
        Assert.AreEqual(TestOutcome.Failed, summary.Results[0].Outcome);
        Assert.AreEqual(TestOutcome.Passed, summary.Results[1].Outcome);
    }

    private static TestSummary Summary(params TestOutcome[] outcomes)
    {
        var results = new TestResult[outcomes.Length];
        for (int index = 0; index < outcomes.Length; index++)
        {
            results[index] = new TestResult("case" + index, outcomes[index], 1.0, string.Empty);
        }

        return new TestSummary(results, 4.0);
    }
}
