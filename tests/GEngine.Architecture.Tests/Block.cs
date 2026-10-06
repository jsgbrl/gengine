// One braced block of code: what introduced it, where it starts, and where it ends.

using System;
using System.Collections.Generic;

namespace GEngine.Architecture.Tests;

internal sealed class Block
{
    private static readonly string[] TypeWords = ["class", "struct", "record", "interface", "enum"];

    private static readonly string[] ControlWords =
    [
        "if", "else", "for", "foreach", "while", "do", "switch", "case", "try", "catch",
        "finally", "lock", "using", "fixed", "checked", "unchecked", "return", "new",
    ];

    private static readonly string[] MemberWords =
    [
        "public", "private", "protected", "internal", "static", "override", "virtual",
        "sealed", "abstract", "partial", "async", "extern", "unsafe",
    ];

    public Block(string header, int firstLine, int lastLine, int depth)
    {
        Header = header.Trim();
        FirstLine = firstLine;
        LastLine = lastLine;
        Depth = depth;
    }

    /// <summary>The code between the previous statement and this block's opening brace.</summary>
    public string Header { get; }

    /// <summary>The one-based line the header starts on.</summary>
    public int FirstLine { get; }

    /// <summary>The one-based line the closing brace is on.</summary>
    public int LastLine { get; }

    /// <summary>How many braces are open around this block.</summary>
    public int Depth { get; }

    /// <summary>How many lines the block spans, header and closing brace included.</summary>
    public int LineCount => (LastLine - FirstLine) + 1;

    /// <summary>True if this block is a type declaration.</summary>
    public bool IsType => StartsWithWord(TypeWords) || HasWord(TypeWords);

    /// <summary>True if this block is a method, a constructor or an accessor with a body.</summary>
    public bool IsMethod => !IsType && Header.Contains('(', StringComparison.Ordinal) && !StartsWithWord(ControlWords);

    /// <summary>True if the header declares a member, which is how a signature is told from a lambda.</summary>
    public bool IsMember => StartsWithWord(MemberWords);

    /// <summary>The declaration with its attributes removed, which is what a signature is.</summary>
    public string Signature
    {
        get
        {
            List<string> kept = [];
            foreach (string line in Header.Split('\n'))
            {
                Keep(kept, line);
            }

            return string.Join(" ", kept).Trim();
        }
    }

    /// <summary>The name of the thing declared, for a failure message.</summary>
    public string Name
    {
        get
        {
            string head = Signature.Split(" where ")[0].Split('(', 2)[0].Split(':', 2)[0].TrimEnd();
            int space = head.LastIndexOf(' ');
            string name = space < 0 ? head : head[(space + 1)..];
            return name.Split('<', 2)[0];
        }
    }

    // An attribute is not part of a signature. Counting `[TestCase(1, 2, 3)]` as parameters
    // said a two-parameter test took sixteen.
    private static void Keep(List<string> kept, string line)
    {
        string trimmed = line.Trim();
        if (trimmed.Length > 0 && !trimmed.StartsWith('['))
        {
            kept.Add(trimmed);
        }
    }

    private bool StartsWithWord(string[] words)
    {
        foreach (string line in Header.Split('\n'))
        {
            string trimmed = line.TrimStart();
            if (trimmed.Length > 0 && !trimmed.StartsWith('[') && Matches(trimmed, words))
            {
                return true;
            }
        }

        return false;
    }

    private bool HasWord(string[] words)
    {
        foreach (string word in words)
        {
            if (Header.Contains(' ' + word + ' ', StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool Matches(string text, string[] words)
    {
        foreach (string word in words)
        {
            if (text.StartsWith(word, StringComparison.Ordinal)
                && (text.Length == word.Length || !char.IsLetterOrDigit(text[word.Length])))
            {
                return true;
            }
        }

        return false;
    }
}
