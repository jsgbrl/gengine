// Printing. Colour comes from System.Console, not from ANSI escapes: the BCL already
// knows how to colour a terminal on all three systems, and the test runner must work
// before the rendering module that enables virtual terminal sequences even exists.

using System;
using System.Collections.Generic;
using System.Globalization;

namespace GEngine.Testing;

/// <summary>Writes a run to the console.</summary>
internal static class TestReport
{
    /// <summary>Prints every result the options ask for, then the totals.</summary>
    /// <param name="summary">The finished run.</param>
    /// <param name="options">How loudly to report.</param>
    public static void Print(TestSummary summary, RunOptions options)
    {
        foreach (TestResult result in summary.Results)
        {
            PrintResult(result, options.Verbose);
        }

        PrintTotals(summary);
    }

    /// <summary>Prints the names of the cases a run would execute.</summary>
    /// <param name="names">Case names, in report order.</param>
    public static void PrintNames(IReadOnlyList<string> names)
    {
        foreach (string name in names)
        {
            Console.WriteLine(name);
        }

        Console.WriteLine(Count(names.Count, "cases"));
    }

    private static void PrintResult(TestResult result, bool verbose)
    {
        if (result.Outcome == TestOutcome.Passed && !verbose)
        {
            return;
        }

        Write(Label(result.Outcome), Colour(result.Outcome));
        Console.WriteLine("  " + result.Name + Timing(result));
        if (result.Message.Length > 0)
        {
            Console.WriteLine(Indent(result.Message));
        }
    }

    private static void PrintTotals(TestSummary summary)
    {
        Console.WriteLine();
        Write(Count(summary.PassedCount, "passed"), ConsoleColor.Green);
        Console.Write(", ");
        Write(Count(summary.FailedCount, "failed"), summary.FailedCount == 0 ? ConsoleColor.Gray : ConsoleColor.Red);
        Console.Write(", ");
        Write(Count(summary.SkippedCount, "skipped"), summary.SkippedCount == 0 ? ConsoleColor.Gray : ConsoleColor.Yellow);
        Console.WriteLine(string.Format(
            CultureInfo.InvariantCulture,
            " in {0:0} ms",
            summary.ElapsedMilliseconds));
    }

    private static void Write(string text, ConsoleColor colour)
    {
        ConsoleColor previous = Console.ForegroundColor;
        Console.ForegroundColor = colour;
        Console.Write(text);
        Console.ForegroundColor = previous;
    }

    private static string Label(TestOutcome outcome)
    {
        return outcome switch
        {
            TestOutcome.Passed => "PASS",
            TestOutcome.Failed => "FAIL",
            _ => "SKIP",
        };
    }

    private static ConsoleColor Colour(TestOutcome outcome)
    {
        return outcome switch
        {
            TestOutcome.Passed => ConsoleColor.Green,
            TestOutcome.Failed => ConsoleColor.Red,
            _ => ConsoleColor.Yellow,
        };
    }

    private static string Timing(TestResult result)
    {
        if (result.Outcome == TestOutcome.Skipped)
        {
            return string.Empty;
        }

        return string.Format(CultureInfo.InvariantCulture, "  ({0:0.00} ms)", result.ElapsedMilliseconds);
    }

    private static string Indent(string message)
    {
        return "        " + message.Replace(Environment.NewLine, Environment.NewLine + "        ", StringComparison.Ordinal);
    }

    private static string Count(int value, string noun)
    {
        return string.Format(CultureInfo.InvariantCulture, "{0} {1}", value, noun);
    }
}
