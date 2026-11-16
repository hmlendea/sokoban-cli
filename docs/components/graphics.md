# Graphics

Location: `SokobanCLI/Graphics/` — `GraphicsManager.cs`, `AsciiSpriteBatch.cs`, `AsciiSprite.cs`, `HorizontalAlignment.cs`, `VerticalAlignment.cs`, the `I*` interfaces, and `Geometry/`

## Purpose

Turn an abstract text composition into console cells, and do so cheaply by writing only the cells that changed since the previous frame. This namespace is a leaf: it knows nothing about tiles, moves, screens, or input.

## GraphicsManager

`public sealed class GraphicsManager : IGraphicsManager` — singleton.

```csharp
public AsciiSpriteBatch SpriteBatch { get; set; }
public Size2D Resolution => new(Console.WindowWidth, Console.WindowHeight);

public void LoadContent();
public void UnloadContent();
public void Update(float gameTime) { }
public void Draw();
public void Clear(ConsoleColor colour);
```

| Member | Behaviour |
|--------|-----------|
| `Resolution` | Computed on every access from `Console.WindowWidth` and `Console.WindowHeight`; never cached |
| `SpriteBatch` | The render framebuffer, supplied by `Program`; settable, so nothing prevents it being replaced mid-frame |
| `drawnData` (private) | The shadow framebuffer recording what the console has already been told |
| `LoadContent` | Allocates `drawnData` and sizes it from the current resolution |
| `Update` | Empty |
| `Draw` | Diffs render against shadow, writes changed cells, then returns the cursor to (0, 0) |
| `Clear` | Sets the console colours, calls `Console.Clear()`, and resets both framebuffers |

### Draw — the diff loop

```csharp
public void Draw()
{
    for (int y = 0; y < SpriteBatch.Size.Height; y++)
    {
        for (int x = 0; x < SpriteBatch.Size.Width; x++)
        {
            if (drawnData.CharArray[x, y] == SpriteBatch.CharArray[x, y] &&
                drawnData.BackgroundColourArray[x, y] == SpriteBatch.BackgroundColourArray[x, y] &&
                drawnData.ForegroundColourArray[x, y] == SpriteBatch.ForegroundColourArray[x, y])
            {
                continue;
            }

            Console.SetCursorPosition(x, y);
            Console.BackgroundColor = SpriteBatch.BackgroundColourArray[x, y];
            Console.ForegroundColor = SpriteBatch.ForegroundColourArray[x, y];

            Console.Write(SpriteBatch.CharArray[x, y]);

            drawnData.CharArray[x, y] = SpriteBatch.CharArray[x, y];
            drawnData.BackgroundColourArray[x, y] = SpriteBatch.BackgroundColourArray[x, y];
            drawnData.ForegroundColourArray[x, y] = SpriteBatch.ForegroundColourArray[x, y];
        }
    }

    Console.SetCursorPosition(0, 0);
}
```

Key properties:

- A cell is written only when its character, background, or foreground differs. Colour changes alone therefore repaint, which is how the menu highlight works: only the glyph colour changes, but the cell is still rewritten.
- The loop iterates over `SpriteBatch.Size` but indexes `drawnData` by the same coordinates. **These two arrays must be the same size.** `AsciiSpriteBatch.Update` reallocates the render batch when the console resolution changes, while `drawnData` is only allocated in `LoadContent`. Growing the terminal therefore makes `Draw` index past the shadow arrays and throw `IndexOutOfRangeException`. Shrinking the terminal leaves `drawnData` larger, which `Draw` tolerates but `Clear` does not (see below).
- `Console.SetCursorPosition` is called before every written cell, so the write position is explicit and does not rely on the console's own cursor advancement.
- The cursor is parked at (0, 0) afterwards, which is invisible because `Program.Main` hid it.
- Only one character is written per cell, so `Console.Write(char)` uses the per-cell colours set immediately before. This is correct but means a colour change costs two property assignments and a cursor move per cell.

### Clear

```csharp
public void Clear(ConsoleColor colour)
{
    Console.BackgroundColor = colour;
    Console.ForegroundColor = colour;
    Console.Clear();

    for (int y = 0; y < drawnData.Size.Height; y++)
    {
        for (int x = 0; x < drawnData.Size.Width; x++)
        {
            drawnData.CharArray[x, y] = ' ';
            drawnData.BackgroundColourArray[x, y] = colour;
            drawnData.ForegroundColourArray[x, y] = colour;

            SpriteBatch.CharArray[x, y] = ' ';
            SpriteBatch.BackgroundColourArray[x, y] = ConsoleColor.Black;
            SpriteBatch.ForegroundColourArray[x, y] = ConsoleColor.White;
        }
    }
}
```

Three details deserve recording:

1. Both foreground and background are set to `colour` before `Console.Clear()`, so the cleared console has no visible text.
2. The shadow batch records the cleared region as `colour` in both foreground and background — matching what the console now shows.
3. The render batch is *reset to a different state*: background `Black`, foreground `White`, character space. The next `Draw` will consequently find almost every cell different (at minimum the foreground colour) and rewrite the visible area. This is deliberate: it guarantees a full repaint after a transition without the caller tracking which cells were previously non-default.
4. The loop iterates over `drawnData.Size` while indexing `SpriteBatch`. After a terminal shrink, `drawnData` is larger than `SpriteBatch`, so this throws `IndexOutOfRangeException`. `Clear` is called from `Program.Main` and from `ScreenManager.ChangeScreens`, so a shrink followed by a screen transition crashes.

## AsciiSpriteBatch

`public sealed class AsciiSpriteBatch : IAsciiSpriteBatch` — the render framebuffer.

| Member | Type | Meaning |
|--------|------|---------|
| `Size` | `Size2D` | Console dimensions captured at load time |
| `CharArray` | `char[,]` | Glyphs, indexed `[x, y]` |
| `BackgroundColourArray` | `ConsoleColor[,]` | Per-cell background |
| `ForegroundColourArray` | `ConsoleColor[,]` | Per-cell foreground |
| `ScreenPreview` | `string` (computed) | The buffer as text, one line per row, newline-terminated |

`ScreenPreview` is the only diagnostic surface in the application. Its summary comment marks it `DEBUG`. Nothing in production calls it.

### LoadContent and Update

```csharp
public void LoadContent()
{
    Size = GraphicsManager.Instance.Resolution;

    CharArray = new char[Size.Width, Size.Height];
    BackgroundColourArray = new ConsoleColor[Size.Width, Size.Height];
    ForegroundColourArray = new ConsoleColor[Size.Width, Size.Height];

    for (int y = 0; y < Size.Height; y++)
    {
        for (int x = 0; x < Size.Width; x++)
        {
            CharArray[x, y] = ' ';
            BackgroundColourArray[x, y] = ConsoleColor.Black;
            ForegroundColourArray[x, y] = ConsoleColor.White;
        }
    }
}

public void Update()
{
    if (GraphicsManager.Instance.Resolution == Size)
    {
        return;
    }

    LoadContent();
}
```

`Update` is a resolution watcher. It relies on `Size2D` having value equality (`==` is implemented via `IEquatable<Size2D>`), so comparing resolutions is exact.

### Draw(AsciiSprite) — sprite compositing

```csharp
public void Draw(AsciiSprite sprite)
{
    string[] lines = sprite.Text.Split('\n');
    int firstRow = 0;

    switch (sprite.VerticalAlignment)
    {
        case VerticalAlignment.Centre:
            firstRow = (sprite.Size.Height - lines.Length) / 2;
            break;

        case VerticalAlignment.Bottom:
            firstRow = sprite.Size.Height - lines.Length;
            break;
    }

    int lastRow = firstRow + lines.Length;

    for (int y = firstRow; y < lastRow; y++)
    {
        int row = y - firstRow;
        int firstCol = 0;

        switch (sprite.HorizontalAlignment)
        {
            case HorizontalAlignment.Centre:
                firstCol = (sprite.Size.Width - lines[row].Length) / 2;
                break;

            case HorizontalAlignment.Right:
                firstCol = sprite.Size.Width - lines[row].Length;
                break;
        }

        int lastCol = firstCol + lines[row].Length;

        for (int x = firstCol; x < lastCol; x++)
        {
            int col = x - firstCol;
            int destX = sprite.Location.X + x;
            int destY = sprite.Location.Y + y;

            if (destX < 0 || destX >= Size.Width || destY < 0 || destY >= Size.Height)
            {
                continue;
            }

            CharArray[destX, destY] = lines[row][col];
            BackgroundColourArray[destX, destY] = sprite.BackgroundColour;
            ForegroundColourArray[destX, destY] = sprite.ForegroundColour;
        }
    }
}
```

Behaviour and edge cases:

- Alignment is resolved against `sprite.Size`, not against the batch: the sprite's `Size` is the box the text is aligned inside, and `Location` positions that box in the batch.
- Text is split on `'\n'` only, so `\r\n` line endings would leave a carriage return in each line's character data. All text produced in this repository uses `Environment.NewLine` assembled from `'\n'`-terminated appends, so on Linux this is exact; on Windows `Environment.NewLine` is `"\r\n"` and the `'\r'` would be composited as a character. See [ambiguities-and-open-questions.md](../ambiguities-and-open-questions.md).
- Clipping is per cell: a sprite partially outside the batch writes only its in-bounds cells. No exception is raised.
- Where two sprites overlap, the later draw wins cell by cell. `UiManager.Draw` iterates `UiElements` in list order, so list order is z-order.
- The origin `(firstCol, firstRow)` of each row is `Location` + the alignment offset, so a `Centre` sprite is centred inside its declared `Size` rather than inside the batch.

## AsciiSprite

`public sealed class AsciiSprite : IAsciiSprite` — one text block prepared for a single draw.

| Property | Purpose |
|----------|---------|
| `Location` | Top-left of the sprite's box in batch coordinates |
| `Size` | The box the text is aligned inside |
| `Text` | The text to composite; newline-separated |
| `HorizontalAlignment`, `VerticalAlignment` | Alignment inside `Size` |
| `BackgroundColour`, `ForegroundColour` | Applied to every written cell |

`LoadContent` and `UnloadContent` both simply set `Text = string.Empty`. `Update` is empty. `Draw(AsciiSpriteBatch)` forwards to `spriteBatch.Draw(this)`, making the sprite a thin parameter object with a convenience overload. Every `UiText` owns one `AsciiSprite` and refreshes its properties from the element before drawing.

## Alignment enumerations

```csharp
public enum HorizontalAlignment { Left, Centre, Right }
public enum VerticalAlignment { Top, Centre, Bottom }
```

Note the British spelling `Centre`, which the `english-language` conventions require and which appears in `default` positions of a `switch` as well as in element defaults (`UiText` defaults to `Centre`/`Centre`). Any code ported from an American-spelled source must be corrected.

## Geometry

`SokobanCLI.Graphics.Geometry` is the shared kernel consumed by `Models`, `GameLogic`, `UI`, and `Graphics` alike. All four types are `public struct` implementations of `IEquatable<T>` with value equality, `==`/`!=` operators, `GetHashCode`, and `Equals(object)`.

| Type | Fields | Notable members |
|------|--------|-----------------|
| `Point2D` | `X`, `Y` | `IsEmpty`, `Empty`, constructors from `(int, int)` and `Size2D`, `+`, `-` |
| `Size2D` | `Width`, `Height` | `IsEmpty`, `Empty`, constructors from `(int, int)` and `Point2D`, `+`, `-` |
| `Rectangle2D` | `X`, `Y`, `Width`, `Height` | `Location`/`Size` projections, `Bottom`, `Left`, `Right`, `Top`, `IsEmpty`, `Empty`, `Contains(Point2D)`, `Contains(Rectangle2D)`, `Contains(int, int)` |
| `Scale2D` | `Horizontal`, `Vertical` | `IsEmpty`, `Empty`, `One`, constructors from `(float, float)`, `Point2D`, `Size2D`, `+`, `-` |

Points to note:

- The types are mutable structs with public setters. Passing one by value copies it, but a caller holding a reference to a field of one can mutate it in place; `Rectangle2D.Location` and `Size` setters write through to the underlying fields.
- `Rectangle2D.Contains(int, int)` is inclusive on all four edges (`Left <= x && Right >= x && …`), so a rectangle spanning `x` from 0 to 2 contains `x == 2`. This is inconsistent with `World.Contains`, which is half-open. `Rectangle2D.Contains` is used in production only through `ClientRectangle`-based layout arithmetic in `GameScreen.SetChildrenProperties` — no production code calls `Rectangle2D.Contains`; only tests do.
- `Rectangle2D.Bottom` is `Y + Height`, so it is one past the last row of the rectangle when used as a bound. `GameScreen.SetChildrenProperties` relies on this when stacking the details panel under the world map: `Worldmap.ClientRectangle.Bottom + 1`.
- `Scale2D` has no production consumers at all; only unit tests use it.
- `Point2D` → `Size2D` and `Size2D` → `Point2D` conversions exist as constructors, which is why `Point2D` can be built from a size and vice versa.

## Mapping extensions

`SokobanCLI.Graphics.Geometry.Mapping` contains three static classes of extension methods converting to and from `System.Drawing` primitives:

| File | Methods |
|------|---------|
| `PointMappingExtensions.cs` | `ToPoint2D(this System.Drawing.Point)`, `ToSystemPoint(this Point2D)` |
| `SizeMappingExtensions.cs` | `ToSize2D(this System.Drawing.Size)`, `ToSystemPoint(this Size2D)` |
| `RectangleMappingExtensions.cs` | `ToRectangle2D(this System.Drawing.Rectangle)`, `ToSystemRectangle(this Rectangle2D)` |

The section comments `// >>> TO NARIVIA` and `// >>> TO SYSTEM` are leftovers from the project these types were extracted from. Two defects are worth recording:

1. `SizeMappingExtensions` names its reverse conversion `ToSystemPoint` while returning `System.Drawing.Size`. Callers expecting a `System.Drawing.Point` would compile against the wrong overload set only if a conflicting extension existed; none does here, so the defect is a naming error rather than a bug.
2. No production code calls any of these methods. They exist for interoperability, and their absence of consumers means a `using System.Drawing` reference is never needed by the application.

`TypeDeclarationConventionTests` and `SpecialisedTypeConventionTests` enforce their placement: mapping methods must live in dedicated `*.Mapping` namespaces, in files named `*MappingExtensions.cs`, and must be static extension methods only.

## Related documents

- [flows/startup-and-rendering.md](../flows/startup-and-rendering.md) — how this fits the per-frame pipeline
- [state-and-persistence.md](../state-and-persistence.md) — the two framebuffers as application state
- [troubleshooting.md](../troubleshooting.md) — the resize failure modes described above