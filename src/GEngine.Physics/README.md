# GEngine.Physics

Boxes, and what happens when they meet. Depends on `GEngine.Core` and on nothing else.

## One step, six phases

```
1  kinematic bodies move            platforms go where the game said
2  passengers are carried           whoever was standing on one moves with it
3  dynamic velocities integrate     semi-implicit Euler: velocity first, then position
4  dynamic bodies move              swept, X then Y, against statics, kinematics and tiles
5  dynamic pairs are resolved       minimum translation, then an impulse
6  contacts are dispatched          enter, stay, exit - each exactly once
```

The split between phases 4 and 5 is the design decision worth arguing with. Against the
level, collision is **continuous** and resolved **one axis at a time**: that is what stops a
fast body tunnelling and what makes a platformer feel right against a wall. Against another
dynamic body it is **discrete**, because two moving boxes have no privileged axis.

## Why one axis at a time

Resolving both axes at once means choosing which of two overlaps to undo, and the wrong
choice snags a player on the seam between two floor tiles - you walk right, catch the
corner of the next tile, and get pushed up. Moving X, resolving X, then moving Y and
resolving Y never has to choose.

## Why swept and not "move then check"

Move a box a whole frame and then ask whether it overlaps anything, and at ten thousand
pixels a second it was on one side before the step and the other side after it, overlapping
nothing in between. `SweptAabb` asks a different question - along this movement, when is the
first touch? - and it has a closed form:

```
grow the obstacle by half the moving box   ->  the moving box becomes a point
the movement becomes a ray                 ->  the test becomes ray against box
a ray against an axis-aligned box          ->  the intersection of two intervals
```

Those intervals are the *slabs*. The axis whose interval starts last is the face that was
hit, which is where the contact normal comes from.

## Broad phase: two of them, on purpose

`BruteForceBroadPhase` compares every pair. `SpatialHashGrid` files bodies into square cells
and compares only bodies that share one. `BroadPhaseEquivalenceTests` asserts that they
return **exactly the same pairs in the same order** on four different layouts - which is the
whole claim an optimisation is allowed to make.

## Contacts

A contact is a touch **this step produced**: the swept pass stopped a movement, a
depenetration pushed a body out, or two boxes overlapped. A body at rest under gravity
pushes into the floor every step, so its contact is renewed every step. A body with no
gravity and no velocity, resting exactly against a surface, reports nothing further - see
`docs/physics.md`.

Tiles raise no contacts at all: a tile has no `RigidBody2D` to be the other half of one.
Anything the game needs to hear about - a question block, a breakable brick - is a body.

## The numbers that are not obvious

| Setting | Why it exists |
|---|---|
| `RestingSpeedPixelsPerSecond` | Below it a bounce is dropped. Without it, any restitution above zero means ever smaller bounces for ever, and what the player sees is a shiver. |
| `DepenetrationPasses` | A body can start a step already inside the level. The swept test cannot help - it answers "when does the movement first touch", and the answer is "before it started". |
| `MaximumBouncesPerStep` | A bounce a quarter of the way through a step leaves three quarters of the step to travel, the other way. Discarding that remainder is a quiet energy leak. |

A fixed step still loses a little energy per bounce: measured, a ball with restitution 0.95
returns to about 0.89 of its height rather than the exact 0.9025. That is discretisation,
not a bug, and halving the step halves the gap.

## Determinism

Bodies are iterated in identity order, contacts are stored in a list and not only in a set,
and no phase reads a clock or a random number. `PhysicsDeterminismTests` runs ten thousand
steps twice and compares one number - `StateHash()`, an FNV-1a over the raw float bits,
because the framework's own hash is seeded randomly at process start.
