using System.Collections.Generic;
using GEngine.Testing;

namespace GEngine.Architecture.Tests;

/// <summary>
/// The budgets from docs/style.md, enforced by reading the repository. They are not arbitrary:
/// a file you cannot scroll, a method you cannot hold in your head and a nest you cannot follow
/// are the three ways a codebase stops being teachable, and this is a course.
/// </summary>
public sealed partial class StyleRulesTests
{
    private const int MostLinesInAFile = 250;
    private const int MostLinesInAType = 150;
    private const int MostLinesInAMethod = 20;
    private const int MostNesting = 3;
    private const int MostParameters = 4;

    // A NativeMethods class is a transcription of somebody else's header file. Splitting it
    // would mean splitting one platform's ABI across files, which helps nobody.
    private static bool IsExemptFromLength(SourceFile file) =>
        file.Path.EndsWith("NativeMethods.cs", System.StringComparison.Ordinal);

    [Test]
    public void NoFileIsLongerThanTwoHundredAndFiftyLines()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            int lines = CountedLines(file);
            if (!IsExemptFromLength(file) && lines > MostLinesInAFile)
            {
                violations.Add(file, lines, "the file is " + lines + " lines, over the budget of " + MostLinesInAFile);
            }
        }

        violations.AssertNone("a file is at most " + MostLinesInAFile + " lines");
    }

    [Test]
    public void NoTypeIsLongerThanOneHundredAndFiftyLines()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckTypes(file, violations);
        }

        violations.AssertNone("a type is at most " + MostLinesInAType + " lines in one file");
    }

    [Test]
    public void NoMethodIsLongerThanTwentyLines()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckMethods(file, violations);
        }

        violations.AssertNone("a method is at most " + MostLinesInAMethod + " lines");
    }

    [Test]
    public void NothingIsNestedMoreThanThreeDeep()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckNesting(file, violations);
        }

        violations.AssertNone("a method nests at most " + MostNesting + " deep");
    }

    [Test]
    public void NoMethodTakesMoreThanFourParameters()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckParameters(file, violations);
        }

        violations.AssertNone("a method takes at most " + MostParameters + " parameters");
    }

    private static void CheckTypes(SourceFile file, Violations violations)
    {
        foreach (Block type in Declarations.Types(file))
        {
            int lines = CodeLinesIn(file, type);
            if (!IsExemptFromLength(file) && lines > MostLinesInAType)
            {
                violations.Add(file, type.FirstLine, type.Name + " is " + lines + " lines of code");
            }
        }
    }

    private static void CheckMethods(SourceFile file, Violations violations)
    {
        foreach (Block method in Declarations.Methods(file))
        {
            int lines = CodeLinesIn(file, method);
            if (!IsExemptFromLength(file) && lines > MostLinesInAMethod)
            {
                violations.Add(file, method.FirstLine, method.Name + " is " + lines + " lines of code");
            }
        }
    }

    private static void CheckNesting(SourceFile file, Violations violations)
    {
        List<Block> blocks = Declarations.Blocks(file);
        foreach (Block block in blocks)
        {
            int nesting = NestingOf(block, blocks);
            if (nesting > MostNesting)
            {
                violations.Add(file, block.FirstLine, "nested " + nesting + " deep");
            }
        }
    }

    // How many blocks stand between this one and the method it lives in. A method itself is
    // zero; the body of an `if` inside it is one.
    private static int NestingOf(Block block, List<Block> blocks)
    {
        Block? method = EnclosingMethod(block, blocks);
        return method is null || block.IsType || block.IsMethod ? 0 : block.Depth - method.Depth;
    }

    private static Block? EnclosingMethod(Block block, List<Block> blocks)
    {
        Block? found = null;
        foreach (Block candidate in blocks)
        {
            if (candidate.IsMethod && candidate.Depth < block.Depth && Surrounds(candidate, block))
            {
                found = found is null || candidate.Depth > found.Depth ? candidate : found;
            }
        }

        return found;
    }

    private static bool Surrounds(Block outer, Block inner) =>
        outer.FirstLine <= inner.FirstLine && outer.LastLine >= inner.LastLine;

    // A type and a method are measured in lines of code: blank lines and the XML documentation
    // that GenerateDocumentationFile makes compulsory are not what makes a type hard to hold in
    // your head, and counting them would mean the budget punished documenting. A file is
    // measured in real lines, because a file budget is about how far you have to scroll.
    private static int CodeLinesIn(SourceFile file, Block block)
    {
        int lines = 0;
        for (int line = block.FirstLine; line <= block.LastLine && line <= file.CodeLines.Count; line++)
        {
            lines += file.CodeLines[line - 1].Trim().Length > 0 ? 1 : 0;
        }

        return lines;
    }

    // A trailing newline splits into one empty line that nobody wrote.
    private static int CountedLines(SourceFile file)
    {
        int lines = file.Lines.Count;
        return lines > 0 && file.Lines[lines - 1].Length == 0 ? lines - 1 : lines;
    }
}
