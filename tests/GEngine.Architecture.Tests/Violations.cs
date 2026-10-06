// What a linter has to do when it finds something: say where, and say what rule.
//
// It collects everything before failing rather than stopping at the first one, because a rule
// that reports one violation per run turns a tidy-up into twenty runs.

using System.Collections.Generic;
using GEngine.Testing;

namespace GEngine.Architecture.Tests;

internal sealed class Violations
{
    private const int MostToPrint = 60;

    private readonly List<string> _found = [];

    /// <summary>Records a violation at a line of a file.</summary>
    /// <param name="file">Where it is.</param>
    /// <param name="line">The one-based line number.</param>
    /// <param name="detail">What is wrong, in the words a reader would use.</param>
    public void Add(SourceFile file, int line, string detail) => _found.Add(file.At(line) + ": " + detail);

    /// <summary>Records a violation of a file as a whole, or of something that is not a file.</summary>
    /// <param name="where">What it is about.</param>
    /// <param name="detail">What is wrong.</param>
    public void Add(string where, string detail) => _found.Add(where + ": " + detail);

    /// <summary>Fails the test if anything was found, printing the rule and every violation.</summary>
    /// <param name="rule">The rule, as docs/style.md words it.</param>
    public void AssertNone(string rule)
    {
        if (_found.Count == 0)
        {
            return;
        }

        _found.Sort(System.StringComparer.Ordinal);
        Assert.Fail(rule + " - " + _found.Count + " violation(s)\n" + Report());
    }

    private string Report()
    {
        List<string> lines = [];
        for (int index = 0; index < _found.Count && index < MostToPrint; index++)
        {
            lines.Add("        " + _found[index]);
        }

        if (_found.Count > MostToPrint)
        {
            lines.Add("        ... and " + (_found.Count - MostToPrint) + " more");
        }

        return string.Join("\n", lines);
    }
}
