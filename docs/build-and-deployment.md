# Build and deployment

## Summary

SokobanCLI is a self-contained console application. There is no server, no container, no installer, and no service topology. The deployment unit is the published output of `SokobanCLI/SokobanCLI.csproj` together with its `Levels/` and `Screens/` directories, which must travel beside the assembly because both are resolved relative to the process working directory.

## Local build

```bash
dotnet build SokobanCLI.slnx
```

Builds all three projects. `SokobanCLI/Program.cs` is the entry point; the output lands in `SokobanCLI/bin/<Configuration>/net10.0/` with `Levels/` and `Screens/` copied by the item groups in the project file:

```xml
<None Update="Levels\*.lvl">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
<None Update="Screens\*.xml">
  <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
</None>
```

VS Code defines the same command as the `build` task, plus a `publish` task and a `watch` task running `dotnet watch run --project SokobanCLI/SokobanCLI.csproj`.

## Publish

```bash
dotnet publish SokobanCLI.slnx
```

Produces a framework-dependent build by default. The release workflow does not call `dotnet publish` directly; it delegates to an external helper that builds self-contained artefacts per runtime identifier (see below).

## Running

```bash
dotnet run --project SokobanCLI
```

`dotnet run` sets the working directory to the project directory, which contains `Levels/` and `Screens/`, so relative content resolution succeeds. The `.vscode/launch.json` configuration mirrors this by setting `cwd` to `${workspaceFolder}/SokobanCLI` while launching `bin/Debug/net10.0/SokobanCLI.dll`.

A published build launched from any other directory fails at the first level load with `FileNotFoundException` unless `Levels/` and `Screens/` are present in that directory.

## Continuous integration

`.github/workflows/dotnet.yml`, triggered on pushes and pull requests to `master`:

| Step | Command |
|------|---------|
| Check out | `actions/checkout@v7` |
| Determine the .NET version | Parses `<TargetFramework>` from `SokobanCLI/SokobanCLI.csproj` with `sed`, strips the `net` prefix, and writes it to `GITHUB_OUTPUT`; fails with an explicit error when the element is absent |
| Set up .NET | `actions/setup-dotnet@v5` with `<version>.x` |
| Restore | `dotnet restore` |
| Build | `dotnet build --no-restore` |
| Test | `dotnet test --no-build --verbosity normal` |

The version is derived from the project rather than hardcoded, so changing `<TargetFramework>` changes the SDK used in CI without editing the workflow. The runner is `ubuntu-latest`, so the pipeline exercises the Linux code path only.

## Security scanning

`.github/workflows/security.yml`, triggered on pushes and pull requests to `master` and weekly on a Sunday-midnight cron:

| Step | Command or action |
|------|-------------------|
| Check out | `actions/checkout@v7` |
| Set up .NET | `actions/setup-dotnet@v5` with `10.x` (hardcoded here, unlike the other workflows) |
| Restore | `dotnet restore` |
| Audit | `dotnet list package --vulnerable --include-transitive` |
| Initialise CodeQL | `github/codeql-action/init@v4` with `languages: csharp` |
| Build | `dotnet build --no-restore` |
| Analyse | `github/codeql-action/analyze@v4` |

Permissions: `security-events: write` for result publication and `contents: read` for checkout.

Note the divergence: `security.yml` hardcodes `10.x` while `dotnet.yml` and `github-release.yml` derive the version from the project. A framework change must be accompanied by a change here.

## Release

`.github/workflows/github-release.yml`, triggered by a published release:

| Step | Behaviour |
|------|-----------|
| Check out | `actions/checkout@v7` with `fetch-depth: 0` so tags and history are available |
| Determine the .NET version | Same `sed` extraction from `SokobanCLI/SokobanCLI.csproj` |
| Set up .NET | `actions/setup-dotnet@v5` with `<version>.x` |
| Extract the version | Takes `github.event.release.tag_name` and strips a leading `v`, so tag `v1.0.0` yields `1.0.0` |
| Normalise the origin remote | `git remote set-url origin "https://github.com/${GITHUB_REPOSITORY}.git"` so the helper can push or query over HTTPS |
| Build the artefacts | `wget --quiet -O - "https://raw.githubusercontent.com/hmlendea/dotnet-release-scripts/refs/heads/master/dotnet/${DOTNET_VERSION}.sh" \| bash /dev/stdin "${RELEASE_VERSION}"` |
| Publish assets | `softprops/action-gh-release@v2` attaching `SokobanCLI/bin/Release/sokoban-cli_*` |

Permissions: `contents: write`, required to attach assets to the release.

Properties of this workflow that matter operationally:

- **The helper is fetched from a branch, not a pinned revision.** The executed script is whatever `master` contains at run time, so a release is not reproducible from this repository alone.
- **The helper determines the artefact set.** `.NET` version and release version are passed in; the runtime identifiers, archive formats, and naming (`sokoban-cli_*`) come from the helper. The asset glob in the publish step matches that naming.
- **The `.NET` version selects the helper.** A framework bump changes which helper script is downloaded. If no helper exists for that major version, the release fails at the `wget` step.
- **The README warns readers**, in the Release section, that piping into `bash` is controversial and that external scripts should be reviewed first.

## Release procedure

1. Ensure `master` is green in both `dotnet.yml` and `security.yml`.
2. Create and publish a release with a `v`-prefixed tag:

```bash
gh release create v1.0.0 --generate-notes
```

3. The release workflow builds and attaches the artefacts. Verify the attached assets and the release notes.

There is no version file and no `CHANGELOG.md`; the version exists only as the release tag, which the workflow converts into the artefact version.

## Deployment topology

| Concern | Design |
|---------|--------|
| Topology | One foreground console process, single-threaded, with an unbounded frame loop |
| Scaling | Not applicable; the application is per-terminal |
| Availability | Not applicable; there is no service |
| Persistence | None, so there is nothing to back up, migrate, or restore |
| Distribution | GitHub release assets only |
| Prerequisites | A .NET 10 runtime or SDK (or the self-contained artefact's bundled runtime) and a real interactive terminal |
| Recovery | None. A corrupt level or screen file is not repaired; the operator restores the file and relaunches |
| Graceful shutdown | None. The loop has no exit condition, so termination is external or an unhandled exception |

Operator-visible output is limited to the rendered terminal and, on failure, an unhandled exception trace on standard error. See [troubleshooting.md](./troubleshooting.md) for the observable failure signatures.

## Deployment checklist

- [ ] Artefact and its `Levels/` and `Screens/` directories are deployed together.
- [ ] The process is launched with the artefact directory as the working directory.
- [ ] The terminal supports cursor positioning and colour, and its window is large enough for the 14 × 14 map plus the details panel (at least 14 rows for the map plus 6 for the panel).
- [ ] The terminal is not resized while the game runs; enlarging it crashes the renderer.
- [ ] Content files are read-only to the process, because the application never writes them and an accidental write would be overwritten by the next build anyway.

## Related documents

- [components/composition-root.md](./components/composition-root.md) — startup ordering the deployment must satisfy
- [dependencies.md](./dependencies.md) — what the build restores
- [testing.md](./testing.md) — what CI runs