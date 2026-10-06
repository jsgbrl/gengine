# Style

The rules this repository is written by, why each one exists, and where each is enforced.

There are four layers, and they are deliberately redundant:

| layer | what it catches | where |
|---|---|---|
| analyzers at maximum | everything Roslyn knows about | `src/Directory.Build.props`, `tests/Directory.Build.props` |
| `.editorconfig` | which rules apply and at what severity | `.editorconfig`, root |
| `dotnet format` | whitespace the compiler does not care about | `dotnet format gengine.slnx --verify-no-changes` |
| the house linter | the rules no analyzer has | `tests/GEngine.Architecture.Tests` |

Nothing is ever silenced by lowering a global property. `TreatWarningsAsErrors`,
`CodeAnalysisTreatWarningsAsErrors`, `EnableNETAnalyzers`, `AnalysisLevel=latest-all`,
`EnforceCodeStyleInBuild` and `GenerateDocumentationFile` are on everywhere, and
`PackagingRulesTests` fails the build if a project turns any of them down.

## The budgets

| thing | budget | measured as |
|---|---|---|
| file | 250 lines | every line, including blanks and comments |
| type | 150 lines | lines of code in one file: blanks and XML docs do not count |
| method | 20 lines | lines of code |
| nesting | 3 | blocks between a statement and the method it lives in |
| parameters | 4 | top-level commas in the declaration, plus one |

A type and a method are measured in **code**, a file in **lines**. Counting the XML documentation
`GenerateDocumentationFile` makes compulsory against a type budget would mean the budget punished
documenting; a file budget is about how far you have to scroll, and you scroll past comments too.

`NativeMethods.cs` is exempt from the length budgets. It is a transcription of somebody else's
header file, and splitting it would mean splitting one platform's ABI across files.

Interop files are exempt from the parameter budget. An IOKit callback takes seven arguments
whether we like it or not, and a callback with the wrong signature is a callback that is never
called.

## The rules with no analyzer

`StyleRulesTests` and `LayoutRulesTests` read the repository as text. They report file, line and
rule, and they are checked against files written to break them — a linter that cannot go red
looks exactly like a clean repository.

- the five budgets above
- no `this.` qualification
- no `#region`
- no nested ternaries
- no `TODO` or `FIXME`
- every file opens with one to three lines saying what it is for, or an XML `<summary>` on its
  one type — everything after the first break is the why, and the why takes as long as it takes
- one public type per file, named after the file
- no control characters in source: a NUL typed into a string literal instead of the escape makes
  the file binary to grep, diff and every pager, and is invisible in an editor

`IDE0003` and `IDE0009` were supposed to enforce the `this.` rule. **Measured:** they do not
report during `dotnet build` in .NET 10 even with an explicit severity, so the `dotnet_diagnostic`
lines that would claim otherwise are absent from `.editorconfig`, the four
`dotnet_style_qualification_*` options remain for the IDE, and the linter does the enforcing.

`IDE1006` was the opposite surprise. **Measured:** the per-rule `dotnet_naming_rule.*.severity`
is not what the build reads — without one explicit `dotnet_diagnostic.IDE1006.severity = error`,
every naming violation in the repository stayed an IDE-only hint.

## Vocabulary

One concept, one word, listed in [glossary.md](glossary.md). `VocabularyTests` reads that file,
so the table and the rule cannot drift: adding a row starts enforcing it, and deleting the last
use of a word makes the table's own test fail.

A synonym is banned in the project that owns the word, not everywhere. `surface` is wrong in
`GEngine.Rendering`, where the word is *frame buffer*, and right in `GEngine.Physics`, where a
surface is the thing you land on.

Abbreviations are a list, not a length. `id`, `aabb`, `hid`, `rgb`, `fps` and `dt` are allowed;
`idx`, `col`, `prev`, `pos`, `vel` and forty more are not. A length rule would ban `x`, `y`, `R`,
`G` and `B`, which are the names of the axes and the channels.

Two renames came out of writing this rule, and both were improvements: `Blit` became
`DrawPixels`, and `BitmapFont` became `PixelFont` — *pixel* is already this repository's word for
a coloured dot.

## Suppressions

Zero `#pragma warning disable` in the repository, and zero `[SuppressMessage]`.
`SuppressionRulesTests` fails the build on either. Every silenced rule lives in `.editorconfig`,
has a comment above it saying why, and appears in the table below — the test checks that too.

### Everywhere

| rule | what it wants | why it is off |
|---|---|---|
| **CA1303** | every literal in a `.resx` | Rule 8 makes the HUD, the menus and the test report English-only on purpose. A resource file would hide the very strings a reader of the rendering code is looking for. |
| **CA1062** | a null guard on every public reference parameter | Nullable reference types are on with warnings as errors, so passing null to a non-nullable parameter is already a compile error. Guards are still written by hand wherever a null can genuinely arrive: from reflection, from a file, or from an interface someone else implements. |
| **CA1031** | never catch `Exception` | Exactly two places do: the top-level game loop and the test runner. Both must restore the terminal and report the failure rather than die with a half-drawn screen. |
| **CA1716** | no identifier reserved in another .NET language | It would rename three words the engine is built on — the `Loop` namespace, `IPhysicsWorld.Step`, `ILogger.Error` — all reserved in Visual Basic, none in C#. The glossary wins over a courtesy nobody here can collect on. |
| **CA1848**, **CA1727** | logging source generators, template casing | `ILogger` here is our own three-method interface. There is no template. |
| **CA1051** | no visible instance fields | `Vector2` and `Color` are readonly value types whose `X`/`Y` and `R`/`G`/`B` *are* the data. Properties would add noise per member in the hottest code in the repository. |
| **CA1002** | `Collection<T>`, not `List<T>` | Every public `List<T>` is a caller-owned output buffer — `FindPairs(results)`, `Query(area, results)` — filled and reused so the step allocates nothing. `Collection<T>` would add a virtual call per element. |
| **CA1819** | arrays behind `IReadOnlyList` | A frame buffer is a flat array by definition, and an interface dispatch in the pixel-copying loop is the one cost this renderer cannot pay. |
| **CA2007** | `ConfigureAwait` | Only matters with a synchronization context. There is no `async` in the game loop at all. |
| **IDE0045**, **IDE0046** | collapse `if`/`else` into a ternary | Exactly the "clever" the style bans. |

### `src/**/Interop/**`

| rule | what it wants | why it is off |
|---|---|---|
| **SYSLIB1054** | `[LibraryImport]` | Its generated marshalling stubs need `AllowUnsafeBlocks`, and rule 6 bans `unsafe`. `[DllImport]` stays. |
| **CA5392** | `DefaultDllImportSearchPaths` | Guards against loading an attacker's library by name from a writable directory. Every import here names a signed OS library already mapped into the process: `kernel32`, `hid`, `setupapi`, `libc`, IOKit. |
| **CA1401** | no public P/Invokes | Ours are not public — `NativeMethods` is internal — but the rule reports on internal members too, so it is scoped off rather than fought member by member. |

### `tests/**`

| rule | why it is off |
|---|---|
| **CA1812**, **IDE0051** | test classes and methods are found by reflection; the analyzer cannot see the caller |
| **CA1822** | a test method must stay an instance method — the runner builds one instance per case so no test sees what the last one left in a field |
| **CA1806** | a test asserting a constructor throws has to call it and drop the result |
| **CA1707** | test names read as sentences: `Jump_WhenGrounded_LeavesTheFloor` |
| **CA1001** | a test class holding a disposable is never disposed; that is the design |
| **CA2000** | some tests are about what happens *without* disposal |
| **CS1591** | XML docs are required on the engine's public surface, not on test methods |

## The final pass

Every file was read once more as a reader rather than as an author, after everything passed.
What that pass changed is listed at the end of the build report.
