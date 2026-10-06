using System;
using System.Collections.Generic;
using GEngine.Testing;

namespace GEngine.Architecture.Tests;

/// <summary>
/// Every public type in the engine has a test class named after it. Not a coverage percentage -
/// a percentage tells you how much of the code ran, and running is not the same as being
/// checked - but a flat rule that nothing ships without somebody having written down what it is
/// supposed to do.
///
/// The exemptions are listed here rather than inferred, so adding one is a decision somebody
/// makes on purpose and a reviewer can see.
/// </summary>
public sealed partial class ApiCoverageTests
{
    // Types whose whole content is checked through something else, with the reason. Three
    // categories are exempt as categories rather than one row each - see IsExempt below.
    private static readonly Dictionary<string, string> Exempt = new(StringComparer.Ordinal)
    {
        ["CoreTestSuite"] = "a handle to an assembly, used by tests.cs",
        ["PhysicsTestSuite"] = "a handle to an assembly, used by tests.cs",
        ["RenderingTestSuite"] = "a handle to an assembly, used by tests.cs",
        ["InputTestSuite"] = "a handle to an assembly, used by tests.cs",
        ["FrameworkTestSuite"] = "a handle to an assembly, used by tests.cs",
        ["MarioTestSuite"] = "a handle to an assembly, used by tests.cs",
        ["ArchitectureTestSuite"] = "a handle to an assembly, used by tests.cs",
        ["TestAttribute"] = "a marker with no behaviour; every test in the repository uses it",
        ["TestCaseAttribute"] = "a marker with no behaviour; the discovery tests read it",
        ["SetupAttribute"] = "a marker with no behaviour; the discovery tests read it",
        ["SkipAttribute"] = "a marker with no behaviour; the discovery tests read it",
    };

    [Test]
    public void EveryPublicTypeHasATestClass()
    {
        HashSet<string> tested = TestClasses();
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckCovered(file, tested, violations);
        }

        violations.AssertNone("every public type has a test class named after it");
    }

    // The silent failure this rule exists for: a test class the runner never sees. Discovery
    // scans exported types for methods marked [Test], so a class that is not public, or that
    // holds no marked method, contributes nothing and says nothing about contributing nothing.
    [Test]
    public void EveryTestClassIsOneTheRunnerCanFind()
    {
        var violations = new Violations();
        foreach (SourceFile file in Repository.Sources)
        {
            CheckRunnable(file, violations);
        }

        violations.AssertNone("a test class is public and has at least one [Test]");
    }

    [Test]
    public void EveryExemptionNamesATypeThatExists()
    {
        HashSet<string> types = PublicTypes();
        var violations = new Violations();
        foreach (KeyValuePair<string, string> exemption in Exempt)
        {
            CheckExemption(types, exemption, violations);
        }

        violations.AssertNone("every exemption is for a type that is still here");
    }
}
