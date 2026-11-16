# Documentation maintenance

## Purpose

This `docs/` tree exists so that future contributors and automated agents can understand the repository without re-deriving it from source. Keeping it accurate is part of the definition of done for a behaviour change.

## Ownership

| Document | Update when |
|----------|-------------|
| `docs/INDEX.md` | A document is added, removed, or renamed, or the maintenance metadata changes |
| `docs/repository-overview.md` | Purpose, scope, actors, or deliberately absent capabilities change |
| `docs/repository-structure.md` | Files, projects, or content directories are added, moved, or removed |
| `docs/quick-start.md` | Prerequisites, commands, key bindings, or first-run gotchas change |
| `docs/architecture.md` | Layer relationships, composition strategy, or the dependency-policy tests change |
| `docs/behaviour/*.md` | A player-visible behaviour changes: what a key does, what a glyph means, how a level ends |
| `docs/components/*.md` | A component's contract, members, lifecycle, or failure modes change |
| `docs/flows/*.md` | An execution path changes: startup order, the frame pipeline, move handling, transitions, advancement |
| `docs/data-model.md` | A field is added, removed, retyped, or its owner changes |
| `docs/invariants.md` | An invariant is introduced, relaxed, or removed — including adding a rule-family precondition change |
| `docs/state-and-persistence.md` | State ownership or lifetime changes, or persistence is introduced |
| `docs/concurrency-and-scheduling.md` | Threading, async, locking, or loop pacing changes |
| `docs/error-handling.md` | A `throw`, `catch`, or guard is added or removed, or a failure mode's consequence changes |
| `docs/logging.md` | Any logging, telemetry, or diagnostics facility is introduced |
| `docs/security.md` | The trust boundary, reflection surface, or dependency pinning changes |
| `docs/integrations.md` | A runtime or build-time boundary is added or removed |
| `docs/dependencies.md` | A package, tool, or framework feature is added, removed, or version-changed |
| `docs/testing.md` | Test counts, fixtures, conventions, or coverage gaps change |
| `docs/build-and-deployment.md` | Build, publish, workflow, release, or deployment prerequisites change |
| `docs/configuration.md` | A configuration source, content property, or working-directory contract changes |
| `docs/design-decisions.md` | A decision is made, reversed, or its rationale changes |
| `docs/change-guide.md` | A convention rule or an impacted file set changes |
| `docs/troubleshooting.md` | A failure mode is discovered, fixed, or its resolution changes |
| `docs/faq.md` | A question is asked repeatedly or an answer changes |
| `docs/ambiguities-and-open-questions.md` | A TODO is resolved, a new one appears, or an assumption is verified or invalidated |

## Synchronisation with root documents

Three root documents describe the same system and must not contradict this tree or the code:

| Root document | Relationship |
|---------------|--------------|
| `README.md` | User-facing: features, controls, requirements, usage. Its fixed prose and section structure are template-governed and must not be rephrased; only placeholder-driven content may change |
| `ARCHITECTURE.md` | Authoritative architectural description of the current implementation. This tree complements it and must not duplicate it. Where this tree states something the root document also states, prefer updating the root document and reference it |
| `PRIVACY.md` | States that no data is collected, nothing is written, and no telemetry is used. Any persistence or telemetry work must update it |

Known contradiction to resolve rather than propagate: the README's Features list claims saved settings and progress, which the code, `PRIVACY.md`, and `ARCHITECTURE.md` all contradict. See [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md).

When a change alters user-facing capability, update the README; when it alters architecture, update the root `ARCHITECTURE.md`; when it alters privacy posture, update `PRIVACY.md`; and update the affected `docs/` files in the same change.

## Keeping the tree honest

Three habits keep this documentation trustworthy:

1. **Ground claims in evidence.** Prefer "`GameManager.MovePlayer` returns before any write when `!IsRunning`" over "moves are validated". Cite the file or symbol so a reader can verify.
2. **Mark inference explicitly.** Where a conclusion is derived rather than observed, say so. [troubleshooting.md](./troubleshooting.md) and [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md) already label confidence.
3. **Re-derive after structural change.** Adding a layer, changing the frame pipeline, or introducing persistence invalidates large parts of `architecture.md`, `invariants.md`, `flows/`, and `design-decisions.md`. Refresh them together rather than incrementally patching.

## Verification

There is no documentation build, link checker, or lint in this repository. Verify manually:

1. Confirm every relative link in `docs/` resolves, including links to `../README.md`, `../ARCHITECTURE.md`, `../SECURITY.md`, and `../PRIVACY.md`.
2. Confirm every code symbol referenced in backticks exists in the current source.
3. Confirm test counts quoted in [testing.md](./testing.md) match a fresh `dotnet test` run.
4. Confirm the layer matrix in [architecture.md](./architecture.md) matches the rules in `SokobanCLI.ArchitectureTests/ProductionArchitectureTests.cs`.
5. Confirm the convention list in [testing.md](./testing.md) and [invariants.md](./invariants.md) matches the fixtures as they are today.

## Regenerating from scratch

If substantial divergence makes incremental repair impractical, regenerate the tree in this order, because later documents depend on earlier findings:

1. Inventory the repository and confirm the project structure.
2. Read the production sources in dependency order: `Models`, `GameLogic`, `Graphics`, `Input`, `UI`, `Program`.
3. Read the content files and the test projects, including the architecture-test rule set.
4. Derive the invariants from what the code assumes, then the data model from what it owns.
5. Write `architecture.md`, the component files, then the flow files.
6. Write the reference documents, then the maintenance documents.
7. Rebuild `INDEX.md` last so its catalogue and metadata reflect the final set.

## Maintenance metadata

| Field | Value |
|-------|-------|
| Last reviewed | 2026-11-16 |
| Reviewed against commit | `f90af36` on `master` |
| Owner | Repository maintainer (`hmlendea`) |
| Coverage | Every production source file, both test projects, all three GitHub Actions workflows, all content directories, and the root documents |
| Known exclusions | The `NuciDAL` package internals, the external release helper script, and runtime behaviour on Windows and macOS were not verified; see [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md) |

## Related documents

- [INDEX.md](./INDEX.md) — the catalogue this document's ownership table mirrors
- [change-guide.md](./change-guide.md) — the code-side counterpart of this document
- [ambiguities-and-open-questions.md](./ambiguities-and-open-questions.md) — what remains unresolved