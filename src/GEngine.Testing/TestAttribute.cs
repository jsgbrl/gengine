// Marks a method as a test. The runner finds these by reflection, the same way xUnit
// and NUnit do: there is no registration list to keep in sync.

using System;

namespace GEngine.Testing;

/// <summary>
/// Marks a parameterless method as a test case. Apply it to a public instance method of
/// a public class; the runner creates one instance of the class per test, so a test can
/// never leak state into the next one.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class TestAttribute : Attribute
{
}
