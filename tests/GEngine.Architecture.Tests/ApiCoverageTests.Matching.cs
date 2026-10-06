// How coverage is actually decided: which test class counts as covering which type, and which
// three categories are exempt as categories rather than one table row each.

using System;
using System.Collections.Generic;

namespace GEngine.Architecture.Tests;

/// <content>Matching test classes to types.</content>
public sealed partial class ApiCoverageTests
{
    private static void CheckCovered(SourceFile file, HashSet<string> tested, Violations violations)
    {
        if (!file.IsProduction)
        {
            return;
        }

        foreach (Block type in PublicTypesIn(file))
        {
            Covered(file, type, tested, violations);
        }
    }

    private static void Covered(SourceFile file, Block type, HashSet<string> tested, Violations violations)
    {
        if (Named(tested, type.Name) || Exempt.ContainsKey(type.Name) || IsExempt(file, type))
        {
            return;
        }

        violations.Add(file, type.FirstLine, "nothing called " + type.Name + "Tests exists");
    }

    // A test class begins with the name of the type it covers. PlayerJumpTests covers Player
    // and PhysicsWorldMovementTests covers PhysicsWorld, and both say what they are about more
    // usefully than one enormous class named exactly after the type would.
    private static bool Named(HashSet<string> tested, string type)
    {
        foreach (string covered in tested)
        {
            if (covered.StartsWith(type, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    // Three categories, three reasons.
    //
    // An interface has no behaviour of its own: what it promises is checked in every class that
    // implements it, and a test for the interface alone would have to invent one.
    //
    // An enum is a list of names. What a name means is checked where it is used - which button
    // bit is Cross is a DualSense question, and DualSenseHardwareTests asks it against hardware.
    //
    // A platform backend can only be exercised on its own platform. Windows was verified by
    // running it; Linux and macOS were not, and docs/platforms.md says so rather than pretending
    // a test that never runs is coverage.
    private static bool IsExempt(SourceFile file, Block type)
    {
        return type.Signature.Contains(" interface ", StringComparison.Ordinal)
            || type.Signature.Contains(" enum ", StringComparison.Ordinal)
            || file.IsInterop
            || IsPlatformDriver(type.Name);
    }

    private static bool IsPlatformDriver(string name)
    {
        foreach (string system in new[] { "Windows", "MacOs", "Linux" })
        {
            if (name.StartsWith(system, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static void CheckRunnable(SourceFile file, Violations violations)
    {
        if (file.IsProduction || !file.Path.EndsWith("Tests.cs", StringComparison.Ordinal))
        {
            return;
        }

        CheckVisible(file, violations);
        CheckHasTests(file, violations);
    }

    private static void CheckVisible(SourceFile file, Violations violations)
    {
        foreach (Block type in Declarations.Types(file))
        {
            Visible(file, type, violations);
        }
    }

    private static void Visible(SourceFile file, Block type, Violations violations)
    {
        bool declared = type.Depth == 0 && type.Name.EndsWith("Tests", StringComparison.Ordinal);
        if (declared && !type.Signature.StartsWith("public", StringComparison.Ordinal))
        {
            violations.Add(file, type.FirstLine, type.Name + " is not public, so the runner never sees it");
        }
    }

    private static void CheckHasTests(SourceFile file, Violations violations)
    {
        if (!file.Text.Contains("[Test]", StringComparison.Ordinal)
            && !file.Text.Contains("[TestCase(", StringComparison.Ordinal))
        {
            violations.Add(file, 1, "a test file with no test in it");
        }
    }

    private static void CheckExemption(HashSet<string> types, KeyValuePair<string, string> exemption, Violations violations)
    {
        if (!types.Contains(exemption.Key))
        {
            violations.Add("ApiCoverageTests", exemption.Key + " is exempt but does not exist");
        }
    }

    private static HashSet<string> TestClasses()
    {
        HashSet<string> found = new(StringComparer.Ordinal);
        foreach (SourceFile file in Repository.Sources)
        {
            AddTestClasses(file, found);
        }

        return found;
    }

    private static void AddTestClasses(SourceFile file, HashSet<string> found)
    {
        foreach (Block type in PublicTypesIn(file))
        {
            if (type.Name.EndsWith("Tests", StringComparison.Ordinal))
            {
                found.Add(type.Name[..^"Tests".Length]);
            }
        }
    }

    private static HashSet<string> PublicTypes()
    {
        HashSet<string> found = new(StringComparer.Ordinal);
        foreach (SourceFile file in Repository.Sources)
        {
            AddPublicTypes(file, found);
        }

        return found;
    }

    private static void AddPublicTypes(SourceFile file, HashSet<string> found)
    {
        foreach (Block type in PublicTypesIn(file))
        {
            found.Add(type.Name);
        }
    }

    private static List<Block> PublicTypesIn(SourceFile file)
    {
        List<Block> found = [];
        foreach (Block type in Declarations.Types(file))
        {
            if (type.Depth == 0 && type.Signature.StartsWith("public", StringComparison.Ordinal))
            {
                found.Add(type);
            }
        }

        return found;
    }
}
