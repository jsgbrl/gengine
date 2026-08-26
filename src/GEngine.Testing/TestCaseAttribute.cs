// One method, many inputs. Each attribute instance becomes a separate test case with
// its own name, its own result line and its own stopwatch.

using System;
using System.Collections.Generic;

namespace GEngine.Testing;

/// <summary>
/// Supplies one set of arguments to a test method. Apply it once per case; the method
/// must declare parameters matching the values, and does not also need
/// <see cref="TestAttribute"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class TestCaseAttribute : Attribute
{
    private readonly object?[] _arguments;

    /// <summary>Creates a case from the given argument values.</summary>
    /// <param name="arguments">Values passed to the test method, in declaration order.</param>
    public TestCaseAttribute(params object?[] arguments)
    {
        _arguments = arguments ?? [];
    }

    /// <summary>The argument values for this case, in declaration order.</summary>
    public IReadOnlyList<object?> Arguments => _arguments;
}
