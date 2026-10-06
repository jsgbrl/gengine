// How the two rules about a file as a whole are actually measured: what counts as an opening
// statement, and what counts as the file's one type.

using System;
using System.Collections.Generic;

namespace GEngine.Architecture.Tests;

/// <content>Reading a file's opening and its type declarations.</content>
public sealed partial class LayoutRulesTests
{
    private static void CheckOpening(SourceFile file, Violations violations)
    {
        string first = FirstRealLine(file, out int line);
        if (!first.StartsWith("//", StringComparison.Ordinal))
        {
            CheckSummary(file, violations);
            return;
        }

        int paragraph = ParagraphLength(file, line);
        if (paragraph > MostLinesInAnOpeningStatement)
        {
            violations.Add(file, line, "the opening statement is " + paragraph + " lines before its first break");
        }
    }

    private static void CheckSummary(SourceFile file, Violations violations)
    {
        if (!file.Text.Contains("/// <summary>", StringComparison.Ordinal))
        {
            violations.Add(file, 1, "nothing says what this file is for");
        }
    }

    private static string FirstRealLine(SourceFile file, out int line)
    {
        for (line = 1; line <= file.Lines.Count; line++)
        {
            string text = file.Lines[line - 1].Trim();
            if (text.Length > 0)
            {
                return text;
            }
        }

        return string.Empty;
    }

    private static int ParagraphLength(SourceFile file, int from)
    {
        for (int line = from; line <= file.Lines.Count; line++)
        {
            string text = file.Lines[line - 1].Trim();
            if (!text.StartsWith("//", StringComparison.Ordinal) || text.TrimEnd('/').Length == 0)
            {
                return line - from;
            }
        }

        return file.Lines.Count - from;
    }

    // Public types, not every type: a nested helper is part of the type it serves, and two
    // internal test doubles in one file are one idea written twice.
    private static void CheckOneTypePerFile(SourceFile file, Violations violations)
    {
        List<Block> top = [];
        foreach (Block type in Declarations.Types(file))
        {
            if (type.Depth == 0 && type.Signature.StartsWith("public", StringComparison.Ordinal))
            {
                top.Add(type);
            }
        }

        Named(file, top, violations);
    }

    private static void Named(SourceFile file, List<Block> types, Violations violations)
    {
        if (types.Count > 1)
        {
            violations.Add(file, types[0].FirstLine, types.Count + " public types share the file");
            return;
        }

        string stem = file.Path.Split('/')[^1].Split('.')[0];
        if (types.Count == 1 && types[0].Name != stem)
        {
            violations.Add(file, types[0].FirstLine, types[0].Name + " does not match the file name");
        }
    }

    private static void CheckPrintable(SourceFile file, Violations violations)
    {
        for (int line = 1; line <= file.Lines.Count; line++)
        {
            Printable(file, line, violations);
        }
    }

    private static void Printable(SourceFile file, int line, Violations violations)
    {
        foreach (char character in file.Lines[line - 1])
        {
            if (char.IsControl(character) && character != '	')
            {
                violations.Add(file, line, "a control character, U+" + ((int)character).ToString("X4", System.Globalization.CultureInfo.InvariantCulture));
                return;
            }
        }
    }
}
