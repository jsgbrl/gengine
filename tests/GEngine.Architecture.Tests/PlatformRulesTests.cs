using System;
using System.Collections.Generic;
using GEngine.Testing;

namespace GEngine.Architecture.Tests;

/// <summary>
/// Rule 4 and rule 5 of the build, as tests. One binary, no conditional compilation, no runtime
/// identifiers - and Windows, macOS and Linux as peers, which means a single-system API may only
/// appear inside the backend that exists to speak to that system.
/// </summary>
public sealed class PlatformRulesTests
{
    private static readonly Dictionary<string, string> BelongsTo = new(StringComparer.Ordinal)
    {
        ["OperatingSystem.IsWindows"] = "Windows",
        ["OperatingSystem.IsMacOS"] = "MacOs",
        ["OperatingSystem.IsLinux"] = "Linux",
    };

    [Test]
    public void NothingIsCompiledConditionally()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckDirectives(file, violations);
        }

        violations.AssertNone("no #if: one binary runs on all three systems, decided at run time");
    }

    [Test]
    public void NoProjectPinsARuntimeIdentifier()
    {
        var violations = new Violations();
        foreach (string project in Repository.Projects)
        {
            CheckRuntime(project, violations);
        }

        violations.AssertNone("no RuntimeIdentifier: one build, every system");
    }

    // A path built with a backslash is a path that works on one system. Path.Combine and a
    // forward slash both work everywhere; a literal backslash in a path does not.
    [Test]
    public void NoPathIsBuiltWithABackslash()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckSeparators(file, violations);
        }

        violations.AssertNone("paths are combined, never concatenated with a separator");
    }

    [Test]
    public void ASingleSystemApiOnlyAppearsInThatSystemsBackend()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckSystemApis(file, violations);
        }

        violations.AssertNone("a question about one system is asked only where that system is spoken to");
    }

    [Test]
    public void EverySystemHasTheSameSetOfBackends()
    {
        var violations = new Violations();
        foreach (string kind in new[] { "ConsoleDriver", "HidBackend" })
        {
            CheckTriple(kind, violations);
        }

        violations.AssertNone("every backend exists for all three systems, or for none");
    }

    private static void CheckDirectives(SourceFile file, Violations violations)
    {
        for (int line = 1; line <= file.CodeLines.Count; line++)
        {
            string code = file.CodeLines[line - 1].TrimStart();
            if (code.StartsWith("#if", StringComparison.Ordinal) || code.StartsWith("#elif", StringComparison.Ordinal))
            {
                violations.Add(file, line, "conditional compilation");
            }
        }
    }

    private static void CheckRuntime(string project, Violations violations)
    {
        string text = System.IO.File.ReadAllText(project);
        foreach (string banned in new[] { "<RuntimeIdentifier", "<RuntimeIdentifiers" })
        {
            if (text.Contains(banned, StringComparison.Ordinal))
            {
                violations.Add(Repository.Relative(project), "pins " + banned.TrimStart('<'));
            }
        }
    }

    private static void CheckSeparators(SourceFile file, Violations violations)
    {
        for (int line = 1; line <= file.Lines.Count; line++)
        {
            CheckSeparator(file, line, violations);
        }
    }

    // Only in a string that looks like a path: `\n` is not a separator, and neither is the
    // backslash the linter itself looks for.
    private static void CheckSeparator(SourceFile file, int line, Violations violations)
    {
        string text = file.Lines[line - 1];
        bool literal = text.Contains("\\\\\"", StringComparison.Ordinal)
            || text.Contains("/\\\\", StringComparison.Ordinal);
        if (literal && !file.Path.StartsWith("tests/GEngine.Architecture.Tests/", StringComparison.Ordinal))
        {
            violations.Add(file, line, "a path with a backslash in it");
        }
    }

    private static void CheckSystemApis(SourceFile file, Violations violations)
    {
        foreach (KeyValuePair<string, string> api in BelongsTo)
        {
            CheckApi(file, api, violations);
        }
    }

    private static void CheckApi(SourceFile file, KeyValuePair<string, string> api, Violations violations)
    {
        if (file.Path.Contains(api.Value, StringComparison.Ordinal) || Neutral(file))
        {
            return;
        }

        for (int line = 1; line <= file.CodeLines.Count; line++)
        {
            Report(file, line, api, violations);
        }
    }

    private static void Report(SourceFile file, int line, KeyValuePair<string, string> api, Violations violations)
    {
        if (file.CodeLines[line - 1].Contains(api.Key, StringComparison.Ordinal))
        {
            violations.Add(file, line, api.Key + " outside the " + api.Value + " backend");
        }
    }

    // The probe and the factories are where the question is supposed to be asked: something has
    // to decide which backend to build, and that decision is the same shape on every system.
    private static bool Neutral(SourceFile file) =>
        file.Path.EndsWith("SystemPlatformProbe.cs", StringComparison.Ordinal)
        || file.Path.EndsWith("Factory.cs", StringComparison.Ordinal)
        || file.Path.StartsWith("tests/", StringComparison.Ordinal);

    private static void CheckTriple(string kind, Violations violations)
    {
        foreach (string system in new[] { "Windows", "MacOs", "Linux" })
        {
            CheckExists(kind, system, violations);
        }
    }

    private static void CheckExists(string kind, string system, Violations violations)
    {
        foreach (SourceFile file in Repository.Sources)
        {
            if (file.Path.EndsWith(system + kind + ".cs", StringComparison.Ordinal))
            {
                return;
            }
        }

        violations.Add("src", "there is no " + system + kind);
    }
}
