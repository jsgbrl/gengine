# GEngine.Testing

The test framework, written here because rule 2 forbids NuGet - and because a reader who
has seen these three hundred lines knows what xUnit is doing behind its magic.

```
[Test] / [TestCase] / [Setup] / [Skip]   attributes a test author writes
        |
TestDiscovery   reflection, once, sorted by name        -> TestCaseInfo
        |
TestExecutor    new instance per case, setup, invoke    -> TestResult
        |
TestSummary     counts, elapsed, exit code
        |
TestReport      colour via System.Console, no ANSI
```

Three ideas are worth the read:

- **A fresh instance per case.** `TestExecutor` calls `Activator.CreateInstance` for every
  case, so a field mutated by one test cannot reach the next. Shared state is the reason
  a suite passes alone and fails in a batch.
- **Discovery is sorted.** Reflection does not promise an order. Sorting by name makes two
  runs of the same code report the same sequence, which is what makes a diff of two runs
  meaningful.
- **Results are data.** Nothing prints until the run is over. That is what lets the
  framework test itself: `TestRunner.Execute` returns a `TestSummary` a test can assert on.

## Writing a test

```csharp
public sealed class JumpTests
{
    private ManualClock _clock = new();

    [Setup]
    public void Setup() => _clock = new ManualClock();

    [Test]
    public void Jump_WhenGrounded_LeavesTheFloor()
    {
        Assert.IsTrue(true, "the body left the floor");
    }

    [TestCase(0), TestCase(1), TestCase(2)]
    public void Rises_ForEveryFrameOfTheAscent(int frame)
    {
        Assert.IsInRange(frame, 0, 2);
    }
}
```

## Running

```
dotnet run tests.cs
dotnet run tests.cs --filter=Physics --verbose
dotnet run tests.cs --list
```

Exit code is 0 when nothing failed, 1 when something did, 2 on a bad argument. Skips are
counted separately and always print their reason: a silent skip reads green and covers
nothing.
