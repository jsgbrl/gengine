# Patterns

Nine design patterns, each with the file and line where it lives and — more usefully — the problem
in *this* engine that made it worth the indirection. A pattern applied without a problem is just a
longer way to write the code.

| pattern | where | what it solves here |
|---|---|---|
| Game Loop | [`GameLoop.cs:1`](../src/GEngine.Core/Loop/GameLoop.cs) | two clocks that must not be the same clock |
| Update Method | [`Component.cs:1`](../src/GEngine.Core/Scenes/Component.cs) | everything that moves, moving once per step |
| Component | [`Entity.cs:1`](../src/GEngine.Core/Scenes/Entity.cs) | a goomba is a body plus a behaviour, not a subclass of something |
| Object Pool | [`Pool.cs:15`](../src/GEngine.Core/Patterns/Pool.cs) | sixty allocations a second is a collection mid-jump |
| Flyweight | [`SpriteAtlas.cs:15`](../src/GEngine.Rendering/SpriteAtlas.cs) | a hundred coins are one sprite |
| Command | [`InputAction.cs:11`](../src/GEngine.Core/Contracts/InputAction.cs) | the game must not know which device it is |
| Observer | [`EventBus.cs:16`](../src/GEngine.Core/Patterns/EventBus.cs) | the score must not be a field the physics can reach |
| State | [`StateMachine.cs:17`](../src/GEngine.Core/Patterns/StateMachine.cs) | six states and the transitions nobody thought about |
| Strategy | [`IBroadPhase.cs:11`](../src/GEngine.Physics/BroadPhase/IBroadPhase.cs) | two ways to find pairs, proven to agree |
| Abstract Factory | [`PlatformFactory.cs:10`](../src/GEngine.Core/Platform/PlatformFactory.cs) | three systems, one binary, no `#if` |
| Service Locator | [`ServiceRegistry.cs:15`](../src/GEngine.Core/Patterns/ServiceRegistry.cs) | provided, and argued against, below |

## Game Loop

`GameLoop.Tick` is eight lines and every one of them is a decision. See
[the README](../README.md#how-the-game-loop-works) for the loop itself; what belongs here is why it
is a *class* rather than a `while (true)` in `Main`.

Because the loop is the one piece of the engine a test cannot run in real time. `GameLoop` takes an
`IClock`, and `ManualClock` lets a test say "advance exactly one second" and assert that sixty fixed
steps happened. A `while (true)` around `Stopwatch` cannot be asked that question.

## Update Method

Every `Component` gets `Update(deltaSeconds)` and `FixedUpdate(fixedDeltaSeconds)`, and a `Scene`
calls them in order. The interesting part is what a scene does **not** do: it never mutates its own
list while iterating it. Additions and removals queue, and `ApplyPendingChanges` runs between
passes.

That is not caution. A goomba that dies during `FixedUpdate` removes itself, and removing itself
from the list being iterated is the crash that only happens when two goombas die on the same frame.

## Component

An `Entity` is a name and a list of `Component`s. A goomba is an entity holding a `Goomba`, which
holds a `RigidBody2D`.

The alternative — `class Goomba : MovingThing : Thing` — works until the fourth kind of enemy needs
two of the three behaviours in the middle of the chain. Composition has no middle of the chain.

`MarioClone` uses a thin version of this: `Actor` is a `Component`, and every actor is one. The full
version, where a goomba is three unrelated components, buys flexibility this game does not need —
and the engine does not force it.

## Object Pool

Sixty frames a second times one allocation per spawned coin is three thousand six hundred objects a
minute for the collector to trace and free. None of them is large, and that is the point: the cost
is not the memory, it is the **pause**. A generation-zero collection in the middle of a jump is a
dropped frame the player feels.

`Pool<TItem>` hands out objects and takes them back. What it deliberately does not do is reset them:
the caller knows what "empty" means for its own type, and a pool that guesses is a pool that leaks
state between uses.

## Flyweight

`SpriteAtlas` loads a sprite once and returns **the same object** every time. `Sprite` is immutable,
so this is safe, and `SpriteAtlasTests` asserts `AreSame` rather than `AreEqual` — because
"equal but separate" would mean a hundred coins are a hundred copies of the same twenty-five pixels.

Mirrored sprites are cached separately and just as aggressively. A player running left is the same
seven sprites as a player running right, flipped once each, ever.

## Command

The whole input system is this pattern reduced to its smallest possible form: an `enum`.

```csharp
public enum InputAction { MoveLeft, MoveRight, Jump, Run, Pause, Confirm, Cancel, … }
```

The classical Command pattern gives you an object with an `Execute` method. Here the "object" is an
enum member and the "execute" is whatever the game decides `Jump` means this frame. The pattern's
actual value is not the object — it is the **indirection**: the game never asks "is space down?",
so a keyboard and a DualSense are interchangeable without the game knowing either exists.

`ReplayTests` is the proof. One recorded run plays through `ConsoleKeyboardBackend` and through
`DualSenseGamepad`, and both reach the flag with the same score. If any code between the device and
the player knew which device it was, that test would fail.

## Observer

`EventBus` is a dictionary from event type to a multicast delegate. It exists so that collecting a
coin can add to the score without `Coin` holding a reference to `GameSession`.

Its semantics are deliberately those of a C# multicast delegate, including the sharp edge:
subscribing the same handler twice calls it twice. That is what `+=` does, and pretending otherwise
would mean a bus that behaves differently from the language it is written in.

`MarioClone` uses direct calls rather than the bus for the score, because with nine actor types the
indirection costs more clarity than it buys. The bus is in the engine for games with more actors
than this one — and the honest note is worth more than a demonstration.

## State

Six states, and a **table of legal transitions** rather than a pile of `if`s:

```csharp
machine.AddTransition(GameStateKind.Playing, GameStateKind.Paused);
machine.AddTransition(GameStateKind.Paused, GameStateKind.Playing);
machine.AddTransition(GameStateKind.Paused, GameStateKind.Title);
```

`TryTransitionTo` refuses anything not in the table. Unpausing into a death, restarting from the
title, finishing a level that has not started — all of these are refused by the machine rather than
being a bug nobody finds until somebody pauses at exactly the wrong moment.

`TimeInStateSeconds` is part of the machine because three of the six states are "hold this screen
for a moment, then move on", and a timer per state is three fields that can drift out of step.

## Strategy

`IBroadPhase` has two implementations: `BruteForceBroadPhase` (O(n²), fifteen lines, obviously
correct) and `SpatialHashGrid` (O(n) for a level-shaped world, ninety lines, correct for reasons you
have to think about).

The pattern's value here is a test, not an interface:

```
BroadPhaseEquivalenceTests: for the same world, both return the same set of pairs.
```

With that test, swapping implementations is a **performance** decision. Without it, it is a
behaviour decision disguised as a performance one, and the disguise holds until a level is big
enough to matter.

## Abstract Factory

`PlatformFactory.Create(probe, choices)` takes three thunks and an `IPlatformProbe` and returns one
of them. `ConsoleDriverFactory` and `HidBackendFactory` are each four lines on top of it.

This is how rule 4 is kept — one binary, zero `#if`, zero runtime identifiers. The choice is made at
run time from `OperatingSystem.IsWindows()` and friends, which are ordinary methods, so all three
branches compile on all three systems and `PlatformRulesTests` can fail the build on a stray `#if`.

An unknown system is **refused by name** rather than guessed at. Guessing means a fourth system
silently gets whichever driver happened to be last in the list, and finding out during a game.

## Service Locator, with its warning

`ServiceRegistry` maps a type to an instance. It is in the engine because a course on patterns that
omits the controversial one is teaching a shorter list, not a better one.

It is also the one pattern here that should be reached for last. A constructor parameter says what a
class needs in a way the compiler checks; a service locator says it in a way that fails at run time,
in whatever order the code happens to run, on whichever machine happens to be missing the
registration. `MarioClone` uses constructor parameters and `MarioGameSettings` everywhere, and
touches the registry nowhere.

Use it when a genuinely cross-cutting service — logging is the usual honest example — would
otherwise be threaded through forty constructors that do not care about it. That is a real problem
and this is a real answer to it. Everything else is an argument for a parameter.
