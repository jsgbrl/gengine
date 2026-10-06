// Does the linter actually go red?
//
// A rule that reads a file it cannot parse reports nothing and passes, which looks exactly
// like a clean repository. So each rule is also run against a file written to break it: if the
// scanner ever stops finding methods, these fail while the real rules go on passing.

using System.Collections.Generic;
using GEngine.Testing;

namespace GEngine.Architecture.Tests;

/// <content>The linter, pointed at itself.</content>
public sealed partial class StyleRulesTests
{
    [Test]
    public void TheLengthRuleFindsALongMethod()
    {
        var violations = new Violations();
        CheckMethods(Fake(Method("Long", MostLinesInAMethod + 5)), violations);
        Assert.Throws<AssertionException>(() => violations.AssertNone("length"), "a 25 line method went unnoticed");
    }

    [Test]
    public void TheLengthRuleLeavesAShortMethodAlone()
    {
        var violations = new Violations();
        CheckMethods(Fake(Method("Short", 3)), violations);
        violations.AssertNone("length");
    }

    [Test]
    public void TheNestingRuleFindsADeepNest()
    {
        var violations = new Violations();
        CheckNesting(Fake(Nested(MostNesting + 1)), violations);
        Assert.Throws<AssertionException>(() => violations.AssertNone("nesting"), "four levels went unnoticed");

        var shallow = new Violations();
        CheckNesting(Fake(Nested(MostNesting)), shallow);
        shallow.AssertNone("nesting");
    }

    [Test]
    public void TheParameterRuleCountsPastGenericsAndDefaults()
    {
        Assert.AreEqual(0, ParametersIn("void Nothing()"));
        Assert.AreEqual(1, ParametersIn("void One(Dictionary<string, int> map)"), "a generic comma is not a parameter");
        Assert.AreEqual(2, ParametersIn("void Two(int a, string b = \"x\")"));
        Assert.AreEqual(4, ParametersIn("void Four(int a, int b, int c, int d)"));
    }

    [Test]
    public void TheParameterRuleFindsAWideSignature()
    {
        var violations = new Violations();
        CheckParameters(Fake("public sealed class Wide\n{\n    public void Take(int a, int b, int c, int d, int e)\n    {\n    }\n}\n"), violations);
        Assert.Throws<AssertionException>(() => violations.AssertNone("parameters"), "five parameters went unnoticed");
    }

    // The scanner has to see through prose, or every rule accuses the documentation that
    // describes it. These are the four shapes of prose C# has.
    [Test]
    public void TheScannerSeesThroughEveryKindOfProse()
    {
        Assert.AreEqual(new string(' ', 7), Stripped("// {{{{"), "a line comment");
        Assert.AreEqual(new string(' ', 10), Stripped("/* {{{{ */"), "a block comment");
        Assert.AreEqual("x=    ;", Stripped("x=\"{{\";"), "a string");
        Assert.AreEqual("x=   ;", Stripped("x='{';"), "a character");
    }

    [Test]
    public void TheScannerKeepsEveryLineAndColumn()
    {
        const string Source = "int a; // one\nint b; /* two\nthree */ int c;\n";
        string stripped = CodeScanner.StripProse(Source);
        Assert.AreEqual(Source.Length, stripped.Length, "the same length, so columns still line up");
        Assert.AreEqual(3, stripped.Split('\n').Length - 1, "and the same number of lines");
    }

    private static string Stripped(string code) => CodeScanner.StripProse(code);

    private static SourceFile Fake(string body) => new("tests/fake.cs", body);

    private static string Method(string name, int lines)
    {
        List<string> text = ["public sealed class Fake", "{", "    private void " + name + "()", "    {"];
        for (int line = 0; line < lines; line++)
        {
            text.Add("        Console.WriteLine(" + line + ");");
        }

        text.Add("    }");
        text.Add("}");
        return string.Join("\n", text);
    }

    private static string Nested(int depth)
    {
        List<string> text = ["public sealed class Fake", "{", "    private void Deep()", "    {"];
        for (int level = 0; level < depth; level++)
        {
            text.Add(new string(' ', 8 + (level * 4)) + "if (true)");
            text.Add(new string(' ', 8 + (level * 4)) + "{");
        }

        text.Add(new string(' ', 8 + (depth * 4)) + "Console.WriteLine(1);");
        return string.Join("\n", [.. text, .. Closing(depth)]);
    }

    private static IEnumerable<string> Closing(int depth)
    {
        for (int level = depth; level > 0; level--)
        {
            yield return new string(' ', 4 + (level * 4)) + "}";
        }

        yield return "    }";
        yield return "}";
    }
}
