// Null Object. The game never checks whether audio is present, because there is always an
// audio backend - this one, which does nothing.

namespace MarioClone.Audio;

/// <summary>An audio backend that plays nothing.</summary>
public sealed class NullAudioBackend : IAudioBackend
{
    /// <summary>The shared instance. It holds no state.</summary>
    public static NullAudioBackend Instance { get; } = new();

    /// <inheritdoc/>
    public string Name => "silent";

    /// <inheritdoc/>
    public void Play(GameSound sound)
    {
    }
}
