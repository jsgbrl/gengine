// Reading the arrows: what a project file says it references, and what a source file says it
// imports. Both questions are answered from text, never by loading an assembly.

using System;
using System.Collections.Generic;

namespace GEngine.Architecture.Tests;

/// <content>Checking one project, one file and one arrow at a time.</content>
public sealed partial class DependencyRulesTests
{
    private static void CheckReferences(KeyValuePair<string, string[]> project, Violations violations)
    {
        string path = "src/" + project.Key + "/" + project.Key + ".csproj";
        foreach (string referenced in Projects.ReferencedBy(path))
        {
            Allowed(path, referenced, project.Value, violations);
        }
    }

    private static void Allowed(string path, string referenced, string[] permitted, Violations violations)
    {
        foreach (string name in permitted)
        {
            if (string.Equals(name, referenced, StringComparison.Ordinal))
            {
                return;
            }
        }

        violations.Add(path, "references " + referenced + ", which it is not allowed to know about");
    }

    private static void CheckPresent(KeyValuePair<string, string[]> project, Violations violations)
    {
        string path = "src/" + project.Key + "/" + project.Key + ".csproj";
        List<string> actual = Projects.ReferencedBy(path);
        foreach (string wanted in project.Value)
        {
            if (!actual.Contains(wanted))
            {
                violations.Add(path, "does not reference " + wanted);
            }
        }
    }

    private static void CheckImports(SourceFile file, Violations violations)
    {
        string? owner = Projects.OwnerOf(file.Path);
        if (owner is null || !Projects.IsModule(owner))
        {
            return;
        }

        foreach (string sibling in Projects.Modules)
        {
            Imported(file, owner, sibling, violations);
        }
    }

    private static void Imported(SourceFile file, string owner, string sibling, Violations violations)
    {
        if (string.Equals(owner, sibling, StringComparison.Ordinal))
        {
            return;
        }

        int line = Projects.LineImporting(file, sibling);
        if (line > 0)
        {
            violations.Add(file, line, owner + " imports " + sibling);
        }
    }

    private static void CheckCore(SourceFile file, Violations violations)
    {
        if (!file.Path.StartsWith("src/GEngine.Core/", StringComparison.Ordinal))
        {
            return;
        }

        foreach (string other in Projects.Modules)
        {
            int line = Projects.LineImporting(file, other);
            if (line > 0)
            {
                violations.Add(file, line, "Core imports " + other);
            }
        }
    }

    private static void CheckDirection(string project, Violations violations)
    {
        string path = Repository.Relative(project);
        if (!path.StartsWith("src/", StringComparison.Ordinal))
        {
            return;
        }

        foreach (string referenced in Projects.ReferencedBy(path))
        {
            Shipped(path, referenced, violations);
        }
    }

    private static void Shipped(string path, string referenced, Violations violations)
    {
        if (referenced.EndsWith(".Tests", StringComparison.Ordinal))
        {
            violations.Add(path, "references the test project " + referenced);
        }
    }

    private static void CheckListed(string solution, string project, Violations violations)
    {
        string name = Repository.Relative(project).Split('/')[^1];
        if (!solution.Contains(name, StringComparison.Ordinal))
        {
            violations.Add(Repository.Relative(project), "is not listed in gengine.slnx");
        }
    }
}
