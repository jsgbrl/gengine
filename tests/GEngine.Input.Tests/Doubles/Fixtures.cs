// Loads the recorded reports that ship inside this test assembly. This is the second of the
// two places rule 6 allows reflection - loading an embedded resource - and it is why the
// fixtures travel with the tests rather than depending on a working directory.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GEngine.Input.Hid;

namespace GEngine.Input.Tests.Doubles;

internal static class Fixtures
{
    public static byte[] Report(string name) => HidReportHex.Parse(Text(name));

    public static string Text(string name)
    {
        Assembly assembly = typeof(Fixtures).Assembly;
        string resource = Find(assembly, name);
        using Stream stream = assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    public static IReadOnlyList<string> Names()
    {
        List<string> found = [];
        foreach (string resource in typeof(Fixtures).Assembly.GetManifestResourceNames())
        {
            if (resource.Contains(".dualsense.", StringComparison.Ordinal))
            {
                found.Add(resource);
            }
        }

        found.Sort(StringComparer.Ordinal);
        return found;
    }

    private static string Find(Assembly assembly, string name)
    {
        string suffix = ".dualsense." + name;
        foreach (string resource in assembly.GetManifestResourceNames())
        {
            if (resource.EndsWith(suffix, StringComparison.Ordinal))
            {
                return resource;
            }
        }

        throw new FileNotFoundException("no fixture called " + name);
    }
}
