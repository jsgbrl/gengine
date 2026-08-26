// The game, as the loop sees it: three callbacks and a state machine.
//
// Update runs at whatever rate the machine manages and handles input and the states that are
// really just waiting. FixedUpdate runs sixty times a second and is where the level actually
// happens - which is why a replay is possible at all. Render draws and changes nothing.

using System;
using GEngine.Core;
using GEngine.Core.Loop;
using GEngine.Core.Patterns;
using GEngine.Input.Actions;
using GEngine.Rendering;
using MarioClone.Levels;
using MarioClone.View;

namespace MarioClone.Game;

/// <summary>The Mario clone: one level, six states, and a heads-up display.</summary>
public sealed partial class MarioGame : IGame
{
    /// <summary>How long the death and level-complete screens hold before moving on, in seconds.</summary>
    public const float PauseBeforeContinuingSeconds = 2.0f;

    private readonly MarioGameSettings _settings;
    private readonly StateMachine<GameStateKind> _states;
    private readonly SpriteAtlas _atlas;
    private readonly LevelView _levelView;
    private readonly TitleScreen _titleScreen;
    private readonly InputState _input;
    private readonly FrameBuffer _frame;
    private readonly string _levelText;
    private Camera2D _camera;
    private MarioWorld _world;

    /// <summary>Creates the game and loads the level.</summary>
    /// <param name="settings">Everything it needs from outside: a renderer, input, assets, audio.</param>
    public MarioGame(MarioGameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        _settings = settings;
        _input = settings.Router.State;
        _atlas = new SpriteAtlas(settings.Assets);
        _atlas.LoadAll();
        _levelView = new LevelView(_atlas);
        _titleScreen = new TitleScreen(settings.KeyboardMap, settings.GamepadMap);
        _frame = new FrameBuffer(Math.Max(1, settings.Renderer.Width), Math.Max(1, settings.Renderer.Height));
        _levelText = settings.Assets.ReadText(settings.LevelPath);
        Session = new GameSession { World = settings.World };
        _world = BuildWorld();
        _camera = BuildCamera();
        _states = BuildStates();
    }

    /// <summary>The score, coins and lives of this run.</summary>
    public GameSession Session { get; }

    /// <summary>Where the game is.</summary>
    public GameStateKind State => _states.CurrentState;

    /// <summary>The level currently loaded and everything in it.</summary>
    public MarioWorld World => _world;

    /// <summary>What part of the level is on screen.</summary>
    public Camera2D Camera => _camera;

    /// <summary>True once the player has asked to leave. The loop stops on the next frame.</summary>
    public bool IsFinished { get; private set; }

    /// <inheritdoc/>
    public void Update(float deltaSeconds)
    {
        _settings.Router.Poll(deltaSeconds);
        _states.Update(deltaSeconds);
        UpdateState();
    }

    /// <inheritdoc/>
    public void FixedUpdate(float fixedDeltaSeconds)
    {
        if (State != GameStateKind.Playing)
        {
            return;
        }

        _world.Step(fixedDeltaSeconds);
        Session.Tick(fixedDeltaSeconds);
        FollowPlayer();
        CheckForAnEnding();
    }

    /// <inheritdoc/>
    public void Render(float interpolation)
    {
        Resize();
        DrawState();
        _settings.Renderer.Present(_frame);
    }

    /// <summary>Asks the game to stop after this frame.</summary>
    public void Quit() => IsFinished = true;

    private MarioWorld BuildWorld()
    {
        Level level = LevelLoader.Parse(_levelText, MarioFactory.TileSizePixels);
        return MarioFactory.Build(level, _input, Session, _settings.Audio);
    }

    // The dead zone is wide and short: the camera should follow a run without twitching, and
    // should follow a jump immediately, because a player who cannot see where they will land
    // will not jump.
    private Camera2D BuildCamera()
    {
        var camera = new Camera2D(new Vector2(_frame.Width, _frame.Height))
        {
            DeadZone = new Vector2(_frame.Width * 0.18f, _frame.Height * 0.12f),
            LevelBounds = _world.Level.Bounds,
            Scroll = CameraScroll.ForwardOnly,
        };

        camera.SnapTo(_world.Level.PlayerStart);
        return camera;
    }

    private void Resize()
    {
        if (_frame.Width == _settings.Renderer.Width && _frame.Height == _settings.Renderer.Height)
        {
            return;
        }

        _frame.Resize(Math.Max(1, _settings.Renderer.Width), Math.Max(1, _settings.Renderer.Height));
        _camera = BuildCamera();
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        if (_world.Player is not null)
        {
            _camera.Follow(_world.Player.Position);
        }
    }
}
