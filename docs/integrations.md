# Integrations

## Overview

The application integrates with the local machine only. There is no HTTP client, no socket, no message queue, and no cloud service.

| Integration | Direction | Type | Contract |
|-------------|-----------|------|----------|
| Imperator: Rome installation | Inbound | File system | Vanilla CSV and text formats described below |
| Working directory | Outbound | File system | Mod directory and descriptor in the game's mod-folder format |
| Command line | Inbound | Process arguments | Option names and aliases documented in [Configuration](./configuration.md) |
| Console | Outbound | Standard output | Two informational lines |
| Imperator: Rome mod loader | Outbound (indirect) | Data format | The game reads the generated directory; the application cannot verify this |

## Vanilla input files

| File | Consumed by | Format |
|------|-------------|--------|
| `game/common/province_setup.csv` | `CsvRepository<CityEntity>` | Header line beginning with `#`, then comma-separated rows in the order of `CityEntity`'s properties |
| `game/common/setup.txt` | Regex line scan | Tab-indented blocks; each contains `government`, `diplomatic_stance`, `primary_culture`, `religion`, `centralization`, `capital` |
| `game/common/countries.txt` | Regex line scan | `<Tag> = "<relative path>"` |
| `game/common/countries/**.txt` | Regex over file content | Contains `color = rgb { r g b }` |
| `game/localization/english/countries_l_english.yml` | Regex line scan | `<Tag>:0 "<localised name>"` |
| `game/common/cultures/**.txt` | Regex line scan | Empty blocks at two-tab indentation, e.g. `\t\tmunicipal = {}` |
| `game/common/religions/00_default.txt` | Regex line scan | Block headers |
| `game/common/governments/**.txt` | Regex line scan | Block headers |
| `game/common/diplomatic_stances/**.txt` | Regex line scan | Block headers |

All parsing is tolerant of missing optional content but strict about missing files or directories. Details and exception types are in [Entities Loader](./components/entities-loader.md#failures).

## Generated output

| Artefact | Consumer | Notes |
|----------|----------|-------|
| `out/<mod-id>/` | Game mod loader | Must exist at the path declared in the descriptor's `path` field |
| `out/<mod-id>.mod` | Game mod launcher / installers | Declares `name` and `path` |
| `out/<mod-id>/descriptor.mod` | Distribution | Duplicated so that archiving the directory yields a complete mod |

Because the descriptor declares `path = "mod/<mod-id>"`, the mod must be placed under the game's mod folder with the same relative layout. The application writes into `out/` relative to the working directory and does not install into the game folder.

## IDE tooling

| Integration | File | Purpose |
|-------------|------|---------|
| VS Code tasks | `.vscode/tasks.json` | `build`, `publish`, `watch`, each invoking `dotnet` against the project path (note: the task paths point at `${workspaceFolder}/ImperatorShatteredWorldGenerator.csproj`, which does not exist at the repository root after the solution restructure) |
| VS Code debug | `.vscode/launch.json` | `coreclr` launch with `preLaunchTask: build`, program path `bin/Debug/net10.0/ImperatorShatteredWorldGenerator.dll` |

## Continuous integration

`.github/workflows/dotnet.yml` determines the .NET version by reading `<TargetFramework>` from the production project, sets up that SDK, restores, builds, and tests on pushes and pull requests to `master`.

## Integrations deliberately absent

- No telemetry, analytics, crash reporting, or update check.
- No package publication workflow (the release is produced by `package.sh` and attached to a GitHub release manually or externally).
- No database, cache, queue, or object store.
- No authentication or authorisation, because there is no remote surface.