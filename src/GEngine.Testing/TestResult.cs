// One line of the report, as data. The runner produces these; printing is somebody
// else's job, which is what lets the framework test itself without capturing stdout.

namespace GEngine.Testing;

/// <summary>The outcome of running a single test case.</summary>
public sealed class TestResult
{
    /// <summary>Creates a result.</summary>
    /// <param name="name">Fully qualified case name, as shown in the report.</param>
    /// <param name="outcome">How the case ended.</param>
    /// <param name="elapsedMilliseconds">Wall time spent inside the case.</param>
    /// <param name="message">Failure detail or skip reason; empty when passed.</param>
    public TestResult(string name, TestOutcome outcome, double elapsedMilliseconds, string message)
    {
        Name = name;
        Outcome = outcome;
        ElapsedMilliseconds = elapsedMilliseconds;
        Message = message;
    }

    /// <summary>Fully qualified case name, as shown in the report.</summary>
    public string Name { get; }

    /// <summary>How the case ended.</summary>
    public TestOutcome Outcome { get; }

    /// <summary>Wall time spent inside the case, in milliseconds.</summary>
    public double ElapsedMilliseconds { get; }

    /// <summary>Failure detail or skip reason. Empty when the case passed.</summary>
    public string Message { get; }
}
