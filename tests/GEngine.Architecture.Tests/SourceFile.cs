// One C# file, in the two forms the rules need: as it was written, and with the prose blanked.
//
// Both have the same number of lines and the same columns, so a rule can find something in the
// code and report the line the reader will actually see.

using System;
using System.Collections.Generic;

namespace GEngine.Architecture.Tests;

internal sealed class SourceFile
{
    private static readonly string[] LineBreaks = ["\r\n", "\n"];

    public SourceFile(string path, string text)
    {
        Path = path;
        Text = text;
        Lines = text.Split(LineBreaks, StringSplitOptions.None);
        CodeLines = CodeScanner.StripProse(text).Split(LineBreaks, StringSplitOptions.None);
    }

    /// <summary>Its path relative to the repository root, with forward slashes.</summary>
    public string Path { get; }

    /// <summary>Its contents, as written.</summary>
    public string Text { get; }

    /// <summary>Its lines, as written.</summary>
    public IReadOnlyList<string> Lines { get; }

    /// <summary>Its lines with comments and literals blanked out.</summary>
    public IReadOnlyList<string> CodeLines { get; }

    /// <summary>True if the file is under src/, and so is shipped rather than a test.</summary>
    public bool IsProduction => Path.StartsWith("src/", StringComparison.Ordinal);

    /// <summary>True if the file is a platform's interop, where the rules are deliberately different.</summary>
    public bool IsInterop => Path.Contains("/Interop/", StringComparison.Ordinal);

    /// <summary>The name a test failure prints for a given line.</summary>
    /// <param name="line">A one-based line number.</param>
    /// <returns>Path and line, in the form an editor can jump to.</returns>
    public string At(int line) => Path + "(" + line + ")";
}
