using System;
using System.IO;
using GEngine.Testing;

namespace GEngine.Architecture.Tests;

/// <summary>
/// Rule 2 and rule 3 of the build. No NuGet anywhere - not in the engine, not in the game, not
/// in the tests - and the game plays as a script, which means run.cs has to reach its projects
/// with `#:project` and nothing has to be built by hand first.
/// </summary>
public sealed class PackagingRulesTests
{
    [Test]
    public void NoProjectReferencesAPackage()
    {
        var violations = new Violations();
        foreach (string project in Repository.Projects)
        {
            CheckNoPackages(project, violations);
        }

        violations.AssertNone("no PackageReference: the base class library and the SDK, and nothing else");
    }

    [Test]
    public void NoScriptReferencesAPackage()
    {
        var violations = new Violations();
        foreach (string script in Repository.Scripts)
        {
            CheckScript(script, violations);
        }

        violations.AssertNone("no #:package in a file-based app either");
    }

    // Belt and braces. Even if a PackageReference appeared, there is nowhere to fetch it from,
    // so the build breaks instead of quietly acquiring a dependency.
    [Test]
    public void TheRepositoryHasNoPackageSources()
    {
        string config = Repository.ReadRootFile("NuGet.config");
        Assert.IsTrue(config.Contains("<clear />", StringComparison.Ordinal), "the sources are cleared");
        Assert.IsFalse(config.Contains("<add key", StringComparison.Ordinal), "and none is added back");
    }

    [Test]
    public void EveryProjectInheritsTheAnalyzerSettings()
    {
        foreach (string folder in new[] { "src", "tests" })
        {
            CheckProperties(folder);
        }
    }

    [Test]
    public void TheAnalyzerSettingsAreNeverLoweredByAProject()
    {
        var violations = new Violations();
        foreach (string project in Repository.Projects)
        {
            CheckNoOverride(project, violations);
        }

        violations.AssertNone("a rule is tuned in .editorconfig, never by turning the analyzers down");
    }

    [Test]
    public void EveryEntryPointIsAScriptThatNamesItsProjects()
    {
        foreach (string script in new[] { "run.cs", "tests.cs" })
        {
            string text = Repository.ReadRootFile(script);
            Assert.IsTrue(text.Contains("#:project", StringComparison.Ordinal), script + " names its projects");
        }
    }

    [Test]
    public void EveryExampleIsAScriptThatRunsOnItsOwn()
    {
        var violations = new Violations();
        foreach (string script in Repository.Scripts)
        {
            CheckRunnable(script, violations);
        }

        violations.AssertNone("every example is a file-based app that names the projects it needs");
    }

    private static void CheckNoPackages(string project, Violations violations)
    {
        if (File.ReadAllText(project).Contains("<PackageReference", StringComparison.Ordinal))
        {
            violations.Add(Repository.Relative(project), "references a NuGet package");
        }
    }

    private static void CheckScript(string script, Violations violations)
    {
        if (File.ReadAllText(script).Contains("#:package", StringComparison.Ordinal))
        {
            violations.Add(Repository.Relative(script), "references a NuGet package");
        }
    }

    private static void CheckProperties(string folder)
    {
        string properties = File.ReadAllText(Path.Combine(Repository.Root, folder, "Directory.Build.props"));
        foreach (string required in Required)
        {
            Assert.IsTrue(properties.Contains(required, StringComparison.Ordinal), folder + " sets " + required);
        }
    }

    private static void CheckNoOverride(string project, Violations violations)
    {
        string text = File.ReadAllText(project);
        foreach (string lowered in Lowered)
        {
            if (text.Contains(lowered, StringComparison.Ordinal))
            {
                violations.Add(Repository.Relative(project), "turns the analyzers down with " + lowered);
            }
        }
    }

    private static void CheckRunnable(string script, Violations violations)
    {
        string path = Repository.Relative(script);
        if (!path.StartsWith("examples/", StringComparison.Ordinal))
        {
            return;
        }

        if (!File.ReadAllText(script).Contains("#:project", StringComparison.Ordinal))
        {
            violations.Add(path, "names no project, so it cannot run on its own");
        }
    }

    private static readonly string[] Required =
    [
        "<TargetFramework>net10.0</TargetFramework>",
        "<TreatWarningsAsErrors>true</TreatWarningsAsErrors>",
        "<CodeAnalysisTreatWarningsAsErrors>true</CodeAnalysisTreatWarningsAsErrors>",
        "<EnableNETAnalyzers>true</EnableNETAnalyzers>",
        "<AnalysisLevel>latest-all</AnalysisLevel>",
        "<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>",
        "<GenerateDocumentationFile>true</GenerateDocumentationFile>",
        "<Nullable>enable</Nullable>",
        "<AllowUnsafeBlocks>false</AllowUnsafeBlocks>",
    ];

    private static readonly string[] Lowered =
    [
        "<TreatWarningsAsErrors>false",
        "<EnableNETAnalyzers>false",
        "<EnforceCodeStyleInBuild>false",
        "<NoWarn>",
        "<AnalysisLevel>none",
        "<AllowUnsafeBlocks>true",
    ];
}
