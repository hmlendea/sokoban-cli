[![Donate](https://img.shields.io/badge/-%E2%99%A5%20Donate-%23ff69b4)](https://hmlendea.go.ro/funding)
[![Latest Release](https://img.shields.io/github/v/release/hmlendea/sokoban-cli)](https://github.com/hmlendea/sokoban-cli/releases/latest)
[![Build Status](https://github.com/hmlendea/sokoban-cli/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/sokoban-cli/actions/workflows/dotnet.yml)
[![Security Scan](https://github.com/hmlendea/sokoban-cli/actions/workflows/security.yml/badge.svg)](https://github.com/hmlendea/sokoban-cli/actions/workflows/security.yml)
[![License](https://img.shields.io/github/license/hmlendea/sokoban-cli)](https://github.com/hmlendea/sokoban-cli/blob/master/LICENSE)

# SokobanCLI

SokobanCLI is a terminal implementation of the classic Sokoban box-pushing puzzle. Move the player, push every box onto a goal, and clear 21 levels while minimising moves.

## 📑 Table of Contents

- [Capabilities](#capabilities)
- [Usage](#usage)
  - [Keyboard Shortcuts](#keyboard-shortcuts)
- [Known Limitations](#known-limitations)
- [System Requirements](#system-requirements)
- [Installation](#installation)
  - [Manual Installation](#manual-installation)
  - [Installation from Source](#installation-from-source)
  - [Verification](#verification)
- [Privacy and Data](#privacy-and-data)
- [Development](#development)
  - [Requirements](#requirements)
  - [Setup](#setup)
  - [Build](#build)
  - [Run](#run)
  - [Test](#test)
  - [Static Analysis](#static-analysis)
  - [Continuous Integration](#continuous-integration)
  - [Release](#release)
  - [Dependencies](#dependencies)
- [GitHub Actions](#github-actions)
- [Project Structure](#project-structure)
  - [Projects and Packages](#projects-and-packages)
  - [Directories](#directories)
- [Architecture](#architecture)
- [Deployment](#deployment)
- [Documentation](#documentation)
- [Troubleshooting](#troubleshooting)
- [FAQ](#faq)
- [Contributing](#contributing)
- [Code of Conduct](#code-of-conduct)
- [Security](#security)
- [Support](#support)
- [Project Engagement](#project-engagement)
- [License](#license)

## ✨ Capabilities

- Play 21 Sokoban levels numbered 0 through 20 in an interactive terminal.
- Push boxes onto goals, count accepted moves, and advance automatically after completion.
- Render walls, boxes, goals, and solved boxes with coloured ASCII characters.
- Navigate a title menu and an XML-defined game screen.

## 🚀 Usage

Run the game from the project directory so its `Levels/` and `Screens/` content is available:

```bash
dotnet run --project SokobanCLI
```

### Keyboard Shortcuts

| Shortcut | Action | Context |
|----------|--------|---------|
| `W` / `↑` | Move up | Game screen |
| `A` / `←` | Move left | Game screen |
| `S` / `↓` | Move down | Game screen |
| `D` / `→` | Move right | Game screen |
| `R` | Restart the current level | Game screen |
| `Enter` or `E` | Activate the highlighted menu entry | Title menu |
| `Esc` | Return to the title screen | Game screen |
| `Ctrl+C` | Terminate the process | Any screen |

## ⚠️ Known Limitations

- Progress and move counts are held in memory and are not saved between runs.
- Selecting the title-menu `Exit` entry does not terminate the process; use `Ctrl+C`.
- Resizing the terminal while playing can crash the renderer; set the terminal size before launching.
- Completing level 20 currently attempts to load missing level 21 and terminates with an exception.

## 🖥️ System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| .NET | .NET 10 runtime | .NET 10 SDK |
| Terminal | Interactive terminal with cursor positioning and colour support | Terminal at least 14 rows for the map plus 6 for the details panel |

## 📦 Installation

[![Obtain it from GitHub](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/github.png)](https://github.com/hmlendea/sokoban-cli/releases)

### Manual Installation

Download the archive for your platform from the [latest GitHub release](https://github.com/hmlendea/sokoban-cli/releases/latest). Keep its `Levels/` and `Screens/` directories beside the executable, then launch it from that directory.

### Installation from Source

```bash
git clone https://github.com/hmlendea/sokoban-cli.git
cd sokoban-cli
dotnet run --project SokobanCLI
```

### Verification

```bash
dotnet test SokobanCLI.slnx
```

## 🛡️ Privacy and Data

SokobanCLI collects no personal data, analytics, or telemetry, and creates no settings, progress, cache, or log files. Gameplay state remains in memory and is discarded when the process exits.

For the detailed description of how the application handles privacy and personal data, see the [Privacy Statement](./PRIVACY.md).

## 🛠️ Development

### Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- An interactive terminal for running the game locally

### Setup

Clone the repository and restore dependencies with the .NET SDK:

```bash
git clone https://github.com/hmlendea/sokoban-cli.git
cd sokoban-cli
dotnet restore
```

### Build

```bash
dotnet build SokobanCLI.slnx
```

### Run

```bash
dotnet run --project SokobanCLI
```

### Test

```bash
dotnet test SokobanCLI.slnx
```

### Static Analysis

The security workflow checks NuGet packages for known vulnerabilities and runs CodeQL for C#:

```bash
dotnet list package --vulnerable --include-transitive
```

### Continuous Integration

The `.NET` workflow restores dependencies, builds the solution, and runs tests on pushes and pull requests targeting `master`. The `Security` workflow audits packages and runs CodeQL on pushes, pull requests, and a weekly schedule.

### Release

Publish a release with a `v`-prefixed tag, for example `v1.0.0`:

```bash
gh release create v1.0.0 --generate-notes
```

The `GitHub Release` workflow determines the .NET version from `SokobanCLI/SokobanCLI.csproj`, downloads and executes an external helper from `https://raw.githubusercontent.com/hmlendea/dotnet-release-scripts/refs/heads/master/dotnet/<version>.sh`, and attaches self-contained artefacts to the release. Review the external script before executing or publishing a release.

### Dependencies

| Package | Version | Scope | Purpose |
|---------|---------|-------|---------|
| `NuciDAL` | `3.1.0` | Runtime | XML deserialisation of screen definitions |
| .NET | `10.0` | Runtime | Console input/output, file access, and application hosting |

## ⚙️ GitHub Actions

| Workflow | Purpose | What it does |
|----------|---------|--------------|
| [ .NET ](https://github.com/hmlendea/sokoban-cli/blob/master/.github/workflows/dotnet.yml) | Continuous integration | Determines the .NET version, restores dependencies, builds the solution, and runs the test suite on pushes and pull requests to `master`. |
| [ Security ](https://github.com/hmlendea/sokoban-cli/blob/master/.github/workflows/security.yml) | Security scanning | Restores dependencies, checks for vulnerable packages, builds the solution, and runs CodeQL analysis for C# on pushes, pull requests, and a weekly schedule. |
| [ GitHub Release ](https://github.com/hmlendea/sokoban-cli/blob/master/.github/workflows/github-release.yml) | Release publishing | Determines the .NET version, extracts the version from the release tag, builds self-contained artefacts through the shared release helper, and attaches them to the published release. |

## 🗂️ Project Structure

SokobanCLI is organised as one production executable and two test projects.

### Projects and Packages

| Project | Type | Purpose |
|---------|------|---------|
| `SokobanCLI` | .NET console application | Runs the game |
| `SokobanCLI.UnitTests` | NUnit test project | Verifies gameplay and geometry behaviour |
| `SokobanCLI.ArchitectureTests` | NUnit architecture test project | Verifies project, source, naming, layering, and convention rules |

### Directories

| Directory | Purpose |
|-----------|---------|
| `SokobanCLI/GameLogic` | Movement, box-pushing, and completion rules |
| `SokobanCLI/Graphics` | ASCII rendering and geometry |
| `SokobanCLI/Input` | Keyboard polling |
| `SokobanCLI/Levels` | 21 level definitions |
| `SokobanCLI/Screens` | XML screen definitions |
| `SokobanCLI/UI` | Screens, menus, and UI elements |
| `docs` | Architecture, behaviour, flow, testing, and maintenance documentation |

## 🏗️ Architecture

SokobanCLI is a single-project console application whose static composition root wires singleton managers around a layered model, game-logic, rendering, input, and UI structure.

See the [Architecture Documentation](./ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## 🚢 Deployment

SokobanCLI is deployed as a console executable or a self-contained GitHub release artefact. The executable must run with its `Levels/` and `Screens/` directories in the process working directory. There is no service, container, installer, or persistent deployment state.

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

## 🩺 Troubleshooting

See [Troubleshooting Guide](./docs/troubleshooting.md) for diagnostic procedures and resolutions to recurring problems.

## ❓ FAQ

See the [Frequently Asked Questions](./docs/faq.md) for responses to frequently raised inquiries.

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

## 💬 Support

For support, please [open an issue](https://github.com/hmlendea/sokoban-cli/issues).

## 💝 Project Engagement

Discovered a problem with SokobanCLI or have a suggestion for it? [Open an issue](https://github.com/hmlendea/sokoban-cli/issues)!

If you find this project useful, consider [funding it](https://hmlendea.go.ro/funding) or starring ⭐️ it on GitHub!

[![Donate](https://raw.githubusercontent.com/hmlendea/readme-assets/master/donate_generic.png)](https://hmlendea.go.ro/funding)

## 📄 License

This project is being distributed under the `GNU General Public License v3.0` or later.
See [LICENSE](./LICENSE) for additional information.
