// Assets from a dictionary. This is what a test hands the loader when the point of the test
// is the parsing and not the file system.

using System;
using System.Collections.Generic;
using GEngine.Core.Contracts;

namespace GEngine.Rendering.Assets;

/// <summary>Assets held in memory, addressed by path.</summary>
public sealed class MemoryAssetSource : IAssetSource
{
    private readonly Dictionary<string, string> _assets = [];

    /// <inheritdoc/>
    public string Name => "memory";

    /// <summary>Adds or replaces an asset.</summary>
    /// <param name="path">Forward-slash separated path.</param>
    /// <param name="text">The contents.</param>
    /// <returns>This source, so calls can be chained while building a fixture.</returns>
    public MemoryAssetSource With(string path, string text)
    {
        _assets[path] = text;
        return this;
    }

    /// <inheritdoc/>
    public bool Exists(string path) => _assets.ContainsKey(path);

    /// <inheritdoc/>
    public string ReadText(string path)
    {
        if (_assets.TryGetValue(path, out string? text))
        {
            return text;
        }

        throw new KeyNotFoundException("no asset named " + path + " in " + Name);
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> ListPaths(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        List<string> found = [];
        foreach (string path in _assets.Keys)
        {
            if (path.StartsWith(prefix, StringComparison.Ordinal))
            {
                found.Add(path);
            }
        }

        found.Sort(StringComparer.Ordinal);
        return found;
    }
}
