// Setup runs before every test in the class, never once for the whole class. Shared
// mutable state between tests is the classic source of order-dependent suites.

using System;

namespace GEngine.Testing;

/// <summary>
/// Marks a public parameterless instance method to run immediately before each test in
/// the same class. A class may declare at most one; more than one is a discovery error.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class SetupAttribute : Attribute
{
}
