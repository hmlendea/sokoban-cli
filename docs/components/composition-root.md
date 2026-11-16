# Composition root

Location: `SokobanCLI/Program.cs`

Type: `SokobanCLI.Program` — `public static class`, exactly one method besides `Main`: `LoadContent`, `Update`, `Draw`, all private static.

## Purpose

`Program` is the process entry point and the only type permitted to drive the application. It owns three things and nothing else:

1. The one-time startup sequence and its ordering constraints.
2. The unbounded update/draw loop and the `gameTime` counter.
3. The bridge that repaints a screen whenever a key is pressed.

## Contract

```csharp
public static class Program
{
    private static AsciiSpriteBatch spriteBatch;
    private static IGameManager game;
    private static float gameTime;

    private static void Main();
    private static void LoadContent();
    private static void Update();
    private static void Draw();
}
```

`Main` is `private static void Main()` with no parameters and no return value. The process therefore cannot be parameterised, and the method has no exit path.

## Execution

`Main` in full:

```csharp
private static void Main()
{
    Console.CursorVisible = false;

    game = new GameManager();

    LoadContent();
    GraphicsManager.Instance.Clear(ConsoleColor.Black);

    while (true)
    {
        Update();
        Draw();
    }
}
```

| Step | Effect | Why it matters |
|------|--------|----------------|
| `Console.CursorVisible = false` | Hides the cursor for the whole session | The renderer moves the cursor on every changed cell, so a visible cursor would flicker across the display |
| `game = new GameManager()` | Assigns a `GameManager` to a static field | The field is never read: `GameScreen` creates its own `GameManager`. This instance is inert |
| `LoadContent()` | Performs the whole wiring sequence below | Startup failures surface here |
| `GraphicsManager.Instance.Clear(ConsoleColor.Black)` | Blanks the console and resets both framebuffers | Establishes the shadow framebuffer in a known state so the first frame only writes real content |
| `while (true)` | Never-terminating loop | No exit condition, no cancellation token, no frame pacing |

Startup ordering inside `LoadContent`:

| Order | Statement | Constraint it satisfies |
|-------|-----------|-------------------------|
| 1 | `spriteBatch = new AsciiSpriteBatch()` | None |
| 2 | `spriteBatch.LoadContent()` | Sizes the render arrays from `GraphicsManager.Instance.Resolution`, which reads `Console.WindowWidth`/`WindowHeight` |
| 3 | `GraphicsManager.Instance.LoadContent()` | Allocates the shadow `drawnData` batch from the same resolution |
| 4 | `GraphicsManager.Instance.SpriteBatch = spriteBatch` | The graphics manager needs the batch to diff and flush |
| 5 | `ScreenManager.Instance.LoadContent()` | Constructs the singleton, which constructs a `TitleScreen` and deserialises `Screens/TitleScreen.xml`; then sizes the screen and loads its elements |
| 6 | `InputManager.Instance.KeyboardKeyPressed += …` | Subscribes the redraw bridge after the screens exist, so the first key press has something to draw |

Swapping steps 4 and 5 would leave `GraphicsManager.SpriteBatch` null while the title screen loads elements that size themselves from `GraphicsManager.Instance.Resolution`… resolution does not need the batch, so the practical effect is only that `GraphicsManager.Instance.SpriteBatch` would be null during `Screen`/element load. Swapping steps 2 and 3 would make the render batch size from a resolution read before `drawnData` existed, which is harmless but leaves the two batches potentially inconsistent until the next resolution change.

`Update`:

```csharp
private static void Update()
{
    spriteBatch.Update();

    InputManager.Instance.Update();
    ScreenManager.Instance.Update(gameTime);
    GraphicsManager.Instance.Update(gameTime);

    gameTime += 1;
}
```

Ordering here is behavioural:

1. `spriteBatch.Update()` reallocates the framebuffer if the console resolution changed. This must precede input and screen updates because a screen update can move elements and the reallocation resets the buffer.
2. `InputManager.Instance.Update()` polls and, if a key is available, raises `KeyboardKeyPressed` synchronously. The key handler may call `ScreenManager.Instance.Draw` or `ChangeScreens` — and `ChangeScreens` invokes `GraphicsManager.Instance.Clear`, which rewrites both framebuffers mid-update.
3. `ScreenManager.Instance.Update(gameTime)` updates the current screen, which updates `UiManager`, which updates every enabled element.
4. `GraphicsManager.Instance.Update(gameTime)` is an empty method.
5. `gameTime` increments by one per frame. Because the loop is not paced, `gameTime` measures frames, not elapsed time; no production code consumes it beyond passing it down.

`Draw`:

```csharp
private static void Draw()
{
    ScreenManager.Instance.Draw(spriteBatch);
    GraphicsManager.Instance.Draw();
}
```

The current screen composes into the render batch, then the graphics manager flushes changed cells. Note this happens every iteration of an unpaced loop, so with no input the process composes and diffs the full framebuffer as fast as the console permits while writing nothing.

## The input-to-draw bridge

```csharp
InputManager.Instance.KeyboardKeyPressed += (sender, e) => ScreenManager.Instance.Draw(spriteBatch);
```

This single lambda is the whole reason a key press appears on screen quickly rather than at the next frame's `Draw`. It is also the reason the architecture test asserting that nothing depends on `Program` is satisfied: the subscriber is a closure inside `Program`, not another type.

Two consequences:

- The lambda captures the static `spriteBatch` field, so it always draws into the current batch even after a resolution change.
- Subscriber order decides who runs first for a given key. The bridge is subscribed inside `LoadContent`, i.e. after the title screen registered its own handler, so for the title screen the screen handler precedes the bridge. The game screen is created later by `ChangeScreens` and appends its handler after the bridge, so for the game screen the bridge runs *first* and paints the pre-move frame, after which `PlayerMoved` triggers a second draw that reflects the move. The net result is correct with one redundant diff pass per movement key.

## What this class depends on

`GameManager`, `AsciiSpriteBatch`, `GraphicsManager`, `ScreenManager`, `InputManager`, and `IGameManager` (the field type). It reaches all four managers through their static `Instance` properties, so no dependency is expressed in a constructor.

## What depends on this class

Nothing. `ProductionArchitectureTests` asserts:

```csharp
private static readonly IObjectProvider<IType> CompositionRootTypes = Types()
    .That()
    .HaveFullName("SokobanCLI.Program");
```

and requires exactly one such type with no inbound dependencies.

## Known weaknesses

| Weakness | Evidence | Consequence |
|----------|----------|-------------|
| No exit path | `while (true)` with no `break` and no return after the loop | The process only ends by external interruption |
| No frame pacing | The loop has no sleep, timer, or throttle | Processor usage scales with terminal performance |
| Inert `game` field | Assigned in `Main`, never read | Dead state that misleads a reader into thinking `Program` drives the game |
| No exception handling | No `try` in the file | Any startup failure (missing level, malformed XML) terminates the process with a stack trace |
| Hidden startup order | Ordering constraints exist only as statement order | Reordering the statements compiles and fails at runtime |

## Related documents

- [flows/startup-and-rendering.md](../flows/startup-and-rendering.md) — the same sequence with frame timing and failure points
- [concurrency-and-scheduling.md](../concurrency-and-scheduling.md) — why the loop is unpaced and single-threaded
- [error-handling.md](../error-handling.md) — how startup failures terminate the process