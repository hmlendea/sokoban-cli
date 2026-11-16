# Repository overview

## Purpose

SokobanCLI is a terminal implementation of the Sokoban puzzle. The player moves a character on a fixed 14 × 14 grid, pushes boxes, and must cover every goal tile. The program renders the grid and a small details panel in the console, counts moves, and advances to the next level when the current level contains no uncovered goal.

The repository exists to deliver that single executable plus its level and screen content. It is not a library and publishes nothing to a package registry.

## Scope

Owned by this repository:

- The production console application `SokobanCLI` (one executable project).
- Level content: `SokobanCLI/Levels/0.lvl` … `20.lvl`, twenty-one files.
- Screen content: `SokobanCLI/Screens/TitleScreen.xml`, `GameScreen.xml`, and an empty `ScreenManager.xml`.
- Unit tests in `SokobanCLI.UnitTests`.
- Architecture and convention tests in `SokobanCLI.ArchitectureTests`.
- Repository documents (`README.md`, `ARCHITECTURE.md`, `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SECURITY.md`, `PRIVACY.md`, `LICENSE`) and three GitHub Actions workflows.

## Boundary

Deliberately not owned here:

- Release helper scripting. `.github/workflows/github-release.yml` downloads and executes `https://raw.githubusercontent.com/hmlendea/dotnet-release-scripts/refs/heads/master/dotnet/<version>.sh`, so packaging mechanics live outside the repository.
- Runtime hosting. There is no container, service, or installer definition; the artefact is a self-contained console build.
- Progress persistence. Nothing is written to disk by the production assembly.

## Actors

| Actor | Interaction |
|-------|-------------|
| Player at a terminal | Supplies keystrokes; observes the rendered grid and details panel |
| Contributor | Edits code, content, or tests; runs `dotnet build` and `dotnet test` |
| Maintenance automation | `.github/workflows` builds, tests, audits packages, runs CodeQL, and publishes release artefacts |
| CI/CD platform | Provides the console-less build environment; never runs the actual game |

## Entry point

`SokobanCLI.Program.Main` in `SokobanCLI/Program.cs`. It is `private static void Main()` with no arguments, so the process cannot be parameterised from the command line. Startup hides the cursor, constructs a `GameManager`, loads content, clears the console, and never returns.

## Principal capabilities

- Load a validated 14 × 14 level grid from a digit-encoded text file.
- Validate movement against bounds and tile identity, including box pushing with goal capture.
- Count accepted moves and detect completion by scanning for uncovered goals.
- Render a character/colour framebuffer to the console, writing only changed cells.
- Resolve walls into box-drawing glyphs from neighbouring wall connectivity.
- Deserialise screen layouts, colours, and menu entries from XML into live UI elements.
- Transition between screens by type name through reflection.
- Enforce layer, naming, layout, project, and test conventions through automated tests.

## Deliberately absent

- No network, database, telemetry, or authentication code; `RuleApplicabilityTests` asserts the absence of network, logging, and persistence fragments in production sources.
- No saved progress or settings, despite the README feature list referring to saved settings and progress; the code contains only `// TODO: Save progress`.
- No exception handling, no exit path from the frame loop, and no last-level guard.
- No coverage tooling, no linter configuration file, and no `.editorconfig`.