# Tutorial: a game from nothing

Five steps, each of which runs. By the end you have a square that walks, jumps, and lands on
platforms, in about eighty lines — and, more usefully, you know which eighty lines the engine
expects and why.

Every step is a **file-based app**: one `.cs` file with `#:project` lines at the top, run with
`dotnet run`. No project file, no build step. Save each one in the repository root.

## Step 1: something on the screen

```csharp
#:project src/GEngine.Core/GEngine.Core.csproj
#:project src/GEngine.Rendering/GEngine.Rendering.csproj

using GEngine.Core;
using GEngine.Rendering;

var frame = new FrameBuffer(60, 30);
frame.Clear(Palette.DeepBlue);
frame.DrawRectangle(Aabb.FromCenterSize(new Vector2(30.0f, 15.0f), new Vector2(8.0f, 8.0f)), Palette.Yellow);
Console.WriteLine(AsciiSnapshot.Capture(frame));
```

```bash
dotnet run step1.cs
```

A `FrameBuffer` is a rectangle of pixels. Not characters — **pixels**: this one is 60 wide and 30
tall, and on a real terminal it would occupy 60 columns and 15 rows, because each cell holds two
pixels stacked (see [rendering.md](rendering.md)).

`AsciiSnapshot` prints it as characters by brightness. It is how the rendering tests assert on
pictures, and it is the quickest way to see something without taking over the terminal.

## Step 2: on the terminal, in colour, sixty times a second

```csharp
#:project src/GEngine.Core/GEngine.Core.csproj
#:project src/GEngine.Rendering/GEngine.Rendering.csproj

using GEngine.Core;
using GEngine.Core.Contracts;
using GEngine.Core.Loop;
using GEngine.Core.Platform;
using GEngine.Core.Time;
using GEngine.Rendering;

using ConsoleSession session = ConsoleSession.Start(SystemPlatformProbe.Instance, NullLogger.Instance);
var game = new Bouncer(session);
var loop = new GameLoop(new StopwatchClock(), game, GameLoopSettings.Default);
game.Attach(loop);
loop.Run();

internal sealed class Bouncer : IGame
{
    private readonly ConsoleSession _session;
    private readonly FrameBuffer _frame;
    private GameLoop? _loop;
    private float _seconds;

    public Bouncer(ConsoleSession session)
    {
        _session = session;
        _frame = new FrameBuffer(session.Renderer.Width, session.Renderer.Height);
    }

    public void Attach(GameLoop loop) => _loop = loop;

    public void Update(float deltaSeconds)
    {
        _seconds += deltaSeconds;
        if (_seconds > 5.0f || _session.IsCancelled)
        {
            _loop?.Stop();
        }
    }

    public void FixedUpdate(float fixedDeltaSeconds)
    {
    }

    public void Render(float interpolation)
    {
        float y = (_frame.Height / 2.0f) + (MathF.Sin(_seconds * 3.0f) * (_frame.Height / 3.0f));
        _frame.Clear(Palette.DeepBlue);
        _frame.DrawRectangle(Aabb.FromCenterSize(new Vector2(_frame.Width / 2.0f, y), new Vector2(6.0f, 6.0f)), Palette.Yellow);
        _session.Renderer.Present(_frame);
    }
}
```

Three things arrived at once.

**`ConsoleSession`** takes over the terminal — alternate screen, hidden cursor, virtual terminal
processing on Windows — and puts every bit of it back when disposed, including on Ctrl-C. It picks
the right driver for the system it is on and announces the colour depth it found.

**`IGame`** is the three callbacks. `Update` runs once per frame at whatever rate the machine
manages; `FixedUpdate` runs at exactly 1/60 s; `Render` draws and changes nothing.

**`GameLoop`** does the accumulating, the interpolation and the frame pacing.
[The README explains what it is doing](../README.md#how-the-game-loop-works); the reason it takes an
`IClock` is that a test hands it a `ManualClock` and says "advance one second", and gets exactly
sixty fixed steps.

Notice that the square's position is computed in `Render` from elapsed time. That is fine for a
sine wave and wrong for a game, which is step 3.

## Step 3: physics

```csharp
#:project src/GEngine.Core/GEngine.Core.csproj
#:project src/GEngine.Physics/GEngine.Physics.csproj
#:project src/GEngine.Rendering/GEngine.Rendering.csproj

// … the same shell as step 2, with these changes:

private readonly PhysicsWorld _world = new(PhysicsSettings.Default, new BruteForceBroadPhase())
{
    Gravity = new Vector2(0.0f, 300.0f),
};

private readonly RigidBody2D _ball = new(BodyType.Dynamic, new Vector2(30.0f, 4.0f), new Vector2(6.0f, 6.0f))
{
    Restitution = 0.8f,
};

// in the constructor:
_world.Add(_ball);
_world.Add(new RigidBody2D(BodyType.Static, new Vector2(30.0f, 40.0f), new Vector2(60.0f, 4.0f)));

// FixedUpdate, not Update:
public void FixedUpdate(float fixedDeltaSeconds) => _world.Step(fixedDeltaSeconds);

// Render reads, and changes nothing:
_frame.DrawRectangle(_ball.Bounds, Palette.Yellow);
```

The move from `Update` to `FixedUpdate` is the whole point of this step. Physics in `Update` means
the ball bounces higher on a faster computer, and a recorded run is unreproducible. Physics in
`FixedUpdate` always advances by exactly 1/60 s, whatever the frame rate, which is what makes
`ReplayTests` possible.

`Restitution` is bounciness: 0 stops dead, 1 returns the speed it arrived with. Below
`RestingSpeedPixelsPerSecond` a bounce becomes a stop, which is what keeps the ball from shivering on
the floor for ever.

## Step 4: a keyboard, and actions

```csharp
#:project src/GEngine.Input/GEngine.Input.csproj

using GEngine.Input.Actions;
using GEngine.Input.Keyboard;

// in the constructor:
_input = new InputState();
_router = new InputRouter(_input);
_router.Add(new ConsoleKeyboardBackend(new ConsoleKeyReader(), InputMap.CreateDefault()));

// in Update:
_router.Poll(deltaSeconds);

// in FixedUpdate:
float direction = _input.AxisValue(InputAction.MoveRight) - _input.AxisValue(InputAction.MoveLeft);
_ball.Velocity = _ball.Velocity.WithX(direction * 60.0f);

if (_input.WasPressedThisFrame(InputAction.Jump) && _ball.IsGrounded)
{
    _ball.Velocity = _ball.Velocity.WithY(-160.0f);
}
```

The game asks about `MoveRight` and `Jump`. It never mentions a key.

That indirection is what makes plugging in a DualSense — step 5 — a two-line change rather than a
rewrite. `InputRouter` merges every source by strongest report, so a keyboard and a controller work
at the same time and either can be unplugged mid-game.

`AxisValue` is analogue: 1.0 from a key, anything from a stick. `IsDown` is the digital question and
`WasPressedThisFrame` is the **edge** — the difference between "jump held" and "jump pressed", which
is the difference between jumping once and jumping every frame you hold the button.

Remember that a terminal never reports a key coming *up*. `ConsoleKeyboardBackend` decays an action
0.2 s after its last press; see [input-and-gamepad.md](input-and-gamepad.md#the-keyboard-problem).

## Step 5: a gamepad, in two lines

```csharp
using GEngine.Core.Contracts;
using GEngine.Input.Gamepad;
using GEngine.Input.Hid;

_router.Add(new DualSenseGamepad(HidBackendFactory.Create(SystemPlatformProbe.Instance),
    GamepadMap.CreateDefault(), NullLogger.Instance));
```

That is the whole change. The game code above is untouched, because it was never about keys.

`HidBackendFactory` picks the Windows, macOS or Linux HID backend at run time. `DualSenseGamepad`
rescans for a controller once a second while none is attached, so plugging one in mid-game just
starts working, and unplugging one hands control back to the keyboard rather than freezing.

## What to read next

You now have the shape of every example in `examples/`, and of `MarioClone` itself. What the game
adds on top:

| | where | what it adds |
|---|---|---|
| a tilemap | `MarioFactory.BuildTiles` | a level as a grid, not a thousand bodies |
| a camera | `Camera2D` | a dead zone so the view does not twitch with every step |
| sprites | `SpriteAtlas` | pictures as text files, loaded once |
| actors | `MarioClone/Actors` | a body plus a behaviour plus a sprite name |
| feel | `Player.Movement` | coyote time, jump buffer, jump cut, skid |
| states | `MarioGame.States` | a table of legal transitions instead of a pile of `if`s |

And the four mercies are worth stealing whatever you build. A platformer whose physics is exactly
right still feels wrong without them:

- **coyote time** — a jump up to 0.10 s after walking off an edge still works. You pressed it; the
  edge just left first.
- **jump buffer** — a press up to 0.10 s before landing fires on landing. You were early, not wrong.
- **jump cut** — releasing early cuts the rise to 40 %. One button, two heights.
- **skid** — turning at speed decelerates harder than friction, or a turn feels like ice.

Each is one number in `PlayerTuning` and each has a test that goes red if it is removed.
