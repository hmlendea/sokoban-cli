# Models and content

Location: `SokobanCLI/Models/World.cs`, `Tile.cs`, `Tiles.cs`, `TileType.cs`; content in `SokobanCLI/Levels/`

## Purpose

Hold the authoritative board state and translate a digit-encoded text file into it. Nothing outside this namespace writes level files, and nothing inside it knows about input, screens, or the console writer.

## World

`public sealed class World` in `SokobanCLI/Models/World.cs`.

```csharp
private static readonly int DefaultSize = 14;

public readonly Tile[,] Tiles;
public Point2D PlayerPosition { get; set; }
public Size2D Size { get; private set; }
public int Moves { get; set; }
public int Level { get; private set; }

public World(int level);
public Tile GetTile(int x, int y) => Tiles[x, y];
public bool Contains(Point2D position);
```

| Member | Type | Mutability | Who writes it |
|--------|------|------------|----------------|
| `Tiles` | `Tile[,]` public readonly field | The array reference is fixed; its elements and cells are freely reassigned | `World.LoadTiles`, `GameManager.TryMoveBox`, unit tests |
| `PlayerPosition` | settable property | Mutable | `World.LoadTiles`, `GameManager.MovePlayer` |
| `Size` | `Size2D`, private setter | Fixed 14 × 14 | `World` constructor only |
| `Moves` | settable property | Mutable | `GameManager.MovePlayer` increments; `World` constructor defaults to 0 |
| `Level` | private setter | Fixed at construction | `World` constructor only |

`Tiles` is a public mutable field rather than a property because the convention rules prefer `IEnumerable` for collection members while still allowing the game to write cells; `CollectionConventionTests` records the collection-shape decision. `GameManager` writes `World.Tiles[x, y]` directly, and unit tests arrange fixtures the same way.

### Construction

```csharp
public World(int level)
{
    string levelPath = Path.Combine("Levels", level + ".lvl");
    string[] rows = File.ReadAllLines(levelPath);

    ValidateRows(rows, levelPath);
    Size = new Size2D(DefaultSize, DefaultSize);
    Tiles = new Tile[Size.Width, Size.Height];
    Level = level;

    LoadTiles(rows);
}
```

Points to note:

1. The path is relative (`Levels/<level>.lvl`), so the process working directory must contain `Levels/`. The `.vscode/launch.json` sets `cwd` accordingly, and the unit test project links the level files into its output directory.
2. `File.ReadAllLines` throws `FileNotFoundException` for a missing level — this is the exception that terminates the process when completion advances past level 20.
3. Validation happens before allocation, so a malformed file never produces a partially built world.
4. `ValidateRows` is called with the path so error messages can name the offending file.

### LoadTiles and the transposed layout

```csharp
private void LoadTiles(string[] rows)
{
    for (int y = 0; y < Size.Height; y += 1)
    {
        for (int x = 0; x < Size.Width; x += 1)
        {
            int id = rows[x][y] - '0';
            Tiles[x, y] = Models.Tiles.ById(id);

            if (id == 4)
            {
                PlayerPosition = new Point2D(x, y);
                Tiles[x, y] = Models.Tiles.ById(0);
            }
            else if (id == 6)
            {
                PlayerPosition = new Point2D(x, y);
                Tiles[x, y] = Models.Tiles.ById(3);
            }
        }
    }
}
```

The index expression is `rows[x][y]`: line `x` of the file supplies grid column `x`, and the character position `y` within that line supplies grid row `y`. The file is therefore transposed relative to the grid the renderer draws. Practical consequences:

- Editing a level means writing columns as lines. `0.lvl` line 5 (`00001111110000`) is column 5 of the grid, drawn vertically.
- `UiWorldmap.GetWallShape` compensates for the transposition only in its naming (its `n` test reads `Tiles[x - 1, y]`), not in the glyph selection, so the box-drawing glyphs appear rotated 90° relative to a designer's mental image.
- Any tooling that reads or writes level files must apply the same transposition.

Identifier 4 (player on floor) and 6 (player on goal) are converted at load time: the player becomes a *position*, and the cell becomes `0` or `3` respectively. Consequently the grid never contains 4 or 6 during play, and walking over a goal leaves the goal in place.

### Validation

```csharp
private static void ValidateRows(string[] rows, string levelPath)
{
    if (rows.Length != DefaultSize)
    {
        throw new InvalidDataException($"Level '{levelPath}' must contain {DefaultSize} rows.");
    }

    foreach (string row in rows)
    {
        if (row.Length != DefaultSize)
        {
            throw new InvalidDataException($"Level '{levelPath}' rows must contain {DefaultSize} tiles.");
        }
    }
}
```

Validated: the file has exactly 14 lines and every line has exactly 14 characters.

Not validated: that characters are digits. The README's tile legend, the code comments, and the architecture document all reflect the assumed encoding, but a stray letter would produce `'a' - '0' == 49`, which `Tiles.ById` resolves to an empty tile because no enumeration value matches 49. A non-digit therefore becomes silent empty floor rather than an error.

Not validated: that the level contains at least one box or goal, that the count of boxes equals the count of goals, or that the level is solvable.

### Contains

```csharp
public bool Contains(Point2D position)
    => position.X >= 0 && position.X < Size.Width &&
        position.Y >= 0 && position.Y < Size.Height;
```

Half-open bounds in both axes. `GameManager` uses it for both the player target and the box destination, so an out-of-bounds push is rejected before any grid access.

## Tile

`public sealed class Tile` — a mutable data holder, used as a value by the game rules but as a class by the type system.

| Property | Type | Meaning |
|----------|------|---------|
| `Type` | `TileType` | Semantic classification |
| `Id` | `int` | The numeric encoding, kept even for identifiers with no enumeration value |
| `Character` | `char` | The glyph the renderer uses |
| `Colour` | `ConsoleColor` | The colour the model intends |
| `IsWalkable` | `bool` (computed) | `Type is TileType.Empty or TileType.Goal` |

Two facts matter for modification:

- `IsWalkable` is not used anywhere in production. `GameManager` performs the equivalent test against `Id` instead. Changing either without the other introduces divergence that no test would catch, because no test asserts `IsWalkable`.
- `Colour` is not used by the renderer. `UiWorldmap` renders every tile with the element's single foreground colour, and the assignment lines that would have applied `Tile.Colour` are commented out. `TilesTests` does assert the colours returned by the factory, so the data is verified even though it is not displayed.

## Tiles factory

`public static class Tiles` — stateless.

```csharp
public static Tile OfType(TileType type) => CreateTile(type, (int)type);
public static Tile ById(int id) => CreateTile(GetTypeForId(id), id);
```

| Method | Behaviour |
|--------|-----------|
| `OfType(TileType)` | Creates a tile whose identifier equals the enumeration value |
| `ById(int)` | Looks up a matching enumeration value by linear scan through `Enum.GetValues<TileType>()`; returns a tile with `Type = Empty` when none matches, while preserving the requested identifier |
| `CreateTile` | Builds a new `Tile` with the glyph from `GetCharacter` and the colour from `GetColour` |

`GetCharacter`: `█` for wall, `O` for box, `+` for goal, `@` for box on goal, space for everything else.

`GetColour`: `Gray` for wall, `DarkYellow` for box, `DarkRed` for goal, `DarkGreen` for box on goal, `Black` for everything else.

A new `Tile` instance is allocated on every call. Because the game replaces grid cells with factory results rather than mutating tiles in place, this is safe; the factory does not cache or share instances despite the architecture document's phrasing about statelessness — "stateless" refers to the factory having no fields, not to tile instance reuse. `Tiles.ById(99)` returns a tile with `Id == 99`, `Character == ' '` and `Colour == Black`, which `TilesTests` asserts explicitly.

Note the two-level naming: `Tile` (the instance) and `Tiles` (the factory), plus `TileType` (the enumeration). `World.LoadTiles` writes `Models.Tiles.ById(id)` with an explicit namespace qualification because the containing namespace is already `SokobanCLI.Models` — a redundancy that `NamespaceConventionTests` tolerates because it is not a fully qualified inline type reference.

## TileType

```csharp
public enum TileType
{
    Empty = 0,
    Wall = 1,
    Box = 2,
    Goal = 3,
    Player = 4,
    BoxOnGoal = 5,
    PlayerOnGoal = 6
}
```

Declared outside a dedicated enumeration namespace, which `SpecialisedTypeConventionTests` permits (it asserts enumerations do *not* live in dedicated enumeration namespaces). Values are unique and match the level-file encoding exactly, so `Tiles.OfType(TileType.Box).Id == 2`.

## Level content

Twenty-one files, `0.lvl` through `20.lvl`. Every file is verified to contain 14 lines of 14 characters. Levels are read by index, with no manifest: adding level 21 requires only the file, and the naming convention `<n>.lvl` is what makes advancement work.

`0.lvl`:

```
00000000000000
00000000000000
00000000000000
00000000000000
00000000000000
00001111110000
00001020310000
00001402310000
00001111110000
00000000000000
00000000000000
00000000000000
00000000000000
00000000000000
```

Read as columns, this places a single room in the middle of the grid. `GameManagerTests` derives the coordinates from this file and records them in a comment: player start `(7, 5)`, boxes `(6, 6)` and `(7, 7)`, goals `(6, 8)` and `(7, 8)`, with walls surrounding the room in rows 5 and 8 and columns 4 and 9. Worked through with the transposed indexing:

| File element | Grid cell | Content |
|--------------|-----------|---------|
| `0.lvl` line 5, character 4 | `(5, 4)` | Wall — top edge of the room |
| `0.lvl` line 7, character 5 | `(7, 5)` | Identifier 4, so the player starts here and the cell becomes empty |
| `0.lvl` line 6, character 6 | `(6, 6)` | Identifier 2, a box |
| `0.lvl` line 7, character 7 | `(7, 7)` | Identifier 2, a box |
| `0.lvl` line 6, character 8 | `(6, 8)` | Identifier 3, a goal |
| `0.lvl` line 7, character 8 | `(7, 8)` | Identifier 3, a goal |

The level therefore has two boxes and two goals, and the first move in `GameManagerTests` (`MovePlayer(0, 1)`, which increases the Y coordinate) steps from `(7, 5)` to `(7, 6)` — the empty cell between the player and the box at `(7, 7)`.

The largest level in the set, `20.lvl`, is a symmetric box room, showing that the encoding supports arbitrary arrangements within the 14 × 14 frame.

## Related documents

- [data-model.md](../data-model.md) — field-level reference and ownership
- [invariants.md](../invariants.md) — the encoding and layout conditions that must hold
- [game-logic.md](./game-logic.md) — how these types are mutated during play