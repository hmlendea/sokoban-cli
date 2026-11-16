# Play a level

## Intent

Advance from a freshly started game screen to a completed level without leaving the level in an unreachable state, and understand exactly what the player can and cannot do.

## Preconditions

- The process is running from a directory containing `Levels/` and `Screens/`.
- `ScreenManager.Instance` has constructed a `TitleScreen` and deserialised `Screens/TitleScreen.xml`.
- The player has activated the `New Game` link, so `GameScreen.LoadContent` has run and `game.Start(0)` has loaded `Levels/0.lvl`.

## The board

The world map is a 14 × 14 text block placed at the origin. Elements below it show level number, player coordinates, and move count, plus a static controls reminder.

| Symbol | Meaning | Source |
|--------|---------|--------|
| `☺` | Player on empty floor | `UiWorldmap.Draw`, when the player cell's identifier is not 3 |
| `☻` | Player standing on a goal | `UiWorldmap.Draw`, when the player cell's identifier is 3 |
| `O` | Box | `Tile.Character` for identifier 2 |
| `@` | Box on a goal | `Tile.Character` for identifier 5 |
| `+` | Uncovered goal | `Tile.Character` for identifier 3 |
| `█`, or a box-drawing glyph | Wall | `UiWorldmap.GetWallShape`, chosen from orthogonal wall neighbours |
| space | Empty floor | `Tile.Character` for identifiers 0 and 4 |

Wall glyphs are derived, not stored: `GetWallShape` tests the four orthogonal neighbours for identifier 1 and maps the resulting mask to `╬ ╣ ╩ ╠ ╝ ║ ╚ ╦ ╗ ═ ╔`. Because the world is stored as `Tiles[x, y]` but rendered row-major in `[x, y]` order, the x-neighbour test reads `Tiles[x - 1, y]` and is labelled `n` while appearing horizontally adjacent on screen. Masks with no mapping — for example a wall with only a southern neighbour and no other connection — fall back to `?`.

## Walking

1. Press `W`, `A`, `S`, or `D`, or an arrow key. `GameScreen.OnKeyPressed` maps the key to a delta and calls `game.MovePlayer(deltaX, deltaY)`.
2. `GameManager.MovePlayer` ignores the request when `IsRunning` is false or both deltas are zero.
3. The target cell is computed as `playerPosition + new Point2D(deltaX, deltaY)` and rejected when outside `World.Size`.
4. A target identifier of `0` (empty) or `3` (goal) is walkable. Identifiers `2` and `5` (box, solved box) trigger a push. Anything else — in practice `1` (wall) — blocks.
5. On success the player position becomes the target, `World.Moves` increments by one, and `PlayerMoved` is raised.
6. `GameScreen.OnGamePlayerMoved` calls `ScreenManager.Instance.Draw(GraphicsManager.Instance.SpriteBatch)` for an immediate repaint. The next frame's normal draw would produce the same result, so this only reduces visible latency for a single move.

Rejected moves mutate nothing and do not increment the counter, so an attempted push into a wall never "costs" a move.

## Pushing

A push is accepted only when the cell beyond the box is inside the bounds and holds identifier `0` or `3`:

| Box cell | Destination cell | Result |
|----------|------------------|--------|
| `2` (box) | `0` (empty) | Destination becomes `2`; source becomes `0` |
| `2` (box) | `3` (goal) | Destination becomes `5`; source becomes `0` |
| `5` (box on goal) | `0` (empty) | Destination becomes `2`; source becomes `3` (goal restored) |
| `5` (box on goal) | `3` (goal) | Destination becomes `5`; source becomes `3` |
| any | wall, box, or out of bounds | Rejected; nothing changes |

Note that both the source and destination cells are rewritten from the `Tiles` factory (`Tiles.ById(0)`, `ById(2)`, `ById(3)`, `ById(5)`), which returns a shared stateless tile for each identifier. Replacing a grid entry therefore swaps one shared tile instance for another; no tile object is ever mutated by the game rules. Consumers must not mutate a `Tile` obtained from `Tiles`, because tile instances are shared by identifier rather than allocated per cell.

## Restarting

`R` calls `game.Start(game.World.Level)`. `Start` constructs a new `World` for the same level number, resetting tiles, player position, and move count to zero, and sets `IsRunning` to true. It does not clear the UI element list; `GameScreen.SetChildrenProperties` rebuilds the details text on the next update, and the world map re-reads `game.World` on every draw, so the restart is visible immediately.

## Completing

Completion is checked every frame by `GameScreen.Update` through `game.GetCompletion()`, which scans every column and row of the grid and returns false as soon as it finds any tile with identifier `3`. A level is therefore complete when no uncovered goal remains anywhere — including goals covered by a box (`5`) or by the player standing on one (the cell identifier stays `3` in that case, so standing on the last goal does not complete the level; only a box does).

On completion `GameScreen.Update` stops the manager, then immediately starts the next level:

```csharp
if (game.GetCompletion())
{
    game.Stop();

    // TODO: Save progress
    // TODO: What if this is the last level?
    // TODO: Switch to a transition screen before starting
    game.Start(game.World.Level + 1);
}
```

The three TODO comments are the whole specification of what is missing: no progress is written, no last-level check exists, and no transition screen appears. See [flows/level-advancement.md](../flows/level-advancement.md) for the consequences.

## Leaving

`Esc` calls `ScreenManager.Instance.ChangeScreens(nameof(TitleScreen))`, which clears the console using the outgoing screen's background colour, unloads the game screen (unregistering its key handler and clearing the UI element list), creates a `TitleScreen` by name, and loads it. Progress in the abandoned level is lost. See [navigate-screens.md](./navigate-screens.md).

## Postconditions

- Every accepted move increased `World.Moves` by exactly one and moved the player to an adjacent in-bounds cell.
- Every rejected move left `World.Tiles`, `World.PlayerPosition`, and `World.Moves` unchanged.
- Boxes are never destroyed: a push moves a box to an adjacent free or goal cell and leaves the origin as empty or goal.
- A completed level replaced `World` with a fresh 14 × 14 grid for the following level number.

## Failure modes

| Observation | Cause |
|-------------|-------|
| The process exits with a stack trace mentioning `Levels/21.lvl` | Level 20 was completed; the last-level guard is missing |
| A wall renders as `?` | The wall's orthogonal connectivity mask has no mapping in `GetWallShape` |
| The screen shows stale content after resizing the terminal | `GraphicsManager.Draw` writes only changed cells; see [troubleshooting.md](../troubleshooting.md) |
| Movement does nothing at all | `IsRunning` is false, which happens after `Stop` until the next `Start` |