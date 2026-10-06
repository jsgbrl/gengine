using System;
using GEngine.Testing;

namespace GEngine.Architecture.Tests;

/// <summary>
/// The rules from docs/style.md that no analyzer enforces. Two of them - `this.` and the
/// opening summary - were meant to be analyzer rules: IDE0003 and IDE0009 turned out not to
/// report during a build in .NET 10, which is why they are here. The rest were never anybody's
/// rules but ours.
/// </summary>
public sealed partial class LayoutRulesTests
{
    private const int MostLinesInAnOpeningStatement = 3;

    [Test]
    public void NothingQualifiesItselfWithThis()
    {
        Find("`this.` is never written; a field that needs it is a field with the wrong name", (file, line, code) =>
            code.Contains("this.", StringComparison.Ordinal) ? "qualified with this." : null);
    }

    [Test]
    public void NoFileUsesRegions()
    {
        Find("no #region; a file that needs folding is a file that needs splitting", (file, line, code) =>
            code.TrimStart().StartsWith("#region", StringComparison.Ordinal) ? "a #region" : null);
    }

    [Test]
    public void NoTernaryIsNestedInsideAnother()
    {
        Find("no nested ternaries; the second `?` is where a reader loses the thread", (file, line, code) =>
            Questions(code) > 1 && code.Contains(':', StringComparison.Ordinal) ? "two ternaries on one line" : null);
    }

    [Test]
    public void NothingIsMarkedTodo()
    {
        Find("no TODO or FIXME; unfinished work belongs in the report, not in the source", (file, line, code) =>
            HasMarker(file, line) ? "a TODO or FIXME" : null);
    }

    // The first thing in a file says what the file is for, in a line or three. A leading `//`
    // comment is one way; the `<summary>` on the file's one type is the other, and for a test
    // class it is the better one, because it is what the documentation file ends up holding.
    // Only the first paragraph counts - everything after the first blank comment line is the
    // why, and the why is allowed to take as long as it takes.
    [Test]
    public void EveryFileOpensBySayingWhatItIsFor()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckOpening(file, violations);
        }

        violations.AssertNone("a file opens with one to three lines saying what it is for");
    }

    // A control character in source compiles and is invisible in an editor, and every tool that
    // reads the file - grep, diff, the pager - decides the file is binary and stops helping. This
    // rule was written because a NUL byte typed into a string literal instead of the escape \0
    // hid two files from a search for an hour.
    [Test]
    public void EveryFileIsTextAToolCanRead()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckPrintable(file, violations);
        }

        violations.AssertNone("source is printable text: control characters are written as escapes");
    }

    [Test]
    public void EveryFileHoldsAtMostOneVisibleType()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckOneTypePerFile(file, violations);
        }

        violations.AssertNone("one visible type per file, named after the file");
    }

    [Test]
    public void TheLayoutRulesGoRedWhenTheyShould()
    {
        var file = new SourceFile("tests/fake.cs", "class Fake\n{\n    int _x;\n    void A() => this._x = 1;\n}\n");
        Assert.IsTrue(HasThis(file), "the this. rule cannot see a this.");

        var documented = new SourceFile("tests/fake.cs", "// this. is what this file is about\nclass Fake\n{\n}\n");
        Assert.IsFalse(HasThis(documented), "the rule accused a comment about the rule");
    }

    private static bool HasThis(SourceFile file)
    {
        foreach (string code in file.CodeLines)
        {
            if (code.Contains("this.", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static void Find(string rule, Func<SourceFile, int, string, string?> check)
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            Scan(file, check, violations);
        }

        violations.AssertNone(rule);
    }

    private static void Scan(SourceFile file, Func<SourceFile, int, string, string?> check, Violations violations)
    {
        for (int index = 0; index < file.CodeLines.Count; index++)
        {
            string? detail = check(file, index + 1, file.CodeLines[index]);
            if (detail is not null)
            {
                violations.Add(file, index + 1, detail);
            }
        }
    }

    // `string? name` and `Foo?.Bar` are not ternaries. A ternary's question mark has a space on
    // both sides; a nullable annotation is written against its type, with nothing before it.
    private static int Questions(string code)
    {
        int found = 0;
        for (int index = 1; index + 1 < code.Length; index++)
        {
            found += code[index] == '?' && code[index - 1] == ' ' && code[index + 1] == ' ' ? 1 : 0;
        }

        return found;
    }

    // Only where a comment begins with one. A rule about markers has to be able to name them
    // in its own message without accusing itself.
    private static bool HasMarker(SourceFile file, int line)
    {
        string text = file.Lines[line - 1];
        foreach (string opener in new[] { "//", "/*", "*" })
        {
            if (Marked(text, opener))
            {
                return true;
            }
        }

        return false;
    }

    private static bool Marked(string text, string opener)
    {
        int start = text.IndexOf(opener, StringComparison.Ordinal);
        if (start < 0)
        {
            return false;
        }

        string rest = text[(start + opener.Length)..].TrimStart();
        return rest.StartsWith("TODO", StringComparison.Ordinal) || rest.StartsWith("FIXME", StringComparison.Ordinal);
    }
}
