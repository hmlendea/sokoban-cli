# Configuration

## Summary

SokobanCLI has no configuration system. It reads no environment variables, accepts no command-line arguments, and looks for no settings file. Its behaviour is determined by three things: compile-time project properties, the content files beside the executable, and the process working directory.

## Configuration sources

| Layer | Source | Read by | Changes require |
|-------|--------|---------|-----------------|
| Compile-time project properties | `SokobanCLI/SokobanCLI.csproj` | The compiler and the .NET host | Rebuilding |
| Level content | `Levels/*.lvl` beside the executable | `World` on every level load | Editing the file and reloading the level (no rebuild) |
| Screen content | `Screens/*.xml` beside the executable | `ScreenManager` on construction and transition | Editing the file and transitioning to that screen |
| Working directory | Process launch | `World` and `Screen` through relative `Path.Combine` | Relaunching |
| Console geometry | `Console.WindowWidth`, `Console.WindowHeight` | `GraphicsManager.Resolution`, read on every access | Resizing the terminal, which currently crashes the renderer |
| Keyboard | The terminal | `InputManager` | Not configurable |

## Compile-time properties

| Property | Value | Effect |
|----------|-------|--------|
| `OutputType` | `Exe` | Produces an executable assembly with an entry point |
| `TargetFramework` | `net10.0` | Determines the runtime, the SDK CI installs, and which release helper script is fetched. Asserted by `ProjectConventionTests` |
| `RootNamespace` | `SokobanCLI` | Default namespace for files without an explicit namespace declaration; every file in the repository declares one explicitly |
| `ImplicitUsings` | `disable` | Every `using` must be written explicitly, which `NamespaceConventionTests` enforces by requiring usings outside the namespace block |
| `LangVersion` | `latest` | Enables collection expressions, primary constructors, pattern combinators, and `System.Threading.Lock` usage the code relies on |
| `Nullable` | `enable` | Nullable reference analysis is on; `Type.GetType(...)!` and `(… as Screen)!` are deliberate suppressions |

Both test projects declare the same `TargetFramework`, `ImplicitUsings`, `LangVersion`, and `Nullable` values, asserted by `ProjectConventionTests`.

## Content configuration

### Level content

| Aspect | Detail |
|--------|--------|
| Path | `Levels/<level>.lvl`, where `<level>` is the integer passed to `GameManager.Start` |
| Format | 14 lines of exactly 14 characters, each a digit in `0`–`6` |
| Orientation | Transposed: line index is the grid X coordinate, character position within the line is the grid Y coordinate |
| Validation | Line count and line length only |
| Effect of a change | Editing a level changes the playable content with no rebuild; the file is copied to the output directory with `PreserveNewest`, so a build refreshes the copy |
| Runtime write-back | None |

### Screen content

| Aspect | Detail |
|--------|--------|
| Path | `Screens/<ScreenTypeName>.xml`, derived from the screen class name |
| Format | XML whose root element is the type name; child element names match screen and element property names; enumeration values are member names such as `Green` and `Left` |
| Validation | None; a malformed file throws during deserialisation |
| Effect of a change | Changes layout, colours, menu text, and menu destinations without a rebuild |
| Runtime write-back | None |

Screen content has no schema, so the effective configuration surface is the union of serialisable properties on the screen class and its elements. The relevant ones are:

| Screen or element | Configurable properties |
|-------------------|-------------------------|
| `Screen` | `BackgroundColour`, `ForegroundColour`, `HighlightColour`, `Id` |
| `MenuScreen` | `Axis`, `Spacing`, `Link` elements (`Text`, `LinkId`, `LinkArgs`), `Action` elements (`Text`, `ActionId`) |
| `GameScreen` | `Worldmap` (`UiWorldmap`), `GameDetailsLabel`, `GameDetails`, `GameControlsLabel`, `GameControls` (all `UiText`) |
| `UiElement` | `Id`, `Location`, `Size`, `Opacity`, `BackgroundColour`, `ForegroundColour`, `IsEnabled`, `IsVisible` |
| `UiText` | `Text`, `VerticalAlignment`, `HorizontalAlignment`, `Margins` |

Declared as `[XmlIgnore]` and therefore not configurable: `Screen.Type`, `Screen.ScreenArgs`, `MenuScreen.Items`, `MenuScreen.ItemNumber`, and on `UiElement` the transient and structural members `IsHovered`, `HasInputFocus`, `Children`, and `IsDisposed`.

## Working-directory contract

Two path constructions are relative:

```csharp
string levelPath = Path.Combine("Levels", level + ".lvl");          // World
XmlPath = Path.Combine("Screens", $"{Type.Name}.xml");              // Screen
```

There is no search path, no fallback location, no environment variable override, and no absolute default. The working directory is therefore part of the configuration contract, and it is satisfied by:

| Launch method | Working directory | Result |
|---------------|-------------------|--------|
| `dotnet run --project SokobanCLI` | The project directory | Works |
| `.vscode/launch.json` | `${workspaceFolder}/SokobanCLI` | Works |
| Running the built assembly from the workspace root | The workspace root | Fails: `Levels/` is under `SokobanCLI/`, not the root |
| Running the published artefact from its own directory | The artefact directory | Works, because the content directories were copied there |
| Running the published artefact from elsewhere | That directory | Fails with `FileNotFoundException` |

## Console geometry as configuration

`GraphicsManager.Resolution` is computed on every access:

```csharp
public Size2D Resolution => new(Console.WindowWidth, Console.WindowHeight);
```

It drives framebuffer allocation indirectly and is read on every frame by `AsciiSpriteBatch.Update`. Two behaviours follow:

- The framebuffer always targets the console size reported at the last `LoadContent`, not the size reported now, unless `Update` detected a change and reallocated.
- A change to the console geometry is the only input that alters the render target at run time, and it is handled asymmetrically: the render batch reallocates, the shadow batch does not. See [troubleshooting.md](./troubleshooting.md).

## Precedence

There is no configuration precedence because there is only one source per setting. Where a value could come from two places, the code chooses one explicitly:

| Value | Alternatives | Chosen |
|-------|--------------|--------|
| Screen colours | Constructor defaults, XML | XML wins when present, because deserialisation runs after construction; a missing file leaves the defaults |
| Level number | `ScreenArgs[0]`, literal 0 | `ScreenArgs[0]` when provided, otherwise 0 |
| World map child size | Literal 14 × 14 in `UiWorldmap.LoadContent`, `World.Size`, `GameScreen.xml`'s `Size` | Three independent literals that must agree; no precedence exists between them |
| Framebuffer size | Console geometry, `GameScreen.xml` element sizes | Console geometry, for the batch; XML only for elements |

## Reload behaviour

| Change | Applied |
|--------|---------|
| Level file edited | On the next `Start`: level load, `R`, or completion advancement |
| Screen XML edited | On the next transition to that screen; the current screen is unaffected |
| Screen XML edited for `TitleScreen` | On the next transition to the title screen; the initial load also reads it, so it is refreshed on launch |
| Project property changed | On rebuild |
| Terminal resized | On the next `AsciiSpriteBatch.Update`, with the shadow-batch mismatch described above |
| Console colours changed externally | Never corrected; the renderer sets colours per written cell, so the display recovers on the next changed cell but the console's own default colours remain altered |

There is no hot-reload mechanism, no file watcher, and no explicit reload command. `dotnet watch run` reloads the whole process on a source change, which also resets all state.

## Absent configuration

| Mechanism | Status |
|-----------|--------|
| Environment variables | None read |
| Command-line options | None; `Main()` takes no parameters |
| `.env` files | None |
| JSON, YAML, INI, or TOML settings | None |
| User profile or application-data directory | None read or written, despite the README's mention of saved settings |
| Secret store or credential provider | None needed; there are no secrets |
| Named profiles or environments | None |
| Network ports | None |
| Feature flags | None |

## Related documents

- [components/models-and-content.md](./components/models-and-content.md) — level format in full
- [components/screens.md](./components/screens.md) — screen XML in full
- [state-and-persistence.md](./state-and-persistence.md) — why no settings storage exists