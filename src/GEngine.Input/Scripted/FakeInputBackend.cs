// An input source that plays a script. This is what makes the replay test possible: the same
// script, the same physics, the same frames - and it is the same class the tests use to press
// a button on frame 30 and let go on frame 34.

using System;
using GEngine.Core.Contracts;

namespace GEngine.Input.Scripted;

/// <summary>An input backend that replays an <see cref="InputScript"/>, one frame per poll.</summary>
public sealed class FakeInputBackend : IInputBackend
{
    private readonly InputScript _script;
    private int _playedFrame = -1;

    /// <summary>Creates a backend over a script.</summary>
    /// <param name="script">What to replay.</param>
    public FakeInputBackend(InputScript script)
    {
        ArgumentNullException.ThrowIfNull(script);
        _script = script;
    }

    /// <inheritdoc/>
    public string Name => "script";

    /// <summary>Which frame the next poll will play, counting from zero.</summary>
    public int Frame { get; private set; }

    /// <summary>
    /// True until the script runs out. A replay that has finished disconnects rather than
    /// holding its last frame for ever, so the router simply stops hearing from it.
    /// </summary>
    public bool IsConnected => Frame < _script.FrameCount;

    /// <summary>Plays the current frame and moves on to the next.</summary>
    /// <param name="deltaSeconds">Ignored: a script advances one frame per poll, not per second.</param>
    public void Poll(float deltaSeconds)
    {
        _playedFrame = Frame;
        Frame++;
    }

    /// <inheritdoc/>
    public bool IsDown(InputAction action) => _playedFrame >= 0 && _script.IsHeld(_playedFrame, action);

    /// <inheritdoc/>
    public float AxisValue(InputAction action) => IsDown(action) ? 1.0f : 0.0f;

    /// <summary>Starts the script again from the beginning.</summary>
    public void Rewind()
    {
        Frame = 0;
        _playedFrame = -1;
    }

    /// <inheritdoc/>
    public void Dispose() => Rewind();
}
