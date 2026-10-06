// The repository as text. Every rule in this project is checked by reading files, never by
// loading assemblies: a rule about Physics must not need Physics to compile, or the linter
// stops being able to say "this project references one it must not".
//
// The root is found by walking up from the test binary until a directory holds the solution.
// That works from `dotnet run tests.cs`, from a built binary, and from an IDE, none of which
// agree about the working directory.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace GEngine.Architecture.Tests;

internal static class Repository
{
    private const string SolutionFile = "gengine.slnx";

    private static IReadOnlyList<SourceFile>? _sources;

    /// <summary>The directory holding the solution.</summary>
    public static string Root { get; } = FindRoot();

    /// <summary>Every hand-written C# file in the repository.</summary>
    public static IReadOnlyList<SourceFile> Sources => _sources ??= ReadSources();

    /// <summary>Every project file, engine and test alike.</summary>
    public static IReadOnlyList<string> Projects { get; } = Find("*.csproj");

    /// <summary>Every file-based app: the entry points and the examples.</summary>
    public static IReadOnlyList<string> Scripts { get; } = FindScripts();

    /// <summary>Reads a file at the root, such as the solution or the analyzer configuration.</summary>
    /// <param name="name">Its name.</param>
    /// <returns>Its contents.</returns>
    public static string ReadRootFile(string name) => File.ReadAllText(Path.Combine(Root, name));

    /// <summary>The path of a file relative to the root, in the form the report prints.</summary>
    /// <param name="path">An absolute path.</param>
    /// <returns>The relative path, with forward slashes.</returns>
    public static string Relative(string path) =>
        Path.GetRelativePath(Root, path).Replace('\\', '/');

    private static List<SourceFile> ReadSources()
    {
        List<SourceFile> sources = [];
        foreach (string path in Find("*.cs"))
        {
            sources.Add(new SourceFile(Relative(path), File.ReadAllText(path)));
        }

        return sources;
    }

    // Both source trees, but nothing a build produced: obj holds generated C# that nobody
    // wrote and no rule here should judge. The test is on the path relative to the root, not
    // the absolute one - this repository lives in a folder called `bin`, and matching that
    // against the absolute path silently excluded every file in it. A linter that reads
    // nothing passes everything, which is the one way a linter can fail without saying so.
    private static List<string> Find(string pattern)
    {
        List<string> found = [];
        foreach (string folder in new[] { "src", "tests" })
        {
            foreach (string path in Directory.EnumerateFiles(Path.Combine(Root, folder), pattern, SearchOption.AllDirectories))
            {
                Keep(found, path);
            }
        }

        found.Sort(StringComparer.Ordinal);
        return found;
    }

    private static void Keep(List<string> found, string path)
    {
        string relative = Relative(path);
        if (!relative.Contains("/bin/", StringComparison.Ordinal)
            && !relative.Contains("/obj/", StringComparison.Ordinal))
        {
            found.Add(path);
        }
    }

    private static List<string> FindScripts()
    {
        List<string> scripts = [.. Directory.EnumerateFiles(Root, "*.cs", SearchOption.TopDirectoryOnly)];
        scripts.AddRange(Directory.EnumerateFiles(Path.Combine(Root, "examples"), "*.cs", SearchOption.TopDirectoryOnly));
        scripts.Sort(StringComparer.Ordinal);
        return scripts;
    }

    // Three places to start looking, in order of how much they can be trusted. The compiler
    // burns this file's own path into the binary, which is the only one that survives
    // `dotnet run tests.cs` - a file-based app builds into a temporary folder far away from
    // the repository, so the test binary's own directory is nowhere near the source.
    private static string FindRoot([CallerFilePath] string thisFile = "")
    {
        return Above(Path.GetDirectoryName(thisFile))
            ?? Above(AppContext.BaseDirectory)
            ?? Above(Directory.GetCurrentDirectory())
            ?? throw new DirectoryNotFoundException("no " + SolutionFile + " above " + thisFile);
    }

    private static string? Above(string? start)
    {
        DirectoryInfo? directory = start is null ? null : new DirectoryInfo(start);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, SolutionFile)))
        {
            directory = directory.Parent;
        }

        return directory?.FullName;
    }
}
