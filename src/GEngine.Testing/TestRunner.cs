// The entry point of the framework. tests.cs hands it the argument list and the test
// assemblies; everything else - discovery, execution, printing, exit code - happens here.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace GEngine.Testing;

/// <summary>Runs the tests found in a set of assemblies and reports them.</summary>
public static class TestRunner
{
    /// <summary>Parses the arguments, runs the matching cases and prints the report.</summary>
    /// <param name="arguments">Arguments as received by the entry point.</param>
    /// <param name="assemblies">Assemblies to scan for test classes.</param>
    /// <returns>Zero when nothing failed, one when something did, two on a bad argument.</returns>
    public static int Run(IReadOnlyList<string> arguments, IReadOnlyList<Assembly> assemblies)
    {
        RunOptions options;
        try
        {
            options = RunOptions.Parse(arguments);
        }
        catch (ArgumentException badArgument)
        {
            Console.WriteLine(badArgument.Message);
            Console.WriteLine(RunOptions.Usage());
            return 2;
        }

        IReadOnlyList<Type> types = TypesOf(assemblies);
        if (options.ListOnly)
        {
            TestReport.PrintNames(List(options, types));
            return 0;
        }

        TestSummary summary = Execute(options, types);
        TestReport.Print(summary, options);
        return summary.ExitCode;
    }

    /// <summary>Runs the matching cases without printing anything.</summary>
    /// <param name="options">Which cases to run.</param>
    /// <param name="types">Candidate types to scan.</param>
    /// <returns>The finished run.</returns>
    public static TestSummary Execute(RunOptions options, IReadOnlyList<Type> types)
    {
        ArgumentNullException.ThrowIfNull(options);
        List<TestResult> results = [];
        var stopwatch = Stopwatch.StartNew();
        foreach (TestCaseInfo testCase in TestDiscovery.Discover(types))
        {
            if (Matches(testCase.Name, options.Filter))
            {
                results.Add(TestExecutor.Execute(testCase));
            }
        }

        stopwatch.Stop();
        return new TestSummary(results, stopwatch.Elapsed.TotalMilliseconds);
    }

    /// <summary>Names the cases a run would execute, without running them.</summary>
    /// <param name="options">Which cases to consider.</param>
    /// <param name="types">Candidate types to scan.</param>
    /// <returns>Matching case names, in report order.</returns>
    public static IReadOnlyList<string> List(RunOptions options, IReadOnlyList<Type> types)
    {
        ArgumentNullException.ThrowIfNull(options);
        List<string> names = [];
        foreach (TestCaseInfo testCase in TestDiscovery.Discover(types))
        {
            if (Matches(testCase.Name, options.Filter))
            {
                names.Add(testCase.Name);
            }
        }

        return names;
    }

    private static List<Type> TypesOf(IReadOnlyList<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        List<Type> types = [];
        foreach (Assembly assembly in assemblies)
        {
            types.AddRange(assembly.GetExportedTypes());
        }

        return types;
    }

    private static bool Matches(string name, string filter)
    {
        return filter.Length == 0 || name.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }
}
