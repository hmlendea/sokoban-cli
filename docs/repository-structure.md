# Repository structure

## Top level

```
sokoban-cli/
├── SokobanCLI.slnx                 # SDK-style solution, three projects, main project first
├── SokobanCLI/                     # production executable project
├── SokobanCLI.ArchitectureTests/   # structure and convention enforcement
├── SokobanCLI.UnitTests/           # behavioural unit tests
├── .github/workflows/              # dotnet.yml, security.yml, github-release.yml
├── .vscode/                        # build/publish/watch tasks, launch configuration
├── sokobancli.ico                  # application icon shipped with the project
├── README.md ARCHITECTURE.md CONTRIBUTING.md CODE_OF_CONDUCT.md
│   SECURITY.md PRIVACY.md LICENSE
└── .gitignore                      # Visual Studio template ignore set
```

There is no `src/` or `tests/` nesting. `SolutionConventionTests` asserts that every project file lives in a sibling folder named after the project and that no project path nests inside a `src`-style directory.

## Production project

```
SokobanCLI/
├── SokobanCLI.csproj               # net10.0, Exe, one package (NuciDAL 3.1.0)
├── Program.cs                      # composition root and frame loop
├── GameLogic/
│   ├── Managers/GameManager.cs, IGameManager.cs
│   └── Events/PlayerMovementEventArgs.cs, PlayerMovementEventHandler.cs
├── Models/
│   ├── World.cs                    # level loading, tile grid, player position, moves
│   ├── Tile.cs                     # tile type, identifier, glyph, colour
│   ├── Tiles.cs                    # stateless factory
│   └── TileType.cs                 # 0..6 identifier enumeration
├── Graphics/
│   ├── GraphicsManager.cs          # console writer, shadow framebuffer
│   ├── AsciiSpriteBatch.cs         # render framebuffer, sprite compositing
│   ├── AsciiSprite.cs              # positioned, aligned text block
│   ├── HorizontalAlignment.cs, VerticalAlignment.cs
│   ├── I*.cs                       # interfaces for each graphics type
│   └── Geometry/
│       ├── Point2D.cs, Size2D.cs, Rectangle2D.cs, Scale2D.cs
│       └── Mapping/*MappingExtensions.cs   # System.Drawing interop
├── Input/
│   ├── InputManager.cs, IInputManager.cs
│   └── Events/ConsoleKeyEventArgs.cs, ConsoleKeyEventHandler.cs
├── Screens/                        # content, copied to output
│   ├── TitleScreen.xml             # Axis, Link "New Game" -> GameScreen, Action "Exit"
│   ├── GameScreen.xml              # world map, details, controls elements
│   └── ScreenManager.xml           # empty root element
├── Levels/                         # content, copied to output
│   └── 0.lvl … 20.lvl              # 14 lines × 14 digits each
└── UI/
    ├── ScreenManager.cs, IScreenManager.cs
    ├── UiManager.cs, IUiManager.cs
    ├── Screens/                    # Screen, MenuScreen, TitleScreen, GameScreen + interfaces
    └── UiElements/                 # UiElement, UiText, UiMenuItem, UiMenuLink, UiMenuAction, UiWorldmap + interfaces
```

Namespace and file organisation are enforced: one type per file, file name equal to type name, one block-scoped namespace matching the folder path, usings outside the namespace, and no file-scoped namespaces (`SourceFileConventionTests`, `NamespaceConventionTests`).

## Test projects

```
SokobanCLI.UnitTests/
├── SokobanCLI.UnitTests.csproj
├── GameLogic/Managers/GameManagerTests.cs, IPlayerMovedSpy.cs
├── GameLogic/Events/PlayerMovementEventArgsTests.cs
├── Input/Events/ConsoleKeyEventArgsTests.cs
├── Models/TilesTests.cs
└── Graphics/Geometry/{Point2D,Size2D,Rectangle2D,Scale2D}Tests.cs
    Graphics/Geometry/Mapping/*MappingExtensionsTests.cs
```

The unit test project links `../SokobanCLI/Levels/*.lvl` into its own output so `World(0)` can load level 0 from the test working directory.

```
SokobanCLI.ArchitectureTests/
├── SokobanCLI.ArchitectureTests.csproj   # ArchUnitNET + Roslyn; generates ArchitecturePaths.g.cs
├── ProductionArchitectureTests.cs        # layer dependency matrix
├── ProjectReferenceTests.cs              # project/assembly reference graph
├── ProjectConventionTests.cs, SolutionConventionTests.cs
├── SourceFileConventionTests.cs, NamespaceConventionTests.cs
├── NamingConventionTests.cs, CodingConventionTests.cs, LayoutConventionTests.cs
├── MemberShapeConventionTests.cs, CollectionConventionTests.cs
├── ErrorHandlingConventionTests.cs, TypeDeclarationConventionTests.cs
├── SpecialisedTypeConventionTests.cs, ConfigurationConventionTests.cs
├── TestSourceConventionTests.cs
├── RuleApplicabilityTests.cs             # records rules that are currently inapplicable
├── ProductionCompilation.cs              # in-memory Roslyn compilation of production sources
└── ProductionSources.cs                  # file enumeration and assertion helpers
```

`ProductionSources` derives the repository root from the generated `ArchitecturePaths.ProductionSourceRoot` MSBuild property, so the tests locate production sources without depending on the current working directory.

## Content directories

Both are declared in `SokobanCLI.csproj` as `None` items with `CopyToOutputDirectory=PreserveNewest`, so they exist beside the built assembly. At runtime `World` resolves `Levels/<n>.lvl` and `Screen` resolves `Screens/<TypeName>.xml` through relative paths, meaning the process working directory must be the output directory (or a directory containing those folders). The `.vscode/launch.json` configuration sets `cwd` to `${workspaceFolder}/SokobanCLI` for that reason.