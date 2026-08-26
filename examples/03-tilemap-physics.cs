#:project ../src/GEngine.Core/GEngine.Core.csproj
#:project ../src/GEngine.Physics/GEngine.Physics.csproj
#:project ../src/GEngine.Rendering/GEngine.Rendering.csproj
#:project ../src/GEngine.Input/GEngine.Input.csproj

// 03 - a square that walks and jumps on a tilemap.
//
// Run it:   dotnet run examples/03-tilemap-physics.cs
// Watch it: dotnet run examples/03-tilemap-physics.cs -- --demo
//
// Arrows or A and D to walk, space or Z to jump, Esc to stop. The --demo flag replays a
// recorded script instead of reading the keyboard, which is how this example is checked in a
// build and how the Mario replay test works.
//
// What to look at: the game never mentions a key. It asks whether Jump is down and how far
// MoveRight is being pushed, and the router puts the answer there - from a keyboard, from a
// script, and after example 04 from a DualSense, without the game knowing which.

using System;
using GEngine.Core;
using GEngine.Core.Contracts;
using GEngine.Core.Loop;
using GEngine.Core.Platform;
using GEngine.Core.Time;
using GEngine.Input.Actions;
using GEngine.Input.Keyboard;
using GEngine.Input.Scripted;
using GEngine.Physics;
using GEngine.Physics.BroadPhase;
using GEngine.Physics.Tiles;
using GEngine.Rendering;

bool isDemo = Array.IndexOf(args, "--demo") >= 0;
var logger = new MemoryLogger();
using ConsoleSession session = ConsoleSession.Start(SystemPlatformProbe.Instance, logger);

var walker = new TileWalker(session, isDemo);
var loop = new GameLoop(new StopwatchClock(), walker, GameLoopSettings.Default);
walker.Attach(loop);
loop.Run();
session.Dispose();

foreach (string line in logger.Messages)
{
    Console.WriteLine(line);
}

Console.WriteLine($"walked to x={walker.PlayerX:0} after {walker.Steps} fixed steps");
return 0;

/// <summary>A tilemap, a box that walks on it, and a camera that follows.</summary>
internal sealed class TileWalker : IGame
{
    private const float TileSize = 8.0f;
    private const float GravityPixelsPerSecondSquared = 320.0f;
    private const float WalkSpeedPixelsPerSecond = 46.0f;
    private const float RunSpeedPixelsPerSecond = 76.0f;
    private const float JumpVelocityPixelsPerSecond = -118.0f;
    private const float JumpCutFactor = 0.45f;
    private const float DemoSeconds = 6.0f;

    private static readonly string[] LevelRows =
    [
        "............................................................",
        "............................................................",
        ".......###..........####.......###...........###............",
        "..................................................######....",
        "....###.........###.......###...........###.................",
        "############################################################",
    ];

    private readonly ConsoleSession _session;
    private readonly InputState _input = new();
    private readonly InputRouter _router;
    private readonly PhysicsWorld _world;
    private readonly TileCollisionSource _tiles;
    private readonly RigidBody2D _player;
    private readonly Camera2D _camera;
    private readonly FrameBuffer _frame;
    private readonly bool _isDemo;
    private GameLoop? _loop;
    private float _elapsedSeconds;

    public TileWalker(ConsoleSession session, bool isDemo)
    {
        _session = session;
        _isDemo = isDemo;
        _frame = new FrameBuffer(session.Renderer.Width, session.Renderer.Height);
        _camera = new Camera2D(new Vector2(_frame.Width, _frame.Height))
        {
            DeadZone = new Vector2(_frame.Width * 0.2f, _frame.Height),
            LevelBounds = new Aabb(
                Vector2.Zero,
                new Vector2(LevelRows[0].Length * TileSize, LevelRows.Length * TileSize)),
        };

        _tiles = BuildTiles();
        _world = new PhysicsWorld(
            new PhysicsSettings { Gravity = new Vector2(0.0f, GravityPixelsPerSecondSquared) },
            new SpatialHashGrid(32.0f))
        {
            Tiles = _tiles,
        };

        _player = _world.Add(new RigidBody2D(BodyType.Dynamic, new Vector2(20.0f, 20.0f), new Vector2(6.0f, 6.0f)));
        _player.MaxVelocity = new Vector2(400.0f, 400.0f);
        _router = new InputRouter(_input);
        _router.Add(isDemo
            ? new FakeInputBackend(DemoScript())
            : new ConsoleKeyboardBackend(ConsoleKeyReader.Instance, InputMap.CreateDefault()));
    }

    public float PlayerX => _player.Position.X;

    public long Steps => _world.StepCount;

    public void Attach(GameLoop loop) => _loop = loop;

    public void Update(float deltaSeconds)
    {
        _elapsedSeconds += deltaSeconds;
        _router.Poll(deltaSeconds);
        if (_input.IsDown(InputAction.Cancel) || _session.IsCancelled || IsDemoOver())
        {
            _loop?.Stop();
        }
    }

    public void FixedUpdate(float fixedDeltaSeconds)
    {
        Walk();
        Jump();
        _world.Step(fixedDeltaSeconds);
        _camera.Follow(_player.Position);
    }

    public void Render(float interpolation)
    {
        _frame.Resize(_session.Renderer.Width, _session.Renderer.Height);
        _frame.Clear(Palette.Sky);
        DrawTiles();
        _frame.DrawRect(Screen(_player.Bounds), Palette.Red);
        DrawHud();
        _session.Renderer.Present(_frame);
    }

    // Right for five seconds, with a jump roughly every second. Enough to clear the first
    // ledges, and identical on every run.
    private static InputScript DemoScript()
    {
        var script = new InputScript().Hold(InputAction.MoveRight, 0, 330).Hold(InputAction.Run, 120, 330);
        for (int jump = 0; jump < 6; jump++)
        {
            script.Hold(InputAction.Jump, 40 + (jump * 55), 52 + (jump * 55));
        }

        return script;
    }

    private bool IsDemoOver() => _isDemo && _elapsedSeconds >= DemoSeconds;

    private TileCollisionSource BuildTiles()
    {
        var tiles = new TileCollisionSource(LevelRows[0].Length, LevelRows.Length, TileSize);
        for (int row = 0; row < LevelRows.Length; row++)
        {
            for (int column = 0; column < LevelRows[row].Length; column++)
            {
                tiles.Set(column, row, LevelRows[row][column] == '#' ? TileCollision.Solid : TileCollision.None);
            }
        }

        return tiles;
    }

    private void Walk()
    {
        float top = _input.IsDown(InputAction.Run) ? RunSpeedPixelsPerSecond : WalkSpeedPixelsPerSecond;
        float direction = _input.AxisValue(InputAction.MoveRight) - _input.AxisValue(InputAction.MoveLeft);
        _player.Velocity = _player.Velocity.WithX(direction * top);
    }

    // Variable jump height in six lines: the ascent starts at full speed and is cut short the
    // moment the button comes up. Holding longer means higher, and nothing else changes.
    private void Jump()
    {
        if (_input.WasPressedThisFrame(InputAction.Jump) && _player.IsGrounded)
        {
            _player.Velocity = _player.Velocity.WithY(JumpVelocityPixelsPerSecond);
        }

        if (_input.WasReleasedThisFrame(InputAction.Jump) && _player.Velocity.Y < 0.0f)
        {
            _player.Velocity = _player.Velocity.WithY(_player.Velocity.Y * JumpCutFactor);
        }
    }

    private Aabb Screen(Aabb world) =>
        new(_camera.WorldToScreen(world.Min), _camera.WorldToScreen(world.Max));

    private void DrawTiles()
    {
        for (int row = 0; row < _tiles.Rows; row++)
        {
            for (int column = 0; column < _tiles.Columns; column++)
            {
                DrawTile(column, row);
            }
        }
    }

    private void DrawTile(int column, int row)
    {
        if (_tiles.At(column, row) != TileCollision.Solid)
        {
            return;
        }

        Aabb bounds = _tiles.BoundsOf(column, row);
        if (!_camera.IsVisible(bounds))
        {
            return;
        }

        _frame.DrawRect(Screen(bounds), row == _tiles.Rows - 1 ? Palette.Brown : Palette.LightBrown);
    }

    private void DrawHud()
    {
        string state = _player.IsGrounded ? "GROUNDED" : "IN AIR";
        _frame.DrawText($"X {_player.Position.X:000} {state}", new Vector2(2.0f, 2.0f), Palette.White);
        _frame.DrawText(_isDemo ? "DEMO" : "ARROWS JUMP SPACE QUIT ESC", new Vector2(2.0f, 11.0f), Palette.DarkGrey);
    }
}
