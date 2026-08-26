// A level in a string, a world built from it, and one method that advances it. Every gameplay
// test in this project is written against this: build a shape of level, hold some buttons for
// some frames, and assert on what happened.
//
// One step is one frame here. The real game runs a variable number of fixed steps per frame;
// a test wants exactly one, so that "sixty steps" means "one second" and nothing else.

using System;
using System.Collections.Generic;
using GEngine.Core.Contracts;
using GEngine.Input.Actions;
using MarioClone.Actors;
using MarioClone.Audio;
using MarioClone.Game;
using MarioClone.Levels;

namespace MarioClone.Tests.Doubles;

internal sealed class TestWorld
{
    public const float FixedDelta = 1.0f / 60.0f;

    private readonly List<InputAction> _held = [];

    public TestWorld(string levelText)
    {
        Level = LevelLoader.Parse(levelText, MarioFactory.TileSizePixels);
        Session = new GameSession();
        Audio = new MemoryAudioBackend();
        Input = new InputState();
        World = MarioFactory.Build(Level, Input, Session, Audio);
    }

    public Level Level { get; }

    public MarioWorld World { get; }

    public GameSession Session { get; }

    public MemoryAudioBackend Audio { get; }

    public InputState Input { get; }

    public Player Player => World.Player!;

    public TestWorld Hold(params InputAction[] actions)
    {
        _held.Clear();
        _held.AddRange(actions);
        return this;
    }

    public TestWorld Release() => Hold();

    public void Step(int steps = 1)
    {
        for (int step = 0; step < steps; step++)
        {
            Input.Begin();
            foreach (InputAction action in _held)
            {
                Input.Report(action, 1.0f);
            }

            Input.End(FixedDelta);
            World.Step(FixedDelta);
        }
    }

    // Holds for a while and then lets go, which is how a jump of a given length is written.
    public void HoldFor(InputAction action, int steps)
    {
        Hold(action);
        Step(steps);
        Release();
    }

    public T? Find<T>()
        where T : Actor
    {
        foreach (Actor actor in World.Actors)
        {
            if (actor is T match)
            {
                return match;
            }
        }

        return null;
    }

    public int CountOf<T>()
        where T : Actor
    {
        int total = 0;
        foreach (Actor actor in World.Actors)
        {
            total += actor is T ? 1 : 0;
        }

        return total;
    }

    public float HighestPointOf(Actor actor, int steps)
    {
        float highest = actor.Bounds.Top;
        for (int step = 0; step < steps; step++)
        {
            Step();
            highest = MathF.Min(highest, actor.Bounds.Top);
        }

        return highest;
    }
}
