# Level advancement

## Intent

Document the two paths by which the game moves from one level to the next — the `R` restart and the automatic advancement on completion — including the failure at the end of the level set.

## The two paths

| Path | Trigger | Implementation | Uses `ChangeScreens` |
|------|---------|----------------|----------------------|
| Restart | `R` on the game screen | `game.Start(game.World.Level)` | No |
| Advancement | Completion detected in `GameScreen.Update` | `game.Stop()` then `game.Start(game.World.Level + 1)` | No |

Both reuse the same `GameManager` instance and the same `GameScreen`; neither reloads the screen, so UI elements, layout, and subscriptions are untouched. Only `World` is replaced.

## Restart sequence

```mermaid
sequenceDiagram
    participant Console as Console device
    participant Screen as GameScreen
    participant Game as GameManager
    participant World as World (new)

    Console->>Screen: R
    Screen->>Game: Start(World.Level)
    Game->>World: new World(level)
    World->>World: File.ReadAllLines("Levels/level.lvl")
    World->>World: ValidateRows then LoadTiles
    Game->>Game: IsRunning = true, World = new World
    Note over Screen: the next Draw reads the fresh World; Moves is 0 again
```

The `World` from the previous attempt becomes unreachable. Its move count is lost, which is the intended semantics of a restart.

## Advancement sequence

```mermaid
sequenceDiagram
    participant Loop as Program.Update
    participant Screen as GameScreen.Update
    participant UiMgr as UiManager
    participant Game as GameManager
    participant World as World (new)

    Loop->>Screen: Update(gameTime)
    Screen->>UiMgr: Update(gameTime)
    Screen->>Screen: SetChildrenProperties (rebuild details text)
    Screen->>Game: GetCompletion()
    Game->>World: scan for any identifier 3
    World-->>Game: true when none remains
    alt complete
        Game->>Game: Stop (IsRunning = false)
        Note over Screen: Save progress, last level, and transition screen are TODO markers
        Game->>Game: Start(World.Level + 1)
        Game->>World: new World(next level)
        Note over Screen: the same update then returns; draw shows the new level
    end
```

The advancement happens inside `Update`, before `Draw` of the same frame, so the new level's first paint is the frame in which completion was detected. The player sees the level change with no intermediate acknowledgement.

## Completion semantics

`GetCompletion` returns true when no cell holds identifier `3`. Consequences for the last-move experience:

- A box pushed onto the final uncovered goal makes that cell `5`, so the scan finds nothing and the level completes in the same frame the move was made.
- Standing on the final uncovered goal does *not* complete the level, because the cell identifier stays `3` (the player is a position, not a cell value).
- Because completion is evaluated before any acknowledgement, there is no "level complete" message, no move count summary, and no pause.

## State carried and lost

| Item | During advancement |
|------|--------------------|
| Player position | Reset to the new level's identifier-4 or identifier-6 cell |
| `World.Moves` | Reset to 0 |
| `World.Level` | The next level number |
| `Tiles` | A newly allocated 14 × 14 array |
| `GameManager.IsRunning` | Briefly false (between `Stop` and `Start`), then true |
| `PlayerMoved` subscribers | Preserved; `GameScreen`'s subscription survives because the screen is not reloaded |
| UI elements and their subscriptions | Preserved |
| Completed-level progress | Discarded — `// TODO: Save progress` |

The `PlayerMoved` subscription surviving is important: `RegisterEvents` runs once per `GameScreen.LoadContent`, and the same `game` instance is reused, so subscribing again would double-invoke. Because `Start` does not touch `PlayerMoved`, no double subscription occurs on advancement. A restart likewise reuses the instance.

## End of the level set

Levels are numbered `0` through `20` and there is no guard:

```csharp
// TODO: What if this is the last level?
game.Start(game.World.Level + 1);
```

Completing level 20 calls `Start(21)`, which constructs `new World(21)`, which calls `File.ReadAllLines("Levels/21.lvl")`. The file does not exist, so `FileNotFoundException` is thrown, propagates out of `GameScreen.Update`, out of `Program.Update`, and out of `Program.Main`, terminating the process with a stack trace. The move count and level number of level 20 are not displayed after completion.

This is the only way the process terminates through its own code path.

## Level numbering assumptions

- Numbering is contiguous from 0 and used directly as the file name (`<level>.lvl`) and as `World.Level`.
- `GameScreen` displays `game.World.Level` verbatim, so the first level is shown as "Level: 0".
- Inserting a level means renumbering every subsequent file, because the next level is always `Level + 1`.
- `ScreenArgs` can start at any level; `TitleScreen.xml` supplies none, so the normal entry point is level 0, but `<LinkArgs>5</LinkArgs>` would start at level 5. `int.Parse` throws `FormatException` for a non-numeric value.

## Related documents

- [flows/player-movement.md](./player-movement.md) — the move that completes a level
- [components/game-logic.md](../components/game-logic.md) — `Start`, `Stop`, and `GetCompletion`
- [troubleshooting.md](../troubleshooting.md) — the `Levels/21.lvl` failure
- [ambiguities-and-open-questions.md](../ambiguities-and-open-questions.md) — the intended but unimplemented transition screen and progress saving