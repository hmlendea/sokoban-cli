# Logging

## Summary

SokobanCLI has no logging framework, no log levels, no structured fields, no sinks, no retention policy, and no redaction requirements, because it emits no logs at all. There is no logger instance, no `Console.WriteLine` diagnostic, no `Debug.WriteLine`, no `Trace`, and no `EventSource`. `RuleApplicabilityTests` asserts the absence explicitly.

## Evidence of absence

```csharp
[Test]
public void GivenTheProductionSources_WhenInspected_ThenNoLoggingOrTelemetryCodeExists()
{
    string[] loggingFragments = [ "NuciLog", "ILogger", "LogInformation", "LogError", "TelemetryClient" ];

    List<string> matches = ProductionSources
        .GetProductionFiles()
        .SelectMany(file => System.IO.File.ReadAllLines(file))
        .Where(line => loggingFragments.Any(fragment => line.Contains(fragment, StringComparison.Ordinal)))
        .ToList();

    Assert.That(matches, Is.Empty, "Logging rules (Operation, LogInfoKey, masking) become enforceable once logging code exists.");
}
```

The test's assertion message states the consequence: the logging conventions (an `Operation` scope, `LogInfoKey` usage, and secret masking) become enforceable only once logging code is introduced. Until then the repository records the rule as inapplicable rather than silently skipping it.

No package reference provides logging. `SokobanCLI.csproj` declares exactly one package, `NuciDAL` 3.1.0, used only for `XmlFileObject<T>`.

## Why logging is absent by design

Console output is the *rendering* mechanism. `GraphicsManager.Draw` sets cursor position, foreground colour, and background colour for every changed cell and writes single characters. Any additional write would:

- appear inside the game grid, because it would use whatever cursor position and colour the renderer last set;
- cause the shadow batch to disagree with the console, so the next diff would either fight the stray text or fail to repaint over it;
- be erased by the next full repaint after a `Clear`.

Logging to standard error would be less destructive but still interleaves with the terminal escape state the renderer manipulates.

## Available diagnostics

The developer-facing diagnostic surfaces, in order of usefulness:

| Surface | Location | What it shows |
|---------|----------|---------------|
| `AsciiSpriteBatch.ScreenPreview` | `Graphics/AsciiSpriteBatch.cs` | The render framebuffer as plain text, one line per row, each line newline-terminated. Its summary comment is annotated `DEBUG`. Nothing in production reads it |
| Stack traces | Unhandled exceptions | The exception type, message, and full stack, including the throwing statement. Because there is no `try`, every fault reaches the terminal this way |
| The display itself | Console | The current frame, which is often enough to see that a level loaded but the player is stuck, or that a menu is empty |
| `World.Level`, `World.Moves`, `World.PlayerPosition` | Game screen | Rendered in the details panel every frame, so the board's observable state is visible without instrumentation |
| `drawnData` versus `SpriteBatch` | `GraphicsManager` | Inspectable in a debugger; their divergence explains a stale or blank display |
| The test suite | `SokobanCLI.UnitTests`, `SokobanCLI.ArchitectureTests` | `ScreenPreview`-based assertions would be possible but none exist; the tests cover rule and structure behaviour instead |

## What a logging facility would have to solve first

| Constraint | Reason |
|------------|--------|
| Destination must not be the rendered console area | The renderer owns cursor position and colour per cell |
| Either a file sink or a debugger channel is required | `Console.Error` still shares the terminal with the display |
| The shadow batch must stay authoritative | Any external write to the console invalidates the diff |
| `Input` and `Graphics` may not depend on `UI`, and `Models`/`GameLogic` may not depend on `Input`/`UI`/`Graphics` | A shared logging abstraction would need a namespace the boundaries permit, most naturally beside the geometry kernel or a new leaf namespace referenced by everyone |
| An `Operation`-scoped log requires an ambient context | The application has no request or operation concept; the natural scope would be the frame or the level |
| Masking is moot today but should still be respected | No personal data, credentials, or secrets pass through the application, so nothing needs redaction — but any future progress or settings persistence would change that |

## Telemetry

There is none. `RuleApplicabilityTests` also asserts the absence of network code:

```csharp
string[] networkFragments = [ "HttpClient", "NuciWeb", "HttpRequestMessage", "IPAddress" ];
```

`PRIVACY.md` states that the application collects no analytics or telemetry, and [security.md](./security.md) records the same conclusion with more context.

## Related documents

- [error-handling.md](./error-handling.md) — the stack trace as the actual failure diagnostic
- [components/graphics.md](./components/graphics.md) — why console output is unsuitable for logging
- [testing.md](./testing.md) — how the absence of logging is verified rather than assumed