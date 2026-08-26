// The logger tests use: it keeps what it was told so a test can assert that the renderer
// really announced its fallback instead of taking it silently.

using System.Collections.Generic;

namespace GEngine.Core.Contracts;

/// <summary>A logger that keeps every message in order.</summary>
public sealed class MemoryLogger : ILogger
{
    private readonly List<string> _messages = [];

    /// <summary>Every message logged so far, prefixed with its level.</summary>
    public IReadOnlyList<string> Messages => _messages;

    /// <inheritdoc/>
    public void Info(string message) => _messages.Add("info: " + message);

    /// <inheritdoc/>
    public void Warning(string message) => _messages.Add("warning: " + message);

    /// <inheritdoc/>
    public void Error(string message) => _messages.Add("error: " + message);

    /// <summary>True when some message contains the given text.</summary>
    /// <param name="text">Text to look for.</param>
    /// <returns>True when at least one message contains it.</returns>
    public bool Contains(string text)
    {
        foreach (string message in _messages)
        {
            if (message.Contains(text, System.StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Forgets every message.</summary>
    public void Clear() => _messages.Clear();
}
