# Troubleshooting

Evidenced failure modes with their observable symptoms and resolutions. Each entry is derived from the code, not from speculation; where a cause has not been reproduced it is marked as inferred.

## The process exits with a stack trace after the last box

| Aspect | Detail |
|--------|--------|
| Symptom | The process terminates while playing level 20, with a stack trace naming `Levels/21.lvl` |
| Cause | `GameScreen.Update` advances with `game.Start(game.World.Level + 1)` and there is no last-level guard, so `World`'s constructor calls `File.ReadAllLines("Levels/21.lvl")`, which does not exist |
| Exception | `FileNotFoundException` |
| Resolution | Add a guard in `GameScreen.Update` that stops instead of advancing when the next level file is absent, or wrap the advancement in a check on `File.Exists` before starting. Note that `RuleApplicabilityTests` does not constrain this and no test covers it |
| Confidence | Certain; the exception path is fully traced |

## The process exits with a stack trace naming a level file

| Aspect | Detail |
|--------|--------|
| Symptom | The process terminates while loading a level, with a stack trace naming `Levels/<n>.lvl` |
| Cause | Either the file is missing or its dimensions are wrong |
| Exception | `FileNotFoundException`/`DirectoryNotFoundException` for a missing file or directory; `InvalidDataException` with the path in the message when a file is not 14 lines of 14 characters |
| Resolution | Ensure `Levels/` sits in the process working directory and that every file has exactly 14 lines of 14 digits. Launch with the artefact directory as the working directory, or use `dotnet run --project SokobanCLI` |
| Confidence | Certain |

## The process exits with a stack trace naming a screen file

| Aspect | Detail |
|--------|--------|
| Symptom | The process terminates on startup or on a transition, with an `XmlException` or an IO exception |
| Cause | `Screens/TitleScreen.xml` or `Screens/GameScreen.xml` is missing, malformed, or unreadable during `XmlFileObject<Screen>.Read` |
| Resolution | Restore the file from the repository. Note the asymmetry: a *missing* screen file is tolerated (constructor defaults are used) but a *malformed* one is fatal |
| Confidence | Certain for the exception; the tolerance of a missing file is read from `CreateScreen`'s `File.Exists` check |

## An `ArgumentNullException` on selecting a menu entry

| Aspect | Detail |
|--------|--------|
| Symptom | Activating a menu entry throws `ArgumentNullException` with a null parameter name |
| Cause | The `LinkId` value does not resolve to a type under `SokobanCLI.UI.Screens`, so `Type.GetType` returns null and `Activator.CreateInstance(null)` throws |
| Resolution | Correct the `LinkId` to match an existing screen class name exactly, including case |
| Confidence | Certain |

## A `NullReferenceException` on selecting a menu entry

| Aspect | Detail |
|--------|--------|
| Symptom | Activating a menu entry throws `NullReferenceException` |
| Cause | The name resolved to a type that is not a `Screen` — for example a type in a different namespace whose name happened to match the prefixed string — so the `as Screen` cast produced null and the next statement dereferenced it |
| Resolution | Ensure the name refers to a class deriving from `Screen` in the `SokobanCLI.UI.Screens` namespace |
| Confidence | Inferred from the `as Screen` cast plus the null-forgiving operator; not reproduced because no such type exists in the repository |

## A `FormatException` when starting a game screen

| Aspect | Detail |
|--------|--------|
| Symptom | Selecting a menu link with arguments throws `FormatException` |
| Cause | `GameScreen.LoadContent` calls `int.Parse(ScreenArgs[0])` without validation, and the `LinkArgs` value is not an integer |
| Resolution | Supply a numeric `LinkArgs`, or add validation in `GameScreen.LoadContent` |
| Confidence | Certain |

## The level advances but the display shows the previous level

| Aspect | Detail |
|--------|--------|
| Symptom | After completing a level the details panel shows the new level number but the grid still looks like the old one |
| Cause | Not expected from the code: advancement happens before `Draw` in the same frame, and `UiWorldmap.Draw` reads `game.World` fresh. The plausible cause is a manual grid inspection during a paused debugger session |
| Resolution | Step one frame rather than inspecting mid-update. If the display genuinely lags, check whether `drawnData` has diverged, which would suppress the diff |
| Confidence | Speculative; recorded because "display does not update" has a distinct known cause below and the two are easy to confuse |

## The display is blank or stale and never refreshes

| Aspect | Detail |
|--------|--------|
| Symptom | The console shows old or empty content and nothing repaints it, even though the process is running |
| Cause | The shadow batch `drawnData` is believed to match the console while the console has actually been cleared or rewritten by something else — a terminal reflow, a `Ctrl+L`, or a resize that reflowed the buffer. The render batch still holds the same content, so `Draw` finds no differences and writes nothing |
| Resolution | Force a difference: move the player or press `Esc` and re-enter, which triggers `ChangeScreens`, or resize and let `AsciiSpriteBatch.Update` reallocate. To fix it properly, give `GraphicsManager` an explicit invalidate that clears `drawnData`'s recorded content |
| Confidence | Inferred from the diff algorithm; the invalidating event is external so it has not been reproduced here |

## The process exits with `IndexOutOfRangeException` after resizing the terminal

| Aspect | Detail |
|--------|--------|
| Symptom | The process crashes with `IndexOutOfRangeException` shortly after the terminal is resized, most reliably when it is enlarged and then a screen transition occurs |
| Cause | `AsciiSpriteBatch.Update` reallocates the render batch when the resolution changes, but `GraphicsManager.drawnData` is allocated only in `LoadContent`. Enlarging makes `Draw` index past the shadow arrays; shrinking makes `Clear` index past the render arrays, and `Clear` iterates the shadow dimensions while indexing the render batch |
| Resolution | Do not resize while playing. A fix is to reallocate `drawnData` in `GraphicsManager.Update` when the resolution changes, and to iterate the smaller dimension in `Clear`. This is untested because it requires a console |
| Confidence | Certain from the array bounds; the exact triggering size change depends on the console |
| Related | [invariants.md](./invariants.md) INV-14 |

## The cursor is invisible after the process exits

| Aspect | Detail |
|--------|--------|
| Symptom | After a crash or an interruption, the shell prompt has no visible cursor, or the terminal retains unusual colours |
| Cause | `Program.Main` sets `Console.CursorVisible = false` and nothing restores it; `GraphicsManager.Draw` sets foreground and background colours per cell and never resets them. On an unhandled exception neither is undone |
| Resolution | `reset` in the terminal, or `tput cnorm` to restore the cursor. A fix would be a `finally` around the loop, or a handler that restores both |
| Confidence | Certain from the code; the terminal's own behaviour determines how long it persists |

## A wall renders as `?`

| Aspect | Detail |
|--------|--------|
| Symptom | An isolated wall or a wall with a single connection shows `?` instead of a box-drawing glyph |
| Cause | `UiWorldmap.GetWallShape` maps only sixteen of the possible connectivity masks; a mask with no mapping falls through to the initialised `?` |
| Resolution | Expected behaviour for unusual wall arrangements, not a defect. Mask the arrangement so every wall has at least two orthogonal neighbours, or extend the mapping table |
| Confidence | Certain; the `char c = '?';` initialiser and the absence of a default case confirm it |

## A wall's box-drawing glyphs look rotated

| Aspect | Detail |
|--------|--------|
| Symptom | Horizontal walls render with vertical glyphs or vice versa |
| Cause | The grid is transposed — `World.LoadTiles` reads `rows[x][y]`, so a level file line is a grid column — and `GetWallShape`'s neighbour names are derived from that orientation while the glyph table is a conventional one |
| Resolution | Expected given the transposition. Choose the glyph table to match the orientation the level author intended, or document the orientation in the level file directory |
| Confidence | Certain from the indexing |
| Related | [invariants.md](./invariants.md) INV-2 |

## A level loads but the board is empty

| Aspect | Detail |
|--------|--------|
| Symptom | A level loads without error but every cell is floor and the player is in a corner |
| Cause | Characters that are not digits in `0`–`6` produce identifiers that `Tiles.ById` resolves to an empty tile, and a level with no identifier 4 or 6 leaves `PlayerPosition` at its default `(0, 0)`. Dimension validation passes because the file still has 14 lines of 14 characters |
| Resolution | Verify every character is a digit in range and that exactly one player marker exists. Adding digit validation to `World.ValidateRows` would convert this silent failure into a clear error |
| Confidence | Certain from `Tiles.ById`'s fallback and the missing validation |

## Movement does nothing

| Aspect | Detail |
|--------|--------|
| Symptom | Keys are accepted by the terminal but the player does not move, and the move counter does not change |
| Cause candidates | `IsRunning` is false (the game was stopped and not restarted); the target cell is a wall; the target is out of bounds; the game screen lost input focus because a `UiElement` with focus consumed the key |
| Resolution | Press `R` to restart, which calls `Start` and sets `IsRunning` true. If the problem persists, verify the target cell's identifier. Note that `GameScreen.OnKeyPressed` runs regardless of UI focus — the raw key reaches the screen because `Screen` subscribes directly — so a focused element cannot block movement; it can only additionally consume the key if it is a menu item, which the game screen has none of |
| Confidence | Certain for the guard branches; the focus explanation is recorded as a non-cause |

## Immediately after a key the player appears to jump two cells

| Aspect | Detail |
|--------|--------|
| Symptom | A single key press appears to move the player twice, or the display lags one cell behind |
| Cause | A movement key triggers two draws: `Program`'s redraw bridge (subscribed before the game screen existed, so it runs first and paints the pre-move frame) and `PlayerMoved` (which paints the post-move frame). Both are correct and the second wins, so the visible result is right, but the intermediate flush is visible on a slow terminal |
| Resolution | Not a defect. Removing either draw would reduce work but the bridge also serves the title screen, and the `PlayerMoved` handler is what makes the move appear within the same update |
| Confidence | Certain from subscription order |

## `dotnet run` fails because the content is not found

| Aspect | Detail |
|--------|--------|
| Symptom | `FileNotFoundException` naming `Levels/0.lvl` when running a built or published artefact |
| Cause | Paths are relative to the process working directory. `dotnet run --project SokobanCLI` sets it to the project directory; launching the assembly elsewhere does not |
| Resolution | Launch from the directory containing `Levels/` and `Screens/`, which is what the build copies them to |
| Confidence | Certain |

## The build fails with an architecture test violation

| Aspect | Detail |
|--------|--------|
| Symptom | `dotnet test` fails in `SokobanCLI.ArchitectureTests` with a message naming a file, line, and rule |
| Cause | A convention or dependency rule was violated. The message format is `<file>:<line>: <message>` from `ProductionSources.Describe` |
| Resolution | Read the rule name in the message and consult [testing.md](./testing.md) for the full rule list, then fix the code rather than the rule unless the rule is genuinely wrong |
| Confidence | Certain; the assertions are self-describing |

## `RuleApplicabilityTests` fails after adding a capability

| Aspect | Detail |
|--------|--------|
| Symptom | A test named `GivenTheProductionSources_WhenInspected_ThenNo…Exists` fails with a message ending "become enforceable once … exists" |
| Cause | This is the intended forcing function. Introducing logging, HTTP, persistence, a game framework, a `Directory.Build.props`, `IsPackable=true`, a partial type, or xUnit invalidates the precondition that kept a rule family inapplicable |
| Resolution | Write the corresponding rules in the appropriate convention fixture in the same change, then update or remove the applicability test. Do not simply delete the assertion |
| Confidence | Certain; it is the documented purpose of the fixture |

## Related documents

- [error-handling.md](./error-handling.md) — the complete exception surface
- [invariants.md](./invariants.md) — the invariants behind the failures above
- [build-and-deployment.md](./build-and-deployment.md) — working-directory and deployment setup