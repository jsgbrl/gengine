// Command line of the test runner, parsed once into a value the rest of the code reads.
// Unknown arguments are a hard error: a typo in --filter must not silently run nothing.

using System;
using System.Collections.Generic;

namespace GEngine.Testing;

/// <summary>How a run was asked for: which cases, and how loudly to report them.</summary>
public sealed class RunOptions
{
    /// <summary>Case-insensitive substring a case name must contain. Empty runs everything.</summary>
    public string Filter { get; init; } = string.Empty;

    /// <summary>When set, names the matching cases instead of running them.</summary>
    public bool ListOnly { get; init; }

    /// <summary>When set, prints one line per case instead of only failures and skips.</summary>
    public bool Verbose { get; init; }

    /// <summary>Parses runner arguments.</summary>
    /// <param name="arguments">Arguments as received by the entry point.</param>
    /// <returns>The parsed options.</returns>
    /// <exception cref="ArgumentException">An argument is not recognised.</exception>
    public static RunOptions Parse(IReadOnlyList<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        string filter = string.Empty;
        bool listOnly = false;
        bool verbose = false;

        for (int index = 0; index < arguments.Count; index++)
        {
            string argument = arguments[index];
            if (argument.StartsWith("--filter=", StringComparison.Ordinal))
            {
                filter = argument["--filter=".Length..];
                continue;
            }

            listOnly |= argument is "--list";
            verbose |= argument is "--verbose";
            Reject(argument);
        }

        return new RunOptions { Filter = filter, ListOnly = listOnly, Verbose = verbose };
    }

    /// <summary>The usage text printed when parsing fails.</summary>
    /// <returns>One line per accepted argument.</returns>
    public static string Usage()
    {
        return string.Join(
            Environment.NewLine,
            "usage: dotnet run tests.cs [--filter=TEXT] [--list] [--verbose]",
            "  --filter=TEXT  run only cases whose name contains TEXT (case-insensitive)",
            "  --list         print matching case names and exit",
            "  --verbose      print a line for every case, not just failures and skips");
    }

    private static void Reject(string argument)
    {
        if (argument is "--list" or "--verbose" || argument.StartsWith("--filter=", StringComparison.Ordinal))
        {
            return;
        }

        throw new ArgumentException("unknown argument: " + argument, nameof(argument));
    }
}
