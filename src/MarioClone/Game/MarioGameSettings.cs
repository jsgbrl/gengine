// Everything the game needs from outside, in one object. It is a settings object rather than a
// seven-argument constructor for the reason section 3 gives: four parameters is the limit, and
// the seventh argument of a constructor is where a bug hides in plain sight.

using GEngine.Core.Contracts;
using GEngine.Input.Actions;
using GEngine.Input.Gamepad;
using MarioClone.Audio;

namespace MarioClone.Game;

/// <summary>What the game is wired to.</summary>
public sealed class MarioGameSettings
{
    /// <summary>Where frames go.</summary>
    public required IRenderer Renderer { get; init; }

    /// <summary>Where input comes from, and which source is answering.</summary>
    public required InputRouter Router { get; init; }

    /// <summary>Where sprites and levels come from.</summary>
    public required IAssetSource Assets { get; init; }

    /// <summary>Where sound goes. Silent unless something else is passed.</summary>
    public IAudioBackend Audio { get; init; } = NullAudioBackend.Instance;

    /// <summary>The key bindings, so the title screen can name the real keys.</summary>
    public InputMap KeyboardMap { get; init; } = InputMap.CreateDefault();

    /// <summary>The button bindings, so the title screen can name the real buttons.</summary>
    public GamepadMap GamepadMap { get; init; } = GamepadMap.CreateDefault();

    /// <summary>Which level to load.</summary>
    public string LevelPath { get; init; } = "levels/1-1.txt";

    /// <summary>What to call the world on the display.</summary>
    public string World { get; init; } = "1-1";

    /// <summary>When true the game starts in the level rather than on the title screen.</summary>
    public bool StartsImmediately { get; init; }
}
