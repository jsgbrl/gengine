// Reading the project files: who references whom, and which project a source file belongs to.
//
// Everything here works on text. Loading the assemblies would answer the same questions, but a
// test that has to reference Physics in order to say "nobody may reference Physics" has already
// broken the rule it is checking.

using System;
using System.Collections.Generic;
using System.IO;

namespace GEngine.Architecture.Tests;

internal static class Projects
{
    /// <summary>The three sibling modules, which are the ones forbidden to know each other.</summary>
    public static IReadOnlyList<string> Modules { get; } = ["GEngine.Physics", "GEngine.Rendering", "GEngine.Input"];

    /// <summary>True if a project is one of the three siblings.</summary>
    /// <param name="name">The project name.</param>
    /// <returns>Whether it is a module.</returns>
    public static bool IsModule(string name)
    {
        foreach (string module in Modules)
        {
            if (string.Equals(module, name, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>The names of the projects a project file references.</summary>
    /// <param name="path">The project's path, relative to the root.</param>
    /// <returns>The referenced project names, without the extension.</returns>
    public static List<string> ReferencedBy(string path)
    {
        List<string> referenced = [];
        foreach (string line in File.ReadAllLines(Path.Combine(Repository.Root, path)))
        {
            ReadReference(referenced, line);
        }

        return referenced;
    }

    /// <summary>The project a source file belongs to, from its path.</summary>
    /// <param name="path">The file's path, relative to the root.</param>
    /// <returns>The project name, or null if the path is not inside one.</returns>
    public static string? OwnerOf(string path)
    {
        string[] parts = path.Split('/');
        return parts.Length < 2 ? null : parts[1];
    }

    /// <summary>The line on which a file imports a namespace from a project, or zero.</summary>
    /// <param name="file">The file.</param>
    /// <param name="project">The project whose namespaces to look for.</param>
    /// <returns>A one-based line number, or zero if it never imports it.</returns>
    public static int LineImporting(SourceFile file, string project)
    {
        for (int line = 1; line <= file.CodeLines.Count; line++)
        {
            if (Imports(file.CodeLines[line - 1], project))
            {
                return line;
            }
        }

        return 0;
    }

    private static bool Imports(string code, string project)
    {
        string trimmed = code.Trim();
        return trimmed.StartsWith("using " + project + ";", StringComparison.Ordinal)
            || trimmed.StartsWith("using " + project + ".", StringComparison.Ordinal);
    }

    private static void ReadReference(List<string> referenced, string line)
    {
        const string Marker = "<ProjectReference Include=\"";
        int start = line.IndexOf(Marker, StringComparison.Ordinal);
        if (start < 0)
        {
            return;
        }

        string rest = line[(start + Marker.Length)..];
        string path = rest[..rest.IndexOf('"', StringComparison.Ordinal)];
        referenced.Add(Path.GetFileNameWithoutExtension(path));
    }
}
