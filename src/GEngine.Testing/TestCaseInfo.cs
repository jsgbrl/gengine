// One runnable case: the class to instantiate, the setup to run first, the method to
// call and the arguments to call it with. Discovery produces these, execution consumes
// them, and neither knows about reflection details of the other.

using System.Collections.Generic;
using System.Reflection;

namespace GEngine.Testing;

/// <summary>A single discovered case, ready to run.</summary>
internal sealed class TestCaseInfo
{
    /// <summary>Creates a case.</summary>
    /// <param name="method">The test method to invoke.</param>
    /// <param name="setup">Method to run before it, or null when the class has none.</param>
    /// <param name="arguments">Arguments for the method, empty for a plain test.</param>
    /// <param name="name">Fully qualified case name for the report.</param>
    public TestCaseInfo(MethodInfo method, MethodInfo? setup, IReadOnlyList<object?> arguments, string name)
    {
        Method = method;
        Setup = setup;
        Arguments = arguments;
        Name = name;
    }

    /// <summary>The test method to invoke.</summary>
    public MethodInfo Method { get; }

    /// <summary>Method to run before the test, or null when the class declares none.</summary>
    public MethodInfo? Setup { get; }

    /// <summary>Arguments for the method, in declaration order.</summary>
    public IReadOnlyList<object?> Arguments { get; }

    /// <summary>Fully qualified case name for the report.</summary>
    public string Name { get; }

    /// <summary>Why this case is skipped, or empty when it should run.</summary>
    public string SkipReason { get; init; } = string.Empty;
}
