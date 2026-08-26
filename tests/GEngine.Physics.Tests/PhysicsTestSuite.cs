// Handle to this assembly. tests.cs names the suite, not one of its tests, so renaming a
// test class never breaks the entry point.

using System.Reflection;

namespace GEngine.Physics.Tests;

/// <summary>The test suite covering GEngine.Physics.</summary>
public static class PhysicsTestSuite
{
    /// <summary>The assembly holding these tests.</summary>
    public static Assembly Assembly => typeof(PhysicsTestSuite).Assembly;
}
