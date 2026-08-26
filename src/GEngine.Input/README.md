# GEngine.Input

Keyboards, gamepads and recorded scripts, all answering the same question. Depends on
`GEngine.Core` and on nothing else.

## The game never names a key

```
ConsoleKeyboardBackend  --\
DualSenseGamepad        ---+--> InputRouter --> InputState --> "is Jump down?"
FakeInputBackend        --/
```

The player controller asks `_input.IsDown(InputAction.Jump)` and `_input.AxisValue(MoveRight)`.
Which device answered - a space bar, a cross button, a line in a text file - is decided by two
lookup tables, `InputMap` for keys and `GamepadMap` for buttons, and by nothing else. That is
the Command pattern with the command reduced to an enum member, and it is what lets the same
game be played, replayed and tested without a single branch on the input device.

`InputRouter` polls every connected source each frame and keeps the **strongest** report per
action. There is no current device to switch, which is why putting the gamepad down and
reaching for the arrow keys mid-level costs nothing and cannot lose a frame.

## The keyboard, and its honest limit

A terminal delivers key **presses**. It never delivers a key **release**: there is no way to
ask whether a key is still held, only more presses while the system's auto-repeat runs. So a
press turns an action on and starts a timer, every repeat refreshes it, and the action goes
off when the timer runs out.

The cost is real. Letting go takes effect a decay later (0.2 s by default), and the first
repeat of a held key waits for the system's own repeat delay. **That is why a gamepad is the
precise way to play this game** - and why `DecaySeconds` is a property rather than a constant.
There is no privileged platform here: the same backend, with the same limitation, on all three.

## The DualSense, over USB, with no driver and no library

```
DualSenseGamepad    decodes reports, applies dead zones, fills in actions
      |
HidReportReader     one background thread, keeps only the newest report
      |
IHidDevice          open / read 64 bytes / close
      |
IHidBackend         enumerate by vendor and product
```

| System | How | Interop |
|---|---|---|
| Linux | `/dev/hidraw*` opened as a file; `/sys/class/hidraw/*/device/uevent` for the ids | **none** - it is file I/O |
| Windows | `SetupDiGetClassDevs` to enumerate, `HidD_GetAttributes` for the ids, `CreateFile` and `ReadFile` to read | `setupapi`, `hid`, `kernel32` |
| macOS | IOKit HID manager, matching dictionary, input report callback on the reader thread's own run loop | IOKit **and** CoreFoundation, with `CFRetain`/`CFRelease` by hand |

Four decisions worth the read:

- **The read is on its own thread, and that thread is a background thread.** A HID read blocks
  until the controller sends something; a blocked native read cannot be cancelled from managed
  code, so the process has to be able to exit with the thread still inside it.
- **`[DllImport]`, not `[LibraryImport]`.** The `LibraryImport` generator emits stubs that need
  `AllowUnsafeBlocks`, which rule 6 bans. `SYSLIB1054` is suppressed in the interop folders and
  nowhere else, with that reason written next to it in `.editorconfig`.
- **The dead zone is radial, not per axis.** A per-axis dead zone carves a square hole out of a
  round stick: push it exactly diagonally at 20% and both axes are ignored, while pushing
  straight right at 20% works. The player feels a controller with corners.
- **Bluetooth is refused, not decoded.** Its report has a different id, a different length and
  different offsets. Decoding one as the other gives a controller that appears to be holding
  buttons nobody is touching, which is far worse than saying plainly that the cable works.

Hot-plug falls out of the design: an unplugged controller makes the next read return nothing,
the backend goes quiet, the router stops hearing from it, and a rescan every second picks it up
again when it comes back.

## Testing input without hardware

`FakeHidDevice` replays reports recorded in hexadecimal;
`tests/GEngine.Input.Tests/fixtures/dualsense` holds them. Decoding, dead zones, edges,
enumeration, unplugging and the Bluetooth refusal are all tested with nothing plugged in.

`FakeInputBackend` replays an `InputScript` - a few lines of "which actions are held during
which frames" - which is what makes a whole level replayable and what the Mario integration
test is built on.

`examples/04-gamepad-probe.cs` is the tool that turns the byte table from a hypothesis into a
measurement: it prints every report that differs from the last one, marks which bytes moved,
and with `--capture` writes one out in the exact shape of a fixture.
