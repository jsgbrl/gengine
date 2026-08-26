// Where sprites and levels come from. Text in, nothing else: an asset in gengine is a
// file you can open in a notepad, which is the whole reason a reader can change the level
// without a tool.

using System.Collections.Generic;

namespace GEngine.Core.Contracts;

/// <summary>A read-only source of text assets addressed by path.</summary>
public interface IAssetSource
{
    /// <summary>Human-readable name of the source, for error messages.</summary>
    string Name { get; }

    /// <summary>Whether an asset exists.</summary>
    /// <param name="path">Forward-slash separated path, such as assets/mario.sprite.</param>
    /// <returns>True when it can be read.</returns>
    bool Exists(string path);

    /// <summary>Reads an asset.</summary>
    /// <param name="path">Forward-slash separated path.</param>
    /// <returns>The contents.</returns>
    string ReadText(string path);

    /// <summary>Lists the assets under a prefix, sorted, so a listing is reproducible.</summary>
    /// <param name="prefix">Path prefix, or an empty string for everything.</param>
    /// <returns>The matching paths.</returns>
    IReadOnlyList<string> ListPaths(string prefix);
}
