// A fixed set of colours, named. Flyweight: a sprite stores a character per pixel and the
// legend turns it into one of these, so a thousand red pixels are a thousand references to
// one value rather than a thousand copies of a triple.

using System.Collections.Generic;
using GEngine.Core;

namespace GEngine.Rendering;

/// <summary>The colours the game draws with, in the spirit of the NES palette.</summary>
public static class Palette
{
    /// <summary>Nothing. A frame buffer clears to this.</summary>
    public static Color Transparent => Color.Transparent;

    /// <summary>Black.</summary>
    public static Color Black { get; } = new(0, 0, 0);

    /// <summary>White.</summary>
    public static Color White { get; } = new(252, 252, 252);

    /// <summary>The sky of world 1-1.</summary>
    public static Color Sky { get; } = new(92, 148, 252);

    /// <summary>Deep blue, for night and for water.</summary>
    public static Color DeepBlue { get; } = new(0, 0, 168);

    /// <summary>The red of a cap and a shirt.</summary>
    public static Color Red { get; } = new(216, 40, 0);

    /// <summary>A lighter red, for highlights.</summary>
    public static Color LightRed { get; } = new(248, 88, 56);

    /// <summary>Skin.</summary>
    public static Color Skin { get; } = new(252, 188, 140);

    /// <summary>The brown of a brick and of boots.</summary>
    public static Color Brown { get; } = new(136, 20, 0);

    /// <summary>A lighter brown, for the top of a block.</summary>
    public static Color LightBrown { get; } = new(200, 76, 12);

    /// <summary>The orange of a question block.</summary>
    public static Color Orange { get; } = new(228, 92, 16);

    /// <summary>The yellow of a coin.</summary>
    public static Color Yellow { get; } = new(252, 224, 68);

    /// <summary>Pipe green.</summary>
    public static Color Green { get; } = new(0, 168, 0);

    /// <summary>A lighter green, for the rim of a pipe.</summary>
    public static Color LightGreen { get; } = new(88, 216, 84);

    /// <summary>The grey of stone and of a HUD frame.</summary>
    public static Color Grey { get; } = new(188, 188, 188);

    /// <summary>A darker grey, for shadow.</summary>
    public static Color DarkGrey { get; } = new(96, 96, 96);

    /// <summary>Every named colour, in declaration order.</summary>
    public static IReadOnlyList<Color> All { get; } =
    [
        Black,
        White,
        Sky,
        DeepBlue,
        Red,
        LightRed,
        Skin,
        Brown,
        LightBrown,
        Orange,
        Yellow,
        Green,
        LightGreen,
        Grey,
        DarkGrey,
    ];

    /// <summary>Looks a colour up by its name, ignoring case.</summary>
    /// <param name="name">Name of the colour, such as Sky.</param>
    /// <param name="color">The colour, when the name is known.</param>
    /// <returns>True when the name is one of the palette's.</returns>
    public static bool TryGetByName(string name, out Color color)
    {
        foreach (KeyValuePair<string, Color> entry in ByName)
        {
            if (string.Equals(entry.Key, name, System.StringComparison.OrdinalIgnoreCase))
            {
                color = entry.Value;
                return true;
            }
        }

        color = Color.Transparent;
        return false;
    }

    private static Dictionary<string, Color> ByName { get; } = new()
    {
        ["transparent"] = Color.Transparent,
        ["black"] = Black,
        ["white"] = White,
        ["sky"] = Sky,
        ["deepblue"] = DeepBlue,
        ["red"] = Red,
        ["lightred"] = LightRed,
        ["skin"] = Skin,
        ["brown"] = Brown,
        ["lightbrown"] = LightBrown,
        ["orange"] = Orange,
        ["yellow"] = Yellow,
        ["green"] = Green,
        ["lightgreen"] = LightGreen,
        ["grey"] = Grey,
        ["darkgrey"] = DarkGrey,
    };
}
