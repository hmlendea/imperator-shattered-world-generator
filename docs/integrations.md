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
| `game/setup/provinces/*.txt` | Clausewitz text scan | One block per province keyed by province id; each contains `culture`, `religion`, `trade_goods`, `province_rank`, `civilization_value`, `barbarian_power`, and pop sub-blocks (`citizen`, `freemen`, `slaves`, `tribesmen`) |
| `game/map_data/definition.csv` | `CsvFile<ProvinceDefinitionEntity>` | Semicolon-separated; column 1 is the province id, column 5 the province name; commented (`#`) rows are ignored |
| `game/setup/main/*.txt` | Clausewitz text scan | `country = { countries = { <TAG> = { government, diplomatic_stance, primary_culture, religion, centralization, capital, own_control_core } } }` |
| `game/setup/countries/countries.txt` | Regex line scan | `<Tag> = "<relative path>"`, where the path is relative to the game directory |
| `game/localization/english/countries_l_english.yml` | Regex line scan | `<Tag>:0 "<localised name>"` |
| `game/common/cultures/**.txt` | Clausewitz text scan | A top-level group block containing a `culture = { ... }` block whose two-tab entries are the subculture identifiers |
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
| `out/<mod-id>/setup/provinces/02_shattered_world.txt` | Game province database | One randomised block per habitable province, keyed by province id |
| `out/<mod-id>/setup/countries/<TAG>.txt` | Game country database | Colour and ship names for each generated country |
| `out/<mod-id>/setup/countries/countries.txt` | Game country database | The fixed entries plus every vanilla and generated tag |
| `out/<mod-id>/setup/main/00_default.txt` | Game setup database | One `countries` block with every country's government, stance, culture, religion, centralisation, capital, and `own_control_core` |

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