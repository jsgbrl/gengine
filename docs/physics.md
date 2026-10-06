# Physics

Boxes, and what happens when they meet. Everything is an axis-aligned bounding box — no circles, no
rotation, no joints — because a platformer needs none of those and every one of them costs a page of
maths that would have to be read before the interesting part.

The screen's Y axis points **down**. `Top` is a smaller number than `Bottom`, gravity is positive,
and a jump velocity is negative. This is stated once here and once in `Aabb`, and then never
apologised for again.

## One step, six phases

```
1  kinematic bodies move            platforms go where the game said
2  passengers are carried           whoever was standing on one moves with it
3  dynamic velocities integrate     semi-implicit Euler
4  dynamic bodies move              swept, X then Y, against statics, kinematics and tiles
5  dynamic pairs are resolved       minimum translation, then an impulse
6  contacts are dispatched          enter, stay, exit - each exactly once
```

```csharp
SwapContactBuffers();
MoveKinematicBodies(fixedDeltaSeconds);
ResetGroundFlags();
IntegrateDynamicBodies(fixedDeltaSeconds);
BroadPhase.Update(_bodies);
MoveDynamicBodies(fixedDeltaSeconds);
BroadPhase.Update(_bodies);
BroadPhase.FindPairs(_pairs);
ResolvePairs();
DispatchContacts();
StepCount++;
```

The order is the design. Phase 1 before phase 4 means a passenger is carried *before* it decides
where it is going, so a player standing on a rising platform is lifted rather than crushed. Phase 6
last means every callback sees the world as it finally is, not as it was halfway through being
solved.

## The three kinds of body

| | moved by | collides with | example |
|---|---|---|---|
| `Static` | nobody | dynamics | the ground, a brick |
| `Kinematic` | the game, directly | dynamics | a moving platform |
| `Dynamic` | the solver | everything | the player, a goomba |

A kinematic body is not a dynamic body with infinite mass. It is a body whose position the *game*
owns: the solver never pushes it, and a dynamic body that ends up inside one is pushed out of it.
That asymmetry is what makes a platform a platform rather than a very heavy crate.

## Integration: semi-implicit Euler

```
v' = v + a·dt
p' = p + v'·dt        ← the new velocity, not the old one
```

Explicit Euler uses the *old* velocity for the position update. The difference is one character and
it decides whether energy is conserved. Explicit Euler adds energy every step — an orbit spirals
outward, a bouncing ball climbs — and symplectic Euler does not.

The visible consequence in this engine: `ABodyAtRestOnTheFloor_DoesNotSinkOrShiverAfterTenThousandSteps`
passes. With explicit Euler it does not.

Velocity is clamped to `RigidBody2D.MaxVelocity` after integration, not before. Terminal velocity is
a property of the body, not of gravity, and clamping before would mean acceleration silently
disappearing.

## Broad phase: which pairs are worth testing

Testing every pair is `n(n-1)/2` tests. For world 1-1's nine actors that is thirty-six, which is
nothing; for a bullet-hell it is the whole frame.

`IBroadPhase` is a Strategy with two implementations, and a test proves they agree:

**`BruteForceBroadPhase`** — every pair, fifteen lines, obviously correct. The reference.

**`SpatialHashGrid`** — each body is inserted into every cell its box overlaps:

```
cell(x, y) = (floor(x / cellSize), floor(y / cellSize))
```

Pairs are then only formed within a cell. Two bodies spanning several shared cells would be reported
several times, so the pair set is deduplicated — the grid returns a *set*, which is exactly the
property `BroadPhaseEquivalenceTests` needs in order to compare it with brute force.

An early version built the cell lists with `cells.ForEach(cell => cell.Add(body))`. That closure
allocated once per body per step: three thousand six hundred allocations a minute, in the one place
the engine promises not to allocate. It is now plain nested loops and a private `CellRange` struct.

**Choosing a cell size.** Roughly the size of the largest thing that moves. Too small and a body
spans many cells; too large and every body shares one cell and the grid is brute force with extra
steps. `MarioFactory` uses four tiles.

## Narrow phase: swept AABB by the slab method

### Why swept

"Move, then check for overlap" fails at speed. A body travelling 10 000 px/s covers 166 px in one
fixed step; a 1 px wall is not there when the check runs, and never was.

`AtTenThousandPixelsASecond_ABodyDoesNotPassThroughAOnePixelWall` is the test that would go red.

### The Minkowski trick

Expand the stationary box by the moving box's **half-size** on every side. The moving box now has
nowhere to be but its own centre, so the question becomes: *does this ray hit this box, and when?*

```
     moving box                  expanded target
     ┌───┐                    ┌───────────────┐
     │ ● │  ───────►          │   ┌───────┐   │
     └───┘                    │   │target │   │      ● is now a point
                              │   └───────┘   │
                              └───────────────┘
```

### The slabs

A box is the intersection of one slab per axis. A ray is inside the box exactly when it is inside
every slab at once.

```
tEnter(axis) = (near(axis) - origin(axis)) / direction(axis)
tExit(axis)  = (far(axis)  - origin(axis)) / direction(axis)
```

If `direction(axis)` is negative, near and far swap. Then:

```
tEnter = max over axes of tEnter(axis)     the last axis to be entered
tExit  = min over axes of tExit(axis)      the first axis to be left

hit  ⇔  tEnter ≤ tExit  and  0 ≤ tEnter ≤ 1
```

The axis that produced the largest `tEnter` is the axis of the collision, and the sign of the
movement along it gives the normal.

**A zero direction component is not a special case.** `x / 0.0f` in IEEE 754 is ±infinity, and
infinity is exactly the right answer for "this ray never enters that slab". Guarding against it with
an `if` produces the same result and one more branch in the hottest loop in the engine.

## Resolution

### Against the level: one axis at a time

Move X. Resolve X. Then move Y. Resolve Y.

Resolving both axes at once means choosing which of two overlaps to undo, and the wrong choice is
the classic platformer bug: you walk right along a floor made of tiles, catch the corner of the next
tile, and get launched upward. Separating the axes never has to choose, because after the X pass
there is no X overlap left to confuse the Y pass.

### Against another dynamic body: discrete

Two moving boxes have no privileged axis, so the swept answer would be arbitrary. Instead they are
separated by the **minimum translation vector** — the smallest push along the axis of least overlap
— and then an impulse is applied.

### Restitution and the resting cut-off

```
v' = -v · e
```

with

```
if |approach speed| ≤ RestingSpeedPixelsPerSecond then v' = 0
```

Without the cut-off a ball with `e = 0.9` never stops. Each bounce is smaller, the numbers get
smaller, and the ball shivers on the floor for ever at sub-pixel amplitude — visibly, because the
renderer rounds.

`RestitutionOfOne_SendsTheBodyBackUpAtTheSpeedItArrived` pins the other end: energy is conserved
exactly when it should be.

One subtlety worth the twelve lines it costs: after a bounce, the rest of the step is **spent with
the new velocity**, up to `MaximumBouncesPerStep` times. An earlier version discarded the remainder,
which lost about 12 % of the energy per bounce instead of the 9.75 % that `e = 0.95` actually
implies. The test that caught it measures bounce heights across ten bounces.

### Friction (Coulomb)

```
vt' = vt - min(|vt|, μ·|vn|) · sign(vt)
```

Tangential velocity is reduced by an amount proportional to the normal force, and never past zero —
the `min` is what stops friction reversing a slide, which looks like the floor pushing back.

### Depenetration

A body that starts a step already inside something — spawned there, or grown there by a mushroom —
is pushed out along the shallowest axis, up to `DepenetrationPasses` times.

Push-out raises a contact, deliberately. An earlier version separated silently, and a player spawned
inside a door was never told it had touched it.

## One-way platforms

Decided entirely by the swept pass, which knows the direction of travel:

```
caught  ⇔  moving downward
       and  previous bottom edge ≤ platform top
```

Everything else passes through. And the discrete solver **skips one-way pairs altogether** — an
earlier version ran the ordinary resolution on them too, which shoved players sideways out of
platforms they were jumping up through.

## Tiles

A level is 140 columns wide. As static bodies that is over a thousand boxes in the broad phase every
step, for a floor that never moves.

Instead `ITileCollisionSource` is a grid, and the sweep asks it directly along the path actually
travelled — a handful of array lookups. `TileSweep` walks the cells the movement crosses, in order,
and stops at the first solid one.

The tilemap and the bodies are two separate passes on purpose. A tile has no identity, no velocity
and no callbacks; giving it those in order to unify the code would mean paying for them a thousand
times over.

## Determinism

`PhysicsWorld` reads no clock and no random number. Given the same bodies and the same sequence of
`Step(dt)` calls it produces bit-identical results, on any machine.

`PhysicsDeterminismTests` proves it with an FNV-1a hash over every body's position and velocity. The
framework's own `HashCode` is **randomly seeded per process** and would give a different answer on
every run — which is the correct behaviour for a hash table and useless for this.

```
hash = 14695981039346656037
for each byte b:
    hash = (hash XOR b) * 1099511628211
```

`examples/02-bouncing-balls.cs` prints that hash. Run it twice; the number is the same.

## Constants worth knowing

| | default | why |
|---|---|---|
| `Gravity` | `(0, 900)` px/s² | tuned by feel, not by physics; `MarioClone` uses 460 |
| `RestingSpeedPixelsPerSecond` | 25 | below this a bounce becomes a stop |
| `MaximumBouncesPerStep` | 4 | a corner can legitimately bounce twice; four is slack |
| `DepenetrationPasses` | 4 | enough to escape a corner, few enough to stay bounded |
