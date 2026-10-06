// Three levels, three methods. A console game cannot print a log line into the frame it is
// drawing, so in the real game this goes to a file or nowhere at all - but the engine
// still needs a way to say "truecolor was not available, degrading to 256".

namespace GEngine.Core.Contracts;

/// <summary>Somewhere the engine can explain itself.</summary>
public interface ILogger
{
    /// <summary>Something worth knowing happened.</summary>
    /// <param name="message">What happened, in a sentence a player could read.</param>
    void Info(string message);

    /// <summary>Something is degraded but the game goes on.</summary>
    /// <param name="message">What is degraded, and what the game will do instead.</param>
    void Warning(string message);

    /// <summary>Something failed.</summary>
    /// <param name="message">What failed, and what the caller can do about it.</param>
    void Error(string message);
}
