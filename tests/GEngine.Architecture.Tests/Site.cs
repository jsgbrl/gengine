// A place in the repository: one file, one line.
//
// It exists because the parameter budget is four, and a rule that reports somewhere naturally
// wants to pass a file, a line, the thing it found, what it was looking for and where to put
// the answer. Two of those are one idea.

namespace GEngine.Architecture.Tests;

internal readonly struct Site
{
    public Site(SourceFile file, int line)
    {
        File = file;
        Line = line;
    }

    /// <summary>The file.</summary>
    public SourceFile File { get; }

    /// <summary>The one-based line number.</summary>
    public int Line { get; }

    /// <summary>Records a violation here.</summary>
    /// <param name="violations">Where to record it.</param>
    /// <param name="detail">What is wrong.</param>
    public void Add(Violations violations, string detail) => violations.Add(File, Line, detail);
}
