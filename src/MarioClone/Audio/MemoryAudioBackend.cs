// The audio backend the tests use: it remembers what it was asked to play, which is how a test
// asserts that stomping a goomba makes the stomp sound without anything making a noise.

using System.Collections.Generic;

namespace MarioClone.Audio;

/// <summary>An audio backend that records every sound it was asked for.</summary>
public sealed class MemoryAudioBackend : IAudioBackend
{
    private readonly List<GameSound> _played = [];

    /// <inheritdoc/>
    public string Name => "memory";

    /// <summary>Every sound asked for, in order.</summary>
    public IReadOnlyList<GameSound> Played => _played;

    /// <inheritdoc/>
    public void Play(GameSound sound) => _played.Add(sound);

    /// <summary>How many times a sound was asked for.</summary>
    /// <param name="sound">The sound.</param>
    /// <returns>The count.</returns>
    public int CountOf(GameSound sound)
    {
        int total = 0;
        foreach (GameSound played in _played)
        {
            total += played == sound ? 1 : 0;
        }

        return total;
    }

    /// <summary>Forgets everything.</summary>
    public void Clear() => _played.Clear();
}
