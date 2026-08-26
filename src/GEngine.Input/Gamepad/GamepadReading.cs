// Where the blocking read happens. A game wants it on its own thread; a test wants it on the
// thread it controls, so that a run is a sequence of steps rather than a race.

namespace GEngine.Input.Gamepad;

/// <summary>How a gamepad backend reads its device.</summary>
public enum GamepadReading
{
    /// <summary>
    /// On a background thread, which is what a game wants: a HID read blocks until the
    /// controller sends something, and a frame cannot afford to wait for that.
    /// </summary>
    OnItsOwnThread,

    /// <summary>
    /// Inside the poll, on the calling thread. Every read blocks the caller, which is only
    /// sensible for a test or for a tool like the gamepad probe, where blocking is the point.
    /// </summary>
    WhenPolled,
}
