// What the loop measured. It is a live object the loop writes into rather than a value it
// returns, because returning one per frame would allocate sixty objects a second and the
// engine promises none.

namespace GEngine.Core.Loop;

/// <summary>Counters and timings of a running <see cref="GameLoop"/>.</summary>
public sealed class GameLoopStatistics
{
    /// <summary>How many frames have run.</summary>
    public long FrameCount { get; internal set; }

    /// <summary>How many fixed steps have run.</summary>
    public long FixedStepCount { get; internal set; }

    /// <summary>Length of the most recent frame, in seconds, after clamping.</summary>
    public float LastFrameSeconds { get; internal set; }

    /// <summary>Frame rate, smoothed so the number on screen is readable.</summary>
    public float FramesPerSecond { get; internal set; }

    /// <summary>Clears every counter.</summary>
    public void Reset()
    {
        FrameCount = 0;
        FixedStepCount = 0;
        LastFrameSeconds = 0.0f;
        FramesPerSecond = 0.0f;
    }
}
