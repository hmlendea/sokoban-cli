# Quick start

## Prerequisites

- .NET 10 SDK (`dotnet --version` must report 10.x). The target framework is declared as `net10.0` in all three project files and is asserted by `ProjectConventionTests`.
- A real interactive terminal. The application polls `Console.KeyAvailable` and positions the cursor, so redirected input or a headless session will not work.

## Get the source

```bash
git clone https://github.com/hmlendea/sokoban-cli.git
cd sokoban-cli
```

## Run

```bash
dotnet run --project SokobanCLI
```

The working directory must contain `Levels/` and `Screens/`. `dotnet run` uses the project directory, which does, so this command works without further setup. A published or copied build must be launched from the directory the content was copied into.

## Build and test

```bash
dotnet build SokobanCLI.slnx
dotnet test SokobanCLI.slnx
```

`dotnet test` runs 101 unit tests plus the architecture and convention suites. Nothing needs to be installed beyond the SDK; the test packages are restored from NuGet.

## Play

| Key | Action |
|-----|--------|
| `W` / `↑` | Decrease the X coordinate (screen "up") |
| `A` / `←` | Decrease the Y coordinate (screen "left") |
| `S` / `↓` | Increase the X coordinate (screen "down") |
| `D` / `→` | Increase the Y coordinate (screen "right") |
| `R` | Restart the current level |
| `Enter` or `E` | Activate the highlighted title-menu entry |
| `Esc` | Leave the game screen and return to the title screen |

Push every box onto a `+` goal. Once no `+` remains the game loads the next level automatically. There is no way to quit from the menu: the `Exit` action is deserialised but not implemented, so terminate the process externally (for example `Ctrl+C`).

## First things to know

- Levels are indexed as `rows[x][y]`, so a level file is effectively transposed relative to the grid the renderer draws. Reading a `.lvl` file top-to-bottom gives columns, not rows.
- `GraphicsManager` writes only cells that changed since the previous frame. Enlarging the terminal grows `SpriteBatch` while the shadow batch keeps its old size, so the next `Draw` indexes past the shadow arrays and throws; resize before launching rather than mid-game.
- Progress is not saved. Restarting the process begins at level 0.