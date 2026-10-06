# Platforms

Windows, macOS and Linux are peers. None is the main one, none is the degraded one: the same
features, the same defaults, the same feel. Where a system cannot do something, it degrades
**the same way on all three**, says so on screen, and never crashes and never goes quiet.

There is no conditional compilation anywhere in the repository. Every difference is a runtime
choice behind an interface, made by `PlatformFactory` from an `IPlatformProbe` - which is also
why a test on any one machine covers all three paths.

## Support matrix

| Subsystem | Windows | macOS | Linux |
|---|---|---|---|
| Game loop, physics, scenes | identical | identical | identical |
| Console colour | truecolor once VT is enabled | truecolor on iTerm2 / Ghostty / WezTerm, 256 on Terminal.app | truecolor when `COLORTERM` says so, 16 on the virtual console |
| Half-block rendering | needs a UTF-8 capable font | native | native |
| Keyboard | identical, presses only | identical, presses only | identical, presses only |
| DualSense over USB | `setupapi` + `hid` + `kernel32` | IOKit HID manager | `/dev/hidraw*` |
| DualSense over Bluetooth | refused, with a message | refused, with a message | refused, with a message |

## What each system needs

### Windows

**Colour.** Escape sequences are ignored until the console mode says otherwise.
`WindowsConsoleDriver` sets `ENABLE_VIRTUAL_TERMINAL_PROCESSING` through `SetConsoleMode` at
startup. Windows Terminal has it on already; a plain `conhost` window does not. If the call
fails - which is what happens when the output is a pipe rather than a terminal - the game
keeps running at sixteen colours and logs why.

**The controller.** A DualSense is very often already open in another process: Steam Input,
DS4Windows, or the PlayStation Accessories app. gengine opens the device with
`FILE_SHARE_READ | FILE_SHARE_WRITE` so both can read it. If the open still fails, the path and
the reason are printed - there is no silent fallback to "no controller found".

**Steam Input, and the cost of sharing.** Sharing has a visible consequence, and it is worth
knowing before it happens to you. With Steam running and PlayStation support enabled, Steam
applies its **Desktop Configuration** to the controller whenever no Steam game has focus - and
that layout maps Cross to a left mouse click. So while you play gengine, every jump also clicks
wherever the mouse pointer happens to be sitting. Parked over a terminal's tab bar, that means
every jump switches tabs.

This is not something the engine can prevent. Opening the device exclusively would stop it and
would also fail outright whenever Steam got there first, which is most of the time. Sharing is
the right trade, and this is its price. To stop it, in Steam: Settings, Controller, and either
turn off **PlayStation Controller Support** or set the **Desktop Configuration** layout to none.
Closing Steam works too.

**Font.** The renderer draws with `U+2580`. Cascadia Mono, Consolas and the Windows Terminal
defaults all have it. A console font that does not will show blanks.

### macOS

**Colour.** Terminal.app sets `TERM=xterm-256color` and does not set `COLORTERM`, because it
genuinely stops at 256 colours - so on a stock Mac the game runs at 256 and says so. iTerm2,
Ghostty and WezTerm set `COLORTERM=truecolor` and get all of it.

**Input Monitoring.** Reading a HID device may require the terminal to be granted **Input
Monitoring** in System Settings, Privacy & Security. If it is refused, `IOHIDDeviceOpen` fails,
the controller is reported as unopenable, and the game keeps going on the keyboard. To grant
it: System Settings, Privacy & Security, Input Monitoring, and add Terminal, iTerm or whichever
program is running the game.

### Linux

**Permissions come first, not as a footnote.** `/dev/hidraw*` is root-only on most
distributions. Without a udev rule, enumeration finds the controller and opening it fails.

Create `/etc/udev/rules.d/99-gengine-dualsense.rules`:

```
# DualSense and DualSense Edge over USB, readable by anyone in the input group
KERNEL=="hidraw*", ATTRS{idVendor}=="054c", ATTRS{idProduct}=="0ce6", MODE="0660", GROUP="input"
KERNEL=="hidraw*", ATTRS{idVendor}=="054c", ATTRS{idProduct}=="0df2", MODE="0660", GROUP="input"
```

Then reload the rules and make sure you are in the group:

```bash
sudo udevadm control --reload-rules && sudo udevadm trigger
sudo usermod -aG input "$USER"
```

Log out and back in for the group to take effect, then unplug and replug the controller.

**Colour.** Most terminal emulators set `COLORTERM=truecolor`. The virtual console -
`TERM=linux`, the one on a machine with no desktop - really does only have sixteen colours, so
the check is not a formality.

## Verifying it yourself

```bash
dotnet run examples/04-gamepad-probe.cs
```

It prints the platform, the HID backend it chose, every HID device the system will admit to,
and then every controller report that differs from the last one. If something is wrong, this is
the command whose output goes in the bug report.

## What was actually run, and what was not

Rule 10 of the build says: do not declare done what you did not run. So, precisely:

| | built | unit tested | run on the system | hardware verified |
|---|---|---|---|---|
| **Windows 11** | yes | yes | yes — the game, all four examples, the whole suite | yes — a DualSense, VID `054C` PID `0CE6`, over USB, on 2026-08-26 |
| **macOS** | yes | yes, through `FixedPlatformProbe` | **no** | **no** |
| **Linux** | yes | yes, through `FixedPlatformProbe` | **no** | **no** |

Everything in this repository was developed and executed on Windows 11. The macOS and Linux code
paths compile — they are in the same binary, and there is no conditional compilation to hide
behind — and the parts that can be tested without the system are tested: the factory picks the
right driver for a given probe, the ANSI encoder produces the right bytes for a given depth, the
HID reader decodes the right buttons from a captured report.

What is **not** covered by any of that:

- **`MacOsHidBackend` and `MacOsHidDevice`.** Written against Apple's IOKit documentation and never
  executed. The two things most likely to be wrong are the `CFRunLoopRunInMode` slice on the reader
  thread (without which the callback is registered, correct, and never called) and the manual
  `CFRetain`/`CFRelease` balance on the paths that fail early.
- **`LinuxHidBackend` and `LinuxHidDevice`.** The simplest of the three — sysfs text and a
  `FileStream` — and still never executed. The likely failure is a permission error surfacing in a
  way that reads badly rather than an incorrect read.
- **`MacOsConsoleDriver` and `LinuxConsoleDriver`.** Both assume an ANSI terminal, which is safe;
  neither has drawn a frame on the system it is named after.
- **Terminal resize behaviour** on either system.
- **The `input` group name** in the udev rule above, which differs between distributions.

`ApiCoverageTests` exempts these types by category, with that reason written into the test. They are
the only exemptions in the repository that are about a system rather than about a type.

If you run gengine on macOS or Linux, `dotnet run examples/04-gamepad-probe.cs` is the command whose
output settles it.
