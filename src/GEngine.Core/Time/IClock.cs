// Time enters the engine here and nowhere else. Rule 7 of the build prompt - nothing
// deterministic may read DateTime.Now or Environment.TickCount - is enforceable only
// because every consumer of time takes an IClock instead of reaching for the wall.

using System;

namespace GEngine.Core.Time;

/// <summary>
/// A monotonic source of elapsed time, plus the one blocking operation the frame pacer
/// needs. Injecting it is what makes the loop, the physics and the game testable: a test
/// hands in a <see cref="ManualClock"/> and decides for itself what "one second" means.
/// </summary>
public interface IClock
{
    /// <summary>Seconds since the clock started. Never goes backwards.</summary>
    double ElapsedSeconds { get; }

    /// <summary>
    /// Waits for approximately the given duration. A zero or negative duration means
    /// "give up the rest of this time slice", which is what the pacer uses to spin out
    /// the last millisecond before a frame is due.
    /// </summary>
    /// <param name="duration">How long to wait.</param>
    void Sleep(TimeSpan duration);
}
