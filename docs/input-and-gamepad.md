# Input and the gamepad

Two devices, one interface, and a byte table that was measured rather than assumed.

## Actions, not keys

The game never asks "is the space bar down?". It asks "is `Jump` down?".

```
device                 backend                 router          game
──────                 ───────                 ──────          ────
keyboard      ──►  ConsoleKeyboardBackend  ─┐
DualSense     ──►  DualSenseGamepad        ─┼─►  InputState  ─►  IsDown(Jump)
a script      ──►  FakeInputBackend        ─┘                    WasPressedThisFrame(Jump)
                                                                 AxisValue(MoveRight)
```

`InputRouter` polls every source each frame and merges them by **strongest report**: if the keyboard
says `MoveRight` is 0 and the stick says 0.6, the answer is 0.6. That is what makes a controller
plugged in mid-game just start working, and a controller unplugged mid-jump hand control back rather
than freeze.

`ActiveBackend` is whichever source last reported anything. The title screen reads it to name the
real keys or the real buttons, because showing keyboard keys to somebody holding a gamepad tells
them the game was not written for them.

## The keyboard problem

A terminal reports key **presses**. It does not report key releases — there is no such escape
sequence, and `Console.ReadKey` has nowhere to put one.

So a held key looks like a stream of presses, and a released key looks like the stream stopping. The
backend turns that into "down" with a **decay**: an action stays down for `DecaySeconds` after its
last press, and the operating system's key-repeat keeps refreshing it.

`DecaySeconds` defaults to 0.2 s, which is longer than any key-repeat interval and short enough that
letting go of *right* stops the player within a fifth of a second. It is a guess dressed as a
constant, and it is the single biggest difference in feel between playing with a keyboard and playing
with the stick.

`ReplayTests` sets it to one frame, because a replay knows exactly when the key came up and does not
need the guess.

Two smaller traps, both of which crashed something at some point:

- `Console.KeyAvailable` **throws** when stdin is redirected. Guarded with `Console.IsInputRedirected`.
- `Console.ReadKey(intercept: true)` is required, or every key the player presses is echoed into the
  middle of the picture.

## HID, per system

A USB gamepad is a HID device: it publishes a report descriptor and then sends fixed-size packets,
about one every 4 ms, whether anything changed or not. No driver, no library — a file handle and the
right bytes.

Opening that handle is the only part that differs, so `IHidBackend` has three implementations behind
one interface.

### Linux — the easy one

Devices appear as `/dev/hidraw0`, `/dev/hidraw1`, … and each has a sysfs entry:

```
/sys/class/hidraw/hidraw0/device/uevent
    HID_ID=0003:0000054C:00000CE6
              bus  vendor    product
```

Enumeration is reading text files. **No P/Invoke at all** — `File.ReadAllLines` and a
`FileStream.Read`. It is worth noticing how much of the complexity on the other two systems is
incidental rather than essential.

**Permissions:** `/dev/hidraw*` is usually root-only. A udev rule such as

```
KERNEL=="hidraw*", ATTRS{idVendor}=="054c", ATTRS{idProduct}=="0ce6", MODE="0666"
```

or adding the user to the owning group is what it takes. If the open fails, the backend logs the
path and carries on with the keyboard.

### Windows — SetupAPI

```
HidD_GetHidGuid                  the HID class GUID
SetupDiGetClassDevs              a device information set
SetupDiEnumDeviceInterfaces      walk it
SetupDiGetDeviceInterfaceDetail  get the path       ← the trap
CreateFile / ReadFile            open it and read
```

**The trap:** `SP_DEVICE_INTERFACE_DETAIL_DATA.cbSize` is the size of the *structure header with its
alignment* — **8 on x64, 6 on x86** — not the size of the buffer you allocated. Pass the buffer size
and the call fails with `ERROR_INVALID_USER_BUFFER`, which is the most-asked question about this API
on the internet.

The device set must be destroyed with `SetupDiDestroyDeviceInfoList` however the function leaves, so
it lives in a `finally`. And an invalid handle from `SetupDiGetClassDevs` is `-1`, not zero.

### macOS — IOKit

```
IOHIDManagerCreate
IOHIDManagerSetDeviceMatching(null)     everything
IOHIDManagerCopyDevices
IOHIDDeviceGetProperty                  VendorID, ProductID as CFNumbers
IOHIDDeviceOpen
IOHIDDeviceRegisterInputReportCallback  reports arrive by callback
```

Two things make this the longest of the three:

- **Core Foundation counts references by hand.** Every `Create` and every `Copy` has to have its
  `CFRelease`, on every path out, including the exception paths. The device class has a full
  finalizer as well as `Dispose`, because a leaked `IOHIDDeviceRef` is a device nobody can open
  again until the process exits.
- **Callbacks need a run loop.** `IOHIDDeviceRegisterInputReportCallback` delivers on whichever run
  loop it was scheduled on, so the reader thread runs `CFRunLoopRunInMode` in short slices. Without
  that slice the callback is registered, correct, and never called — the failure looks exactly like
  a controller that sends nothing.

**Permissions:** macOS requires Input Monitoring for the terminal application (System Settings →
Privacy & Security → Input Monitoring). Without it, enumeration succeeds and every read returns
nothing.

**Not verified.** This backend was written against Apple's documentation and has never been run on a
Mac. See [platforms.md](platforms.md).

### Common ground

All three read on a **background thread** with `IsBackground = true`, so a controller that stops
sending can never keep the process alive. The reader keeps only the latest report — a game does not
want a queue of stale input — and `TryTakeLatest` hands it over without blocking.

All three use `[DllImport]` rather than `[LibraryImport]`. The source generator behind
`[LibraryImport]` emits marshalling stubs that need `AllowUnsafeBlocks`, and rule 6 bans `unsafe`.
That is the whole reason, and it is written in `.editorconfig` next to the suppression.

## The DualSense report

**Verified against hardware on 2026-08-26**: DualSense, VID `054C`, PID `0CE6`, over USB, on
Windows 11, captured one button at a time with `examples/04-gamepad-probe.cs --capture`.

```
byte  meaning
────  ─────────────────────────────────────────────────────────────
  0   report id — 0x01 over USB
  1   left stick X    0 = left,  255 = right
  2   left stick Y    0 = up,    255 = down
  3   right stick X
  4   right stick Y
  5   L2 analogue, 0..255
  6   R2 analogue, 0..255
  7   sequence counter, increments every report
  8   low nibble  = D-pad: 0 N, 1 NE, 2 E, 3 SE, 4 S, 5 SW, 6 W, 7 NW, 8 neutral
      high nibble = 0x10 Square  0x20 Cross  0x40 Circle  0x80 Triangle
  9   0x01 L1  0x02 R1  0x04 L2  0x08 R2
      0x10 Create  0x20 Options  0x40 L3  0x80 R3
 10   0x01 PS  0x02 touchpad  0x04 mute
 11+  gyroscope, accelerometer, touchpad, battery — not decoded
```

Total length **64 bytes** over USB.

### How it was measured

The appendix this project started from was a hypothesis. The procedure that turned it into a
measurement:

1. Run `examples/04-gamepad-probe.cs --capture`, which prints every report and writes the changed
   ones to a file.
2. Hold exactly one thing. Release it. Repeat for all fifteen buttons and all eight D-pad directions.
3. **Identify each capture by its bit pattern, not by the label the tool guessed.** The first session
   produced three stray Square presses that shifted every face-button label by one; comparing bits
   against the resting report is what caught it.
4. Save each as a fixture with a header saying what was held and on what hardware.

`DualSenseHardwareTests` decodes each fixture and asserts it produces exactly that button and nothing
else. If a future refactor gets a bit wrong, twenty-three tests go red and each names the button.

### What the resting report taught

```
01 7F 80 79 8B 00 00 AA 08 00 00 …
   ▲▲ ▲▲ ▲▲ ▲▲
   left X, Y   right X, Y
```

Nothing touched, controller flat on a desk — and the sticks read `7F 80 79 8B`, not `80 80 80 80`.
They also wander by a unit or two between reports.

That is the justification for `DeadZone`, written down from an actual measurement rather than from
folklore. A game that trusts the raw axis drifts across the screen while the player watches.

The dead zone is **radial**, not per-axis:

```
magnitude = |stick|
if magnitude < threshold: return (0, 0)
return stick / magnitude · ((magnitude - threshold) / (1 - threshold))
```

Rescaling the remainder means the first pixel of movement past the threshold is smooth rather than a
jump from zero to a fifth of full speed. Per-axis dead zones make diagonals shorter than straight
lines — which is why some games feel as though they have eight directions when they have three
hundred and sixty.

### A convenient accident

Byte value 0 means *up* on the stick, and the engine's Y axis points *down* the screen. The two
conventions agree, so there is no flip anywhere in the code — and this note exists so that nobody
adds one.

### Bluetooth

Refused, with a message saying to use the cable. Over Bluetooth the DualSense sends report `0x31`,
78 bytes, with the same data shifted by one byte and a CRC on the end. Decoding it is not hard; it is
untested, and shipping an untested decoder that silently produces wrong buttons is worse than
refusing.

`DualSenseReport.TransportOf` tells them apart by report id and length.

## The fixtures

Thirty-four files in `tests/GEngine.Input.Tests/fixtures/dualsense/`. Every one says in its header
where it came from, and `EveryFixtureSaysWhetherItWasCapturedOrConstructed` fails the build if one
does not.

**Captured from hardware (23):** `resting`, `square`, `cross`, `circle`, `triangle`, `l1`, `r1`,
`l2`, `r2`, `create`, `options`, `l3`, `r3`, `ps`, `touchpad`, `mute`, and the eight `hat-*`
directions.

**Constructed, not captured (10):** `neutral`, `all-face`, `shoulders`, `system`, `stick-left`,
`stick-up`, `stick-down-right`, `triggers`, `sequence`, `bluetooth`. These are combinations and edge
cases — several buttons at once, a stick at an exact value, a Bluetooth-shaped report — that are
easier to write than to hold, and each is built from the measured layout above rather than being
independent evidence for it.
