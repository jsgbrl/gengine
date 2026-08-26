// Handle to this assembly. tests.cs names the suite, not one of its tests, so renaming a
// test class never breaks the entry point.

using System.Reflection;

namespace GEngine.Input.Tests;

/// <summary>The test suite covering GEngine.Input.</summary>
public static class InputTestSuite
{
    /// <summary>The assembly holding these tests.</summary>
    public static Assembly Assembly => typeof(InputTestSuite).Assembly;
}
