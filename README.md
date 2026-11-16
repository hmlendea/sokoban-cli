[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/sokoban-cli)](https://github.com/hmlendea/sokoban-cli/releases/latest)
[![Build Status](https://github.com/hmlendea/sokoban-cli/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/sokoban-cli/actions/workflows/dotnet.yml)
[![Security Scan](https://github.com/hmlendea/sokoban-cli/actions/workflows/security.yml/badge.svg)](https://github.com/hmlendea/sokoban-cli/actions/workflows/security.yml)
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

Releases are produced by the `GitHub Release` workflow, which runs when a release is published in the repository. Publish the release with its `v`-prefixed tag, for example `v1.0.0`; the tag supplies the version used for the artefacts.

```bash
gh release create v1.0.0 --generate-notes
```

The workflow determines the .NET version from `SokobanCLI/SokobanCLI.csproj`, downloads and executes an external release helper from `https://raw.githubusercontent.com/hmlendea/dotnet-release-scripts/refs/heads/master/dotnet/<version>.sh` to build the self-contained artefacts, and attaches them to the release.

**Note:** Piping into `bash` is an intensely controversial topic. Please review any external scripts before running them in your environment!

## 🛡️ Privacy and Data

SokobanCLI runs entirely on the local machine and collects no personal data, analytics, or telemetry, and it creates no settings, progress, or log files.

For the detailed description of how the application handles privacy and personal data, see the [Privacy Statement](./PRIVACY.md).

## ⚙️ GitHub Actions

| Workflow | Purpose | What it does |
|----------|---------|--------------|
| [ .NET ](https://github.com/hmlendea/sokoban-cli/blob/master/.github/workflows/dotnet.yml) | Continuous integration | Determines the .NET version from the production project, restores dependencies, builds the solution, and runs the test suite on pushes and pull requests to `master`. |
| [ Security ](https://github.com/hmlendea/sokoban-cli/blob/master/.github/workflows/security.yml) | Security scanning | Sets up the .NET SDK, restores dependencies, checks for vulnerable packages, and runs CodeQL analysis for C#, publishing results to the Security tab on pushes, pull requests, and a weekly schedule. |
| [ GitHub Release ](https://github.com/hmlendea/sokoban-cli/blob/master/.github/workflows/github-release.yml) | Release publishing | Determines the .NET version from the production project, extracts the version from the release tag, builds the self-contained artefacts for every supported runtime identifier through the shared release helper, and attaches them to the published release. |

## 🗂️ Project Structure

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

## 🏗️ Architecture

SokobanCLI is a single-project console application whose static composition root wires singleton managers around a layered model, game-logic, rendering, input, and UI structure.

See the [Architecture Documentation](./ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## 📚 Documentation

A structured knowledge base under [`docs/`](./docs) complements the architecture documentation with component deep dives, execution flows, invariants, and maintenance guidance.

| Resource | Description |
|----------|-------------|
| [`docs/INDEX.md`](./docs/INDEX.md) | Navigation hub for the whole documentation set, with a catalogue and reading paths. |
| [`docs/quick-start.md`](./docs/quick-start.md) | Build, run, and play in the fewest steps. |
| [`docs/architecture.md`](./docs/architecture.md) | Layering, composition strategy, and boundary enforcement in depth. |
| [`docs/behaviour/play-a-level.md`](./docs/behaviour/play-a-level.md) | What the player observes from key press to level completion. |
| [`docs/behaviour/navigate-screens.md`](./docs/behaviour/navigate-screens.md) | Title menu, focus, activation, and screen transitions. |
| [`docs/components/`](./docs/components) | Per-component deep dives for the composition root, game logic, models, graphics, input, screens, and UI elements. |
| [`docs/flows/`](./docs/flows) | End-to-end execution flows for startup and rendering, player movement, screen navigation, and level advancement. |
| [`docs/invariants.md`](./docs/invariants.md) | Conditions that must remain true, including the test-enforced conventions. |
| [`docs/testing.md`](./docs/testing.md) | Unit and architecture test strategy, coverage, and known gaps. |
| [`docs/change-guide.md`](./docs/change-guide.md) | How to modify common areas safely, with the rules that will fail. |
| [`docs/troubleshooting.md`](./docs/troubleshooting.md) | Evidenced failure modes and their resolutions. |
| [`docs/faq.md`](./docs/faq.md) | Frequently asked questions grounded in the implementation. |

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Preserve the existing public contract unless a breaking change is intentional
- Submit focused pull requests that conform to the existing code style
- Maintain your branch synchronised with `master`
- Revise the documentation when functionality changes
- Properly test all modifications, including edge cases and error conditions
- Include tests for additional or modified functionality
- Raise a new [issue](https://github.com/hmlendea/sokoban-cli/issues) for problems or suggestions

See the [Contributing Guidelines](./CONTRIBUTING.md) for details on how to report issues, suggest enhancements, and submit changes.

## 📜 Code of Conduct

See the [Code of Conduct](./CODE_OF_CONDUCT.md) for the standards of conduct expected in all project spaces.

## 🔒 Security

SokobanCLI runs entirely offline and processes no untrusted network input, and only the latest released version receives security maintenance.

For information on reporting security vulnerabilities, see the [Security Policy](./SECURITY.md).

## Support

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding).

## License

Licensed under the GNU General Public License v3.0 or later.
See [LICENSE](./LICENSE) for details.
