# Invariants

Conditions that must remain true for the game, its tests, and its conventions to keep working. Each entry states the invariant, where it is enforced or assumed, and what breaks if it is violated.

## Content and encoding invariants

### INV-1: Every level file is exactly 14 lines of 14 characters

Enforced by `World.ValidateRows`, which throws `InvalidDataException` naming the file.

Assumed by: `World.LoadTiles` (which indexes `rows[x][y]` up to 13 in both axes), `UiWorldmap` (which sizes its child `UiText` to a literal 14 × 14 and iterates `game.World.Size`), and `GameManagerTests` (whose fixtures assume level 0's coordinates).

Violation: validation catches the dimension mismatch, but only for lines longer or shorter than 14 and for a line count other than 14. A file with, say, 14 lines of 14 *non-digit* characters passes validation and produces a board of empty tiles because `Tiles.ById` resolves unknown identifiers to empty.

### INV-2: Level files are transposed: line index is the grid X coordinate

Assumed by: `World.LoadTiles` (`rows[x][y]`), `UiWorldmap.Draw` (which builds a line per `x` and appends across `y`), `GameManagerTests` fixtures, and the comment block at the top of `GameManagerTests`.

Violation: a level authored as visual rows would render rotated 90°. Nothing validates orientation, so the error is silent and would only be noticed by playing the level.

### INV-3: Tile digits are 0–6 and map to distinct `TileType` values

Assumed by: `Tiles.ById`, `Tiles.OfType` (which derives the identifier from the enumeration value), `GameManager` (which branches on literal identifiers), and the level files.

Violation: digits outside the range produce an empty tile with the requested identifier, which `GameManager` treats as walkable floor for `0` but as a wall for anything else, since it tests `is 0 or 3` for walkable and `is 2 or 5` for a box and rejects everything else. So an invalid digit 9 creates an invisible wall.

### INV-4: Identifier 4 and 6 appear at most once per level and mark the player

Assumed by: `World.LoadTiles`, which assigns `PlayerPosition` on each occurrence so a later occurrence silently wins.

Violation: multiple player markers leave the player at the last one encountered in `x`-then-`y` order; no validation or warning exists.

## Model invariants

### INV-5: `World.Tiles` is indexed `[x, y]` with both indices below `Size`

Enforced by: `GameManager` performing `World.Contains` before every target and box access, and `UiWorldmap` iterating within `game.World.Size`.

Violation: direct grid access without a bounds check throws `IndexOutOfRangeException`. Unit tests index literal coordinates and rely on the level being large enough.

### INV-6: The player is a position, never a grid cell

Assumed by: `World.LoadTiles` (which rewrites identifier 4 to 0 and 6 to 3), `GameManager.MovePlayer` (which writes only `PlayerPosition`), `UiWorldmap.Draw` (which chooses the glyph by comparing coordinates), and `GetCompletion` (which counts only identifier 3, so standing on a goal must not change the cell).

Violation: writing the player into `Tiles` would make `GetCompletion` unable to complete a level whose last goal the player occupies, and would make walking over a goal consume or duplicate it.

### INV-7: `World.Size` is exactly 14 × 14 for every level

Assumed by: `World.DefaultSize`, `ValidateRows`, `UiWorldmap`'s literal child size, `GameScreen.xml`'s declared world-map size, and `GameManagerTests`' coordinates.

Violation: any change requires a coordinated update of the constant, the validation, all 21 level files, `GameScreen.xml`, and the world map's child size literal. `ProductionArchitectureTests` does not police this; only the unit tests and manual play would catch an inconsistency.

### INV-8: Tile instances are not mutated after creation

Assumed by: `Tiles` allocating a fresh `Tile` per call and the game replacing grid entries rather than editing them. `Tile`'s properties have public setters, so the invariant is a convention rather than a guarantee.

Violation: mutating a `Tile` obtained from `Tiles.ById` or reached through `Tiles[x, y]` would alter the appearance of every cell that shares it… except that the factory allocates a new instance per call, so an instance is shared only when the same reference is assigned to two cells, which the current code never does. The real exposure is a future cache in the factory; if `Tiles` ever returned shared instances, in-place mutation would corrupt unrelated cells.

## Game logic invariants

### INV-9: A rejected move mutates nothing

Enforced by: `GameManager.MovePlayer` and `TryMoveBox` returning before any write on every rejection path.

Violation: any rearrangement that writes one grid cell before validating the second would leave the board inconsistent; `TryMoveBox` deliberately performs both writes after both checks.

### INV-10: A box is never destroyed and the box count is constant within a level

Assumed by: `TryMoveBox`, which moves a box by rewriting two cells and never removes one.

Violation: a level becomes unsolvable if a box is lost, and `GetCompletion` would never report true. No test asserts a box count.

### INV-11: A goal covered by a box carries identifier 5, and vacating it restores 3

Enforced by: `TryMoveBox`'s `boxId` and `targetId` selection using `targetIsGoal` (whether the vacated cell was a goal) and `destinationId` (whether the destination was a goal).

Violation: getting the two flags the wrong way round makes goals appear or disappear as boxes move. `GameManagerTests` covers all four combinations of source/destination goal state.

### INV-12: `World.Moves` increments exactly once per accepted move and never for a rejection

Enforced by: the single `World.Moves += 1` after the success check.

Violation: counting attempts rather than accepted moves changes the displayed counter's meaning.

### INV-13: Completion means no cell holds identifier 3

Enforced by: `GetCompletion`'s scan.

Violation: using `Tile.Type` or `IsWalkable` as the criterion, or treating a player on a goal as covered, would change when a level ends. Note this invariant implies that a level with zero goals is complete at once and that a level whose goals are all covered by boxes at load time (which identifier 5 makes possible) starts complete.

## Rendering invariants

### INV-14: The render batch and the shadow batch have the same dimensions when `Draw` or `Clear` runs

Enforced by: nothing. `AsciiSpriteBatch.Update` reallocates the render batch on resolution change; `drawnData` is allocated only in `GraphicsManager.LoadContent`.

Violation: enlarging the terminal makes `Draw` index past `drawnData`; shrinking it makes `Clear` index past `SpriteBatch`. Both throw `IndexOutOfRangeException`. See [troubleshooting.md](./troubleshooting.md).

### INV-15: Screen text uses `'\n'` as its only line separator

Assumed by: `AsciiSpriteBatch.Draw`, which splits on `'\n'` and treats every remaining character as content.

Violation: on Windows `Environment.NewLine` is `"\r\n"`. `UiWorldmap.Draw` and `GameScreen.SetChildrenProperties` build text with `Environment.NewLine`, so on Windows every line would carry a trailing `'\r'` composited into the cell before the line break, shifting or corrupting the display. The repository is developed and CI-tested on Linux, where the assumption holds. This is the one platform-dependent assumption in the rendering path.

### INV-16: A sprite's `Size` is large enough for its text, or the sprite is clipped harmlessly

Assumed by: `AsciiSpriteBatch.Draw`, which computes a negative start offset when the text is wider or taller than the declared size; negative coordinates are then clipped per cell by the bounds check, so the result is truncated text rather than an exception.

Violation: text longer than its box is silently cut off. `GameScreen.SetChildrenProperties` resizes labels from text length each frame, which is why the level-details label stays correct as the level number grows.

### INV-17: Draw order equals `UiManager.UiElements` order

Assumed by: `UiManager.Draw`'s ordered iteration and by overlap behaviour.

Violation: reordering elements in a screen's `LoadContent` changes which element wins an overlap. No element currently overlaps another, so the invariant is latent.

## UI invariants

### INV-18: Exactly one menu item has input focus while a menu is displayed

Enforced by: `MenuScreen.Update` calling `UiManager.FocusElement(...)` every frame, and `UiManager.FocusElement` clearing focus on all elements before setting one.

Violation: skipping the per-frame call leaves focus wherever it was; a stale focused element could still activate. `UiElement.OnInputManagerKeyboardKeyPressed` requires `HasInputFocus`, so a menu with no focused item is inert rather than misbehaving.

### INV-19: `MenuScreen.Items` is non-empty

Assumed by: `MenuScreen.Update`, which clamps with an `Items.Count > 0` guard but then indexes `Items[newSelectedItemIndex]` unconditionally.

Violation: a screen deriving from `MenuScreen` whose XML defines no `Link` and no `Action` throws `ArgumentOutOfRangeException` on its first update.

### INV-20: A screen's class name, its XML file name, and any `LinkId` referring to it agree

Enforced by: nothing at compile time; asserted only by manual consistency.

Violation: `ScreenManager.CreateScreen` prefixes the name with `SokobanCLI.UI.Screens`; a name matching no type throws `ArgumentNullException`, and a name matching a type whose XML is missing yields default colours with an empty menu.

## Convention invariants (test-enforced)

These are enforced by `SokobanCLI.ArchitectureTests` and fail the build when violated. They are invariants of the repository, not of the runtime.

| Invariant | Enforcer |
|-----------|----------|
| `Models` depends on no `GameLogic`, `Input`, `UI`, or `SokobanCLI.Graphics` type | `ProductionArchitectureTests` |
| `Graphics` depends on no `Models`, `GameLogic`, `Input`, or `UI` type | `ProductionArchitectureTests` |
| `Input` depends on no `Models`, `GameLogic`, `Graphics`, or `UI` type | `ProductionArchitectureTests` |
| `GameLogic` depends on no `Input` or `UI` type | `ProductionArchitectureTests` |
| No type depends on `SokobanCLI.Program` | `ProductionArchitectureTests` |
| The production assembly declares no `InternalsVisibleTo` | `ProductionArchitectureTests`, `ProjectReferenceTests` |
| The production assembly contains no test types and no test or database references | `ProductionArchitectureTests`, `ProjectReferenceTests` |
| The production project declares no `ProjectReference` and no test or database package | `ProjectReferenceTests` |
| Every concrete project file lives in a sibling folder of the same name | `SolutionConventionTests` |
| The solution lists `SokobanCLI` first, then the rest alphabetically | `SolutionConventionTests` |
| Every checked project targets `net10.0` and declares its packages alphabetically | `ProjectConventionTests` |
| One type per file; file name matches type name; one block-scoped namespace matching the folder | `SourceFileConventionTests` |
| No file-scoped namespace, no top-level statements, no regions or `#nullable` directives, single trailing newline | `SourceFileConventionTests` |
| Public and protected members PascalCase; fields camelCase without underscore; interfaces `I`-prefixed; booleans predicate-prefixed | `NamingConventionTests` |
| Single-statement members expression-bodied; expression bodies place the arrow on its own line when long | `MemberShapeConventionTests` |
| No `var`, no ternary conditionals, no tuple or `ref` parameters, no optional parameters | `CodingConventionTests` |
| Braces on every control-flow body; `else if` on one line; explicit accessibility on every member | `CodingConventionTests` |
| Null checks use `is null`/`is not null`; no null-coalescing operator; bare rethrows; no empty catch | `ErrorHandlingConventionTests` |
| `as` preferred to direct casts (no unsafe casts); `Equals` not called on `this` or a member | `ErrorHandlingConventionTests`, `CodingConventionTests` |
| Authorised control transfer only — no `goto`, no `break`/`continue` outside loops | `CodingConventionTests` |
| Collection members expose `IEnumerable` except XML-serialised lists, which use `List<T>` | `CollectionConventionTests` |
| Collection initialisation uses collection expressions | `CollectionConventionTests` |
| Mapping methods live in `*.Mapping` namespaces in `*MappingExtensions.cs` files and are static extension methods | `SpecialisedTypeConventionTests` |
| Enumerations do not live in dedicated enumeration namespaces; enumeration values are unique | `SpecialisedTypeConventionTests` |
| No `date` parsing without an explicit culture; no `JsonSerializerOptions` allocated in loops | `SpecialisedTypeConventionTests` |
| No type declared `partial`; no property uses an `init` accessor; classes sealed or inheritable by design | `TypeDeclarationConventionTests` |
| Every non-static class with public methods has a matching interface, and implementations match interface parameter names | `TypeDeclarationConventionTests` |
| Every test class ends `Tests`; every test method is `Given…When…Then…`; assertions use `AssertThat`; every test asserts or verifies | `TestSourceConventionTests` |
| Fixtures are marked `[TestFixture]`; setup methods are `SetUp`; top-level fakes live in their own files | `TestSourceConventionTests` |
| Rules that cannot currently apply are recorded with their structural precondition | `RuleApplicabilityTests` |

Full rule-by-rule attribution with the corresponding instruction file is in [testing.md](./testing.md).

## Related documents

- [data-model.md](./data-model.md) — the fields these invariants constrain
- [change-guide.md](./change-guide.md) — which invariant a given change threatens
- [error-handling.md](./error-handling.md) — what happens when an invariant is violated at runtime