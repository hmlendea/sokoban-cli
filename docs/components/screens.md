# Screens

Location: `SokobanCLI/UI/ScreenManager.cs`, `IScreenManager.cs`, `Screens/Screen.cs`, `MenuScreen.cs`, `TitleScreen.cs`, `GameScreen.cs` and their interfaces; content in `SokobanCLI/Screens/`

## Purpose

Own screen lifecycle — load, update, draw, unload — and resolve screen names to instances, including deserialising layout, colours, and menu entries from XML.

## ScreenManager

`public sealed class ScreenManager : IScreenManager` — singleton.

```csharp
private static readonly string[] NoScreenArgs = [];
private readonly XmlFileObject<Screen> screenReader;
private Screen currentScreen;

public Size2D Size { get; private set; }              // [XmlIgnore]
public void LoadContent();
public void UnloadContent();
public void Update(float gameTime);
public void Draw(AsciiSpriteBatch spriteBatch);
public void ChangeScreens(string screenName);
public void ChangeScreens(string screenName, string[] screenArgs);
```

### Construction side effects

```csharp
public ScreenManager()
{
    currentScreen = new TitleScreen();

    screenReader = new XmlFileObject<Screen>
    {
        Type = currentScreen.Type
    };

    currentScreen = screenReader.Read(currentScreen.XmlPath);
}
```

Reaching `ScreenManager.Instance` for the first time constructs a `TitleScreen`, points the shared `XmlFileObject<Screen>` at `TitleScreen`'s type, and reads `Screens/TitleScreen.xml`. Consequences:

- The constructor performs file I/O, so the first access to `Instance` can throw (`DirectoryNotFoundException`, `XmlException`, or an IO error). This happens inside `Program.LoadContent`.
- The initial screen comes from `new TitleScreen()` rather than from name resolution, so the first screen is compiler-checked while every later transition is string-resolved.
- `XmlFileObject<Screen>` is a single reused instance with a mutable `Type`; `CreateScreen` reassigns it per transition. It is therefore not safe to deserialise two screens concurrently, which the single-threaded loop makes irrelevant.

### Load and Update

```csharp
public void LoadContent()
{
    Size = GraphicsManager.Instance.Resolution;
    currentScreen.LoadContent();
}

public void Update(float gameTime)
{
    Size = GraphicsManager.Instance.Resolution;
    currentScreen.Update(gameTime);
}
```

`Size` is refreshed from the console on both load and every update. `MenuScreen.AlignMenuItems` and `MenuScreen.OnKeyPressed` read `ScreenManager.Instance.Size`, so the menu recentres against the current terminal size whenever the item count changes… in practice only during `LoadContent`, because `AlignMenuItems` is called from there. `Size` also satisfies `IScreenManager.Size`, which the menu uses.

### ChangeScreens

```csharp
public void ChangeScreens(string screenName) => ChangeScreens(screenName, NoScreenArgs);

public void ChangeScreens(string screenName, string[] screenArgs)
{
    GraphicsManager.Instance.Clear(currentScreen.BackgroundColour);

    Screen newScreen = CreateScreen(screenName, screenArgs);

    currentScreen.UnloadContent();
    currentScreen = newScreen;
    currentScreen.LoadContent();
}
```

The transition is destructive and ordered; note that the console is cleared with the *outgoing* screen's background colour and that the new screen is fully constructed (type resolved, XML read) *before* the old screen is unloaded, so a resolution failure leaves the old screen still loaded.

### CreateScreen

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

- The name is prefixed with `SokobanCLI.UI.Screens`. A name that resolves outside that namespace yields a non-`Screen` type, the `as Screen` cast yields null, and the null-forgiving `!` hides it until `screenReader.Type = screen.Type` throws `NullReferenceException`.
- A name that resolves to nothing makes `Type.GetType` return null, and `Activator.CreateInstance(null)` throws `ArgumentNullException`.
- A missing XML file is tolerated: the constructed instance keeps its own defaults.
- `ScreenArgs` is assigned after deserialisation so it cannot be overwritten by XML.

## Screen

`public class Screen : IScreen` — the base of every screen. Not sealed, because `MenuScreen` and `GameScreen` derive from it; `TypeDeclarationConventionTests` permits that and requires the inheritance to be by design.

| Member | Purpose |
|--------|---------|
| `Id` | A `Guid` string by default; `MenuScreen` overrides it to `string.Empty` |
| `XmlPath` | `Screens/<TypeName>.xml`, derived in the constructor from `GetType().Name` |
| `Type` | `GetType()`, marked `[XmlIgnore]` |
| `BackgroundColour`, `ForegroundColour`, `HighlightColour` | Defaults `Black`, `White`, `Yellow`; settable by XML |
| `ScreenArgs` | Initial arguments; defaults to null for a directly constructed screen |
| `KeyPressed` | Event forwarded from the input handler |
| `LoadContent`, `UnloadContent`, `Update`, `Draw` | Virtual lifecycle methods |
| `RegisterEvents`, `UnregisterEvents`, `OnKeyPressed` | Virtual event plumbing, `protected` |

```csharp
public virtual void LoadContent()
{
    UiManager.Instance.LoadContent();

    RegisterEvents();
}

public virtual void UnloadContent()
{
    UiManager.Instance.UnloadContent();

    UnregisterEvents();
}
```

Ordering matters: `LoadContent` loads UI elements *before* registering the key handler, so an element's `LoadContent` cannot be interrupted by a key event; `UnloadContent` unloads elements *before* unregistering the handler, so a key arriving mid-unload would still reach a partly unloaded element. `UnregisterEvents` happens last, which is the safer order of the two.

`XmlPath` is derived from the runtime type name, which is the mechanism that ties class names to XML file names. Renaming a screen class silently changes where the manager looks for its definition.

## MenuScreen

`public class MenuScreen : Screen, IMenuScreen` — adds menu layout and navigation.

| Member | Default | Purpose |
|--------|---------|---------|
| `Axis` | `"Y"` | Which axis the items are arranged on |
| `Spacing` | `1` | Gap between items |
| `Links` | empty list, `[XmlElement("Link")]` | `UiMenuLink` entries |
| `Actions` | empty list, `[XmlElement("Action")]` | `UiMenuAction` entries |
| `Items` | computed, `[XmlIgnore]` | `Links` then `Actions`, projected to `UiMenuItem` |
| `ItemNumber` | `0`, private setter, `[XmlIgnore]` | Index of the focused item |

`Items` order is stable and defines activation order: all links first, in XML order, then all actions, in XML order. `TitleScreen.xml` declares `New Game` as a link and `Exit` as an action, so index 0 is `New Game` and index 1 is `Exit`.

### LoadContent

```csharp
public override void LoadContent()
{
    UiManager.Instance.UiElements.AddRange(Links);
    UiManager.Instance.UiElements.AddRange(Actions);

    base.LoadContent();

    AlignMenuItems();
}
```

Elements are registered before `base.LoadContent()` loads them, so `UiManager.LoadContent` finds them. `AlignMenuItems` runs after, so element sizes are available.

### AlignMenuItems

```csharp
private void AlignMenuItems()
{
    Size2D dimensions = Size2D.Empty;

    Items.ForEach(item => dimensions += new Size2D(item.Size.Width + Spacing / 2,
                                                    item.Size.Height + Spacing / 2));

    dimensions = new((ScreenManager.Instance.Size.Width - dimensions.Width) / 2,
                    (ScreenManager.Instance.Size.Height - dimensions.Height) / 2);

    foreach (UiMenuItem item in Items)
    {
        if ("Xx".Contains(Axis))
        {
            item.Location = new Point2D(dimensions.Width, (ScreenManager.Instance.Size.Height - item.Size.Height) / 2);
        }
        else if ("Yy".Contains(Axis))
        {
            item.Location = new Point2D((ScreenManager.Instance.Size.Width - item.Size.Width) / 2, dimensions.Height);
        }

        dimensions += new Size2D(item.Size.Width + Spacing / 2,
                                item.Size.Height + Spacing / 2);
    }
}
```

Behaviour: the first pass accumulates the total size of all items plus half the spacing each, the sum is converted into a starting offset by centring it in the screen, and the second pass walks items placing each and advancing the offset. Note that `Spacing / 2` is integer division, so a spacing of 1 contributes 0 and items are effectively adjacent; only even spacing values halve exactly. A non-`X`/`Y` axis value leaves every item at its default location `(0, 0)` because neither branch executes.

`UiMenuItem`'s constructor sets `Size` to `(48, 1)`, so a vertical menu is centred horizontally against a 48-character item width rather than against the actual text length. Text is drawn centred inside that 48-wide box by `UiText`'s default `Centre`/`Centre` alignment, so short labels still appear centred.

### Update — focus assignment

```csharp
public override void Update(float gameTime)
{
    int newSelectedItemIndex = ItemNumber;

    if (newSelectedItemIndex < 0)
    {
        newSelectedItemIndex = 0;
    }
    else if (newSelectedItemIndex > Items.Count - 1 && Items.Count > 0)
    {
        newSelectedItemIndex = Items.Count - 1;
    }

    UiManager.Instance.FocusElement(Items[newSelectedItemIndex]);

    ItemNumber = newSelectedItemIndex;

    base.Update(gameTime);
}
```

Clamping happens here rather than in the key handler. `FocusElement` clears `HasInputFocus` on every element and sets it on the named one, which is what allows `UiMenuItem.OnKeyPressed` to activate.

An empty `Items` list makes `Items[0]` throw `ArgumentOutOfRangeException`: the clamp guards `Items.Count - 1` with `Items.Count > 0` but the indexing that follows is unguarded. A screen with no links and no actions therefore crashes on its first update.

## TitleScreen

`public sealed class TitleScreen : MenuScreen, ITitleScreen` — a pass-through subclass. Every method overrides the base and calls it, adding nothing. It exists to give the initial screen a name that resolves to a specific XML file and a specific type for `ScreenManager`'s constructor. Sealed, so screens wanting menu behaviour should derive from `MenuScreen` rather than from this.

## GameScreen

`public sealed class GameScreen : Screen, IGameScreen` — the playable screen. See [components/game-logic.md](./game-logic.md) for the rules it drives.

| Member | Purpose |
|--------|---------|
| `Worldmap`, `GameDetailsLabel`, `GameDetails`, `GameControlsLabel`, `GameControls` | Public properties populated by XML deserialisation |
| `game` (private) | Its own `GameManager`, created in `LoadContent` |
| `LoadContent` | Creates the manager, parses `ScreenArgs[0]` as the level, starts, registers elements, associates the manager with the world map, sets child properties |
| `Update` | Updates children, re-sets child properties, checks completion and advances |
| `RegisterEvents`/`UnregisterEvents` | Adds and removes `game.PlayerMoved → OnGamePlayerMoved` |
| `OnKeyPressed` | Movement, restart, and escape |
| `SetChildrenProperties` | Positions the details and controls panels and rebuilds the details text |

### LoadContent

```csharp
public override void LoadContent()
{
    game = new GameManager();
    int level = 0;

    if (ScreenArgs is not null && ScreenArgs.Length >= 1)
    {
        level = int.Parse(ScreenArgs[0]);
    }

    game.Start(level);

    UiManager.Instance.UiElements.Add(Worldmap);
    UiManager.Instance.UiElements.Add(GameDetailsLabel);
    UiManager.Instance.UiElements.Add(GameDetails);
    UiManager.Instance.UiElements.Add(GameControlsLabel);
    UiManager.Instance.UiElements.Add(GameControls);

    Worldmap.AssociateGameManager(game);

    SetChildrenProperties();

    base.LoadContent();
}
```

Notable: `int.Parse` is unguarded, so a non-numeric `LinkArgs` such as `<LinkArgs>abc</LinkArgs>` throws `FormatException` and terminates the process. `int.Parse` also uses the current culture, which for a plain integer is normally harmless but is not culture-invariant; `SpecialisedTypeConventionTests` asserts culture is passed to date handling, not to integer parsing.

Element registration order fixes draw order: world map, then details label, details, controls label, controls. All five are re-registered on the singleton `UiManager` list, and `Screen.UnloadContent` clears the list on transition, so the screen does not accumulate elements across visits.

### Update — completion handling

```csharp
public override void Update(float gameTime)
{
    base.Update(gameTime);

    SetChildrenProperties();

    if (game.GetCompletion())
    {
        game.Stop();

        // TODO: Save progress
        // TODO: What if this is the last level?
        // TODO: Switch to a transition screen before starting
        game.Start(game.World.Level + 1);
    }
}
```

`base.Update` updates the elements, then the screen recomputes child properties, then completion is checked. `Stop` is called immediately before `Start`, so `IsRunning` is false only across the gap between the two statements; no input can be processed in that window because the whole update is synchronous.

### SetChildrenProperties — layout

```csharp
private void SetChildrenProperties()
{
    GameDetailsLabel.Location = new Point2D(0, Worldmap.ClientRectangle.Bottom + 1);
    GameDetailsLabel.Size = new Size2D(GameDetailsLabel.Text.Length, 1);

    GameDetails.Location = new Point2D(0, GameDetailsLabel.ClientRectangle.Bottom);
    GameDetails.Size = new Size2D(GraphicsManager.Instance.Resolution.Width, 3);

    GameControlsLabel.Location = new Point2D(0, GameDetails.ClientRectangle.Bottom + 1);
    GameControlsLabel.Size = new Size2D(GameControlsLabel.Text.Length, 1);

    GameControls.Location = new Point2D(0, GameControlsLabel.ClientRectangle.Bottom);
    GameControls.Size = new Size2D(GraphicsManager.Instance.Resolution.Width, 1);

    GameDetails.Text = $"Level: {game.World.Level}" + Environment.NewLine +
                        $"Player coordinates: {game.World.PlayerPosition.X},{game.World.PlayerPosition.Y}" + Environment.NewLine +
                        $"Moves: {game.World.Moves}";
}
```

Layout is stacked vertically below the world map using `Rectangle2D.Bottom`, which is `Y + Height` and therefore already one past the last row; the extra `+ 1` for the labels adds a blank line. The label sizes are derived from their text length, so a longer label in the XML changes the size on the next update. `GameDetails` is sized three rows tall because the text has three lines.

Because `SetChildrenProperties` runs on every update and is also called from `UiElement.Update` for children, the details text is refreshed per frame.

### Key handling

```csharp
protected override void OnKeyPressed(object sender, ConsoleKeyEventArgs e)
{
    switch (e.Key)
    {
        case ConsoleKey.W:
        case ConsoleKey.UpArrow:
            game.MovePlayer(-1, 0);
            break;

        case ConsoleKey.A:
        case ConsoleKey.LeftArrow:
            game.MovePlayer(0, -1);
            break;

        case ConsoleKey.S:
        case ConsoleKey.DownArrow:
            game.MovePlayer(1, 0);
            break;

        case ConsoleKey.D:
        case ConsoleKey.RightArrow:
            game.MovePlayer(0, 1);
            break;

        case ConsoleKey.R:
            game.Start(game.World.Level);
            break;

        case ConsoleKey.Escape:
            ScreenManager.Instance.ChangeScreens(nameof(TitleScreen));
            break;
    }

    base.OnKeyPressed(sender, e);
}
```

`W` decreases X and `A` decreases Y, matching the transposed grid: X is the vertical axis on screen and Y is the horizontal axis. `R` restarts by number, which is also what `ScreenArgs` supplied. `Esc` changes screens *inside* a key handler that is itself called from `InputManager.Update`, so `ChangeScreens` runs nested inside the input dispatch; the outgoing screen's handler is unregistered while that handler is executing, which is safe because the delegate list was already captured for the current invocation.

### The immediate repaint

```csharp
private void OnGamePlayerMoved(object sender, PlayerMovementEventArgs e)
    => ScreenManager.Instance.Draw(GraphicsManager.Instance.SpriteBatch);
```

The event argument is ignored; the whole screen is redrawn. Subscriber order makes this the *second* draw of a movement key press: `Program`'s bridge subscribed before `GameScreen` existed, so for this screen the bridge runs first (painting the pre-move frame) and this handler runs afterwards (painting the post-move frame). The net effect is a correct display with one redundant diff pass per move.

## Screen content

### TitleScreen.xml

```xml
<?xml version="1.0" encoding="UTF-8"?>
<TitleScreen>
  <Axis>Y</Axis>
  <Link>
    <Text>New Game</Text>
    <LinkId>GameScreen</LinkId>
  </Link>
  <Action>
    <Text>Exit</Text>
    <ActionId>Exit</ActionId>
  </Action>
</TitleScreen>
```

### GameScreen.xml

```xml
<?xml version="1.0" encoding="UTF-8"?>
<GameScreen>
	<Worldmap>
		<Location>
			<X>0</X>
			<Y>0</Y>
		</Location>
		<Size>
			<Width>14</Width>
			<Height>14</Height>
		</Size>
	</Worldmap>
	<GameDetailsLabel>
		<ForegroundColour>Green</ForegroundColour>
		<Text>Game details</Text>
		<HorizontalAlignment>Left</HorizontalAlignment>
		<VerticalAlignment>Top</VerticalAlignment>
	</GameDetailsLabel>
	...
</GameScreen>
```

The element names must match the `GameScreen` property names exactly, because `XmlFileObject<Screen>` deserialises by name. Enum values such as `Green` and `Left` are parsed from the enumeration member names. Note that `GameScreen.xml` uses tab indentation while `TitleScreen.xml` uses two spaces; both are equivalent to the parser.

`Screens/ScreenManager.xml` is an empty `<ScreenManager>` element, copied to the output but never read.

### Deserialisation implications

| Fact | Consequence |
|------|-------------|
| Properties are matched by name, case-sensitively | Renaming a property silently drops the XML value; the constructor default applies |
| `List<UiMenuLink>` is mapped from repeated `<Link>` elements via `[XmlElement("Link")]` | The plural property name would otherwise require a wrapping element |
| `Type`, `ScreenArgs`, `ItemNumber`, `Items`, and every `UiElement`'s `IsHovered`, `HasInputFocus`, `Children`, `IsDisposed` are `[XmlIgnore]` | They cannot be set from XML, so transient state is safe |
| `UiMenuLink`/`UiMenuAction` derive from `UiMenuItem`, and the declared collection type is the derived type | Deserialisation uses the declared element type, so class-name strings in XML are not involved |
| A malformed document throws inside `XmlFileObject<T>.Read` | `ScreenManager` performs no validation and the exception reaches `Program.Main` unhandled |

## Related documents

- [behaviour/navigate-screens.md](../behaviour/navigate-screens.md) — the player-facing transition behaviour
- [ui-elements.md](./ui-elements.md) — the element types the screens register
- [flows/screen-navigation.md](../flows/screen-navigation.md) — the transition sequence step by step