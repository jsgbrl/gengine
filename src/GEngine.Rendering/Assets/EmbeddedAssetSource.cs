// Assets compiled into the assembly. This is what makes "dotnet run run.cs" enough: the
// sprites and the level travel inside the dll, so there is no working directory to get
// wrong and no file to forget to copy.
//
// MSBuild names an embedded resource by replacing every directory separator with a dot, so
// assets/mario.sprite becomes MarioClone.assets.mario.sprite - and there is no way to tell,
// from the name alone, which dots were separators. The map is therefore built by matching
// the tail of each resource name, once, at construction.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GEngine.Core.Contracts;

namespace GEngine.Rendering.Assets;

/// <summary>Assets embedded in an assembly as manifest resources.</summary>
public sealed class EmbeddedAssetSource : IAssetSource
{
    private readonly Assembly _assembly;
    private readonly Dictionary<string, string> _resourcesByPath = [];

    /// <summary>Creates a source over the embedded resources of an assembly.</summary>
    /// <param name="assembly">The assembly holding the resources.</param>
    public EmbeddedAssetSource(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        _assembly = assembly;
        foreach (string resource in assembly.GetManifestResourceNames())
        {
            _resourcesByPath[PathOf(resource)] = resource;
        }
    }

    /// <inheritdoc/>
    public string Name => "embedded in " + _assembly.GetName().Name;

    /// <inheritdoc/>
    public bool Exists(string path) => _resourcesByPath.ContainsKey(path);

    /// <inheritdoc/>
    public string ReadText(string path)
    {
        if (!_resourcesByPath.TryGetValue(path, out string? resource))
        {
            throw new KeyNotFoundException("no asset named " + path + " " + Name);
        }

        using Stream stream = _assembly.GetManifestResourceStream(resource)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> ListPaths(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        List<string> found = [];
        foreach (string path in _resourcesByPath.Keys)
        {
            if (path.StartsWith(prefix, StringComparison.Ordinal))
            {
                found.Add(path);
            }
        }

        found.Sort(StringComparer.Ordinal);
        return found;
    }

    // MarioClone.assets.mario.sprite -> assets/mario.sprite. The last dot introduces the
    // extension, the first segment is the assembly's root namespace, and everything between
    // was a directory.
    private static string PathOf(string resource)
    {
        int extension = resource.LastIndexOf('.');
        if (extension <= 0)
        {
            return resource;
        }

        string withoutExtension = resource[..extension];
        int firstDot = withoutExtension.IndexOf('.', StringComparison.Ordinal);
        string body = firstDot < 0 ? withoutExtension : withoutExtension[(firstDot + 1)..];
        return body.Replace('.', '/') + resource[extension..];
    }
}
