using System;
using System.Collections.Generic;
using GEngine.Testing;

namespace GEngine.Architecture.Tests;

/// <summary>
/// Where a silenced rule is allowed to live. Nowhere in the source: a `#pragma warning disable`
/// hides a rule at the one place a reader is least likely to ask why, and an attribute does the
/// same thing more politely. Every suppression is in .editorconfig, above a comment saying why,
/// and repeated in docs/style.md where somebody might actually read it.
/// </summary>
public sealed class SuppressionRulesTests
{
    [Test]
    public void NothingInTheRepositoryDisablesAWarningInPlace()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckPragmas(file, violations);
        }

        violations.AssertNone("no #pragma warning disable: a suppression belongs in .editorconfig");
    }

    [Test]
    public void NothingSuppressesARuleWithAnAttribute()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckAttributes(file, violations);
        }

        violations.AssertNone("no SuppressMessage attribute either");
    }

    // A silenced rule with no reason above it is a rule somebody found inconvenient. A silenced
    // rule with a reason is a decision, and a decision can be argued with.
    [Test]
    public void EverySilencedRuleSaysWhy()
    {
        var violations = new Violations();
        List<string> lines = [.. Repository.ReadRootFile(".editorconfig").Split('\n')];
        for (int line = 1; line <= lines.Count; line++)
        {
            CheckReason(lines, line, violations);
        }

        violations.AssertNone("every silenced rule has a comment above it saying why");
    }

    [Test]
    public void EverySilencedRuleIsListedInTheStyleDocument()
    {
        string style = Repository.ReadRootFile("docs/style.md");
        var violations = new Violations();
        foreach (string rule in SilencedRules())
        {
            CheckListed(style, rule, violations);
        }

        violations.AssertNone("every silenced rule is in docs/style.md, with the same reason");
    }

    [Test]
    public void TheSuppressionRuleGoesRedWhenItShould()
    {
        var violations = new Violations();
        CheckPragmas(new SourceFile("src/Fake.cs", "#pragma warning disable CA1000\nclass Fake { }\n"), violations);
        Assert.Throws<AssertionException>(() => violations.AssertNone("pragmas"), "a pragma went unnoticed");
    }

    private static void CheckPragmas(SourceFile file, Violations violations)
    {
        for (int line = 1; line <= file.CodeLines.Count; line++)
        {
            string code = file.CodeLines[line - 1].TrimStart();
            if (code.StartsWith("#pragma warning", StringComparison.Ordinal))
            {
                violations.Add(file, line, "a warning silenced in place");
            }
        }
    }

    private static void CheckAttributes(SourceFile file, Violations violations)
    {
        for (int line = 1; line <= file.CodeLines.Count; line++)
        {
            if (file.CodeLines[line - 1].Contains("SuppressMessage", StringComparison.Ordinal))
            {
                violations.Add(file, line, "a rule suppressed by attribute");
            }
        }
    }

    private static void CheckReason(List<string> lines, int line, Violations violations)
    {
        if (!IsSilenced(lines[line - 1]))
        {
            return;
        }

        if (!HasReasonAbove(lines, line))
        {
            violations.Add(".editorconfig(" + line + ")", lines[line - 1].Trim() + " has no reason above it");
        }
    }

    // A reason is a comment somewhere in the same block, where a block is everything between
    // two blank lines. Several rules can share one reason - CA1848 and CA1727 are a single
    // decision written once - and a section header may stand between the reason and the rule.
    private static bool HasReasonAbove(List<string> lines, int line)
    {
        for (int above = line - 1; above >= 1; above--)
        {
            string text = lines[above - 1].Trim();
            if (text.Length == 0)
            {
                return false;
            }

            if (text.StartsWith('#') && !text.StartsWith('['))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSilenced(string line)
    {
        string text = line.Trim();
        return text.StartsWith("dotnet_diagnostic.", StringComparison.Ordinal)
            && text.EndsWith("= none", StringComparison.Ordinal);
    }

    private static List<string> SilencedRules()
    {
        List<string> rules = [];
        foreach (string line in Repository.ReadRootFile(".editorconfig").Split('\n'))
        {
            AddRule(rules, line);
        }

        return rules;
    }

    private static void AddRule(List<string> rules, string line)
    {
        if (!IsSilenced(line))
        {
            return;
        }

        string rule = line.Trim()["dotnet_diagnostic.".Length..].Split('.')[0];
        if (!rules.Contains(rule))
        {
            rules.Add(rule);
        }
    }

    private static void CheckListed(string style, string rule, Violations violations)
    {
        if (!style.Contains(rule, StringComparison.Ordinal))
        {
            violations.Add("docs/style.md", "does not explain why " + rule + " is off");
        }
    }
}
