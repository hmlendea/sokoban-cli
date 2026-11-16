# State and persistence

## Summary

SokobanCLI keeps all state in memory. It writes nothing to disk, reads nothing but its own content files, and reads no environment variables, command-line arguments, or user configuration. A completed level, the current move count, and any menu selection are discarded when the process ends or when the level changes.

Neither `SokobanCLI.csproj` nor any production source references a settings, preferences, or profile API, and `RuleApplicabilityTests` asserts that no `EntityFramework`, `Dapper`, `DynamoDB`, `SqlConnection`, or `Npgsql` fragment appears in production sources, keeping the persistence boundary unestablished by design.

## In-memory state inventory

| State | Owner | Lifetime | Reset by |
|-------|-------|----------|----------|
| `GameManager.World` and everything it holds | `GameScreen.game` | From `GameScreen.LoadContent` until the screen is unloaded | `Start` replaces it |
| `World.Tiles` cells | `World` | Per level | `Start` |
| `World.PlayerPosition` | `World` | Per level | `Start` |
| `World.Moves` | `World` | Per level | `Start` |
| `GameManager.IsRunning` | `GameManager` | From `Start` until `Stop` | `Start` sets true, `Stop` sets false |
| `GameManager.PlayerMoved` subscribers | `GameScreen` | From screen load to screen unload | `UnregisterEvents` |
| `GraphicsManager.SpriteBatch` contents | Screens and elements | Process lifetime; reset by `Clear` and reallocated on resolution change | `LoadContent`, `Clear` |
| `GraphicsManager.drawnData` | `GraphicsManager` | Process lifetime; allocated in `LoadContent` | `LoadContent`, rewritten by `Draw` and `Clear` |
| `UiManager.UiElements` | `UiManager` | From screen load to screen unload | `UnloadContent` clears it |
| `MenuScreen.ItemNumber` | `MenuScreen` | Per screen instance | New instance on transition |
| `UiElement.HasInputFocus` | `UiManager.FocusElement` | Per frame (reassigned each update) | `FocusElement` clears then sets |
| `UiElement` cached old values | Each element | Per element | `SetChildrenProperties` |
| `ScreenManager.currentScreen` | `ScreenManager` | Process lifetime | `ChangeScreens` |
| `ScreenManager.screenReader` | `ScreenManager` | Process lifetime | `Type` reassigned per transition |
| `InputManager.KeyboardKeyPressed` subscribers | `InputManager` | Process lifetime | Screens and elements unsubscribe on unload |
| `Program.gameTime` | `Program` | Process lifetime | Never; monotonically increasing frame count |

## The two framebuffers

`GraphicsManager` holds two `AsciiSpriteBatch` instances with different roles, and understanding the distinction is necessary for any rendering change.

| Batch | Role | Written by | Read by |
|-------|------|-----------|---------|
| `SpriteBatch` (the render batch) | What the current frame should look like | `AsciiSpriteBatch.Draw(AsciiSprite)` during screen draw | `GraphicsManager.Draw` for the diff; `GameScreen.OnGamePlayerMoved` and `Program`'s bridge pass it to `ScreenManager.Draw` |
| `drawnData` (the shadow batch) | What the console currently displays | `GraphicsManager.Draw` after writing a cell; `GraphicsManager.Clear` | `GraphicsManager.Draw` for the diff |

The diff algorithm depends on `drawnData` accurately mirroring the console. Two operations can break that:

1. **External console clearing** — anything other than the application clearing the screen (for example a terminal resize that reflows, or a `Ctrl+L`) leaves `drawnData` claiming content is present. Since the render batch also still contains that content, `Draw` finds no differences and writes nothing, so the display stays blank until something changes.
2. **A resolution change** — `AsciiSpriteBatch.Update` reallocates the render batch only. The shadow batch retains the old dimensions until `GraphicsManager.Clear` or `LoadContent` runs, and `Clear` iterates the *shadow* dimensions while indexing the *render* batch, which throws after a shrink.

## Persistence: what does not exist

| Expected capability | Reality |
|---------------------|---------|
| Saved progress across sessions | Not implemented. `GameScreen.Update` contains `// TODO: Save progress` with no statement |
| Saved settings or preferences | Not implemented. The README features section mentions "Settings and progress saved in the local application data directory", but no production code reads or writes such a location, and `PRIVACY.md` states that the application "creates no settings, progress, or log files" |
| High scores or move records | Not implemented |
| Level packs from disk | Only `Levels/<n>.lvl` beside the executable, resolved relative to the working directory |
| Configuration files | None |
| Environment variables | None read |
| Command-line arguments | `Main()` takes no parameters |

This divergence between the README's stated feature and the implementation is the most consequential documentation/behaviour mismatch in the repository. `PRIVACY.md` agrees with the code; the README does not. See [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md).

## Content as readable state

The only durable inputs are:

| Content | Location | Format | Read when |
|---------|----------|--------|-----------|
| Level definitions | `Levels/0.lvl` … `20.lvl` | 14 lines × 14 digits, transposed | `World` construction, i.e. every `GameManager.Start` |
| Screen definitions | `Screens/TitleScreen.xml`, `Screens/GameScreen.xml` | XML matching the screen type name | On screen construction or transition |
| Unused screen definition | `Screens/ScreenManager.xml` | Empty root element | Never |

Both directories are copied to the build output with `CopyToOutputDirectory=PreserveNewest`. At runtime the paths are relative (`Path.Combine("Levels", level + ".lvl")` and `Path.Combine("Screens", $"{Type.Name}.xml")`), so the working directory is part of the deployment contract.

## Impact on change

| Change | State consequence |
|--------|-------------------|
| Adding progress persistence | Needs a new boundary outside the current layers (no `Models` dependency on a store exists, and `Models` may not depend on `UI`); `GameScreen.Update`'s TODO is the intended hook |
| Adding a settings screen | Requires a new `Screen` subclass, an XML definition, and a `Link` in the menu; settings would have to live in the same absent store |
| Making the renderer headless | Requires abstracting `Console` behind an interface, because `GraphicsManager` and `InputManager` call it directly and `Input` may not depend on `Graphics`, so a shared console abstraction would need a namespace both may reference |
| Supporting level packs | `World` builds the path from the level number alone, so a directory or prefix would have to become a parameter — and `Start(int level)` would need a matching overload without breaking `IGameManager` |

## Related documents

- [data-model.md](./data-model.md) — field-level detail with owners
- [components/graphics.md](./components/graphics.md) — the diff algorithm and its preconditions
- [invariants.md](./invariants.md) — the invariants that keep the batches consistent