# Testing

## Summary

Verification runs at two levels. `SokobanCLI.UnitTests` covers behaviour that can be exercised without a console — movement rules, the tile factory, event arguments, and geometry. `SokobanCLI.ArchitectureTests` covers structure — layer dependencies, project and solution conventions, naming, layout, member shape, collection shape, error handling, specialised types, and test-source conventions.

Verified counts on 2026-11-16:

```
Passed! - Failed: 0, Passed: 101, Skipped: 0, Total: 101 - SokobanCLI.UnitTests.dll (net10.0)
Passed! - Failed: 0, Passed: 138, Skipped: 0, Total: 138 - SokobanCLI.ArchitectureTests.dll (net10.0)
```

Run both with:

```bash
dotnet test SokobanCLI.slnx
```

Neither project is packable (`IsPackable=false`), both set `ExcludeFromCodeCoverage=true`, and no coverage tool is configured, so there is no coverage figure to quote and no coverage gate.

## Unit test project

Location: `SokobanCLI.UnitTests/`, mirroring the production namespace and folder structure.

| Fixture | Tests | Coverage |
|---------|-------|----------|
| `GameLogic/Managers/GameManagerTests` | 23 | Start/stop state, zero-delta rejection, wall rejection, empty and goal acceptance, push onto empty, push onto goal producing `5`, push blocked by wall, box-on-goal pushed onto empty restoring the goal, box-on-goal pushed onto a goal, event firing with the expected position, both completion outcomes |
| `Graphics/Geometry/Rectangle2DTests` | 20 | Construction, `Location`/`Size` projections, `Bottom`/`Left`/`Right`/`Top`, `IsEmpty`, equality and hash code, `Contains` for point, rectangle, and coordinates |
| `Graphics/Geometry/Scale2DTests` | 13 | Construction from floats, point, and size, `Empty`, `One`, equality, addition, subtraction |
| `Graphics/Geometry/Point2DTests` | 11 | Construction, `Empty`, equality and operators, addition, subtraction |
| `Graphics/Geometry/Size2DTests` | 11 | As above for size |
| `Models/TilesTests` | 7 | `Tiles.ById` for identifiers 0–5 plus an unknown identifier, asserting `Id`, `Character`, and `Colour` |
| `Graphics/Geometry/Mapping/*MappingExtensionsTests` | 12 (4 each) | Round-trip conversions to and from `System.Drawing` types |
| `Input/Events/ConsoleKeyEventArgsTests` | 2 | Constructor stores the key |
| `GameLogic/Events/PlayerMovementEventArgsTests` | 2 | Constructor stores the location, including the empty location |
| `GameLogic/Managers/IPlayerMovedSpy` | 0 | Interface used as a `Mock<T>` target for event verification |

### How `GameManagerTests` arranges boards

The fixture constructs a `GameManager`, assigns `PlayerMoved = delegate { }` to avoid null dispatch, and calls `Start(0)`. Level 0 is loaded from the linked `Levels/0.lvl` in the test output directory. Individual tests then write directly into the grid:

```csharp
game.World.Tiles[7, 6] = Tiles.ById(2); // box ahead of player
game.World.Tiles[7, 7] = Tiles.ById(0); // empty tile behind box

game.MovePlayer(0, 1);

Assert.That(game.World.Tiles[7, 6].Id, Is.EqualTo(0)); // vacated
Assert.That(game.World.Tiles[7, 7].Id, Is.EqualTo(2)); // box moved here
```

This is possible because `World.Tiles` is a public mutable field. The fixture therefore depends on level 0's exact layout, which its comment block documents: player `(7, 5)`, boxes `(6, 6)` and `(7, 7)`, goals `(6, 8)` and `(7, 8)`. Editing `0.lvl` breaks these tests, and the direction convention in the moves (`MovePlayer(0, 1)` moves the player to `(7, 6)`) encodes the `[x, y]` indexing.

### Test conventions enforced

`TestSourceConventionTests` (14 tests) requires: every test class name ends with `Tests`; every test method follows `Given…When…Then…`; every assertion uses `Assert.That`; every test method contains an assertion or a verification; no test method exceeds seven parameters; top-level fakes live in their own files (which is why `IPlayerMovedSpy` is a separate file); no legacy `Assert.AreEqual`-style forms; no comment banners grouping tests; every parameterised test has at least two cases; no reflection on non-public code; every fixture is marked `[TestFixture]`; setup methods are named `SetUp` and construct the subject; no fixture declares visibility into production internals; test files are ordered by name.

## Architecture test project

Location: `SokobanCLI.ArchitectureTests/`. The added value over conventional tests is that it enforces repository-wide rules that a code reviewer would otherwise have to hold in mind.

### Infrastructure

| Type | Role |
|------|------|
| `ProductionSources` | Resolves the repository root from the build-generated `ArchitecturePaths` constants, enumerates `*.cs` files per project, excludes `/bin/`, `/obj/`, `*.g.cs`, `*.Designer.cs`, and `*.AssemblyInfo.cs`; provides `Describe`, `AssertClean`, `AssertScanned`, and violation formatting helpers |
| `ProductionCompilation` | Parses every production source with `LanguageVersion.Latest` and builds a `CSharpCompilation` named `SokobanCLI.ArchitectureAnalysis` referencing the trusted platform assemblies, the loaded assemblies, and every dependency of the production assembly. This is what makes semantic rules possible without referencing the built binary for symbol data |
| `ArchitecturePaths` | MSBuild-generated constants `ProductionSourceRoot` and `SolutionPath`, written by the `GenerateArchitecturePaths` target before `CoreCompile`, so the tests locate sources regardless of working directory |

### Fixtures and what each enforces

| Fixture | Tests | Enforcement |
|---------|-------|-------------|
| `ProductionArchitectureTests` | 10 | ArchUnitNET rules: each boundary namespace selects types; `Models` depends on no `GameLogic`, `Input`, `UI`, or `SokobanCLI.Graphics`; `Graphics` on no `Models`, `GameLogic`, `Input`, `UI`; `Input` on no `Models`, `GameLogic`, `Graphics`, `UI`; `GameLogic` on no `Input`, `UI`; nothing on `SokobanCLI.Program`; exactly one composition root; no `InternalsVisibleTo`; no test types; no test or database library references |
| `ProjectReferenceTests` | 5 | Project-level and assembly-level reference graph: no `ProjectReference` in production; no test or database package; no `InternalsVisibleTo` in the project file or as an assembly attribute; the compiled assembly references no forbidden assembly; only the two test projects reference the production project |
| `ProjectConventionTests` | 3 (7 with cases) | Every project targets `net10.0`; package references are alphabetised per project; test projects are not packable; the architecture test project declares `IsTestProject` and `ExcludeFromCodeCoverage`; the production project declares the level files as content |
| `SolutionConventionTests` | 4 | `.slnx` uses the SDK-style root element with no XML declaration and no `Version`; project entries carry only a `Path` attribute; `SokobanCLI` comes first and the rest are alphabetised; every project lives in a sibling folder named after it and no path nests in a `src`-style directory |
| `ConfigurationConventionTests` | 6 | Project-file configuration conventions not covered elsewhere, plus analyser-enforcement expectations |
| `SourceFileConventionTests` | 8 | One type per file for production; file name stem matches every declared type name for checked projects; one block-scoped namespace and no file-scoped namespace; namespace matches the folder path; no regions or `#nullable` directives; no top-level statements; single trailing newline |
| `NamespaceConventionTests` | 7 | Usings outside the namespace; no global usings or `GlobalUsings.g.cs`; using groups separated by blank lines and alphabetised; exactly one blank line between usings and the namespace declaration; no inline fully qualified type names; no prohibited namespace segments; Roslyn source trees are available |
| `NamingConventionTests` | 8 | Public and protected members start uppercase; classes and methods PascalCase; private fields camelCase without an underscore prefix; boolean members use predicate prefixes; interfaces are `I`-prefixed PascalCase; configuration classes use a `Settings` suffix; domain model names are plain nouns |
| `CodingConventionTests` | 17 | No `var`; no ternary conditionals; no tuple or `ref` parameters; no optional parameters; no regions or `#nullable` directives; every control-flow body braced; `else if` on one line; explicit accessibility on every member; every type inside a namespace block; member-kind order; no member with more than seven parameters; overload groups not split; overloads ordered by parameter count; no blank line before a closing delimiter; instance `Equals` avoided; no explicit self-increment; no patterns on `this`/member; collection members expose `IEnumerable`; XML-serialised collections use `List`; collection initialisation uses collection expressions; no redundant collection lookup loops |
| `LayoutConventionTests` | 8 | Long conditionals split one clause per line; continuation lines indented four spaces; no parameter list over the signature threshold; expression-bodied members place the arrow on its own line when long; each auto-property occupies one line; exactly one blank line between members; canonical modifier order; no consecutive blank lines |
| `MemberShapeConventionTests` | 7 | Single-statement members expression-bodied; static methods do not access instance state; constants not declared with `const` for the selected cases; no redundant default field initialisers; no repeated regex allocation inline; no reusable array literal as a method or lambda argument; no URI separator embedded in a string |
| `CollectionConventionTests` | 4 | Collection member exposure and XML-serialised list shape as above, isolated for focused reporting |
| `ErrorHandlingConventionTests` | 7 | Null checks use `is null`/`is not null` patterns; no null-coalescing operator; bare rethrows; no empty catch blocks; no custom exception type declared without need; no exception constructed with a sensitive message; no unsafe cast where `as` is preferred |
| `TypeDeclarationConventionTests` | 9 | No `partial` type; no `init` accessor; every class sealed or inheritable by design; every non-static class with public methods has a matching interface; implementations match interface parameter names; domain types with equality implement the contract; no preview language feature; no file-scoped namespace; `Program` declares `static void Main` |
| `SpecialisedTypeConventionTests` | 8 | Mapping extensions in dedicated files and namespaces and static with only extension methods; no mapping methods outside mapping extensions; enumerations outside dedicated enumeration namespaces; enum values unique; date handling passes a culture; no `JsonSerializerOptions` allocated in loops; arguments and locals not named after sensitive data |
| `TestSourceConventionTests` | 14 | The test conventions listed in the previous section |
| `RuleApplicabilityTests` | 8 | Records rules that cannot currently apply, each with the structural precondition that keeps it inapplicable: no logging or telemetry code; no HTTP or network code; no game-framework rendering API; no analyser configuration file; the production project is not packable; no interop, Dynamo, or database code; no `GeneratedRegex` or partial-type precondition; the unit test project uses the NUnit/Moq pair and not xUnit |

### The intentionally inverted rules

`RuleApplicabilityTests` is unusual: each test asserts that a *precondition for other rules* is absent. For example:

```csharp
Assert.That(matches, Is.Empty, "Logging rules (Operation, LogInfoKey, masking) become enforceable once logging code exists.");
```

The consequence is that introducing logging, networking, persistence, a game framework, a `Directory.Build.props`, `IsPackable=true`, a partial type, or xUnit into the repository immediately fails these tests, forcing the corresponding rules to be written instead of silently skipped. This is a deliberate forcing function rather than an incidental assertion.

## Load-bearing rules and their instruction sources

The convention rules encode the maintainer's C# instruction files. Where a rule originated in a specific instruction document, that provenance is recorded in the fixture summaries:

| Fixture | Instruction source recorded in the fixture |
|---------|--------------------------------------------|
| `CodingConventionTests` | `csharp.coding.instructions.md` |
| `CollectionConventionTests` | `csharp.collections.instructions.md` |
| `ErrorHandlingConventionTests` | `csharp.error-handling.instructions.md` |
| `LayoutConventionTests` | `csharp.indentation.instructions.md` |
| `MemberShapeConventionTests` | `csharp.coding.instructions.md` |
| `NamespaceConventionTests` | `csharp.namespaces.instructions.md` |
| `NamingConventionTests` | `csharp.naming.instructions.md` |
| `TypeDeclarationConventionTests` | `csharp.coding.instructions.md`, `csharp.interfaces.instructions.md` |
| `SpecialisedTypeConventionTests` | mapping, enumeration, serialisation, and logging instruction sets |
| `TestSourceConventionTests` | `csharp.testing.instructions.md`, `csharp.testing.unit.instructions.md` |

## Coverage gaps

No automated test covers:

| Area | Reason | Consequence |
|------|--------|-------------|
| `Program` | Console-dependent, and nothing may depend on it by rule | Startup ordering and the frame loop are verified only by running the application |
| `GraphicsManager`, `AsciiSpriteBatch`, `AsciiSprite` | Require a real console for output; `ScreenPreview` would permit buffer inspection but no test uses it | The diff algorithm, the resize failure, and sprite clipping are untested |
| `InputManager` | Requires `Console.KeyAvailable` and `Console.ReadKey` | Key polling is untested; only the event arguments are |
| `UI` entirely | Requires the console and reflection-driven construction; `ScreenManager` reads the file system in its constructor | Screen lifecycle, transitions, focus, menu layout, wall-shape resolution, and XML deserialisation are untested |
| Levels 1–20 content | Only level 0 is loaded by tests | A malformed or unsolvable level later in the set is not detected |
| Exception paths | No test asserts any thrown exception | Error handling changes have no regression baseline |
| Persistence | None exists | The `// TODO: Save progress` work has no test scaffolding |
| `World.ValidateRows` | Would need invalid level fixtures | Level validation is untested despite being the only deliberately thrown exception |
| `Tile.IsWalkable` | Never called in production | An unused member with no test |

The absence of UI tests is the most consequential gap, because the UI layer contains the reflection surface, the framebuffer resize asymmetry, and the menu focus logic — three of the repository's four most fragile behaviours.

## Adding tests

| Adding | Constraints |
|--------|-------------|
| A unit test | The class must end `Tests`, the method must read `Given…When…Then…`, and every test must use `Assert.That`. Top-level fakes need their own file. New fixtures mirror the production folder structure |
| A parameterised test | At least two cases are required by `TestSourceConventionTests` |
| A test touching a console or the file system | It will need a seam that does not exist yet; introducing one must respect the layering rules, and `Input` may not depend on `Graphics` |
| An architecture rule | Place it in the fixture matching its concern; if it needs symbols, use `ProductionCompilation.GetModels()` |

## Related documents

- [build-and-deployment.md](./build-and-deployment.md) — how the suite runs in CI
- [invariants.md](./invariants.md) — which invariants are test-enforced and which are not
- [change-guide.md](./change-guide.md) — which test fails when a given change is made