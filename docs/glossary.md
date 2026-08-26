# Glossary

One concept, one word, everywhere in the repository. `VocabularyTests` in
`GEngine.Architecture.Tests` fails the build when a synonym from the right-hand column shows up
in engine code.

## The words

| Word | What it means here | Never called |
|---|---|---|
| **body** | A thing the physics world simulates: a box with a velocity (`RigidBody2D`) | object, actor, thing, collider |
| **entity** | A named thing in a scene, built out of components | object, actor, thing, game object |
| **component** | A behaviour attached to an entity | behaviour, script, module |
| **scene** | The collection of entities being updated | world, level, stage |
| **level** | The tilemap and the things placed in it, loaded from a file | map, stage, scene |
| **frame** | One turn of the game loop | tick, iteration |
| **step** | One fixed-time slice of the simulation | tick, iteration, update |
| **fixed step** | The `1/60` s slice physics and gameplay run in | fixed tick, physics frame |
| **delta seconds** | How much time a frame or a step covers | dt, elapsed, timestep |
| **contact** | One touch between two bodies, this step | hit, collision event, touch |
| **normal** | Unit vector from the other body towards this one | direction, axis |
| **penetration** | How deep an overlap was before it was resolved | depth, intersection |
| **broad phase** | The pass that decides which pairs are worth testing | culling, partitioning |
| **narrow phase** | The pass that computes the exact answer for a pair | resolution, detection |
| **sweep** | A continuous test along a movement | cast, trace, raycast |
| **tile** | One cell of the level's collision grid | block, brick, square |
| **frame buffer** | The rectangle of pixels being drawn into | canvas, surface, bitmap |
| **pixel** | One colour in a frame buffer. Two per character cell | dot, point |
| **cell** | One character position in the terminal | character, glyph, tile |
| **sprite** | A named rectangle of pixels loaded from text | image, texture, bitmap |
| **present** | Putting a finished frame on a surface | flush, blit, draw, swap |
| **camera** | The rectangle of the world currently on screen | view, viewport |
| **action** | Something the player asks for: Jump, MoveLeft | command, binding, input |
| **backend** | One source of input, or one implementation of a platform contract | driver, provider, adapter |
| **driver** | The terminal, per platform (`IConsoleDriver`) | backend, terminal, console |
| **report** | The 64 bytes a HID device sends | packet, message, frame |
| **loop** | The game loop | main loop, run loop (except macOS, where CFRunLoop is Apple's word) |
| **suite** | Every test in one assembly | test project, fixture |
| **fixture** | A recorded input the tests replay | sample, data, mock |

## Two words that look like synonyms and are not

**Frame** and **step**. A frame is one turn of the loop and takes however long it takes. A step
is one fixed slice of simulated time and always covers exactly `1/60` s. One frame contains
zero, one or several steps. Everything reproducible lives in steps; everything that reacts to
the player lives in frames. Confusing the two is how a game ends up running faster on a faster
machine.

**Scene** and **level**. A level is a file: a tilemap and where things start. A scene is what is
running: the entities currently being updated. Loading a level fills a scene.

## Units, always in the name

Anything with a unit says so: `JumpVelocityPixelsPerSecond`, `RestingSpeedPixelsPerSecond`,
`FixedDeltaSeconds`, `MaximumFrameSeconds`, `RunLoopSliceSeconds`. A number called `speed` or
`delay` is a bug waiting for the second person to read it.

## Abbreviations, and the only ones allowed

`id`, `aabb`, `hid`, `rgb`, `fps`, `dt`. Everything else is spelled out - `index` not `i`,
`column` not `col`, `previous` not `prev`. The loop counters in the inner rendering loops are
`x` and `y` because that is what the axes are called.
