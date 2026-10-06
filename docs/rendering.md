# Rendering

How a terminal becomes a screen with square pixels, and what it costs.

## The half block

A terminal cell is about twice as tall as it is wide. Drawing one pixel per cell gives you a picture
stretched vertically by a factor of two — every circle is an ellipse and every jump looks wrong.

The fix is one character: `▀`, U+2580, UPPER HALF BLOCK. Printed with a foreground colour and a
background colour, it is **two pixels**, stacked, each of them square.

```
   cell             what it is
   ┌───┐
   │▀▀▀│  ← foreground colour: the top pixel
   │   │  ← background colour: the bottom pixel
   └───┘
```

So a terminal 80 columns by 24 rows is a screen 80 pixels by 48. `ConsoleRenderer` reports exactly
that, and everything above it — the camera, the frame buffer, the sprites — works in those pixels
and never thinks about cells again.

The choice of the *upper* half block rather than the lower is not arbitrary. When a terminal is
resized or a line wraps, the leftover row is background-coloured; with the upper block that leftover
is the bottom half of a pixel row rather than the top of one, which reads as a slightly short screen
rather than a misaligned one.

## Colour, in three steps down

There is no way to ask a terminal what it supports. There is only convention, and the convention is
environment variables:

| | when | escape | example |
|---|---|---|---|
| **truecolor** | `COLORTERM` is `truecolor` or `24bit` | `ESC[38;2;r;g;bm` | `ESC[38;2;252;224;68m` |
| **256 colour** | `TERM` contains `256color` | `ESC[38;5;nm` | `ESC[38;5;220m` |
| **16 colour** | anything else | `ESC[3nm` / `ESC[9nm` | `ESC[93m` |

Backgrounds are the same numbers plus ten: `48;2;…`, `48;5;…`, `4n` / `10n`.

Degradation is **identical on all three systems** and is announced on screen when the game starts:

```
info: windows console - 16 colours: the safe fallback, set TERM to a 256color variant for more
```

Never a crash, never a silent downgrade. A player whose colours look wrong can read one line and
know why.

### 256 colours

Indices 16–231 are a 6×6×6 cube; 232–255 are a 24-step grey ramp. A colour is mapped by quantising
each channel to six levels, except that near-greys go to the ramp, which has finer steps than the
cube's grey diagonal.

### 16 colours

The first version thresholded each channel and produced nonsense: `(120, 120, 255)` came out white
and `(90, 90, 90)` came out black. It is now a **nearest-colour search** over the sixteen xterm
values by squared distance — sixteen comparisons, done once per distinct colour, and the results are
right because they are measured against the actual palette rather than a rule of thumb.

## The frame is written once

The naive renderer writes a colour escape and a character per cell: for 80×24 that is about 2 000
writes and 40 KB of string per frame, sixty times a second. Three and a half thousand allocations a
minute, all of them garbage.

Instead:

1. **Diff.** Each cell of the new frame is compared with the last. Unchanged cells are skipped.
2. **Batch.** Runs of changed cells share one cursor move; consecutive cells with the same colours
   share one escape.
3. **One write.** The whole frame goes into a reusable `char[]` and out with a single
   `Console.Out.Write`.

`AnsiBuffer` is that reusable array. It formats integers **digit by digit** rather than calling
`int.ToString()`, because `ToString` allocates a string per number and there are up to six numbers
per cell. It is the ugliest code in the repository and it is the reason the renderer allocates
nothing per frame.

A first frame, or a frame after a resize, has no previous frame to diff against and is written in
full. That is the only time.

## The terminal is left as it was found

A game takes over the terminal: alternate screen, cursor hidden, colours changed. If it exits
without putting all of that back, the user is left with an invisible cursor in a coloured shell, and
their next reaction is `reset`.

`ConsoleSession` restores through three doors, because a program can leave through three:

```csharp
try { … }                                // normal exit and exceptions
finally { Stop(); }
Console.CancelKeyPress += …               // Ctrl-C
AppDomain.CurrentDomain.ProcessExit += …  // everything else
```

`Stop` is idempotent, since more than one of those can fire.

## The three drivers

`IConsoleDriver` is what a terminal has to be able to do: enable itself, report its size, and be
disposed. There are three implementations and they are nearly identical — which is the point, and is
why they are three files rather than one file with three branches.

- **`WindowsConsoleDriver`** additionally calls `SetConsoleMode` with
  `ENABLE_VIRTUAL_TERMINAL_PROCESSING`. Modern Windows Terminal has it on; `conhost` does not, and
  without it every escape sequence is printed as text. If the call fails, the driver says so and
  carries on — a garbled screen with an explanation beats a crash.
- **`MacOsConsoleDriver`** and **`LinuxConsoleDriver`** assume an ANSI terminal, which is a safe
  assumption on both.

`ConsoleDriverFactory` picks one from an `IPlatformProbe`. An unknown system is refused **by name**
rather than guessed at.

## Sprites are text

```
# coin
legend:
. transparent
Y yellow
A orange
pixels:
.YYY.
YYAYY
```

A `legend:` block naming colours, a `pixels:` marker, then the picture. Colours are palette names or
`r g b` triples. The format exists so that a sprite can be read, diffed and edited by a person with
no tool — which matters more in a repository meant to be read than any binary format's compactness
would.

`SpriteAtlas` loads each sprite once and hands back the same immutable object every time
(see [patterns.md](patterns.md#flyweight)).

## Testing something that draws

Two seams make the renderer testable without a terminal:

- **`HeadlessRenderer`** implements `IRenderer` into memory. Every gameplay test uses it.
- **`AsciiSnapshot`** turns a frame buffer into characters by brightness, so a test can assert on a
  picture as text — and a failure prints the picture rather than a pixel index.

```
+++++++++++++++++++++++++++---+++++++++
++++++++++++++++++++++++++-----+++++++++
++++++++++++++++++++++++++## ##+++++++++
```

`ConsoleDriver` also takes an optional `TextWriter`, so the ANSI output itself can be asserted
against without anything reaching a real screen. That injection exists because one early test called
`Enable()` on the real console and switched the test run into the alternate screen — the output
vanished, and the run looked like a hang.
