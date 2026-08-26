// Runs one case and turns whatever happens into a TestResult. Two failure shapes are
// deliberately kept apart: an AssertionException means the code under test is wrong,
// anything else means the test itself is wrong, and only the second prints a stack trace.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;

namespace GEngine.Testing;

/// <summary>Executes discovered cases, one at a time, on the calling thread.</summary>
internal static class TestExecutor
{
    /// <summary>Runs one case.</summary>
    /// <param name="testCase">The case to run.</param>
    /// <returns>What happened, with the time it took.</returns>
    public static TestResult Execute(TestCaseInfo testCase)
    {
        if (testCase.SkipReason.Length > 0)
        {
            return new TestResult(testCase.Name, TestOutcome.Skipped, 0.0, testCase.SkipReason);
        }

        var stopwatch = Stopwatch.StartNew();
        string failure = Invoke(testCase);
        stopwatch.Stop();

        TestOutcome outcome = failure.Length == 0 ? TestOutcome.Passed : TestOutcome.Failed;
        return new TestResult(testCase.Name, outcome, stopwatch.Elapsed.TotalMilliseconds, failure);
    }

    private static string Invoke(TestCaseInfo testCase)
    {
        try
        {
            object instance = Activator.CreateInstance(testCase.Method.DeclaringType!)!;
            testCase.Setup?.Invoke(instance, null);
            testCase.Method.Invoke(instance, Arguments(testCase));
            return string.Empty;
        }
        catch (TargetInvocationException invocation)
        {
            return Explain(invocation.InnerException ?? invocation);
        }
        catch (Exception harness)
        {
            return Explain(harness);
        }
    }

    private static object?[]? Arguments(TestCaseInfo testCase)
    {
        if (testCase.Arguments.Count == 0)
        {
            return null;
        }

        object?[] values = new object?[testCase.Arguments.Count];
        ParameterInfo[] parameters = testCase.Method.GetParameters();
        for (int index = 0; index < values.Length; index++)
        {
            values[index] = Coerce(testCase.Arguments[index], parameters[index].ParameterType);
        }

        return values;
    }

    private static object? Coerce(object? value, Type target)
    {
        if (value is null || target.IsInstanceOfType(value))
        {
            return value;
        }

        Type wanted = Nullable.GetUnderlyingType(target) ?? target;
        return wanted.IsEnum
            ? Enum.ToObject(wanted, value)
            : Convert.ChangeType(value, wanted, CultureInfo.InvariantCulture);
    }

    private static string Explain(Exception failure)
    {
        if (failure is AssertionException)
        {
            return failure.Message;
        }

        return failure.GetType().Name + ": " + failure.Message + Environment.NewLine + failure.StackTrace;
    }
}
