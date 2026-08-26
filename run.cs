#:project src/GEngine.Core/GEngine.Core.csproj
#:project src/GEngine.Physics/GEngine.Physics.csproj
#:project src/GEngine.Rendering/GEngine.Rendering.csproj
#:project src/GEngine.Input/GEngine.Input.csproj
#:project src/MarioClone/MarioClone.csproj

// The game.
//
//     dotnet run run.cs
//
// Arrows or A and D to move, space or Z to jump, X or C to run, P or Esc to pause, Esc again
// to leave. Plug in a DualSense with a USB cable and it is picked up within a second, with no
// driver and no restart; unplug it mid-level and the keyboard carries on.
//
// This file is the composition root and it is the only place in the repository that names all
// five projects. Everything below is wiring: which terminal, which input sources, where the
// assets come from - and then a loop.

using GEngine.Core.Contracts;
using GEngine.Core.Loop;
using GEngine.Core.Platform;
using GEngine.Core.Time;
using GEngine.Input.Actions;
using GEngine.Input.Gamepad;
using GEngine.Input.Hid;
using GEngine.Input.Keyboard;
using GEngine.Rendering;
using GEngine.Rendering.Assets;
using MarioClone.Game;

IPlatformProbe platform = SystemPlatformProbe.Instance;
var logger = new MemoryLogger();

using ConsoleSession session = ConsoleSession.Start(platform, logger);

var input = new InputState();
using var router = new InputRouter(input);
router.Add(new ConsoleKeyboardBackend(ConsoleKeyReader.Instance, InputMap.CreateDefault()));
router.Add(new DualSenseGamepad(HidBackendFactory.Create(platform), GamepadMap.CreateDefault(), logger));

var game = new MarioGame(new MarioGameSettings
{
    Renderer = session.Renderer,
    Router = router,
    Assets = new EmbeddedAssetSource(typeof(MarioGame).Assembly),
});

var loop = new GameLoop(new StopwatchClock(), game, GameLoopSettings.Default);
while (!game.IsFinished && !session.IsCancelled)
{
    loop.Tick();
}

session.Dispose();
foreach (string line in logger.Messages)
{
    Console.WriteLine(line);
}

Console.WriteLine($"score {game.Session.Score}, {game.Session.Coins} coins, {game.Session.Lives} lives left");
return 0;
