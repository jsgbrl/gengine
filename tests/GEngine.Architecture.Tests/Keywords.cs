// The words C# reserves, which nobody chose and no rule should judge.

using System;
using System.Collections.Generic;

namespace GEngine.Architecture.Tests;

internal static class Keywords
{
    private static readonly HashSet<string> Reserved = new(StringComparer.Ordinal)
    {
        "if", "do", "in", "is", "as", "or", "for", "new", "out", "ref", "int", "var", "get",
        "set", "try", "not", "and", "byte", "bool", "char", "long", "uint", "void", "case",
        "else", "enum", "goto", "lock", "null", "this", "base", "true", "when", "with",
        "init", "add", "record", "struct", "class", "where", "value",
    };

    /// <summary>True if the word is one C# reserves rather than one somebody chose.</summary>
    /// <param name="word">One camel-case word of an identifier, not the whole identifier.</param>
    /// <returns>Whether it is reserved.</returns>
    public static bool Contains(string word) => Reserved.Contains(word);
}
