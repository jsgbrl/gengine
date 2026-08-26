// Every number the loop uses, in one place, so an experiment is a one-line change.

namespace GEngine.Core.Loop;

/// <summary>How a <see cref="GameLoop"/> is tuned.</summary>
public sealed class GameLoopSettings
{
    /// <summary>The settings the game and the examples use: 60 fixed steps, 60 frames, quarter-second clamp.</summary>
    public static GameLoopSettings Default { get; } = new();

    /// <summary>Length of one fixed step, in seconds. Sixty a second is the classic choice.</summary>
    public float FixedDeltaSeconds { get; init; } = 1.0f / 60.0f;

    /// <summary>
    /// Longest frame the loop will admit to, in seconds. A frame that really took longer -
    /// the window was dragged, a breakpoint was hit - is reported as this instead, so the
    /// accumulator cannot ask for a hundred fixed steps at once and fall further behind on
    /// every frame. That runaway is the spiral of death.
    /// </summary>
    public float MaximumFrameSeconds { get; init; } = 0.25f;

    /// <summary>Frames per second the pacer aims for. Zero runs as fast as the machine allows.</summary>
    public int TargetFramesPerSecond { get; init; } = 60;

    /// <summary>
    /// How long a window the reported frame rate is measured over, in seconds. Counting
    /// frames in a window and dividing gives the true rate for that window; a running
    /// average over single frames does not, and reads as several hundred frames per second
    /// for the first half second because the first frame of a process has nothing in it.
    /// </summary>
    public float FrameRateWindowSeconds { get; init; } = 0.5f;
}
