// The glossary, as data.
//
// docs/glossary.md is the source of truth for the repository's vocabulary, and this reads it
// rather than restating it: a word that is added to the table is checked from that moment on,
// and a rule that disagrees with the documentation is a rule that fails.

using System;
using System.Collections.Generic;

namespace GEngine.Architecture.Tests;

internal static class Vocabulary
{
    /// <summary>The abbreviations the glossary allows. Everything else is spelled out.</summary>
    public static IReadOnlyList<string> AllowedAbbreviations { get; } = ["id", "aabb", "hid", "rgb", "fps", "dt"];

    /// <summary>
    /// The short forms this repository refuses. A list rather than a length: `x` and `y` are
    /// the names of the axes, `R`, `G` and `B` are the names of the channels, and a rule that
    /// banned every two-letter word would ban all of them and every number besides.
    /// </summary>
    public static IReadOnlyList<string> Abbreviations { get; } =
    [
        "idx", "col", "prev", "nxt", "tmp", "cfg", "btn", "pos", "vel", "msg", "err", "buf",
        "ctx", "mgr", "impl", "ptr", "len", "val", "obj", "num", "cnt", "elem", "attr",
        "param", "coord", "func", "proc", "util", "misc", "calc", "spec", "dict", "arr",
        "str", "req", "res", "dest", "src", "cur", "curr", "iter", "amt", "qty", "rect",
        "img", "ctrl", "evt", "arg", "ret", "fn", "cb", "sz",
    ];

    /// <summary>Identifiers that come from the base class library and are not ours to name.</summary>
    public static IReadOnlySet<string> Borrowed { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        "IntPtr", "UIntPtr", "Func", "Action", "PtrToStringUni", "obj", "ElapsedMilliseconds", "GetTempPath",
    };

    /// <summary>Every word in the glossary's table, in the order it appears.</summary>
    public static IReadOnlyList<string> Words { get; } = Read(column: 1);

    /// <summary>Every synonym the glossary forbids, across all words.</summary>
    public static IReadOnlySet<string> Synonyms { get; } = Flattened(Read(column: 3));

    /// <summary>True if the glossary's table forbids a term, however it is spaced or cased.</summary>
    /// <param name="term">A word or a phrase, in any case and with any spacing.</param>
    /// <returns>Whether the table forbids it.</returns>
    public static bool Forbids(string term) => Synonyms.Contains(Flatten(term));

    /// <summary>Upper case with the spaces taken out, which is how a synonym is compared.</summary>
    /// <param name="term">A word or a phrase, in any case and with any spacing.</param>
    /// <returns>The flattened form.</returns>
    public static string Flatten(string term) =>
        term.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

    private static List<string> Read(int column)
    {
        List<string> found = [];
        foreach (string line in Repository.ReadRootFile("docs/glossary.md").Split('\n'))
        {
            ReadRow(found, line, column);
        }

        return found;
    }

    private static HashSet<string> Flattened(List<string> terms)
    {
        HashSet<string> flattened = new(StringComparer.Ordinal);
        foreach (string term in terms)
        {
            flattened.Add(Flatten(term));
        }

        return flattened;
    }

    private static void ReadRow(List<string> found, string line, int column)
    {
        string[] cells = line.Split('|');
        if (cells.Length < 5 || cells[1].Trim().StartsWith("---", StringComparison.Ordinal))
        {
            return;
        }

        foreach (string term in cells[column].Split(','))
        {
            Add(found, term);
        }
    }

    private static void Add(List<string> found, string term)
    {
        string word = term.Replace("*", string.Empty, StringComparison.Ordinal).Trim();
        if (word.Length > 0 && !word.StartsWith("Word", StringComparison.Ordinal) && word != "Never called")
        {
            found.Add(word);
        }
    }
}
