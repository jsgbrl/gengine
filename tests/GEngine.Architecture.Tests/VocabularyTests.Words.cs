// Splitting an identifier into the words it is made of, which is the whole difficulty of a
// vocabulary rule.
//
// `Position` is not an abbreviation even though `pos` is inside it, and `IntPtr` is a name the
// base class library chose, not one this repository did. Both mistakes are avoided the same
// way: compare whole words, and skip the names that are not ours.

using System;
using System.Collections.Generic;

namespace GEngine.Architecture.Tests;

/// <content>Reading identifiers word by word.</content>
public sealed partial class VocabularyTests
{
    private static void CheckSynonyms(SourceFile file, Violations violations)
    {
        foreach (KeyValuePair<string, string[]> project in BannedIn)
        {
            if (file.Path.Contains("/" + project.Key + "/", StringComparison.Ordinal))
            {
                Scan(file, project.Value, violations);
            }
        }
    }

    // Both halves of the same question. A one-word synonym is a word of an identifier, so
    // `Texture` is found inside `TextureAtlas`; a two-word one is the identifier itself, since
    // `game object` is only ever written `GameObject`.
    private static void Scan(SourceFile file, string[] banned, Violations violations)
    {
        for (int line = 1; line <= file.CodeLines.Count; line++)
        {
            string code = file.CodeLines[line - 1];
            var site = new Site(file, line);
            Report(site, WordsIn(code), banned, violations);
            Report(site, IdentifiersIn(code), banned, violations);
        }
    }

    private static void Report(Site site, List<string> names, string[] banned, Violations violations)
    {
        foreach (string name in names)
        {
            Match(site, name, banned, violations);
        }
    }

    private static void Match(Site site, string name, string[] banned, Violations violations)
    {
        foreach (string synonym in banned)
        {
            if (Vocabulary.Flatten(name) == Vocabulary.Flatten(synonym))
            {
                site.Add(violations, "'" + name + "' is a synonym; see docs/glossary.md");
            }
        }
    }

    private static void CheckAbbreviations(SourceFile file, Violations violations)
    {
        for (int line = 1; line <= file.CodeLines.Count; line++)
        {
            foreach (string word in WordsIn(file.CodeLines[line - 1]))
            {
                ReportShort(file, line, word, violations);
            }
        }
    }

    private static void ReportShort(SourceFile file, int line, string word, Violations violations)
    {
        foreach (string abbreviation in Vocabulary.Abbreviations)
        {
            if (string.Equals(word, abbreviation, StringComparison.OrdinalIgnoreCase))
            {
                violations.Add(file, line, "'" + word + "' is short for something; spell it out");
            }
        }
    }

    // Every camel-case word of every identifier on the line, minus the ones the base class
    // library named and the ones C# reserves.
    private static List<string> WordsIn(string code)
    {
        List<string> words = [];
        foreach (string identifier in IdentifiersIn(code))
        {
            Split(words, identifier);
        }

        return words;
    }

    private static void Split(List<string> words, string identifier)
    {
        if (Vocabulary.Borrowed.Contains(identifier) || Keywords.Contains(identifier))
        {
            return;
        }

        int start = 0;
        for (int index = 1; index <= identifier.Length; index++)
        {
            start = Cut(words, identifier, start, index);
        }
    }

    // A word ends where the next capital begins, so `DrawPixels` is two words and `Aabb` is one.
    private static int Cut(List<string> words, string identifier, int start, int index)
    {
        bool boundary = index == identifier.Length || char.IsUpper(identifier[index]);
        if (!boundary || index == start)
        {
            return start;
        }

        words.Add(identifier[start..index]);
        return index;
    }

    private static List<string> IdentifiersIn(string code)
    {
        List<string> identifiers = [];
        int start = -1;
        for (int index = 0; index <= code.Length; index++)
        {
            start = Take(identifiers, code, start, index);
        }

        return identifiers;
    }

    private static int Take(List<string> identifiers, string code, int start, int index)
    {
        bool part = index < code.Length && (char.IsLetterOrDigit(code[index]) || code[index] == '_');
        if (part)
        {
            return start < 0 ? index : start;
        }

        if (start >= 0)
        {
            identifiers.Add(code[start..index].TrimStart('_'));
        }

        return -1;
    }
}
