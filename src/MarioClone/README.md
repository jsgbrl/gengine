# MarioClone

The game. It depends on all four engine projects and is the only project that does — the
engine never imports the game, so everything here is a decision the engine deliberately
refused to make for you.

## What is in here

```
Levels/   the file format: a grid of characters and a legend
Actors/   the nine things that live in a level, and one object of tuning numbers
Game/     the composition root, the world, the session and the six states
View/     the level, the display and the title screen - all read-only
```

## The composition root

`MarioFactory` is the one place that knows a brick is a static body on the level layer, that a
goomba walks left, and that a coin is a trigger. That knowledge is not in `Brick`, `Goomba` or
`Coin`: an actor decides what it *does*, the factory decides what it *is*. If you want to know
how the game is wired, there is exactly one file to read.

## The level format

```
# anything up here is a header and is ignored
tiles:
....?..........
.M...o....G...F
###############
```

The grid starts after a line saying `tiles:`, the same way a sprite's picture starts after
`pixels:`. That marker is not decoration. `#` is the ground, so `#` cannot also start a
comment — the first version of this loader treated it as one and silently deleted every floor
in the game. `LevelLegend` holds the whole vocabulary; a character in neither table is a
`FormatException` naming the character, not a silently empty cell.

A character is either a **tile** or a **thing**. A thing leaves its cell empty behind it,
because you cannot stand on a coin.

## Tiles are not bodies

World 1-1 is 140 columns wide. As static boxes that would be well over a thousand bodies in the
broad phase, every step, for a floor that never moves. Instead the grid goes into a
`TileCollisionSource` and the sweep asks it directly: a few array lookups along the path
actually travelled. The nine actors are bodies; the world is a grid.

## The four mercies

A platformer that resolves the physics correctly still feels wrong. Four small lies fix it, and
each has a test that goes red if it is removed:

| mercy | what it does | why |
|---|---|---|
| coyote time | jump up to 0.10 s after walking off | you pressed it, the edge just left first |
| jump buffer | a press up to 0.10 s early still jumps on landing | you were early, not wrong |
| jump cut | releasing early cuts the rise to 40 % | one button, two heights |
| skid | turning at speed decelerates harder than friction | otherwise a turn feels like ice |

All four live in `Player.Movement`, and all four are numbers in `PlayerTuning` — the one object
where the whole feel of the game is written down, in named units.

## The six states

```
Title ──► Playing ──► Paused ──► Title
             │  ▲        │
             │  └────────┘
             ├──► Death ──► Playing
             │      └─────► GameOver ──► Title
             └──► LevelComplete ──► Title
```

Declared as a table of legal moves rather than written as ifs, so an illegal one — unpausing
into a death, restarting from the title — is refused by the machine instead of being a bug
nobody finds until somebody pauses at exactly the wrong moment.

## Sound

There is none. A console has no audio device the engine could portably reach, so what the game
has instead is the *seam* where sound would go: `IAudioBackend`, a silent implementation, and a
remembering one. The remembering one turns out to be the most useful object in the test suite —
it is the game narrating itself, so a test can ask "did that stomp?" without looking at pixels.

## The replay

`tests/MarioClone.Tests/fixtures/replay-1-1.txt` is one recorded run of world 1-1, and
`ReplayTests` plays it twice: once as key presses through the keyboard backend, once as 64-byte
HID reports through the DualSense decoder. Both have to reach the flag with the same score.

That is the test the whole engine is arranged around. If it passes, then nothing between the
device and the player knows which device it was — which is the entire point of `InputAction`.
