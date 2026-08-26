// The real clock. Stopwatch is used rather than DateTime because it is monotonic: it
// cannot jump when the machine changes time zone or a daylight-saving change lands
// mid-frame.

using System;
using System.Diagnostics;
using System.Threading;

namespace GEngine.Core.Time;

/// <summary>The wall clock, as a monotonic stopwatch started when the instance is created.</summary>
public sealed class StopwatchClock : IClock
{
    /// <summary>
    /// How many tight iterations one spin costs. Small enough that the pacer keeps its
    /// resolution, large enough that it is not a syscall per microsecond.
    /// </summary>
    private const int SpinIterations = 32;

    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    /// <inheritdoc/>
    public double ElapsedSeconds => _stopwatch.Elapsed.TotalSeconds;

    /// <inheritdoc/>
    public void Sleep(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            Thread.SpinWait(SpinIterations);
            return;
        }

        Thread.Sleep(duration);
    }
}
