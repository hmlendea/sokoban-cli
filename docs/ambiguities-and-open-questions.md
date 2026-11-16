# Ambiguities and open questions

Recorded gaps, TODO markers, unverified assumptions, and questions the code does not answer. Each entry states what is known, what is not, and what evidence would resolve it.

## TODO markers in production code

| Marker | Location | Meaning | Blocking |
|--------|----------|---------|----------|
| `// TODO: Save progress` | `GameScreen.Update` | Progress persistence is intended where a level completes | Yes, for any resume capability |
| `// TODO: What if this is the last level?` | `GameScreen.Update` | The last-level guard is acknowledged as missing | Yes, for the crash after level 20 |
| `// TODO: Switch to a transition screen before starting` | `GameScreen.Update` | A transition screen between levels is intended | No |
| `// TODO: Close the game somehow...` | `UiMenuAction.OnActivated` | Process termination from the `Exit` menu entry is intended | Yes, for a clean quit |
| `// TODO: Handle this better` | `UiWorldmap.AssociateGameManager` | The setter-based manager injection is acknowledged as poor | No |
| `// TODO: Maybe implement my own handler and args` | `UiMenuItem` | The `EventHandler`/`EventArgs` pairing for activation is acknowledged as a placeholder | No |

## Documentation versus code mismatches

### The README claims saved settings and progress

| Aspect | Detail |
|--------|--------|
| README states | "Settings and progress saved in the local application data directory" (Features list) |
| Code reality | No file is written. There is no settings, preferences, or profile API; `GameScreen.Update` contains only the `// TODO: Save progress` marker |
| Corroborating evidence | `PRIVACY.md` states the application "creates no settings, progress, or log files"; `ARCHITECTURE.md` states "There is no progress, settings, or score persistence in the production assembly" |
| Resolution | Two of the three documents agree with the code; the README feature line is the outlier. It should be corrected to describe the implemented behaviour |
| Open question | Was the feature removed, or was the README written against an intended design? Git history does not show the feature ever having been implemented in the current source tree |

### `ARCHITECTURE.md` describes tile instances as shared

| Aspect | Detail |
|--------|--------|
| Architecture states | Tiles are "Created per factory call; shared statically by identifier, so they must not be mutated by consumers" |
| Code reality | `Tiles.CreateTile` allocates a new `Tile` on every call. No cache or static store exists, so instances are not shared by identifier |
| Consequence | The "must not be mutated" caution is harmless but the stated mechanism is inaccurate |
| Resolution | Correct the architecture wording, or add a cache if sharing was the intent — but a cache would make in-place mutation of one tile corrupt every cell referencing it, which the current design avoids |

## Unverified assumptions

### The renderer assumes `'\n'` as the only line separator

`AsciiSpriteBatch.Draw` splits sprite text on `'\n'` and treats every remaining character as content. `UiWorldmap.Draw` and `GameScreen.SetChildrenProperties` build text with `Environment.NewLine`, which is `"\r\n"` on Windows. On Windows each line would carry a trailing `'\r'` composited into a cell.

Open question: is Windows supported? CI runs `ubuntu-latest` only, the release helper builds per runtime identifier, and no test or document states a Windows verification. Recorded as an open compatibility question rather than a confirmed defect. See [invariants.md](./invariants.md) INV-15.

### The transposed level layout is undocumented outside code

`World.LoadTiles` indexes `rows[x][y]` and `UiWorldmap.Draw` builds one line per `x`, so the orientation is internally consistent. Nothing in the README, `ARCHITECTURE.md`, `CONTRIBUTING.md`, or the `Levels/` directory states that a file line is a grid column.

Open question: was the orientation deliberate, or is it an inversion that happens to be self-consistent? A single authoritative statement in the level directory or `CONTRIBUTING.md` would resolve it. See [invariants.md](./invariants.md) INV-2.

### `GetWallShape`'s neighbour semantics versus its glyph table

The method names its neighbours `n`, `w`, `s`, `e` from the grid's own axes (so `n` tests `Tiles[x - 1, y]`) and uses a conventional box-drawing table. Combined with the projection in `Draw`, horizontal and vertical glyphs appear swapped relative to a designer's expectation.

Open question: is the glyph table correct for the projected orientation, or should the table be transposed? Resolving it requires visual comparison of a known level against its file, which is a manual check no test performs.

### `ScreenManager.CreateScreen`'s effective reflection surface

The name is prefixed with `SokobanCLI.UI.Screens`, which in practice limits resolution to that namespace. `ARCHITECTURE.md` describes the surface as accepting "any type name resolvable in the loaded assembly", which overstates it, while also recommending constraining it to the expected namespace — which the prefix already does.

Open question: was the prefix intended as the constraint, or is it incidental? The `!` null-forgiving operator and the `as Screen` cast suggest the reflection path was not hardened deliberately. See [security.md](./security.md).

### Whether levels beyond 0 are valid

Only level 0 is loaded by tests. Levels 1–20 have verified dimensions (each file has 14 lines of 14 characters) but no test asserts their content, box/goal balance, solvability, or that the player marker exists exactly once.

Open question: is every level solvable? Answering requires either a solver or manual play; the repository provides neither.

### Whether a level can be complete at load time

`GetCompletion` returns true when no cell holds identifier 3. A level whose goals are all encoded as identifier 5 (box on goal) would therefore start complete and immediately advance, and a level with no goals at all would do the same. No shipped level does this, and no validation prevents it.

## Implementational oddities without stated intent

| Observation | Evidence | Why it is ambiguous |
|-------------|----------|---------------------|
| `Program.game` is assigned and never read | `Program.Main` | Dead state; either a leftover or the intended owner before `GameScreen` took over |
| `GameManager` branches on literal identifiers rather than `TileType` or `Tile.IsWalkable` | `GameManager.MovePlayer` | `IsWalkable` exists, is correct, is tested nowhere, and is never called; it is unclear whether the model or the rules were meant to own the predicate |
| `MenuScreen`'s horizontal navigation is inverted | `D`/`RightArrow` decreases `ItemNumber`; `A`/`LeftArrow` increases it | The vertical case is conventional; the horizontal case is the opposite. No comment explains it, and `TitleScreen` uses the vertical axis so the inversion is never exercised |
| `UiElement.CanRaiseEvents` is never overridden | `UiElement` | The virtual hook exists and every non-menu element passes it, but nothing distinguishes one element from another |
| `UiElement.Margins` is never read | `UiText` | A serialisable property with no consumer |
| `Scale2D` has no production consumer | Only `Scale2DTests` references it | Intentional shared kernel, or an unused extraction? |
| The three mapping extension classes have no production consumer | Only the mapping tests reference them | Same question; the `// >>> TO NARIVIA` comments suggest the types were extracted from another project |
| `SizeMappingExtensions.ToSystemPoint` returns a `Size` | `SizeMappingExtensions` | A naming defect: the method should be `ToSystemSize` |
| `Screens/ScreenManager.xml` is copied but never read | `ScreenManager` points the reader at the title screen | Vestigial configuration from an earlier design |
| `UiMenuItem.OnActivated` fires with a null `EventArgs` | `OnActivated(this, null)` | Any handler dereferencing `e` would throw; none exists |
| `PlayerMovementEventArgs`'s XML documentation says "Key." | `PlayerMovementEventArgs` | Copy-paste from a keyed entity type; a documentation defect |
| `sokobancli.ico` is not referenced by any project file | `SokobanCLI.csproj` | An application icon that is not applied |
| `.gitignore` is the stock Visual Studio template | `.gitignore` | Not adjusted for the repository's actual artefacts |
| `gameTime` counts frames and is consumed by nobody | `Program.Update` passes it to `Update(float)` implementations, all of which ignore it | Either an intended clock or scaffolding for future animation |

## Open design questions

| Question | Why it is open | What would resolve it |
|----------|----------------|-----------------------|
| Should the frame loop be paced? | No timing requirement is documented, but the loop saturates a core while idle and `gameTime` counts frames rather than time | A stated target frame rate or a decision that unpaced is acceptable |
| Where should progress be stored? | The `// TODO: Save progress` marker does not name a boundary, and the layer rules constrain the options: `Models` may depend only on geometry and `System.IO`, `GameLogic` only on `Models` and geometry, and `UI` on everything | A decision recorded as an architecture decision, plus a new boundary and rule |
| Should the last level loop, stop, or show a completion screen? | Three TODOs touch level completion and none states the intended end behaviour | A product decision |
| Should `Exit` terminate the process? | `UiMenuAction`'s comment asks "Close the game somehow…", implying `Environment.Exit` or a loop exit condition was undecided | A decision, plus removal of the `while (true)` unconditional loop |
| Should a console abstraction be introduced? | Testing the renderer, the input manager, and any screen requires it, but no abstraction exists and `Input` may not depend on `Graphics`, so a shared abstraction needs a new namespace in the layer matrix | A decision about testability investment |
| Should the resize crash be fixed, and how? | The minimal fix touches batch ownership: either `AsciiSpriteBatch.Update` reallocates both batches (entangling ownership) or `GraphicsManager.Update` observes the resolution (better separation but a new responsibility for an empty method) | A decision recorded here or in the root architecture document; the defect itself is unambiguous |
| Should level content be validated beyond dimensions? | The silent-empty-board failure mode is the most likely content error, and digit validation would remove it | A decision, given that validation would break any level file relying on the current leniency — none does |

## Areas not investigated to completion

| Area | Limitation |
|------|-----------|
| `NuciDAL` 3.1.0 internals | `XmlFileObject<T>` behaviour is treated as a contract: construct with a `Type`, read a path, receive an instance. Its error semantics (exact exception types for malformed content, and whether `Read` tolerates unknown elements) were not verified against package documentation or source |
| Runtime behaviour of the console | Cursor visibility persistence, colour persistence across processes, and the exact exception thrown for an out-of-range `SetCursorPosition` vary by terminal; no terminal was tested |
| Levels 1–20 content | Dimensions verified; solvability, box/goal balance, and player-marker uniqueness not analysed |
| The release helper script | Not fetched or inspected. Its runtime identifier list, archive formats, and failure modes are unknown |
| Windows and macOS behaviour | No test, workflow, or manual verification covers either; the `'\r\n'` risk above is the concrete symptom |
| Visual output | The rendering path was reasoned from code, not observed. Glyph correctness, colour rendering, and layout centring were not verified on screen |

## Related documents

- [design-decisions.md](./design-decisions.md) — the decisions that were made, with rationale
- [invariants.md](./invariants.md) — the assumptions listed here in invariant form
- [troubleshooting.md](./troubleshooting.md) — the observable symptoms of the defects above
- [testing.md](./testing.md) — the coverage gaps that let these survive