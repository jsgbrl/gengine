// Flyweight.
//
// A sprite is loaded from its text file once and then shared by everything that draws it: a
// hundred coins on screen are a hundred references to one five-by-five array, not a hundred
// copies of it. Mirrored versions are built once too, on the first request, because a character
// that walks both ways needs exactly two of them and never a third.

using System;
using System.Collections.Generic;
using GEngine.Core.Contracts;

namespace GEngine.Rendering;

/// <summary>Loads sprites from an asset source and hands out shared instances.</summary>
public sealed class SpriteAtlas
{
    private readonly Dictionary<string, Sprite> _sprites = [];
    private readonly Dictionary<string, Sprite> _mirrored = [];
    private readonly IAssetSource _assets;
    private readonly string _folder;

    /// <summary>Creates an atlas over an asset source.</summary>
    /// <param name="assets">Where the sprite files come from.</param>
    /// <param name="folder">The folder they live in, without a trailing slash.</param>
    public SpriteAtlas(IAssetSource assets, string folder = "assets")
    {
        ArgumentNullException.ThrowIfNull(assets);
        ArgumentNullException.ThrowIfNull(folder);
        _assets = assets;
        _folder = folder;
    }

    /// <summary>How many sprites have been loaded so far.</summary>
    public int LoadedCount => _sprites.Count;

    /// <summary>The asset path a sprite name maps to.</summary>
    /// <param name="name">The sprite name, without a folder or an extension.</param>
    /// <returns>The path.</returns>
    public string PathOf(string name) => _folder + "/" + name + ".sprite";

    /// <summary>Loads a sprite, or hands back the one already loaded.</summary>
    /// <param name="name">The sprite name, without a folder or an extension.</param>
    /// <returns>The shared sprite.</returns>
    /// <exception cref="KeyNotFoundException">No such sprite exists.</exception>
    public Sprite Get(string name)
    {
        if (TryGet(name, out Sprite? sprite) && sprite is not null)
        {
            return sprite;
        }

        throw new KeyNotFoundException("no sprite called " + name + " in " + _assets.Name);
    }

    /// <summary>Loads a sprite without failing when it is missing.</summary>
    /// <param name="name">The sprite name.</param>
    /// <param name="sprite">The shared sprite, or null.</param>
    /// <returns>True when it exists.</returns>
    public bool TryGet(string name, out Sprite? sprite)
    {
        if (_sprites.TryGetValue(name, out sprite))
        {
            return true;
        }

        string path = PathOf(name);
        if (!_assets.Exists(path))
        {
            sprite = null;
            return false;
        }

        sprite = SpriteParser.Parse(name, _assets.ReadText(path));
        _sprites[name] = sprite;
        return true;
    }

    /// <summary>The same sprite with its columns reversed, built once and then shared.</summary>
    /// <param name="name">The sprite name.</param>
    /// <returns>The shared mirrored sprite.</returns>
    public Sprite GetMirrored(string name)
    {
        if (_mirrored.TryGetValue(name, out Sprite? mirrored))
        {
            return mirrored;
        }

        mirrored = Get(name).Mirrored();
        _mirrored[name] = mirrored;
        return mirrored;
    }

    /// <summary>Loads every sprite the source holds, so nothing is loaded mid-frame.</summary>
    /// <returns>How many were loaded.</returns>
    public int LoadAll()
    {
        foreach (string path in _assets.ListPaths(_folder + "/"))
        {
            if (path.EndsWith(".sprite", StringComparison.Ordinal))
            {
                Get(path[(_folder.Length + 1)..^".sprite".Length]);
            }
        }

        return _sprites.Count;
    }
}
