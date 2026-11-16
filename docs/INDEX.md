# Documentation index

## Repository summary

SokobanCLI is a single-project .NET 10 console application that plays the classic Sokoban box-pushing puzzle in a terminal. One static composition root (`SokobanCLI.Program`) drives an unbounded update/draw loop over four layers: a `World` model that loads `Levels/*.lvl` grids, a `GameManager` that validates movement and box pushing, a diff-based ASCII renderer that writes only changed console cells, and an XML-driven screen/UI layer read through `NuciDAL`. Layer boundaries are enforced by an automated architecture test project rather than by project separation.

## Root document map

- [README.md](../README.md) — project description, gameplay controls, requirements, usage, development commands
- [ARCHITECTURE.md](../ARCHITECTURE.md) — authoritative architectural description of the current implementation
- [CONTRIBUTING.md](../CONTRIBUTING.md) — contribution workflow and expectations
- [CODE_OF_CONDUCT.md](../CODE_OF_CONDUCT.md) — community standards
- [SECURITY.md](../SECURITY.md) — vulnerability reporting policy
- [PRIVACY.md](../PRIVACY.md) — statement that no personal data is collected
- [LICENSE](../LICENSE) — GNU General Public License v3.0 or later

There is no `ROADMAP.md` in this repository.

## Documentation catalogue

### Orientation

| Document | Description |
|----------|-------------|
| [repository-overview.md](./repository-overview.md) | Purpose, scope, actors, entry point, and what the repository deliberately does not contain |
| [repository-structure.md](./repository-structure.md) | Source tree layout, project roles, and content directories |
| [quick-start.md](./quick-start.md) | Build, run, and play in the fewest steps |
| [architecture.md](./architecture.md) | Layer relationships, composition strategy, and boundary rationale that the root document does not restate |

### Behaviour

| Document | Description |
|----------|-------------|
| [behaviour/play-a-level.md](./behaviour/play-a-level.md) | What the player observes from key press to rendered cell to level completion |
| [behaviour/navigate-screens.md](./behaviour/navigate-screens.md) | Title menu, focus, activation, and screen transitions as a user experiences them |

### Components

| Document | Description |
|----------|-------------|
| [components/composition-root.md](./components/composition-root.md) | `Program`: startup order, frame loop, and the input-to-draw bridge |
| [components/game-logic.md](./components/game-logic.md) | `GameManager`: movement validation, box pushing, move counting, completion |
| [components/models-and-content.md](./components/models-and-content.md) | `World`, `Tile`, `Tiles`, `TileType`, the level format, and validation |
| [components/graphics.md](./components/graphics.md) | `GraphicsManager`, `AsciiSpriteBatch`, `AsciiSprite`, alignment, and geometry primitives |
| [components/input.md](./components/input.md) | `InputManager` polling and key event contracts |
| [components/screens.md](./components/screens.md) | `ScreenManager`, `Screen`, `MenuScreen`, `TitleScreen`, `GameScreen`, and XML definitions |
| [components/ui-elements.md](./components/ui-elements.md) | `UiManager`, `UiElement`, text, menu item, link, action, and world-map elements |

### Flows

| Document | Description |
|----------|-------------|
| [flows/startup-and-rendering.md](./flows/startup-and-rendering.md) | Process start to first frame, and the per-frame render pipeline |
| [flows/player-movement.md](./flows/player-movement.md) | Key event to accepted move, box push, event, and repaint |
| [flows/screen-navigation.md](./flows/screen-navigation.md) | Menu activation through reflection-based screen creation |
| [flows/level-advancement.md](./flows/level-advancement.md) | Completion detection, restart, and advancement to the next level |

### Reference

| Document | Description |
|----------|-------------|
| [data-model.md](./data-model.md) | Entities, their fields, ownership, and lifecycle |
| [invariants.md](./invariants.md) | Conditions that must remain true for the game to work |
| [state-and-persistence.md](./state-and-persistence.md) | In-memory state, the shadow framebuffer, and the absence of persistence |
| [concurrency-and-scheduling.md](./concurrency-and-scheduling.md) | Single-threaded frame loop, singleton locks, and the missing frame pacing |
| [error-handling.md](./error-handling.md) | Exception surfaces, silent guards, and termination behaviour |
| [logging.md](./logging.md) | The absence of logging and what counts as the only available diagnostics |
| [security.md](./security.md) | Trust boundary, reflection surface, and content tampering exposure |
| [integrations.md](./integrations.md) | Console device, level files, screen XML, and CI/release workflows |
| [dependencies.md](./dependencies.md) | Production, test, and tooling dependencies with architectural consequences |
| [testing.md](./testing.md) | Unit and architecture test strategy, coverage, and gaps |

### Maintenance

| Document | Description |
|----------|-------------|
| [build-and-deployment.md](./build-and-deployment.md) | Build, publish, release workflow, and runtime prerequisites |
| [configuration.md](./configuration.md) | Compile-time, content, and working-directory configuration |
| [design-decisions.md](./design-decisions.md) | Recorded choices with rationale and consequence |
| [change-guide.md](./change-guide.md) | How to safely modify common areas, with the rules that will fail |
| [troubleshooting.md](./troubleshooting.md) | Evidenced failure modes and resolutions |
| [faq.md](./faq.md) | Frequently asked questions grounded in code |
| [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md) | TODO markers, implementational oddities, and unresolved questions |
| [documentation-maintenance.md](./documentation-maintenance.md) | How to keep this documentation accurate |

## Navigation aids

**Start here:** [repository-overview.md](./repository-overview.md) → [quick-start.md](./quick-start.md) → [architecture.md](./architecture.md).

**Deep dive for component owners:** [components/game-logic.md](./components/game-logic.md) and [components/screens.md](./components/screens.md) cover the two areas most likely to change; [invariants.md](./invariants.md) lists what may break silently.

**Flows for debuggers:** [flows/startup-and-rendering.md](./flows/startup-and-rendering.md) for "nothing appears", [flows/player-movement.md](./flows/player-movement.md) for "the player cannot move", [flows/level-advancement.md](./flows/level-advancement.md) for "the process dies after the last goal".

**Before editing anything:** [change-guide.md](./change-guide.md), then [testing.md](./testing.md).

## API reference

This repository exposes no HTTP, RPC, or library API. There are no controllers and therefore no `api-reference/` tree; the only public surface is the C# assembly described in [architecture.md](./architecture.md#public-surface) and [invariants.md](./invariants.md).

## Maintenance metadata

- Last reviewed: 2026-11-16
- Owner: repository maintainer (`hmlendea`)
- Coverage: exhaustive over the production project, both test projects, content directories, and the three GitHub Actions workflows. Unverified areas are listed in [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md).