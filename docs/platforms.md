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
