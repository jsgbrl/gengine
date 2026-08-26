// Handle to this assembly. tests.cs names the suite, not one of its tests, so renaming a
// test class never breaks the entry point.

using System.Reflection;

namespace GEngine.Architecture.Tests;

/// <summary>The test suite covering the repository itself: style, vocabulary, platform and dependency rules.</summary>
public static class ArchitectureTestSuite
{
    /// <summary>The assembly holding these tests.</summary>
    public static Assembly Assembly => typeof(ArchitectureTestSuite).Assembly;
}
