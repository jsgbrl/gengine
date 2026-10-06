using System;
using System.Collections.Generic;
using GEngine.Testing;

namespace GEngine.Architecture.Tests;

/// <summary>
/// The shape of the engine, checked against the project files rather than against a diagram.
/// Core knows nobody; Physics, Rendering and Input each know only Core and never each other;
/// the game knows all four. Every arrow that is not in this table is a bug, and a diagram in a
/// README cannot fail a build.
/// </summary>
public sealed partial class DependencyRulesTests
{
    private static readonly Dictionary<string, string[]> MayReference = new(StringComparer.Ordinal)
    {
        ["GEngine.Core"] = [],
        ["GEngine.Physics"] = ["GEngine.Core"],
        ["GEngine.Rendering"] = ["GEngine.Core"],
        ["GEngine.Input"] = ["GEngine.Core"],
        ["GEngine.Testing"] = [],
        ["MarioClone"] = ["GEngine.Core", "GEngine.Physics", "GEngine.Rendering", "GEngine.Input"],
    };

    [Test]
    public void EveryEngineProjectReferencesOnlyWhatItIsAllowedTo()
    {
        var violations = new Violations();
        foreach (KeyValuePair<string, string[]> project in MayReference)
        {
            CheckReferences(project, violations);
        }

        violations.AssertNone("a project references only the projects docs/architecture.md gives it");
    }

    // The three modules are siblings, not a stack. Physics that knew about Rendering could not
    // be tested without a terminal, and Rendering that knew about Physics would have opinions
    // about what a body is.
    // Exactly, not at most. "Every reference is allowed" is also true of a project file the
    // reader failed to parse, and a rule that passes on an empty answer is not a rule.
    [Test]
    public void EveryEngineProjectReferencesEverythingItIsSupposedTo()
    {
        var violations = new Violations();
        foreach (KeyValuePair<string, string[]> project in MayReference)
        {
            CheckPresent(project, violations);
        }

        violations.AssertNone("a project references every project docs/architecture.md gives it");
    }

    [Test]
    public void TheThreeModulesNeverImportEachOther()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckImports(file, violations);
        }

        violations.AssertNone("Physics, Rendering and Input never import each other");
    }

    [Test]
    public void CoreImportsNothingFromTheEngine()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckCore(file, violations);
        }

        violations.AssertNone("GEngine.Core depends on nobody");
    }

    [Test]
    public void NothingShippedDependsOnATest()
    {
        var violations = new Violations();
        foreach (string project in Repository.Projects)
        {
            CheckDirection(project, violations);
        }

        violations.AssertNone("nothing under src/ references anything under tests/");
    }

    [Test]
    public void EveryProjectInTheRepositoryIsInTheSolution()
    {
        string solution = Repository.ReadRootFile("gengine.slnx");
        var violations = new Violations();
        foreach (string project in Repository.Projects)
        {
            CheckListed(solution, project, violations);
        }

        violations.AssertNone("every project is in gengine.slnx, or the build does not see it");
    }
}
