// Reflection lives here and nowhere else in the framework. Everything is sorted by name
// before it is returned, so two runs of the same assemblies report in the same order:
// a suite whose order depends on reflection is a suite that fails differently every time.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace GEngine.Testing;

/// <summary>Finds the runnable cases in a set of types.</summary>
internal static class TestDiscovery
{
    /// <summary>Discovers every case declared by the given types, in a stable order.</summary>
    /// <param name="types">Candidate types, usually every public type of an assembly.</param>
    /// <returns>The discovered cases, ordered by name.</returns>
    public static IReadOnlyList<TestCaseInfo> Discover(IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(types);
        List<TestCaseInfo> cases = [];
        foreach (Type type in Sorted(types))
        {
            AddCasesOf(type, cases);
        }

        cases.Sort(static (left, right) => string.CompareOrdinal(left.Name, right.Name));
        return cases;
    }

    private static List<Type> Sorted(IEnumerable<Type> types)
    {
        List<Type> candidates = [];
        foreach (Type type in types)
        {
            if (IsCandidate(type))
            {
                candidates.Add(type);
            }
        }

        candidates.Sort(static (left, right) => string.CompareOrdinal(left.FullName, right.FullName));
        return candidates;
    }

    // Visibility is decided by whoever builds the type list, not here: TestRunner.Run
    // scans exported types only, so a test class has to be public, while the framework's
    // own tests hand in internal sample classes on purpose - a sample that fails must not
    // be picked up by the real run.
    private static bool IsCandidate(Type type)
    {
        return type.IsClass
            && !type.IsAbstract
            && !type.IsGenericTypeDefinition
            && type.GetConstructor(Type.EmptyTypes) is not null;
    }

    private static void AddCasesOf(Type type, List<TestCaseInfo> cases)
    {
        MethodInfo? setup = FindSetup(type);
        string typeSkip = type.GetCustomAttribute<SkipAttribute>()?.Reason ?? string.Empty;
        foreach (MethodInfo method in DeclaredMethods(type))
        {
            AddCasesOf(method, setup, typeSkip, cases);
        }
    }

    private static void AddCasesOf(MethodInfo method, MethodInfo? setup, string typeSkip, List<TestCaseInfo> cases)
    {
        string skip = method.GetCustomAttribute<SkipAttribute>()?.Reason ?? typeSkip;
        List<TestCaseAttribute> parameterised = [.. method.GetCustomAttributes<TestCaseAttribute>()];
        if (parameterised.Count == 0)
        {
            if (method.GetCustomAttribute<TestAttribute>() is not null)
            {
                cases.Add(new TestCaseInfo(method, setup, [], NameOf(method, [])) { SkipReason = skip });
            }

            return;
        }

        foreach (TestCaseAttribute attribute in parameterised)
        {
            IReadOnlyList<object?> arguments = attribute.Arguments;
            cases.Add(new TestCaseInfo(method, setup, arguments, NameOf(method, arguments)) { SkipReason = skip });
        }
    }

    private static MethodInfo? FindSetup(Type type)
    {
        MethodInfo? found = null;
        foreach (MethodInfo method in DeclaredMethods(type))
        {
            if (method.GetCustomAttribute<SetupAttribute>() is null)
            {
                continue;
            }

            Require(found is null, type.FullName + " declares more than one [Setup] method");
            found = method;
        }

        return found;
    }

    private static MethodInfo[] DeclaredMethods(Type type)
    {
        MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        Array.Sort(methods, static (left, right) => string.CompareOrdinal(left.Name, right.Name));
        return methods;
    }

    private static string NameOf(MethodInfo method, IReadOnlyList<object?> arguments)
    {
        string qualified = method.DeclaringType!.FullName + "." + method.Name;
        if (arguments.Count == 0)
        {
            return qualified;
        }

        List<string> rendered = [];
        foreach (object? argument in arguments)
        {
            rendered.Add(Convert.ToString(argument, CultureInfo.InvariantCulture) ?? "null");
        }

        return qualified + "(" + string.Join(", ", rendered) + ")";
    }

    private static void Require(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }
}
