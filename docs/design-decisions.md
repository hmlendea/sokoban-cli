# Design decisions

Each entry records a decision that is visible in the code, the reasoning it implies, its consequences, and where it is implemented. Where the root [ARCHITECTURE.md](../ARCHITECTURE.md) records the same decision, this document adds the reasoning and the modification consequences.

## Single production project with layered namespaces

| Aspect | Detail |
|--------|--------|
| Decision | One executable project (`SokobanCLI`) whose namespaces carry the layering, rather than separate `SokobanCLI.Models`, `SokobanCLI.GameLogic`, and similar projects |
| Rationale | The application is small; project separation would add five or six project files, reference edges, and build overhead for a codebase of a few hundred lines, and the compiler's protection is not the primary concern |
| Consequence | The compiler cannot prevent a namespace violation, so the layering is enforced by `ProductionArchitectureTests` over the compiled assembly and by Roslyn rules over the sources |
| Cost | A hand-maintained rule set that must be extended as the codebase grows, plus an architecture test project with five packages and a several-second run time |
| Evidence | ProductionArchitectureTests, ProjectReferenceTests |

## Executable layer rules instead of documented conventions

| Aspect | Detail |
|--------|--------|
| Decision | Express the layer boundaries as ArchUnitNET assertions and Roslyn source rules that fail the build |
| Rationale | Documented boundaries drift silently; an executable rule fails at the moment a violation is introduced |
| Consequence | The matrix in [architecture.md](./architecture.md) is verified on every `dotnet test`. Adding a boundary means adding a rule |
| Cost | Rules must be maintained, and a rule that is written too broadly fails legitimate code (for example `SokobanCLI.Graphics` versus `SokobanCLI.Graphics.*`) |
| Evidence | `ProductionArchitectureTests`, and the namespace split between `SokobanCLI.Graphics` and `SokobanCLI.Graphics.Geometry` |

## Static composition root with no container

| Aspect | Detail |
|--------|--------|
| Decision | `Program` wires everything; there is no dependency-injection container |
| Rationale | With four managers and no lifetime variation, a container would add a dependency and indirection for no benefit; keeping the wiring in one file makes startup order readable |
| Consequence | Startup ordering is implicit — it exists only as statement order — and there is no compile-time check that a manager was initialised before use |
| Cost | Reordering four statements in `Program.LoadContent` compiles and fails at run time; the constraints are documented in [components/composition-root.md](./components/composition-root.md) rather than encoded |
| Evidence | `SokobanCLI/Program.cs` |

## Manager singletons reached through static `Instance` properties

| Aspect | Detail |
|--------|--------|
| Decision | `GraphicsManager`, `InputManager`, `ScreenManager`, and `UiManager` are process-wide singletons accessed through a static property with double-checked locking |
| Rationale | `UiElement` and `Screen` need shared graphics, input, and screen state without threading four dependencies through every constructor and every derived type; a service locator avoids that plumbing |
| Consequence | Global mutable state; dependencies are invisible in signatures, so no constructor declares what a type actually needs; substitution for testing is impossible without changing call sites |
| Cost | `ProductionArchitectureTests` can only check the post-hoc namespace graph; unit tests cannot inject a fake screen manager or graphics manager, which is a direct cause of the UI coverage gap in [testing.md](./testing.md) |
| Evidence | The four `Instance` properties; `UiElement.RegisterEvents` reaching `InputManager` directly |

## One static `Main` with an unbounded loop

| Aspect | Detail |
|--------|--------|
| Decision | `private static void Main()` with no arguments and `while (true) { Update(); Draw(); }` |
| Rationale | The classic game-loop shape: poll, update, draw, repeat. For a terminal game there is no need for a scheduler or a host |
| Consequence | The process cannot be parameterised and has no exit path; termination depends on external interruption |
| Cost | No graceful shutdown, no opportunity to restore the cursor or console colours, no way to script a headless run, and no frame pacing so processor usage scales with terminal performance |
| Evidence | `SokobanCLI/Program.cs`; the unimplemented `Exit` action in `UiMenuAction` |

## Diff-based console rendering with a shadow framebuffer

| Aspect | Detail |
|--------|--------|
| Decision | Keep two framebuffers: one the screens compose into, one recording what the console displays; write only cells that differ |
| Rationale | Console writes are the slowest operation available, so avoiding unchanged writes dominates the cost of the comparison |
| Consequence | Rendering cost becomes proportional to change size rather than screen size, which is what makes an unpaced loop tolerable |
| Cost | Correctness depends on the shadow batch mirroring the console. External console writes invalidate it, and a resolution change reallocates only the render batch, producing the resize crash |
| Evidence | `GraphicsManager.Draw`, `GraphicsManager.Clear`, `AsciiSpriteBatch.Update` |

## Content-driven screens read through XML

| Aspect | Detail |
|--------|--------|
| Decision | Screen layout, colours, menu text, and menu destinations live in `Screens/*.xml` and are deserialised by `NuciDAL`'s `XmlFileObject<T>`, which is the reason the project takes a package dependency |
| Rationale | Layout and labelling become content rather than code, so a screen can be adjusted without a rebuild; it also keeps the C# screen classes small |
| Consequence | Screen class names, XML file names, and `LinkId` values form a three-way runtime contract with no compile-time check |
| Cost | Serialisable property names are part of the contract; renaming a property silently drops its XML value; malformed XML fails at run time rather than at build time; and a missing file silently falls back to defaults rather than reporting an error |
| Evidence | `ScreenManager`, `Screens/TitleScreen.xml`, `Screens/GameScreen.xml` |

## Reflection-based screen creation by name

| Aspect | Detail |
|--------|--------|
| Decision | Resolve a screen from a string with `Type.GetType` and instantiate with `Activator.CreateInstance`, rather than a factory switch or a registry |
| Rationale | Avoids a switch statement that must be edited for every screen, and lets XML name its destination directly |
| Consequence | A typo produces a null reference instead of a diagnostic; the surface is broader than the intended screen set (bounded in practice by the namespace prefix the manager prepends) |
| Cost | No compiler protection for the screen-name contract; two distinct failure modes (`ArgumentNullException` for no type, `NullReferenceException` for a non-`Screen` type) where a registry would give one clear error |
| Evidence | `ScreenManager.CreateScreen`; [security.md](./security.md) records the surface analysis |

## Screen class name as the XML file name

| Aspect | Detail |
|--------|---------|
| Decision | `Screen`'s constructor derives `XmlPath` as `Screens/<GetType().Name>.xml` |
| Rationale | A screen can declare its own content location with no registration; renaming the class renames the content file by construction |
| Consequence | The class name is a content address, so renaming a screen class requires renaming its XML file, and a mismatch silently yields default colours and an empty menu |
| Cost | No compile-time link; the coupling is invisible from either side |
| Evidence | `Screen`'s constructor |

## Tiles as a factory with numeric identifiers as the game's vocabulary

| Aspect | Detail |
|--------|---------|
| Decision | `TileType` enumerates 0–6 matching the level digits, `Tiles` maps identifiers to glyph and colour, and `GameManager` branches on literal identifiers |
| Rationale | The level file encoding, the tile model, and the rules share one vocabulary, so loading a level is a digit-to-tile mapping with two special cases for the player |
| Consequence | The identifiers are a contract shared by code and all 21 level files; changing one requires migrating every level |
| Cost | `GameManager` does not use `TileType` or `Tile.IsWalkable`, so the rules and the model can diverge unnoticed; no test asserts `IsWalkable` or that the enumeration values match the digits |
| Evidence | `TileType`, `Tiles`, `GameManager.MovePlayer` |

## Transposed level layout

| Aspect | Detail |
|--------|---------|
| Decision | `World.LoadTiles` reads `rows[x][y]`, so a file line is a grid column |
| Rationale | Not recorded anywhere. The layout is consistent within the codebase, and `UiWorldmap.Draw` builds a line per `x` so rendering matches |
| Consequence | Level files read as columns; wall glyph selection is described with neighbour names that do not match their screen direction; a designer's intuition about orientation is wrong |
| Cost | Any level-authoring tool must apply the same transposition; the assumption is undocumented outside code comments and is recorded here as one of the repository's least obvious facts |
| Evidence | `World.LoadTiles`, `UiWorldmap.Draw`, the `GameManagerTests` comment block |

## The player as a position rather than a cell value

| Aspect | Detail |
|--------|---------|
| Decision | Identifiers 4 and 6 set `PlayerPosition` and are immediately rewritten to 0 or 3; `GameManager` writes only the position, never the grid |
| Rationale | The player and the floor underneath are independent: walking over a goal must not consume it, and the renderer needs to distinguish a player on a goal from a player on floor |
| Consequence | Completion is defined purely in terms of box coverage, so standing on the last goal does not complete a level |
| Cost | The renderer needs coordinate comparison rather than a cell lookup; a level file with no player marker leaves the position at `(0, 0)` with no error |
| Evidence | `World.LoadTiles`, `GameManager.MovePlayer`, `UiWorldmap.Draw`, `GameManager.GetCompletion` |

## Silent guards instead of exceptions for expected outcomes

| Aspect | Detail |
|--------|---------|
| Decision | Blocked moves, absent keys, disabled elements, and unknown axes return early rather than throwing or reporting |
| Rationale | These are ordinary gameplay situations, not errors; a move into a wall is a routine event |
| Consequence | Rejected moves are side-effect-free, which is asserted by the movement invariants and covered by tests |
| Cost | No diagnostic exists for a rejected move, so "the player is stuck" is indistinguishable from "the input was not delivered" without inspecting state |
| Evidence | `GameManager.MovePlayer`, `InputManager.Update`, `UiElement.OnInputKeyboardKeyPressed` |

## No error handling at the application boundary

| Aspect | Detail |
|--------|---------|
| Decision | No `try` statement anywhere in production code |
| Rationale | Not recorded. The plausible reading is that content errors were expected to be caught during development, and that a console game has no meaningful recovery path |
| Consequence | Every unexpected condition terminates the process with a stack trace, leaving the terminal with the application's colours and a hidden cursor |
| Cost | A malformed level or screen file is a hard crash; the most easily triggered crash is a terminal resize |
| Evidence | [error-handling.md](./error-handling.md) enumerates every surface; there is no `catch` in the production sources |

## No persistence

| Aspect | Detail |
|--------|---------|
| Decision | Nothing is written to disk, and no progress, settings, or score is retained |
| Rationale | Recorded only as intent: `GameScreen.Update` carries `// TODO: Save progress`, and the target state in the root architecture document describes saving progress as future work |
| Consequence | Restarting the process always begins at level 0; the README's claim of saved settings and progress is not implemented, while `PRIVACY.md` and the code agree that nothing is written |
| Cost | A player cannot resume; the gap between README and behaviour is the largest documentation defect in the repository |
| Evidence | `GameScreen.Update`; `PRIVACY.md`; [state-and-persistence.md](./state-and-persistence.md) |

## `InternalsVisibleTo` forbidden

| Aspect | Detail |
|--------|---------|
| Decision | Production exposes no internals to test projects |
| Rationale | Keeps the tested surface equal to the public surface, so test convenience cannot shape production visibility |
| Consequence | Unit tests use only public members; `GameManagerTests` arranges boards by writing the public `World.Tiles` array, which is why that member is a public mutable field rather than an encapsulated grid |
| Cost | Behaviour reachable only through private members — `TryMoveBox`, `GetWallShape`, `ScreenManager.CreateScreen` — is testable only through its effects, and private-method behaviour can regress without a focused test |
| Evidence | `ProductionArchitectureTests`, `ProjectReferenceTests` |

## Rule applicability as enforced preconditions

| Aspect | Detail |
|--------|---------|
| Decision | `RuleApplicabilityTests` asserts that conditions making other rules inapplicable are *absent*, so introducing such a condition fails a test |
| Rationale | Without it, a rule set covering logging, networking, persistence, and game-framework conventions would be silently skipped for this repository, and would stay skipped after those capabilities arrive |
| Consequence | Adding logging, HTTP, a database, a game framework, a `Directory.Build.props`, package publishing, a partial type, or xUnit breaks the build until the corresponding rules are written |
| Cost | An intentional capability change must be accompanied by rule work, which is a deliberate friction |
| Evidence | `RuleApplicabilityTests`; each test's assertion message names the rule family that becomes enforceable |

## Decisions deliberately not taken

| Not decided | Why it matters |
|-------------|----------------|
| A console abstraction | `GraphicsManager` and `InputManager` call `Console` directly, and `Input` may not depend on `Graphics`, so headless testing or an alternative backend needs a new shared namespace referenced by both |
| A frame-rate limit | The loop has no pacing, so idle processor usage is a design property rather than a bug |
| A last-level guard | Advancing past level 20 crashes; the fix is a decision in `GameScreen.Update` that the TODOs record as unmade |
| A transition screen between levels | Recorded as `// TODO: Switch to a transition screen before starting` |
| Progress storage location | Recorded as `// TODO: Save progress` with no boundary chosen; whichever is chosen must respect the layer rules, and `Models` may not depend on `UI` |
| Global exception handling | No boundary exists; adding one means deciding what a console game should do after a content fault |

## Related documents

- [architecture.md](./architecture.md) — the resulting structure
- [invariants.md](./invariants.md) — what these decisions imply must stay true
- [change-guide.md](./change-guide.md) — how to modify the areas these decisions affect