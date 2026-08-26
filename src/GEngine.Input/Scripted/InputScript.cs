// Input as a file. A script says which actions are held during which frames, and that is
// enough to replay a whole level: the game is deterministic, the physics is deterministic,
// so the same script always produces the same run.
//
//     # mario walks right and jumps twice
//     0-400    MoveRight Run
//     60-70    Jump
//     120-130  Jump
//
// Ranges rather than one line per frame, because a level is thousands of frames and a dozen
// intentions.

using System;
using System.Collections.Generic;
using System.Globalization;
using GEngine.Core.Contracts;

namespace GEngine.Input.Scripted;

/// <summary>Which actions are held during which frames.</summary>
public sealed partial class InputScript
{
    private readonly List<HoldRange> _holds = [];

    /// <summary>The last frame any hold covers, so a replay knows when it is over.</summary>
    public int FrameCount { get; private set; }

    /// <summary>How many holds the script contains.</summary>
    public int HoldCount => _holds.Count;

    /// <summary>Holds an action over a range of frames, both ends included.</summary>
    /// <param name="action">The action.</param>
    /// <param name="firstFrame">First frame it is held, counting from zero.</param>
    /// <param name="lastFrame">Last frame it is held.</param>
    /// <returns>This script, so holds can be chained.</returns>
    public InputScript Hold(InputAction action, int firstFrame, int lastFrame)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(firstFrame);
        ArgumentOutOfRangeException.ThrowIfLessThan(lastFrame, firstFrame);
        _holds.Add(new HoldRange(action, firstFrame, lastFrame));
        FrameCount = Math.Max(FrameCount, lastFrame + 1);
        return this;
    }

    /// <summary>Whether an action is held on a frame.</summary>
    /// <param name="frame">The frame, counting from zero.</param>
    /// <param name="action">The action.</param>
    /// <returns>True when some hold covers that frame.</returns>
    public bool IsHeld(int frame, InputAction action)
    {
        foreach (HoldRange hold in _holds)
        {
            if (hold.Action == action && frame >= hold.FirstFrame && frame <= hold.LastFrame)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Writes the script back out in the order it was built.</summary>
    /// <returns>The text form.</returns>
    public string ToText()
    {
        List<string> lines = [];
        foreach (HoldRange hold in _holds)
        {
            lines.Add(string.Format(
                CultureInfo.InvariantCulture,
                "{0}-{1} {2}",
                hold.FirstFrame,
                hold.LastFrame,
                hold.Action));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private readonly struct HoldRange
    {
        public HoldRange(InputAction action, int firstFrame, int lastFrame)
        {
            Action = action;
            FirstFrame = firstFrame;
            LastFrame = lastFrame;
        }

        public InputAction Action { get; }

        public int FirstFrame { get; }

        public int LastFrame { get; }
    }
}
