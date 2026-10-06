# Tests

1 013 tests, no test framework from NuGet, and a linter that reads the repository as text.

```bash
dotnet run tests.cs                        # all of them
dotnet run tests.cs -- --filter=Physics    # one suite, or one class, or one method
dotnet run tests.cs -- --list              # names only, nothing run
dotnet run tests.cs -- --verbose           # every test, not just the failures
```

Exit code 0 when everything passed, 1 when something failed, 2 when an argument was wrong.

## Why there is a framework in here at all

Rule 2 says zero NuGet packages, in the whole repository, tests included. That rules out xUnit, NUnit
and MSTest, so `GEngine.Testing` is eighteen files that do the parts of xUnit this project uses.

It is not a hardship, and writing it turned out to be the best possible introduction to the rest:
discovery is `Assembly.GetExportedTypes` and an attribute, running a test is `MethodInfo.Invoke`
inside a `try`, and an assertion is an `if` and a `throw`. A test framework is a small program, and
knowing that changes how you read one.

## Writing a test

A public class whose name ends in `Tests`, with public methods marked `[Test]`:

```csharp
public sealed class GravityTests
{
    [Test]
    public void ABodyFalls()
    {
        var world = new PhysicsWorld(PhysicsSettings.Default, new BruteForceBroadPhase());
        var body = new RigidBody2D(BodyType.Dynamic, Vector2.Zero, new Vector2(2.0f, 2.0f));
        world.Add(body);
        world.Step(1.0f / 60.0f);
        Assert.IsTrue(body.Position.Y > 0.0f, "down is positive Y");
    }
}
```

`[TestCase(…)]` runs the same method with different arguments. `[Setup]` marks a method that runs
before each test. `[Skip("reason")]` skips one, and the reason is printed — a skipped test with no
reason is a test nobody will ever come back to.

**One instance per test case.** The runner builds a new instance for every case, which is why no test
can see what the last one left in a field, and why `CA1822` (make it static) is turned off for test
projects.

## The assertions

```
Assert.IsTrue / IsFalse           with a message that says what was expected
Assert.AreEqual / AreNotEqual     value equality
Assert.AreSame                    reference identity — the flyweight tests need this
Assert.IsNull / IsNotNull
Assert.Throws<TException>         returns the exception, so the message can be checked
Assert.ApproximatelyEqual         floats, with an explicit tolerance
Assert.IsInRange / IsFinite
Assert.MatchesSnapshot            two strings, reported as the first differing line
```

`ApproximatelyEqual` takes the tolerance as an argument rather than picking one. A tolerance the test
did not choose is a test that passes for a reason nobody wrote down.

`MatchesSnapshot` exists for `AsciiSnapshot`: when a rendering test fails it prints the line number
and both pictures, so the failure is legible instead of being a pixel index.

## What is tested, per project

| suite | files | what it is mostly about |
|---|---|---|
| `GEngine.Core.Tests` | 28 | vectors and boxes to the last edge case; the loop against a `ManualClock` |
| `GEngine.Physics.Tests` | 22 | tunnelling, resting, restitution, one-way, tiles, determinism, broad-phase equivalence |
| `GEngine.Rendering.Tests` | 22 | the frame buffer, the sprite parser, the three colour depths, the ANSI bytes themselves |
| `GEngine.Input.Tests` | 26 | actions and decay; every DualSense button against a captured report |
| `GEngine.Testing.Tests` | 11 | the framework testing itself, including a deliberately failing case |
| `MarioClone.Tests` | 28 | the four mercies, every actor, and the replay |
| `GEngine.Architecture.Tests` | 25 | the repository itself: style, vocabulary, platform, dependency, packaging, suppression |

### The tests worth reading first

- **`ReplayTests`** — one recorded run of world 1-1, played back twice: once as key presses, once as
  64-byte HID reports. Both reach the flag with the same score, which is the proof that nothing
  between the device and the player knows which device it was.
- **`PhysicsDeterminismTests`** — the same simulation, twice, hashed. Bit-identical.
- **`BroadPhaseEquivalenceTests`** — brute force and the spatial hash return the same pairs, which is
  what makes the choice between them a performance decision.
- **`DualSenseHardwareTests`** — twenty-three reports captured from a real controller, one button at
  a time.
- **`AtTenThousandPixelsASecond_ABodyDoesNotPassThroughAOnePixelWall`** — the reason the narrow phase
  is swept.

## Testing things that have no business being testable

Four seams, each of which exists so that something untestable becomes ordinary:

| | replaces | so that |
|---|---|---|
| `ManualClock` | the system clock | "advance one second" means exactly sixty fixed steps |
| `HeadlessRenderer` | the terminal | a gameplay test needs no screen |
| `FakeHidBackend` / `FakeHidDevice` | a plugged-in controller | the gamepad is tested with no gamepad |
| `FixedPlatformProbe` | the operating system | the macOS path is exercised on Windows |

The doubles for HID ship **in the engine**, not in the test project, so that somebody writing a game
against gengine can test their own input handling without a controller. That also means they need
tests of their own — a bug in a double is a test that passes while the engine is broken — which is
what `FakeHidBackendTests` and `FakeHidDeviceTests` are for.

## The house linter

`GEngine.Architecture.Tests` references no engine project. It reads the repository as **text**,
because a rule saying "nobody may reference Physics" must not itself reference Physics.

| class | what it enforces |
|---|---|
| `StyleRulesTests` | the five budgets: file 250, type 150, method 20, nesting 3, parameters 4 |
| `LayoutRulesTests` | no `this.`, no `#region`, no nested ternaries, no `TODO`, one public type per file, every file opens by saying what it is for |
| `VocabularyTests` | one concept one word, read from `docs/glossary.md` |
| `PlatformRulesTests` | no `#if`, no runtime identifiers, no backslash paths, no single-system API outside its backend |
| `DependencyRulesTests` | the arrows in `docs/architecture.md`, checked against the `.csproj` files |
| `PackagingRulesTests` | no `PackageReference` anywhere, and no project turning the analyzers down |
| `SuppressionRulesTests` | zero `#pragma warning disable`; every silenced rule has a reason and appears in `docs/style.md` |
| `ApiCoverageTests` | every public type has a test class, with a justified exemption list |

### A linter that cannot go red

Each rule is also run against source written to break it, because **a rule that reads a file it
cannot parse reports nothing and passes** — which looks exactly like a clean repository.

That is not hypothetical. This one was found by running the numbers:

```
Repository.Root = C:\bin\projects\claude\gengine   sources = 0   projects = 0
```

The generated-file filter tested for `\bin\` in the **absolute** path. This repository lives in a
folder called `bin`, so every file in it was excluded, and every rule passed on an empty set. It
now filters on the path relative to the root, and `TheLengthRuleFindsALongMethod` and its siblings
would have caught it in the first place.

## What the tests found

A test suite is worth what it catches. Some of the bugs in this repository that were found by a test
rather than by playing:

- A ball with `e = 0.95` lost 12 % of its energy per bounce instead of 9.75 %, because the solver
  discarded the rest of the step after a bounce instead of spending it with the new velocity.
- One-way platforms pushed bodies *out* sideways, because the discrete solver ran on them too.
- `ManualClock.Advance(TimeSpan.FromSeconds(1.0/60))` rounded, so sixty advances produced fifty-nine
  fixed steps.
- The frame-rate counter reported 1 364 fps for one frame, because exponential smoothing was seeded
  from a near-zero first frame.
- 16-colour mapping turned `(120, 120, 255)` white and `(90, 90, 90)` black.
- The level loader silently deleted every floor row, because `#` was both the ground and the comment
  marker.
- A goomba never turned around at a tile wall, because tiles raise no contacts.
- `SpatialHashGrid` allocated a closure per body per step, in the one place the engine promises not
  to allocate.

Every one of those has a test now, and every one of those tests goes red if the fix is removed.
