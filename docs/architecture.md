# Architecture

## The shape

```
              +---------------+
              |  MarioClone   |   knows all four, wires them together
              +-------+-------+
         +------------+------------+
         v            v            v
     Physics      Rendering      Input      each knows only Core
         +------------+------------+
                      v
                 GEngine.Core       knows nobody
```

`GEngine.Testing` sits outside this graph entirely: it is the test framework, and it has to be
able to test `Core` itself.

`DependencyRulesTests` reads every `.csproj` and fails the build when an arrow is added that is
not in the picture. It is not bureaucracy - it is the working example of dependency inversion
that this document is about, kept honest by a test rather than by good intentions.

## Why Core depends on nobody

Because everything else is a choice, and Core is what the choices have in common.

`Core` declares `IRenderer`, `IInputBackend`, `IPhysicsWorld`, `IClock`, `IAssetSource`,
`ILogger`, `IPlatformProbe`. It does not know that a renderer might be a terminal, that input
might be a DualSense, or that physics might use a spatial hash. Those are decided by
`MarioClone` at startup, which is the only place in the repository that names all four modules
in one file.

The payoff is not architectural purity, it is that:

- the whole game runs headless, because `HeadlessRenderer` is an `IRenderer` too;
- the whole game runs from a script, because `FakeInputBackend` is an `IInputBackend` too;
- the whole game runs deterministically, because `ManualClock` is an `IClock` too.

Every one of those is a test that exists.

## The contracts are deliberately thin

`IPhysicsWorld` has two members - gravity and one step - not twenty. Raycasts, overlap queries
and adding bodies live on the concrete `PhysicsWorld`.

That is on purpose. Core describes exactly what the composition root needs in order to wire a
game together, and no more. Describing a raycast in Core would mean describing a rigid body in
Core, and Core would have swallowed the module it exists to stay independent of.

The same rule shapes `IRenderer`: it takes an `IPixelSource` and presents it. It cannot draw a
sprite, because a sprite is a rendering concept, and a renderer that could draw would be a
renderer nobody could replace.

## The three clocks of a frame

```
Update(deltaSeconds)        variable   input, animation, anything frame-rate dependent
FixedUpdate(1/60)           fixed      physics, gameplay rules, anything reproducible
Render(interpolation)       none       drawing, and it changes no state at all
```

This split is the single most important idea in the engine, and every other decision leans on
it. `docs/physics.md` explains what it buys the simulation; the short version is that a replay
is only possible because the same input produces the same sequence of `FixedUpdate` calls on
every machine.

## Platform differences, without conditional compilation

Rule 4 of the build prompt bans `#if`. Every platform difference is therefore a value chosen at
runtime:

```csharp
var choices = new PlatformChoices<IConsoleDriver>(
    () => new WindowsConsoleDriver(logger),
    () => new MacOsConsoleDriver(logger),
    () => new LinuxConsoleDriver(logger));

IConsoleDriver driver = PlatformFactory.Create(probe, choices);
```

Because the probe is an interface, a test on any machine can ask what the engine would do on
the other two - and `PlatformRulesTests` fails the build if a `#if` or a backslash path ever
appears.

## Where each module's story is told

| Module | Read |
|---|---|
| `GEngine.Core` | `src/GEngine.Core/README.md` |
| `GEngine.Physics` | `src/GEngine.Physics/README.md`, `docs/physics.md` |
| `GEngine.Rendering` | `src/GEngine.Rendering/README.md`, `docs/rendering.md` |
| `GEngine.Input` | `src/GEngine.Input/README.md`, `docs/input-and-gamepad.md` |
| `GEngine.Testing` | `src/GEngine.Testing/README.md`, `docs/tests.md` |
| `MarioClone` | `src/MarioClone/README.md` |
