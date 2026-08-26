#:project ../src/GEngine.Core/GEngine.Core.csproj

// 01 - the game loop with nothing around it.
//
// Run it:  dotnet run examples/01-hello-loop.cs
//
// What to look at: the loop calls Update once per frame with a delta that wobbles, and
// FixedUpdate a whole number of times with a delta that never changes. The frame count
// depends on how fast the machine is; the fixed step count depends only on how long the
// loop was alive - sixty for every second of it, on any machine. That difference is the
// reason physics goes in FixedUpdate and nowhere else.

using System;
using GEngine.Core.Loop;
using GEngine.Core.Time;

var clock = new StopwatchClock();
var game = new HelloLoop(clock);
var loop = new GameLoop(clock, game, GameLoopSettings.Default);
game.Attach(loop);

Console.WriteLine("gengine example 01 - hello loop");
Console.WriteLine("running for 3 seconds at a target of 60 frames per second");
Console.WriteLine();
loop.Run();

Console.WriteLine();
Console.WriteLine($"ran for:     {clock.ElapsedSeconds:0.00} s");
Console.WriteLine($"frames:      {loop.Statistics.FrameCount,4}   how fast this machine is");
Console.WriteLine($"fixed steps: {loop.Statistics.FixedStepCount,4}   how long it ran, times 60: about {clock.ElapsedSeconds * 60.0:0}");
return 0;

/// <summary>Prints one line of timing every quarter of a second, then stops the loop.</summary>
internal sealed class HelloLoop : IGame
{
    private const double RunSeconds = 3.0;
    private const double ReportEverySeconds = 0.5;

    private readonly IClock _clock;
    private GameLoop? _loop;
    private double _nextReportSeconds = ReportEverySeconds;
    private int _fixedStepsThisFrame;

    public HelloLoop(IClock clock)
    {
        _clock = clock;
    }

    public void Attach(GameLoop loop)
    {
        _loop = loop;
    }

    public void Update(float deltaSeconds)
    {
        _fixedStepsThisFrame = 0;
        if (_clock.ElapsedSeconds >= RunSeconds)
        {
            _loop?.Stop();
        }
    }

    public void FixedUpdate(float fixedDeltaSeconds)
    {
        _fixedStepsThisFrame++;
    }

    public void Render(float interpolation)
    {
        if (_clock.ElapsedSeconds < _nextReportSeconds || _loop is null)
        {
            return;
        }

        _nextReportSeconds += ReportEverySeconds;
        Console.WriteLine(
            $"t={_clock.ElapsedSeconds,5:0.00}s  fps={_loop.Statistics.FramesPerSecond,6:0.0}  " +
            $"frame={_loop.Statistics.LastFrameSeconds * 1000.0f,5:0.0}ms  " +
            $"fixed steps this frame={_fixedStepsThisFrame}  interpolation={interpolation:0.00}");
    }
}
