# Frequently asked questions

Answers grounded in the code. Where the repository documents an intention rather than an implementation, that is stated.

## Getting started

### What is SokobanCLI?

A terminal implementation of the Sokoban box-pushing puzzle. You move a character on a 14 × 14 grid, push boxes, and must cover every goal tile. See [repository-overview.md](./repository-overview.md).

### What do I need to run it?

The .NET 10 SDK or runtime, plus a real interactive terminal. The application polls `Console.KeyAvailable` and positions the cursor, so a redirected stdin or a headless session will not work. See [quick-start.md](./quick-start.md).

### How many levels are there?

Twenty-one, numbered 0 through 20, stored as `Levels/0.lvl` … `Levels/20.lvl`. The first level displayed is "Level: 0" because the number comes straight from the file name.

### Why does it start at level 0 instead of 1?

Because the level number *is* the file name and the code uses it directly. `TitleScreen.xml` supplies no `LinkArgs`, so `GameScreen.LoadContent` defaults to 0. Providing `<LinkArgs>5</LinkArgs>` would start at level 5.

### Can I start at a specific level?

Not through the UI. You would edit `Screens/TitleScreen.xml` to add `<LinkArgs>n</LinkArgs>` to the `New Game` link. The value must be a numeral, because `int.Parse` is unguarded.

## Gameplay

### Why does W move the player up but change the X coordinate?

Because the level grid is transposed. `World.LoadTiles` reads `rows[x][y]`, so a line in a level file is a *column* of the grid. `W` therefore decreases X and `A` decreases Y. See [flows/player-movement.md](./flows/player-movement.md).

### Why does standing on the last goal not complete the level?

Because completion is defined as "no cell holds identifier 3", and the player is a position rather than a grid cell, so standing on a goal leaves the cell as `3`. Only a box (identifier 2 pushed onto a goal, becoming 5) removes the goal from the count. See [components/game-logic.md](./components/game-logic.md).

### Do blocked moves count as moves?

No. The move counter increments only after a move is accepted. A push into a wall, a step into a wall, and an out-of-bounds step all leave `World.Moves` unchanged.

### Can I undo a move?

No. There is no undo, no history stack, and no rewind. `R` restarts the current level from scratch, discarding the move count.

### Can I pull a box?

No. Only pushes are implemented, and only directly away from the player: a box is pushed to the cell beyond it in the direction of travel.

### How do I quit?

There is no implemented quit. The `Esc` key returns to the title screen from the game screen, and the `Exit` menu entry exists but its `UiMenuAction` switch arm is empty. Terminate the process externally (`Ctrl+C`). See [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md).

### What happens if I press `Esc` mid-level?

You return to the title screen via `ChangeScreens`, which unloads the game screen and clears its UI elements. The level, its move count, and its progress are discarded: the `GameManager` and `World` belonged to that screen instance.

## Display

### The player glyph is sometimes `☺` and sometimes `☻`. What do they mean?

`☺` is the player on a non-goal cell; `☻` is the player standing on a goal. The renderer chooses by inspecting the underlying cell's identifier.

### Why do walls use box-drawing characters?

`UiWorldmap.GetWallShape` inspects the four orthogonal neighbours of each wall cell and selects a glyph from the resulting connectivity mask — `╬ ╣ ╩ ╠ ╝ ║ ╚ ╦ ╗ ═ ╔` — falling back to `?` for arrangements with no mapping.

### Why is everything white when the tile model has colours?

Because per-tile colour rendering is not implemented. The tile model supplies `Colour` (`Gray` walls, `DarkYellow` boxes, `DarkRed` goals, `DarkGreen` solved boxes), and `TilesTests` asserts those values, but `UiWorldmap.Draw` renders with the element's single foreground colour; the assignments that would apply `Tile.Colour` are commented out.

### What terminal size do I need?

At least 14 rows for the map plus about 6 for the details and controls panels, so roughly 20 rows and 60 columns. The world map has a fixed 14 × 14 size; the panels are placed below it and use the console width for their own size.

### Can I resize the terminal while playing?

Enlarging it crashes the renderer with `IndexOutOfRangeException`, and shrinking it can crash on the next screen transition. The render batch reallocates on a resolution change but the shadow batch does not. See [troubleshooting.md](./troubleshooting.md).

## Progress and data

### Is my progress saved?

No. Nothing is written to disk. `GameScreen.Update` contains the comment `// TODO: Save progress` with no implementation, and `PRIVACY.md` states that the application "creates no settings, progress, or log files".

### The README says settings and progress are saved. Which is right?

The README overstates the current implementation. The README's feature list includes "Settings and progress saved in the local application data directory", but no production code reads or writes such a location, and the repository's own architecture document records the persistence work as intended rather than implemented. The code and `PRIVACY.md` agree with each other; the README does not. See [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md).

### Does it collect any data or send anything anywhere?

No. There is no telemetry, no analytics, no network code of any kind, and no file writes. `RuleApplicabilityTests` asserts the absence of network and telemetry fragments in the production sources.

### Where does it read files from?

`Levels/<n>.lvl` and `Screens/<TypeName>.xml`, both relative to the process working directory. There is no configuration, no search path, and no environment variable override.

## Development

### How do I build and test?

```bash
dotnet build SokobanCLI.slnx
dotnet test SokobanCLI.slnx
```

Expect 101 unit tests and 138 architecture tests, all passing. See [testing.md](./testing.md).

### Why is there an architecture test project?

Because the production code is a single project, so the compiler cannot enforce the layering. `SokobanCLI.ArchitectureTests` asserts the layer dependency matrix with ArchUnitNET and enforces naming, layout, member-shape, collection, error-handling, and test conventions with Roslyn. The project also records which rule families are currently inapplicable, and fails when a precondition for one of them changes.

### Why do my new tests fail to compile or run?

`TestSourceConventionTests` requires: test class names ending in `Tests`, method names of the form `Given…When…Then…`, `Assert.That` for every assertion, a fixture attribute, a `SetUp` method that constructs the subject, top-level fakes in their own files, and at least two cases for every parameterised test.

### Why does introducing `var` or a ternary conditional fail the build?

`CodingConventionTests` forbids both, along with optional parameters, tuple parameters, `ref` parameters, unbraced control-flow bodies, and a suite of related rules. See [change-guide.md](./change-guide.md) for the list.

### Why does adding logging or an HTTP client fail a test?

`RuleApplicabilityTests` asserts that logging, HTTP, persistence, and game-framework code are *absent*, so that their associated rule families stay recorded as inapplicable rather than silently skipped. Introducing one of those capabilities requires writing the corresponding rules in the same change.

### How do I add a level?

Create `SokobanCLI/Levels/<n>.lvl` with exactly 14 lines of 14 digits, remembering that a line is a *column* of the grid. Renumbering is required if you insert rather than append, because `GameScreen` advances with `World.Level + 1`. See [change-guide.md](./change-guide.md).

### How do I add a screen?

Add a class deriving from `Screen` or `MenuScreen` under `SokobanCLI/UI/Screens`, give it a matching `Screens/<TypeName>.xml`, and reference the type name from a `LinkId`. The class name, the XML file name, and every `LinkId` must agree; nothing checks this automatically. See [components/screens.md](./components/screens.md).

### Why does my new screen crash on its first update?

If it derives from `MenuScreen` without declaring any `Link` or `Action`, `MenuScreen.Update` indexes `Items[0]` on an empty list. The clamp guards the upper bound with `Items.Count > 0` but the indexing that follows is unguarded.

### How do I run it in a debugger?

Use the `.vscode/launch.json` configuration, which sets `cwd` to `${workspaceFolder}/SokobanCLI` so relative content resolution works. Without that working directory the first level load throws.

### Can I test the renderer without a console?

Not currently. `AsciiSpriteBatch` exposes `ScreenPreview`, which renders the framebuffer as text and would allow buffer assertions without a console, but no test uses it and no seam isolates `Console` from `GraphicsManager` or `InputManager`.

### Why do two `GameManager` instances exist?

`Program.Main` assigns `game = new GameManager()` to a static field that is never read, while `GameScreen.LoadContent` creates its own. Only the screen's instance receives input and drives the world. The `Program` field is dead state. See [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md).

## Distribution and release

### Is there a package on NuGet or a package manager?

No. The production project is not packable, and the only distribution channel is GitHub release assets produced by `.github/workflows/github-release.yml`.

### How are releases built?

By an external helper script that the release workflow downloads from `https://raw.githubusercontent.com/hmlendea/dotnet-release-scripts/refs/heads/master/dotnet/<version>.sh` and pipes into `bash`. The script is fetched from a moving branch rather than a pinned revision, so a release is not reproducible from this repository alone. The README warns readers to review external scripts before running them.

### What version is this?

There is no version file and no changelog. The version is the release tag, and the workflow strips the leading `v` to obtain the artefact version.

## Related documents

- [repository-overview.md](./repository-overview.md) — purpose and scope
- [quick-start.md](./quick-start.md) — build, run, play
- [troubleshooting.md](./troubleshooting.md) — failure modes and resolutions
- [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md) — documented gaps and TODOs