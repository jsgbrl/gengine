using System;
using System.Collections.Generic;
using GEngine.Testing.Tests.Samples;

namespace GEngine.Testing.Tests;

/// <summary>Covers discovery, execution and filtering through <see cref="TestRunner"/>.</summary>
public sealed class TestRunnerTests
{
    [Test]
    public void Execute_RunsEveryTestMethodOfTheType()
    {
        TestSummary summary = Run(typeof(PassingSample));
        Assert.AreEqual(2, summary.PassedCount);
        Assert.AreEqual(0, summary.FailedCount);
        Assert.AreEqual(0, summary.ExitCode);
    }

    [Test]
    public void Execute_ReportsAFailedAssertionWithItsMessage()
    {
        TestSummary summary = Run(typeof(FailingSample));
        Assert.AreEqual(1, summary.FailedCount);
        Assert.AreEqual(1, summary.ExitCode);
        Assert.AreEqual(FailingSample.ExpectedMessage, summary.Results[0].Message);
    }

    [Test]
    public void Execute_ReportsANonAssertionExceptionWithItsTypeAndStack()
    {
        TestSummary summary = Run(typeof(ThrowingSample));
        string message = summary.Results[0].Message;
        Assert.AreEqual(TestOutcome.Failed, summary.Results[0].Outcome);
        Assert.IsTrue(message.StartsWith(nameof(InvalidOperationException), StringComparison.Ordinal));
        Assert.IsTrue(message.Contains(ThrowingSample.ExpectedMessage, StringComparison.Ordinal));
        Assert.IsTrue(message.Contains("at ", StringComparison.Ordinal), "a stack trace is included");
    }

    [Test]
    public void Execute_RunsSetupOnceBeforeEachTestOnAFreshInstance()
    {
        TestSummary summary = Run(typeof(SetupSample));
        Assert.AreEqual(2, summary.PassedCount);
    }

    [Test]
    public void Execute_SkipsAMarkedMethodAndKeepsTheRest()
    {
        TestSummary summary = Run(typeof(SkippedMethodSample));
        Assert.AreEqual(1, summary.SkippedCount);
        Assert.AreEqual(1, summary.PassedCount);
        Assert.AreEqual(SkippedMethodSample.Reason, Skipped(summary).Message);
    }

    [Test]
    public void Execute_SkipsEveryTestOfAMarkedClass()
    {
        TestSummary summary = Run(typeof(SkippedClassSample));
        Assert.AreEqual(2, summary.SkippedCount);
        Assert.AreEqual(0, summary.FailedCount);
    }

    [Test]
    public void Execute_ExpandsOneMethodIntoOneCasePerTestCaseAttribute()
    {
        TestSummary summary = Run(typeof(ParameterisedSample));
        Assert.AreEqual(4, summary.PassedCount);
    }

    [Test]
    public void Execute_ConvertsArgumentsToTheDeclaredParameterTypes()
    {
        TestSummary summary = Run(typeof(ParameterisedSample));
        Assert.AreEqual(0, summary.FailedCount);
    }

    [Test]
    public void Execute_HonoursTheFilterCaseInsensitively()
    {
        var options = new RunOptions { Filter = "alsopasses" };
        TestSummary summary = TestRunner.Execute(options, [typeof(PassingSample)]);
        Assert.AreEqual(1, summary.Results.Count);
    }

    [Test]
    public void List_NamesTheCasesWithoutRunningThem()
    {
        IReadOnlyList<string> names = TestRunner.List(new RunOptions(), [typeof(FailingSample)]);
        Assert.AreEqual(1, names.Count);
        Assert.IsTrue(names[0].EndsWith("FailingSample.AlwaysFails", StringComparison.Ordinal));
    }

    [Test]
    public void List_NamesAParameterisedCaseWithItsArguments()
    {
        IReadOnlyList<string> names = TestRunner.List(
            new RunOptions { Filter = "EachValueIsPositive" },
            [typeof(ParameterisedSample)]);
        Assert.AreEqual(3, names.Count);
        Assert.IsTrue(names[0].EndsWith("(1)", StringComparison.Ordinal));
    }

    [Test]
    public void Discovery_OrdersCasesByNameSoTwoRunsReportTheSameSequence()
    {
        IReadOnlyList<string> first = TestRunner.List(new RunOptions(), [typeof(PassingSample), typeof(FailingSample)]);
        IReadOnlyList<string> second = TestRunner.List(new RunOptions(), [typeof(FailingSample), typeof(PassingSample)]);
        Assert.MatchesSnapshot(string.Join("\n", second), string.Join("\n", first));
    }

    [Test]
    public void Discovery_IgnoresATypeWithNoTestMethods()
    {
        Assert.AreEqual(0, TestRunner.List(new RunOptions(), [typeof(EmptySample)]).Count);
    }

    private static TestSummary Run(Type sample)
    {
        return TestRunner.Execute(new RunOptions(), [sample]);
    }

    private static TestResult Skipped(TestSummary summary)
    {
        foreach (TestResult result in summary.Results)
        {
            if (result.Outcome == TestOutcome.Skipped)
            {
                return result;
            }
        }

        throw new AssertionException("no skipped case in the summary");
    }
}
