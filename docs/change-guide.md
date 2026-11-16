# Change guide

How to make common changes safely, which rules will fail, and what else must move with them. Read [invariants.md](./invariants.md) before any change that touches the grid, the framebuffers, or the screen-name contract.

## Before you start

```bash
dotnet build SokobanCLI.slnx
dotnet test SokobanCLI.slnx
```

Record the baseline: 101 unit tests and 138 architecture tests, all passing. Any failure after a change is either yours or a previously latent violation.

## Conventions that fail the build

These are enforced statically, so a violation is caught immediately rather than in review.

| If you… | This fails |
|---------|-----------|
| use `var` | `CodingConventionTests` — no `var` |
| write a ternary conditional | `CodingConventionTests` — no ternary |
| use optional parameters, tuple parameters, or `ref` parameters | `CodingConventionTests` |
| omit braces on a single-statement `if` | `CodingConventionTests` — every control-flow body braced |
| use a file-scoped namespace | `SourceFileConventionTests`, `TypeDeclarationConventionTests` |
| use a global using or `ImplicitUsings` | `NamespaceConventionTests`, `ConfigurationConventionTests` |
| put two types in one file, or name the file differently from the type | `SourceFileConventionTests` |
| inline a fully qualified type name instead of a using directive | `NamespaceConventionTests` |
| use a null-coalescing operator or `!= null` instead of `is null` | `ErrorHandlingConventionTests` |
| declare a `partial` type or an `init` accessor | `TypeDeclarationConventionTests` |
| add a non-static class with public methods but no interface | `TypeDeclarationConventionTests` |
| leave a blank line before a closing brace, or two consecutive blank lines | `LayoutConventionTests` |
| exceed seven parameters on a method, constructor, or lambda | `CodingConventionTests` |
| write a multi-statement member body for a single statement | `MemberShapeConventionTests` |
| expose a `List<T>` collection member that is not XML-serialised | `CollectionConventionTests` |
| put a mapping method outside a `*.Mapping` namespace | `SpecialisedTypeConventionTests` |

The full rule list with attribution is in [testing.md](./testing.md).

## Adding a level

1. Create `SokobanCLI/Levels/<n>.lvl` with exactly 14 lines of 14 digits, using 0/1/2/3/4/5/6 and remembering that a **line is a column** of the grid.
2. Rely on the existing `None Update="Levels\*.lvl"` copy item; do not add a new one.
3. If the level is used by a test, add a matching content link to `SokobanCLI.UnitTests.csproj` (it currently links the whole `Levels` glob, so nothing is needed).
4. Verify it loads and completes.

Constraints:

- Numbering must stay contiguous from 0, because `GameScreen` advances with `World.Level + 1` and `World` builds the file name as `<level>.lvl`.
- `World.DefaultSize` is 14, so the file cannot be resized. See [invariants.md](./invariants.md) INV-7.
- `RuleApplicabilityTests` will not object, but no test validates levels beyond 0, so a malformed new level is only caught by playing it.
- If you insert a level rather than appending, every subsequent file must be renumbered.
- Adding a level beyond 20 does not remove the crash on completing the current highest level; that requires a guard in `GameScreen.Update`.

## Changing a level's encoding

Changing `TileType` values or the digits in level files is a breaking change to a shared contract:

| Must change together | Because |
|----------------------|---------|
| `TileType` values | `Tiles.ById` matches by value and `Tiles.OfType` derives the identifier from the value |
| `Tiles.GetCharacter` and `GetColour` | Glyph and colour per type |
| `GameManager.MovePlayer`, `TryMoveBox` | They branch on literal identifiers `0`, `2`, `3`, `5`, and `IsWalkable` tests types `Empty` and `Goal` |
| Every `Levels/*.lvl` | Content encoding |
| `World.LoadTiles` | It special-cases identifiers 4 and 6 |
| `UiWorldmap.Draw` | It special-cases identifier 1 for walls and identifier 3 for the player glyph |
| `SokobanCLI.UnitTests/Models/TilesTests.cs` and `GameManagerTests.cs` | Fixtures and expectations |
| The README tile legend | Documentation |

The cheapest safe approach is to convert `GameManager` to use `TileType` (or `Tile.IsWalkable` plus identifier-free predicates) and add a test asserting `Tiles.OfType(TileType.X).Id == (int)TileType.X` first, so the encoding becomes a single source of truth.

## Changing the level format

Any change to dimensions or orientation touches:

| Change | Files affected |
|--------|----------------|
| Dimensions (other than 14 × 14) | `World.DefaultSize`, `World.ValidateRows`, all 21 level files, `UiWorldmap.LoadContent`'s literal child size, `Screens/GameScreen.xml`'s `Worldmap/Size`, `GameManagerTests` coordinates |
| Orientation (rows × columns instead of transposed) | `World.LoadTiles`, `UiWorldmap.Draw`'s loop nesting, every level file, `UiWorldmap.GetWallShape`'s neighbour semantics, the `GameManagerTests` comment block, [components/models-and-content.md](./components/models-and-content.md) |

The safest sequence for an orientation change: add a test that loads a known level and asserts a specific tile at specific coordinates, so the transposition is pinned, then change the loader and the files together.

## Adding a screen

1. Add a `Screen`, `MenuScreen`, or `Screen`-derived class under `SokobanCLI/UI/Screens/`, sealed if it is a leaf, with a public parameterless constructor.
2. Expose every property that XML should populate, as a public get/set.
3. Declare a matching interface (`I<Name>Screen`) if the class has public methods, otherwise `TypeDeclarationConventionTests` fails.
4. Create `SokobanCLI/Screens/<TypeName>.xml` with the type name as the root element.
5. Reference the type name from a `LinkId` in the screen that should open it.
6. If it needs arguments, read them from `ScreenArgs` — and validate them, because `int.Parse` on a non-numeric argument throws.

Constraints:

- The three-way contract stands: class name = XML file name = `LinkId` value. Nothing checks it.
- A class deriving from `MenuScreen` must have at least one `Link` or `Action`, or `MenuScreen.Update` throws on `Items[0]`.
- Screen classes are resolved under `SokobanCLI.UI.Screens`, so the class must live in that namespace.

## Adding a UI element

1. Implement the matching `IUi*` interface and derive from `UiElement` or `UiMenuItem`.
2. Override `LoadContent` (call `base` last as the existing elements do), `Draw` (call `base` if children must draw), and `SetChildrenProperties` (call `base` **first**, then set the derived state).
3. Register it with `UiManager.Instance.UiElements` from the owning screen's `LoadContent`, or expose it as a property so the screen XML can supply its layout.
4. Keep every `Tile`-independent behaviour testable without a console, and add unit coverage for it.

Two rules that are easy to break:

- `SetChildrenProperties` must call `base` **before** mutating derived state, because the base method caches the old values that `RaiseEvents` compares against. Reversing the order silently disables the change events.
- Elements must dispose or remove their children so `UiManager.Update`'s sweep remains correct.

## Adding a key binding

Add the case to the screen that should own it, not to `InputManager`:

- `GameScreen.OnKeyPressed` for gameplay keys.
- `MenuScreen.OnKeyPressed` for menu navigation, which is axis-dependent.
- `UiMenuItem.OnKeyPressed` for activation keys, which currently accepts `Enter` and `E`.

Constraints: `Input` may not depend on `Models`, `GameLogic`, `Graphics`, or `UI`, so no key-to-meaning mapping may live there. Screens receive a raw `ConsoleKey`.

## Changing rendering

The renderer is the most constrained area.

| Change | Consequence |
|--------|-------------|
| Modify `GraphicsManager.Draw` | Keep the diff semantics; a full write every frame is correct but abandons the reason the shadow batch exists |
| Modify `GraphicsManager.Clear` | It iterates `drawnData.Size` while indexing `SpriteBatch`; the two must have equal dimensions or it throws |
| Modify `AsciiSpriteBatch.Update` | It reallocates the render batch only. If you make it also reallocate the shadow batch, the resize crash is fixed but the two objects' ownership becomes entangled — a cleaner fix is to have `GraphicsManager` observe the resolution itself |
| Add per-tile colours | The intended code is commented out in `UiWorldmap.Draw`; enabling it means setting `Console.ForegroundColor` per cell, which the graphics manager currently does from the sprite rather than from the tile |
| Change alignment | `AsciiSpriteBatch.Draw` resolves alignment against `sprite.Size`, not the batch; `UiText` copies `Size` from the element |
| Change line handling | Text is split on `'\n'` only. Supporting `'\r\n'` means normalising text before drawing, which would also make the renderer platform-independent |

Any change here is untested: there is no test for `GraphicsManager`, `AsciiSpriteBatch`, or `AsciiSprite`. Add a test using `AsciiSpriteBatch.ScreenPreview` where possible, which needs no console.

## Adding persistence

This is the largest pending change and the least scaffolded.

1. Decide the boundary. `Models` may depend on `SokobanCLI.Graphics.Geometry` and `System.IO` and nothing else; `GameLogic` may depend on `Models` and geometry; `UI` may depend on everything. A store is most naturally a new leaf namespace referenced by `GameLogic` or `UI`.
2. Respect `ProductionArchitectureTests`: a new namespace needs a new rule or an explicit exception.
3. Wire it at the `// TODO: Save progress` site in `GameScreen.Update`, and add a load site — probably in `GameScreen.LoadContent` where the level number is currently taken from `ScreenArgs`.
4. Update `PRIVACY.md` and the README feature list, because the current documents state that nothing is written. `RuleApplicabilityTests` will not object, but `SpecialisedTypeConventionTests` will require a culture on date handling if timestamps are stored, and `RuleApplicabilityTests` will object if a database provider is introduced without rules.

## Fixing the terminal resize crash

The defect: `AsciiSpriteBatch.Update` reallocates the render batch on a resolution change, while `GraphicsManager.drawnData` is allocated only in `LoadContent`. Enlarging the terminal makes `Draw` index past the shadow arrays; shrinking it makes `Clear` index past the render arrays.

Minimal correct fix: have `GraphicsManager` detect the resolution change in its own `Update` (currently empty) and reallocate `drawnData` to match, iterating the smaller of the two dimensions in `Clear`. The same frame's `Draw` must then handle the case where the render batch is larger, which it does because it iterates `SpriteBatch.Size` — provided `drawnData` was resized first.

Add a regression note: the fix is untestable without a console seam, so at minimum verify manually by resizing in both directions.

## Adding a new NuGet package

1. Keep package references alphabetised within the project (`ProjectConventionTests`).
2. If it lands in the production project, its name must not contain `NUnit`, `Moq`, `Test.Sdk`, `TestAdapter`, `xunit`, `ArchUnitNET`, `EntityFramework`, `Dapper`, `SqlClient`, `Npgsql`, or `Dynamo` (`ProjectReferenceTests`).
3. If it introduces logging, HTTP, persistence, or game-framework fragments into production sources, `RuleApplicabilityTests` fails by design; write the corresponding rules in the same change.

## Modifying documentation

- `README.md`, `ARCHITECTURE.md`, `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SECURITY.md`, `PRIVACY.md`, and any future `ROADMAP.md` are template-governed; their fixed prose must not be rephrased.
- This `docs/` tree is not template-governed.
- When behaviour changes, update the affected `docs/` file, the root `ARCHITECTURE.md` section that describes it, and the README if the change alters a user-facing capability or the README's feature list. The README currently overstates one capability (saved progress) — see [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md).

## Checklist for any change

- [ ] Baseline test run recorded.
- [ ] Layer boundaries respected; no new dependency on `Program`.
- [ ] No `InternalsVisibleTo` introduced.
- [ ] One type per file, file name matches the type, block-scoped namespace matching the folder.
- [ ] Interface added for any new class with public methods.
- [ ] Single-statement members expression-bodied; no `var`; no ternary; braces on every control-flow body.
- [ ] New behaviour covered by a unit test using `Assert.That` and a `Given…When…Then…` name, or the coverage gap recorded in [testing.md](./testing.md).
- [ ] `invariants.md` updated if an invariant changed; the corresponding `RuleApplicabilityTests` precondition reconsidered if a capability was added.
- [ ] Documentation updated: this tree, the affected root document section, and the README where user-facing behaviour changed.

## Related documents

- [invariants.md](./invariants.md) — what must stay true
- [testing.md](./testing.md) — which test fails for which change
- [components/screens.md](./components/screens.md) and [components/ui-elements.md](./components/ui-elements.md) — the two areas most likely to change