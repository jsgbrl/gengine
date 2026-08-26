// Null Object. The engine never checks whether a logger is present, because there is
// always one - this one, when nobody wants the output.

namespace GEngine.Core.Contracts;

/// <summary>A logger that discards everything.</summary>
public sealed class NullLogger : ILogger
{
    /// <summary>The shared instance. It holds no state, so one is enough.</summary>
    public static NullLogger Instance { get; } = new();

    /// <inheritdoc/>
    public void Info(string message)
    {
    }

    /// <inheritdoc/>
    public void Warning(string message)
    {
    }

    /// <inheritdoc/>
    public void Error(string message)
    {
    }
}
