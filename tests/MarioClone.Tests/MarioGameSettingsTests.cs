using GEngine.Input.Actions;
using GEngine.Input.Gamepad;
using GEngine.Rendering;
using GEngine.Rendering.Assets;
using GEngine.Testing;
using MarioClone.Audio;
using MarioClone.Game;

namespace MarioClone.Tests;

/// <summary>
/// Everything the game needs from outside, in one object with three required members and six
/// defaults. It is a settings object rather than a nine-argument constructor because the
/// parameter budget is four, and because a caller that only wants to change the level should
/// not have to restate the renderer.
/// </summary>
public sealed class MarioGameSettingsTests
{
    [Test]
    public void TheThreeThingsAGameCannotInventForItselfAreRequired()
    {
        MarioGameSettings settings = Minimum();
        Assert.IsNotNull(settings.Renderer);
        Assert.IsNotNull(settings.Router);
        Assert.IsNotNull(settings.Assets);
    }

    // Silence, the default bindings, world 1-1 and the title screen. A game built with the
    // three required members and nothing else is the game as it ships.
    [Test]
    public void EverythingElseHasADefaultThatIsTheShippedGame()
    {
        MarioGameSettings settings = Minimum();
        Assert.AreSame(NullAudioBackend.Instance, settings.Audio);
        Assert.AreEqual("levels/1-1.txt", settings.LevelPath);
        Assert.AreEqual("1-1", settings.World);
        Assert.IsFalse(settings.StartsImmediately, "the shipped game opens on the title screen");
        Assert.IsNotNull(settings.KeyboardMap);
        Assert.IsNotNull(settings.GamepadMap);
    }

    [Test]
    public void EachDefaultCanBeReplacedWithoutRestatingTheRest()
    {
        MarioGameSettings settings = With(new MarioGameSettings
        {
            Renderer = new HeadlessRenderer(8, 8),
            Router = Router(),
            Assets = Assets(),
            Audio = new MemoryAudioBackend(),
            LevelPath = "levels/other.txt",
            World = "1-2",
            StartsImmediately = true,
        });

        Assert.AreEqual("levels/other.txt", settings.LevelPath);
        Assert.AreEqual("1-2", settings.World);
        Assert.IsTrue(settings.StartsImmediately);
        Assert.AreEqual("memory", settings.Audio.Name);
    }

    // The maps are what the title screen reads to name the real keys and buttons, so a game
    // with rebound controls must be able to hand over the bindings it actually uses.
    [Test]
    public void TheBindingsAreSettableSoTheTitleScreenCanNameTheRealKeys()
    {
        var keyboard = new InputMap();
        var gamepad = new GamepadMap();
        MarioGameSettings settings = With(new MarioGameSettings
        {
            Renderer = new HeadlessRenderer(8, 8),
            Router = Router(),
            Assets = Assets(),
            KeyboardMap = keyboard,
            GamepadMap = gamepad,
        });

        Assert.AreSame(keyboard, settings.KeyboardMap);
        Assert.AreSame(gamepad, settings.GamepadMap);
    }

    private static MarioGameSettings Minimum() => With(new MarioGameSettings
    {
        Renderer = new HeadlessRenderer(8, 8),
        Router = Router(),
        Assets = Assets(),
    });

    private static MarioGameSettings With(MarioGameSettings settings) => settings;

    private static InputRouter Router() => new(new InputState());

    private static EmbeddedAssetSource Assets() => new EmbeddedAssetSource(typeof(MarioGame).Assembly);
}
