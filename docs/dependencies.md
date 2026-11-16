# Dependencies

## Production dependencies

`SokobanCLI/SokobanCLI.csproj` declares exactly one package and no project references.

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>SokobanCLI</RootNamespace>
    <ImplicitUsings>disable</ImplicitUsings>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <None Update="Levels\*.lvl">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
    <None Update="Screens\*.xml">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    </None>
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="NuciDAL" Version="3.1.0" />
  </ItemGroup>
</Project>
```

| Dependency | Version | Scope | Purpose | Architectural consequence |
|------------|---------|-------|---------|---------------------------|
| `NuciDAL` | 3.1.0 | Runtime | XML object deserialisation through `XmlFileObject<T>` | The only runtime package. Screen definitions must stay compatible with its conventions, and `ScreenManager` holds a single mutable reader instance whose `Type` selects the deserialisation target |
| `NuciExtensions` | transitive | Runtime | Helper methods pulled in by `NuciDAL` | Present in the output directory (`NuciExtensions.dll`) but not referenced by any repository source |
| .NET 10 runtime and BCL | `net10.0` | Runtime | Process hosting, `System.Console`, `System.IO`, `System.Threading.Lock`, `System.Xml.Serialization` | The console *is* the display and input device; no abstraction isolates the application from it |

`ProjectReferenceTests` asserts that the production project declares no `ProjectReference`, no test or database package (checking for `NUnit`, `Moq`, `Test.Sdk`, `TestAdapter`, `xunit`, `ArchUnitNET`, `EntityFramework`, `Dapper`, `SqlClient`, `Npgsql`, `Dynamo` fragments), and no `InternalsVisibleTo`, and that the compiled assembly references no such assembly either.

## Framework and language features relied upon

| Feature | Where used | Why it matters |
|---------|-----------|----------------|
| `System.Threading.Lock` | The four singleton `Instance` properties | A .NET 9+ type; requires a recent runtime, which `net10.0` provides |
| Collection expressions (`[]`) | `NoScreenArgs`, `Children`, `Links`, `Actions`, `UiMenuArguments` arrays | Requires C# 12+; `LangVersion` is `latest` |
| Primary constructors on classes | `PlayerMovementEventArgs`, `ConsoleKeyEventArgs` | Requires C# 12+ |
| List patterns and `is … or …` patterns | `GameManager` (`Id is 0 or 3`, `Id is 2 or 5`, `destinationId is not 0 and not 3`) | The rule encoding depends on pattern syntax |
| `Enum.GetValues<T>()` | `Tiles.GetTypeForId` | Generic enumeration access |
| Nullable reference types | Enabled in all three projects | `Type.GetType(...)!` and `(… as Screen)!` are deliberate suppressions of nullability warnings |

## Test dependencies

### `SokobanCLI.UnitTests`

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.NET.Test.Sdk` | 17.11.1 | Test host |
| `Moq` | 4.20.72 | The only mock used is `Mock<IPlayerMovedSpy>` in `GameManagerTests` to verify `PlayerMoved` fires exactly once with the expected position |
| `NUnit` | 4.2.2 | Test framework |
| `NUnit3TestAdapter` | 4.6.0 | Test discovery for `dotnet test` |

Also references `..\SokobanCLI\SokobanCLI.csproj` and links `..\SokobanCLI\Levels\*.lvl` into its output so level 0 is loadable when tests run.

### `SokobanCLI.ArchitectureTests`

| Package | Version | Purpose |
|---------|---------|---------|
| `Microsoft.CodeAnalysis.CSharp` | 4.14.0 | Parses and compiles production sources in memory so rules can resolve symbols and overloads |
| `Microsoft.NET.Test.Sdk` | 18.11.0 | Test host |
| `NUnit` | 5.0.0 | Test framework |
| `NUnit3TestAdapter` | 6.3.1 | Test discovery |
| `TngTech.ArchUnitNET.NUnit` | 0.13.5 | Loads the production assembly and evaluates layering rules |

Also references `../SokobanCLI/SokobanCLI.csproj`, which is what makes `typeof(World).Assembly` available for ArchUnitNET, and generates `ArchitecturePaths.g.cs` at compile time with the absolute production source root and solution path.

Note that the two test projects use different NUnit major versions (4.2.2 and 5.0.0) and different test SDK versions (17.11.1 and 18.11.0). `ProjectConventionTests` requires each project's package references to be alphabetised, not to be version-aligned across projects.

## Dependency direction and layering constraint

`ProductionArchitectureTests` evaluates dependencies at the *type* level using the compiled assembly, while `ProjectReferenceTests` evaluates them at the *project* level. Both project references point from a test project to the production project, which is the only permitted direction:

```csharp
[Test]
public void GivenTheArchitectureTestProject_WhenInspected_ThenItIsTheOnlyConsumerOfTheProductionProject()
{
    // expects exactly SokobanCLI.ArchitectureTests.csproj and SokobanCLI.UnitTests.csproj
}
```

The production project therefore has no dependency on either test project, and the production assembly references no test assembly.

## Build-only tooling

| Tool | Source | Purpose |
|------|--------|---------|
| .NET SDK 10 | Developer machine, CI | Restore, build, test, publish; the version is derived from `<TargetFramework>` by the workflows |
| `dotnet-release-scripts` | Fetched at release time from `https://raw.githubusercontent.com/hmlendea/dotnet-release-scripts/refs/heads/master/dotnet/<version>.sh` | Builds self-contained release artefacts for multiple runtime identifiers |
| VS Code tasks | `.vscode/tasks.json` | `build` (`dotnet build SokobanCLI.slnx`), `publish`, and `watch` (`dotnet watch run --project SokobanCLI/SokobanCLI.csproj`) |
| VS Code launch configuration | `.vscode/launch.json` | Launches `SokobanCLI/bin/Debug/net10.0/SokobanCLI.dll` with `cwd` set to `SokobanCLI`, which is required for relative content resolution |

## Absent dependencies, deliberately

| Type of dependency | Absence evidence |
|--------------------|------------------|
| Logging framework | `RuleApplicabilityTests` asserts no `NuciLog`, `ILogger`, `LogInformation`, `LogError`, or `TelemetryClient` fragment |
| HTTP or networking library | `RuleApplicabilityTests` asserts no `HttpClient`, `NuciWeb`, `HttpRequestMessage`, or `IPAddress` fragment |
| Game framework | `RuleApplicabilityTests` asserts no `GuiImage`, `GuiText`, `GuiControl`, `Texture2D`, or `SpriteFont` fragment |
| Database or ORM | `RuleApplicabilityTests` asserts no `EntityFramework`, `Dapper`, `DynamoDB`, `SqlConnection`, or `Npgsql` fragment |
| Dependency injection container | No container is used; composition is static singletons plus one composition root |
| Test doubles library beyond Moq | Only Moq, for one spy |
| Coverage tooling | Both test projects set `ExcludeFromCodeCoverage`; no coverage package is referenced |
| Analyser configuration | `RuleApplicabilityTests` asserts no `.editorconfig`, `Directory.Build.props`, or `*.ruleset` file exists at the repository root |

The `ConfigurationConventionTests` fixture verifies analyser enforcement expectations, and `RuleApplicabilityTests` records that thresholds become enforceable only once such a configuration file is added.

## Updating dependencies

The repository has a documented procedure for NuGet updates (`update-nuget-packages` skill). When performing one:

1. `ProjectConventionTests` requires package references to remain alphabetised within each project.
2. `ProductionArchitectureTests` and `ProjectReferenceTests` will fail if a new package lands in the production project with a test- or database-indicative name.
3. `RuleApplicabilityTests` will fail if a new package introduces logging, HTTP, persistence, or game-framework fragments into production sources, because those checks assert absence — which is the intended forcing function: the corresponding rules must then be written rather than skipped.
4. The target framework is `net10.0` in all three projects, asserted by `ProjectConventionTests`.

## Related documents

- [build-and-deployment.md](./build-and-deployment.md) — how the toolchain and workflows use these dependencies
- [integrations.md](./integrations.md) — the runtime boundaries the dependencies support
- [testing.md](./testing.md) — what each test package enables