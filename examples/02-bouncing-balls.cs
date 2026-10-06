#:project ../src/GEngine.Core/GEngine.Core.csproj
#:project ../src/GEngine.Physics/GEngine.Physics.csproj
#:project ../src/GEngine.Rendering/GEngine.Rendering.csproj

// 02 - physics with no game around it, in colour.
//
// Run it:  dotnet run examples/02-bouncing-balls.cs
//
// Six balls in a closed room, each with a different restitution: the one on the left keeps
// none of its speed on a bounce and is asleep within a second, the one on the right keeps
// almost all of it. Nothing here is random, so the same run produces the same frames - the
// state hash printed at the end is the same on every machine and every run.
//
// What to look at: the picture is drawn into a FrameBuffer that knows nothing about
// terminals, and the ConsoleRenderer turns it into half-block characters. Two pixels per
// character cell, one write per frame, and only the cells that changed.

using System;
using GEngine.Core;
using GEngine.Core.Contracts;
using GEngine.Core.Loop;
using GEngine.Core.Platform;
using GEngine.Core.Time;
using GEngine.Physics;
using GEngine.Physics.BroadPhase;
using GEngine.Rendering;

var logger = new MemoryLogger();
using ConsoleSession session = ConsoleSession.Start(SystemPlatformProbe.Instance, logger);

var demo = new BouncingBalls(session, logger);
var loop = new GameLoop(new StopwatchClock(), demo, GameLoopSettings.Default);
demo.Attach(loop);
loop.Run();
session.Dispose();

foreach (string line in logger.Messages)
{
    Console.WriteLine(line);
}

Console.WriteLine($"state hash after {demo.Steps} steps: {demo.StateHash:X16}");
Console.WriteLine("run it again: the hash is the same, because nothing in here reads a clock or a random number.");
return 0;

/// <summary>Six balls, a room, and a frame buffer.</summary>
internal sealed class BouncingBalls : IGame
{
    private const float RunSeconds = 8.0f;
    private const float WallThickness = 6.0f;

    private static readonly Color[] BallColors =
    [
        Palette.Grey, Palette.LightRed, Palette.Orange, Palette.Yellow, Palette.LightGreen, Palette.White,
    ];

    private readonly float[] _restitutions = [0.0f, 0.2f, 0.4f, 0.6f, 0.8f, 0.95f];
    private readonly ConsoleSession _session;
    private readonly ILogger _logger;
    private readonly PhysicsWorld _world;
    private readonly RigidBody2D[] _balls;
    private readonly FrameBuffer _frame;
    private GameLoop? _loop;
    private float _elapsedSeconds;

    public BouncingBalls(ConsoleSession session, ILogger logger)
    {
        _session = session;
        _logger = logger;
        _frame = new FrameBuffer(session.Renderer.Width, session.Renderer.Height);
        _world = new PhysicsWorld(PhysicsSettings.Default, new SpatialHashGrid());
        AddWalls();
        _balls = AddBalls();
    }

    public long Steps => _world.StepCount;

    public long StateHash => _world.StateHash();

    public void Attach(GameLoop loop) => _loop = loop;

    public void Update(float deltaSeconds)
    {
        _elapsedSeconds += deltaSeconds;
        if (_elapsedSeconds >= RunSeconds || _session.IsCancelled || IsEscapePressed())
        {
            _loop?.Stop();
        }
    }

    public void FixedUpdate(float fixedDeltaSeconds) => _world.Step(fixedDeltaSeconds);

    public void Render(float interpolation)
    {
        _frame.Resize(_session.Renderer.Width, _session.Renderer.Height);
        _frame.Clear(Palette.DeepBlue);
        foreach (RigidBody2D body in _world.Bodies)
        {
            _frame.DrawRectangle(body.Bounds, ColorOf(body));
        }

        _frame.DrawText("RESTITUTION 0.00 0.20 0.40 0.60 0.80 0.95", new Vector2(2.0f, 2.0f), Palette.White);
        _frame.DrawText("ESC OR CTRL+C TO STOP", new Vector2(2.0f, 11.0f), Palette.Grey);
        _session.Renderer.Present(_frame);
    }

    // KeyAvailable throws outright when standard input is a file rather than a terminal,
    // which is what happens the moment anyone pipes the output of this example anywhere.
    private static bool IsEscapePressed()
    {
        if (Console.IsInputRedirected)
        {
            return false;
        }

        return Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Escape;
    }

    private Color ColorOf(RigidBody2D body)
    {
        for (int index = 0; index < _balls.Length; index++)
        {
            if (ReferenceEquals(_balls[index], body))
            {
                return BallColors[index];
            }
        }

        return Palette.Brown;
    }

    private void AddWalls()
    {
        float width = _frame.Width;
        float height = _frame.Height;
        _logger.Info($"room is {width} by {height} pixels");
        Add(BodyType.Static, new Vector2(width / 2.0f, height), new Vector2(width, WallThickness));
        Add(BodyType.Static, new Vector2(0.0f, height / 2.0f), new Vector2(WallThickness, height));
        Add(BodyType.Static, new Vector2(width, height / 2.0f), new Vector2(WallThickness, height));
    }

    private RigidBody2D[] AddBalls()
    {
        var balls = new RigidBody2D[_restitutions.Length];
        float spacing = _frame.Width / (_restitutions.Length + 1.0f);
        for (int index = 0; index < balls.Length; index++)
        {
            balls[index] = Add(
                BodyType.Dynamic,
                new Vector2(spacing * (index + 1), 20.0f),
                new Vector2(4.0f, 4.0f));
            balls[index].Restitution = _restitutions[index];
        }

        return balls;
    }

    private RigidBody2D Add(BodyType type, Vector2 position, Vector2 size) =>
        _world.Add(new RigidBody2D(type, position, size));
}
