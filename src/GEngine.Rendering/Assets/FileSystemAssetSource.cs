// Assets from a directory on disk. Paths are always forward-slash separated and always
// relative to the root, so nothing in the engine ever writes a backslash - which is rule 5
// of the build prompt, and what PlatformRulesTests checks for.

using System;
using System.Collections.Generic;
using System.IO;
using GEngine.Core.Contracts;

namespace GEngine.Rendering.Assets;

/// <summary>Assets read from a directory.</summary>
public sealed class FileSystemAssetSource : IAssetSource
{
    private readonly string _root;

    /// <summary>Creates a source rooted at a directory.</summary>
    /// <param name="root">The directory holding the assets.</param>
    public FileSystemAssetSource(string root)
    {
        ArgumentNullException.ThrowIfNull(root);
        _root = root;
    }

    /// <inheritdoc/>
    public string Name => "files under " + _root;

    /// <inheritdoc/>
    public bool Exists(string path) => File.Exists(Resolve(path));

    /// <inheritdoc/>
    public string ReadText(string path) => File.ReadAllText(Resolve(path));

    /// <inheritdoc/>
    public IReadOnlyList<string> ListPaths(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        if (!Directory.Exists(_root))
        {
            return [];
        }

        List<string> found = [];
        foreach (string file in Directory.EnumerateFiles(_root, "*", SearchOption.AllDirectories))
        {
            AddIfMatching(file, prefix, found);
        }

        found.Sort(StringComparer.Ordinal);
        return found;
    }

    private void AddIfMatching(string file, string prefix, List<string> found)
    {
        string relative = Path.GetRelativePath(_root, file).Replace(Path.DirectorySeparatorChar, '/');
        if (relative.StartsWith(prefix, StringComparison.Ordinal))
        {
            found.Add(relative);
        }
    }

    // Path.Combine is given the segments one at a time so that a forward-slash path works on
    // a system whose separator is a backslash.
    private string Resolve(string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        string[] segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string resolved = _root;
        foreach (string segment in segments)
        {
            resolved = Path.Combine(resolved, segment);
        }

        return resolved;
    }
}
