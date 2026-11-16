# Input

Location: `SokobanCLI/Input/InputManager.cs`, `IInputManager.cs`, `Events/ConsoleKeyEventArgs.cs`, `Events/ConsoleKeyEventHandler.cs`

## Purpose

Poll the console once per frame and publish a key event. Deliberately transport-only: it does not interpret keys, map them to directions, or know which screen is active.

## InputManager

`public sealed class InputManager : IInputManager` — singleton.

```csharp
public event ConsoleKeyEventHandler KeyboardKeyPressed;

public void Update()
{
    if (!Console.KeyAvailable)
    {
        return;
    }

    ExposePressedKey();
}

private void ExposePressedKey()
{
    ConsoleKey key = Console.ReadKey(true).Key;

    KeyboardKeyPressed?.Invoke(this, new ConsoleKeyEventArgs(key));
}
```

Behaviour:

- `Update` is called once per frame by `Program.Update`. Non-blocking: when no key is buffered it returns immediately, so the frame loop never waits for input.
- At most one key is read per frame, even when several are buffered. A fast typist's keys are consumed one per iteration, which for an unpaced loop is imperceptible.
- `Console.ReadKey(true)` suppresses echoing, so the key does not appear on the display.
- The event is raised synchronously on the calling thread, so every subscriber runs inside `InputManager.Update` before the rest of the frame proceeds. `ScreenManager.ChangeScreens` can therefore run in the middle of `Program.Update`, including its `Console.Clear()`.

There is no key-release concept and no modifier state beyond what `ConsoleKeyInfo` would carry — the code discards everything except `ConsoleKeyInfo.Key`.

## Singleton pattern

```csharp
private static readonly Lock SyncRoot = new();
private static volatile InputManager instance;

public static InputManager Instance
{
    get
    {
        if (instance is null)
        {
            lock (SyncRoot)
            {
                instance ??= new InputManager();
            }
        }

        return instance;
    }
}
```

Identical in shape to `GraphicsManager`, `ScreenManager`, and `UiManager`, with the field naming differing (`SyncRoot` here and in `ScreenManager`; `syncRoot` in `GraphicsManager` and `UiManager`). Construction subscribes nothing and allocates nothing, so reaching `Instance` early is harmless.

## Event contracts

```csharp
public delegate void ConsoleKeyEventHandler(object sender, ConsoleKeyEventArgs e);

public sealed class ConsoleKeyEventArgs(ConsoleKey key)
{
    public ConsoleKey Key { get; private set; } = key;
}
```

The event arguments carry the raw `ConsoleKey`. Interpretation lives in the consumers:

| Consumer | Interpretation |
|----------|----------------|
| `GameScreen.OnKeyPressed` | `W`/`UpArrow` → `MovePlayer(-1, 0)`; `A`/`LeftArrow` → `MovePlayer(0, -1)`; `S`/`DownArrow` → `MovePlayer(1, 0)`; `D`/`RightArrow` → `MovePlayer(0, 1)`; `R` → restart; `Escape` → title screen |
| `MenuScreen.OnKeyPressed` | Axis-dependent navigation as documented in [behaviour/navigate-screens.md](../behaviour/navigate-screens.md) |
| `UiMenuItem.OnKeyPressed` | `Enter` or `E` → `Activated` |
| `Screen.OnKeyPressed` | Forwards to the `KeyPressed` event; no key-specific logic |
| `UiElement.OnInputManagerKeyboardKeyPressed` | Filters on enabled/visible/`CanRaiseEvents`/focus, then raises `KeyPressed` |

Because `UiElement` subscribes to `KeyboardKeyPressed` for every element instance and `Screen` subscribes for the current screen, adding elements increases the subscriber count. Unsubscription happens in `UnloadContent`; an element that is added to `UiManager.UiElements` but is never disposed keeps its subscription for the process lifetime. `ScreenManager.ChangeScreens` calls the outgoing screen's `UnloadContent`, which unloads and clears the list, so in the normal transition path subscriptions do not accumulate.

## Dependencies

`Input` depends on `System` and on its own `Events` namespace, and on nothing else in the application. `ProductionArchitectureTests` asserts:

```csharp
IArchRule rule = Types()
    .That()
    .Are(InputTypes)
    .Should()
    .NotDependOnAny(ModelTypes)
    .AndShould()
    .NotDependOnAny(GameLogicTypes)
    .AndShould()
    .NotDependOnAny(GraphicsTypes)
    .AndShould()
    .NotDependOnAny(UiTypes)
    .As("input must not depend on models, game logic, graphics, or UI");
```

Adding a direction enum to this namespace to make key mapping "cleaner" would place a game concept in a leaf capability layer; the constraint is that key meaning belongs to screens.

## Testing

`ConsoleKeyEventArgsTests` (2 tests) asserts the constructor stores the key, for `W` and `Escape`. `InputManager` itself is untested because it reads `Console.KeyAvailable` and `Console.ReadKey`, which require a live console; the same reason excludes it from any headless verification. `RuleApplicabilityTests` does not record this gap explicitly — [testing.md](../testing.md) does.

## Related documents

- [screens.md](./screens.md) — where keys are interpreted
- [flows/player-movement.md](../flows/player-movement.md) — the subscribe-to-effect path of a movement key
- [concurrency-and-scheduling.md](../concurrency-and-scheduling.md) — synchronous event dispatch inside the frame loop