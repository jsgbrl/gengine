// Handle to this assembly. tests.cs names the suite, not one of its tests, so renaming a
// test class never breaks the entry point.

using System.Reflection;

namespace MarioClone.Tests;

/// <summary>The test suite covering MarioClone.</summary>
public static class MarioTestSuite
{
    /// <summary>The assembly holding these tests.</summary>
    public static Assembly Assembly => typeof(MarioTestSuite).Assembly;
}
