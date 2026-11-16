# Navigate screens

## Intent

Move between the title screen and the game screen using the keyboard, and understand the exact lifecycle each transition performs.

## Preconditions

- `ScreenManager.Instance` exists and holds a `TitleScreen` whose layout came from `Screens/TitleScreen.xml`.
- `InputManager.Instance.KeyboardKeyPressed` is subscribed by the current screen, by every current `UiElement`, and by `Program`'s redraw handler.

## Screens in the repository

| Screen type | XML definition | Role |
|-------------|----------------|------|
| `TitleScreen` | `Screens/TitleScreen.xml` | Menu with axis `Y`, one `Link` (`New Game` → `GameScreen`) and one `Action` (`Exit`) |
| `GameScreen` | `Screens/GameScreen.xml` | World map, details label, details, controls label, controls |
| `MenuScreen` | none | Base class providing menu layout and navigation; not instantiated directly |
| `Screen` | none | Base class providing lifecycle and event plumbing; not instantiated directly |

`Screens/ScreenManager.xml` is an empty `<ScreenManager>` element. It is copied to the output but is never read: `ScreenManager` constructs `XmlFileObject<Screen>` with `Type = currentScreen.Type`, which is `TitleScreen`, so the reader is pointed at `Screens/TitleScreen.xml`.

## What the player sees

The title menu is a vertical list centred on the screen. Exactly one item has input focus at a time and is drawn with `HighlightColour` (`Yellow` by default, from `UiMenuItem`'s constructor) instead of `ForegroundColour` (`White`). The focused item is decided by `MenuScreen.Update`, which clamps `ItemNumber` into range and calls `UiManager.Instance.FocusElement(Items[newSelectedItemIndex])`.

## Moving the selection

`MenuScreen.OnKeyPressed` handles navigation and the axis determines which keys apply:

| Axis value | Keys that decrease `ItemNumber` | Keys that increase `ItemNumber` |
|------------|---------------------------------|---------------------------------|
| `Y` or `y` (used by `TitleScreen`) | `W`, `UpArrow` | `S`, `DownArrow` |
| `X` or `x` | `D`, `RightArrow` | `A`, `LeftArrow` |

Note the inversion in the horizontal case: `D`/`RightArrow` *decreases* the index while `A`/`LeftArrow` *increases* it. In the vertical case the mapping is conventional. This asymmetry is in the code as written and is not compensated elsewhere.

Navigation has no wrap-around. Pressing `W` at index 0 sets `ItemNumber` to −1, and the next `Update` clamps it back to 0. Pressing `S` past the last item clamps to `Items.Count - 1`. Clamping happens in `Update`, not in the key handler, so the correction is applied on the following frame.

## Activating an item

`UiMenuItem.OnKeyPressed` activates on `Enter` or `E`. Activation only occurs if the item passed the `UiElement` filter — enabled, visible, able to raise events, and holding input focus. Because `MenuScreen.Update` focuses exactly one item per frame, at most one item responds.

Two activation behaviours exist:

- `UiMenuLink` (a `<Link>` element) splits `LinkArgs` on spaces and calls `ScreenManager.Instance.ChangeScreens(LinkId, args)`. `TitleScreen.xml` declares `LinkId` `GameScreen` with no arguments, so `GameScreen.ScreenArgs` is an empty array and the screen starts level 0.
- `UiMenuAction` (an `<Action>` element) switches on `ActionId`. The only case, `"Exit"`, contains a `// TODO: Close the game somehow...` comment and no statement, so activating `Exit` does nothing observable.

## Transition lifecycle

`ScreenManager.ChangeScreens(string screenName, string[] screenArgs)` performs the transition destructively, in this order:

1. `GraphicsManager.Instance.Clear(currentScreen.BackgroundColour)` — sets console background and foreground to the outgoing screen's background colour and calls `Console.Clear()`, then resets both framebuffers. Note that the foreground is also set to the background colour, so subsequent writes require the renderer to set colours explicitly, which it does per cell in `GraphicsManager.Draw`.
2. `CreateScreen(screenName, screenArgs)` — resolves the type, deserialises the definition, assigns arguments.
3. `currentScreen.UnloadContent()` — for `Screen` this unregisters the input handler and calls `UiManager.Instance.UnloadContent()`, which unloads every element and clears the list.
4. `currentScreen = newScreen`.
5. `currentScreen.LoadContent()` — registers events, loads elements.

Because the clear happens before the new screen is created and loaded, the visible sequence is: console blanked in the outgoing background colour, then the new screen's elements written by the next `Draw`. There is no fade, transition animation, or intermediate screen.

## Screen resolution by name

```csharp
private Screen CreateScreen(string screenName, string[] screenArgs)
{
    Type screenType = Type.GetType($"{typeof(Screen).Namespace}.{screenName}")!;
    Screen screen = (Activator.CreateInstance(screenType) as Screen)!;

    screenReader.Type = screen.Type;

    if (File.Exists(screen.XmlPath))
    {
        screen = screenReader.Read(screen.XmlPath);
    }

    screen.ScreenArgs = screenArgs;

    return screen;
}
```

Four consequences:

1. `screenName` is prefixed with `SokobanCLI.UI.Screens`, so a bare type name such as `TitleScreen` works. A fully qualified name would resolve to the same type only if it happened to match; a name resolving to a different namespace produces a type that is not a `Screen` and the cast yields null.
2. The null-forgiving `!` on `Type.GetType` suppresses the compiler's null warning. An unknown name produces `screenType == null`, then `Activator.CreateInstance(null)` throws `ArgumentNullException`, which is unhandled and terminates the process.
3. A missing XML file is not an error: the constructed screen keeps its constructor defaults (black background, white foreground, yellow highlight) and an empty menu. A malformed file throws inside `XmlFileObject<Screen>.Read`.
4. `screenReader` is a single shared instance whose `Type` is reassigned per transition, so deserialisation is stateful and not reentrant.

## Postconditions

- Exactly one `Screen` instance is live; the previous screen's key handler is unregistered and its UI elements were unloaded and removed from `UiManager.UiElements`.
- The console was cleared with the outgoing screen's background colour.
- The new screen's `ScreenArgs` hold the space-separated `LinkArgs`, with an empty array when `LinkArgs` is null, empty, or whitespace.

## Failure modes

| Observation | Cause |
|-------------|-------|
| `ArgumentNullException` on menu activation | `LinkId` does not resolve under `SokobanCLI.UI.Screens` |
| The new screen appears with default colours and no menu | A `Screens/<TypeName>.xml` file is missing |
| Duplicate key handling or unusual double activation | An element was added to `UiManager.UiElements` without a matching unload, so a stale handler still holds focus logic |
| The game screen starts on a level other than 0 | `LinkArgs` in the XML contains a level number |