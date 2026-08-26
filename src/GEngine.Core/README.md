# GEngine.Core

The layer that references nothing. Every other project points at this one and none of them
points at each other, which is what `DependencyRulesTests` checks on every build.

Core holds three kinds of thing:

| Kind | What is in it |
|---|---|
| Vocabulary | `Vector2`, `Aabb`, `Color`, `MathG` - the words the whole repository speaks |
| Machinery | the game loop, the scene graph, and four patterns worth naming |
| Contracts | `IRenderer`, `IInputBackend`, `IPhysicsWorld`, `IClock`, `IAssetSource`, `ILogger`, `IPlatformProbe` |

## The loop, in nine lines

```
frame      = clamp(now - previous, 0, MaximumFrameSeconds)
accumulator += frame
record(frame)
Update(frame)                       <- input, animation, anything frame-rate dependent
while accumulator >= fixed:         <- physics, gameplay rules, anything reproducible
    FixedUpdate(fixed)
    accumulator -= fixed
Render(accumulator / fixed)         <- draw, using the leftover to interpolate
pace()                              <- sleep most of the wait, spin the last few ms
```

The clamp is the whole guard against the *spiral of death*: a frame that really took two
seconds is reported as a quarter of a second, so the accumulator can never queue up more
steps than the machine can run before the next frame.

## Why the axes point down

`Vector2.Y` grows downwards, the way screen rows do. Gravity is therefore a **positive** Y,
`Aabb.Top` is the **smaller** Y, and no conversion happens between the tilemap, the physics
and the frame buffer. Pick the other convention and every one of those three places needs a
flip, and one of them will be forgotten.

## Contracts, kept deliberately thin

`IPhysicsWorld` has two members, not twenty. Core describes only what the composition root
needs in order to wire a game together; the rest of a module's API lives on its concrete
type, because describing a raycast here would mean describing a rigid body here, and Core
would have swallowed the module it exists to stay independent of.

## Patterns, and where they are

| Pattern | Here |
|---|---|
| Game Loop | `Loop/GameLoop.cs` |
| Update Method | `Scenes/Component.cs` |
| Template Method | `Scenes/Component.cs` - the base owns the sequence, the subclass fills in steps |
| Component | `Scenes/Entity.cs` |
| Observer | `Patterns/EventBus.cs` |
| State | `Patterns/StateMachine.cs` |
| Object Pool | `Patterns/Pool.cs` |
| Service Locator | `Patterns/ServiceRegistry.cs` - with its warning attached |
| Factory | `Platform/PlatformFactory.cs` |

## Determinism

Nothing in Core reads `DateTime.Now` or `Environment.TickCount`. Time arrives through
`IClock`; hand a `ManualClock` to a game loop and the same input produces the same frames,
which is what makes the replay test in `MarioClone.Tests` possible at all.
