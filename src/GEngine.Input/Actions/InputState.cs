// What the game asks. Three questions per action - is it down, did it go down this frame,
// did it come up this frame - plus how long it has been held, which is what a variable-height
// jump is made of.
//
// Everything is an array indexed by the action, so a question costs one bounds check and one
// comparison. There is no dictionary and no allocation anywhere in a frame.

using System;
using GEngine.Core;
using GEngine.Core.Contracts;

namespace GEngine.Input.Actions;

/// <summary>The state of every action this frame and last frame.</summary>
public sealed class InputState
{
    private readonly float[] _current = new float[InputActions.Count];
    private readonly float[] _previous = new float[InputActions.Count];
    private readonly float[] _heldSeconds = new float[InputActions.Count];

    /// <summary>How far an analogue input must move before it counts as pressed.</summary>
    public float DownThreshold { get; init; } = 0.5f;

    /// <summary>Starts a frame: this frame's values become last frame's, and are cleared.</summary>
    public void Begin()
    {
        Array.Copy(_current, _previous, _current.Length);
        Array.Clear(_current);
    }

    /// <summary>
    /// Reports how strongly a source is asking for an action. Several sources may report the
    /// same action in one frame; the strongest wins, which is what lets a player hold left on
    /// the stick and left on the keyboard without the two cancelling out.
    /// </summary>
    /// <param name="action">The action.</param>
    /// <param name="value">How strongly, from zero to one.</param>
    public void Report(InputAction action, float value)
    {
        int index = (int)action;
        _current[index] = MathF.Max(_current[index], MathG.Clamp01(value));
    }

    /// <summary>Ends a frame, advancing the hold time of everything still down.</summary>
    /// <param name="deltaSeconds">Seconds since the previous frame.</param>
    public void End(float deltaSeconds)
    {
        for (int index = 0; index < _current.Length; index++)
        {
            _heldSeconds[index] = _current[index] >= DownThreshold ? _heldSeconds[index] + deltaSeconds : 0.0f;
        }
    }

    /// <summary>Whether an action is being asked for right now.</summary>
    /// <param name="action">The action.</param>
    /// <returns>True while it is held.</returns>
    public bool IsDown(InputAction action) => _current[(int)action] >= DownThreshold;

    /// <summary>Whether an action went down between the previous frame and this one.</summary>
    /// <param name="action">The action.</param>
    /// <returns>True on exactly the frame it was pressed.</returns>
    public bool WasPressedThisFrame(InputAction action) =>
        _current[(int)action] >= DownThreshold && _previous[(int)action] < DownThreshold;

    /// <summary>Whether an action came up between the previous frame and this one.</summary>
    /// <param name="action">The action.</param>
    /// <returns>True on exactly the frame it was released.</returns>
    public bool WasReleasedThisFrame(InputAction action) =>
        _current[(int)action] < DownThreshold && _previous[(int)action] >= DownThreshold;

    /// <summary>How long an action has been held, in seconds. Zero when it is not held.</summary>
    /// <param name="action">The action.</param>
    /// <returns>The hold time.</returns>
    public float HoldTimeSeconds(InputAction action) => _heldSeconds[(int)action];

    /// <summary>How strongly an action is being asked for, from zero to one.</summary>
    /// <param name="action">The action.</param>
    /// <returns>The strength: one for a key, however far it is pushed for a stick.</returns>
    public float AxisValue(InputAction action) => _current[(int)action];

    /// <summary>Forgets everything, as when a game is paused or a level restarts.</summary>
    public void Clear()
    {
        Array.Clear(_current);
        Array.Clear(_previous);
        Array.Clear(_heldSeconds);
    }
}
