// The keyboard, and the honest limitation at the heart of it.
//
// A terminal delivers key presses, not key releases. There is no way to ask whether a key is
// still held: all that arrives is a press, and then more presses while the operating system's
// auto-repeat is running. So a press turns an action on and starts a timer, every repeat
// refreshes the timer, and the action goes off when the timer runs out.
//
// The cost is real and worth stating plainly: releasing a key takes effect a decay later, and
// the first repeat of a held key arrives after the system's own repeat delay - typically a
// third of a second. That is why a gamepad, which reports releases the moment they happen, is
// the precise way to play this game, and why the decay is a property rather than a constant.

using System;
using GEngine.Core.Contracts;
using GEngine.Input.Actions;

namespace GEngine.Input.Keyboard;

/// <summary>An input backend that turns terminal key presses into actions.</summary>
public sealed class ConsoleKeyboardBackend : IInputBackend
{
    /// <summary>How long an action stays on after the last press that asked for it.</summary>
    public const float DefaultDecaySeconds = 0.20f;

    private readonly float[] _remainingSeconds = new float[InputActions.Count];
    private readonly IKeyReader _reader;

    /// <summary>Creates the backend.</summary>
    /// <param name="reader">Where key presses come from.</param>
    /// <param name="map">Which keys ask for which actions.</param>
    public ConsoleKeyboardBackend(IKeyReader reader, InputMap map)
    {
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentNullException.ThrowIfNull(map);
        _reader = reader;
        Map = map;
    }

    /// <summary>Which keys ask for which actions. Rebinding takes effect immediately.</summary>
    public InputMap Map { get; }

    /// <inheritdoc/>
    public string Name => "keyboard";

    /// <summary>A keyboard is always there, even when nobody is typing on it.</summary>
    public bool IsConnected => true;

    /// <summary>How long an action stays on after the last press that asked for it.</summary>
    public float DecaySeconds { get; init; } = DefaultDecaySeconds;

    /// <summary>How many key presses the last poll consumed.</summary>
    public int LastKeyCount { get; private set; }

    /// <inheritdoc/>
    public void Poll(float deltaSeconds)
    {
        Decay(deltaSeconds);
        LastKeyCount = 0;
        while (_reader.TryReadKey(out ConsoleKey key))
        {
            LastKeyCount++;
            if (Map.TryGetAction(key, out InputAction action))
            {
                _remainingSeconds[(int)action] = DecaySeconds;
            }
        }
    }

    /// <inheritdoc/>
    public bool IsDown(InputAction action) => _remainingSeconds[(int)action] > 0.0f;

    /// <inheritdoc/>
    public float AxisValue(InputAction action) => IsDown(action) ? 1.0f : 0.0f;

    /// <summary>Forgets every held action, as when a game is paused.</summary>
    public void Clear() => Array.Clear(_remainingSeconds);

    /// <inheritdoc/>
    public void Dispose() => Clear();

    private void Decay(float deltaSeconds)
    {
        for (int index = 0; index < _remainingSeconds.Length; index++)
        {
            _remainingSeconds[index] = MathF.Max(0.0f, _remainingSeconds[index] - deltaSeconds);
        }
    }
}
