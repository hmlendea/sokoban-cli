[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/sokoban-cli)](https://github.com/hmlendea/sokoban-cli/releases/latest)
[![Build Status](https://github.com/hmlendea/sokoban-cli/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/sokoban-cli/actions/workflows/dotnet.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://gnu.org/licenses/gpl-3.0)

# SokobanCLI

**SokobanCLI** is a text-based implementation of the classic [Sokoban](https://en.wikipedia.org/wiki/Sokoban) puzzle game that runs entirely in the terminal.

Push every box onto a goal tile to clear the level. The fewer moves, the better.

## Features

- 21 levels (0 – 20)
- Full box-pushing mechanics — boxes can only be pushed onto empty tiles or goal positions
- Move counter per level
- ASCII colour rendering (walls, boxes, goals, and solved boxes each have distinct colours)
- XML-driven UI screens with a title menu and in-game controls display
- Settings and progress saved in the local application data directory

## Gameplay

### Tile legend

| Symbol | Colour | Meaning |
|--------|--------|---------|
| `█` | Gray | Wall |
| `O` | Dark Yellow | Box |
| `+` | Dark Red | Goal |
| `@` | Dark Green | Box on goal (solved) |
| ` ` | — | Empty floor / player position |

### Controls

| Key | Action |
|-----|--------|
| `W` / `↑` | Move up |
| `A` / `←` | Move left |
| `S` / `↓` | Move down |
| `D` / `→` | Move right |
| `R` | Restart current level |
| `Esc` | Quit |

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Running from source

```bash
git clone https://github.com/hmlendea/sokoban-cli.git
cd sokoban-cli
dotnet run --project SokobanCLI
```

## Development

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run --project SokobanCLI
```

### Test

```bash
dotnet test
```

### Release

The repository includes `release.sh`, which delegates to the upstream deployment script used by the project maintainer.

```bash
bash ./release.sh 1.0.0
```

## Project structure

```
SokobanCLI/
├── GameLogic/          # Game rules, player movement, win detection
├── Graphics/           # ASCII sprite batch renderer and geometry types
├── Input/              # Keyboard input manager
├── Levels/             # Level files (0.lvl – 20.lvl)
├── Models/             # Tile and World data models
├── Screens/            # XML screen/UI layout definitions
└── UI/                 # Screen manager, UI elements (text, menu, worldmap)
```

## Contributing

Contributions are welcome. Please maintain cross-platform compatibility and follow the existing code style.

This script downloads and executes an external release helper from `https://raw.githubusercontent.com/hmlendea/deployment-scripts/master/release/dotnet/10.0.sh`.

**Note:** Piping into `bash` is an intensely controversial topic. Please review any external scripts before running them in your environment!

## Project Structure

## Contributing

Contributions are welcome. Please:
- Keep changes cross-platform
- Keep the existing public API intact unless a breaking change is intentional
- Keep pull requests focused and consistent with the existing code style
- Update documentation when behaviour changes

## Support

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding).

## License

Licensed under the GNU General Public License v3.0 or later.
See [LICENSE](./LICENSE) for details.
