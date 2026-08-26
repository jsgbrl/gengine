# GEngine.Rendering

Pictures, and how they reach a terminal. Depends on `GEngine.Core` and on nothing else.

## The half block

A character cell is about twice as tall as it is wide, so a picture drawn one pixel per cell
comes out squashed. Print `U+2580` - the **upper half block** - give it a foreground colour
and a background colour, and one cell holds two square pixels:

```
    cell          foreground = top pixel      +---+
   +----+   ->                                | R |   two square pixels
   | ## |        background = bottom pixel    +---+
   | ## |                                     | G |
   +----+                                     +---+
```

Square pixels and twice the vertical resolution, for the price of one character. A terminal
of 120 by 30 cells is a screen of 120 by 60 pixels.

## The diff

`Console.Clear()` once a frame is where flicker comes from, and redrawing every cell is
thousands of escape sequences a frame. `ConsoleRenderer` keeps the two colours of every cell
it last sent and writes only the cells that changed, in **one** write at the end of the
frame. A frame identical to the last one sends **nothing at all** - there is a test that says
so.

Inside a run of changed cells the colour is sent only when it differs from the colour already
in effect, so a row of sky costs one escape and then one character per cell.

## Degrading, the same way on all three systems

| Terminal says | Depth | What is sent |
|---|---|---|
| `COLORTERM=truecolor` or `24bit` | `TrueColor` | `ESC[38;2;r;g;b` - the colour asked for |
| `TERM=*256color*` | `Palette256` | the xterm cube, or its greyscale ramp for greys |
| anything else | `Basic16` | the nearest of the sixteen, by squared distance |

Never "not supported". The detection is a pure function of two strings, so every terminal on
every system is tested from one machine, and the depth chosen is always announced through
`ILogger` - a fallback nobody is told about is a bug report waiting to happen.

Windows needs one thing more: escape sequences are ignored until `SetConsoleMode` says
otherwise. That is the only P/Invoke in this project, it lives behind `IConsoleDriver`, and
when it fails the game keeps running at sixteen colours and says why.

## The terminal is always left usable

`ConsoleSession` hooks `CancelKeyPress` and `ProcessExit` on top of the usual `try`/`finally`,
because only two of the four ways out of a game - it finished, the player pressed Esc - go
through the code you wrote. The other two are Ctrl+C and an exception.

## Assets are text

A sprite is a legend and a picture:

```
# coin
legend:
. transparent
Y yellow
pixels:
.YY.
YYYY
.YY.
```

Editable in a notepad, reviewable as a diff. Three sources implement `IAssetSource`: embedded
in the assembly (what the game uses, so `dotnet run run.cs` needs no working directory), on
disk (for editing without a rebuild), and in memory (for tests).

## Reading the pipeline

```
Sprite / BitmapFont  ->  FrameBuffer  ->  IRenderer  ->  ConsoleRenderer -> terminal
                                                      -> HeadlessRenderer -> AsciiSnapshot
```

`FrameBuffer` knows nothing about terminals; `ConsoleRenderer` knows nothing about sprites.
That seam is why every drawing test in the repository runs headless, and why the frame the
tests assert on is the same frame the terminal gets.
