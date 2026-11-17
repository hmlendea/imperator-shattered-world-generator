# Mod Writer

## Purpose

`ModWriter` creates the mod directory tree and serialises the in-memory entities into the game's file formats. It performs no generation.

## Location

`ImperatorShatteredWorldGenerator/Service/ModWriter.cs` — a `public sealed class` implementing `IModWriter`.

## Entry

`CreateMod()`, called once by `ShatteredWorldGenerator.Generate` after the capital stage.

## Execution

`CreateMod` runs a fixed sequence:
1. `PrepareDirectoryStructure()`
2. `WriteModDescriptor()`
3. `WriteProvinces()`
4. `WriteCountryFiles()`
5. `WriteCountriesDefinitionIndexFile()`
6. `WriteCountriesLocalisationFile()`
7. `WriteCountrySetupFile()`
8. `WriteEventsFile()`
9. `WriteDefinesFile()`

### Directory preparation

Creates `out/<mod-id>/setup/provinces`, `out/<mod-id>/setup/countries`, `out/<mod-id>/setup/main`, `out/<mod-id>/common/defines`, `out/<mod-id>/events`, and `out/<mod-id>/localization/english`. Before creating, it recursively deletes `out/<mod-id>` when present and deletes `out/<mod-id>.mod` when present. The delete-then-create order means a failure after deletion leaves no output at all.

### Files

| File | Writer | Content | Encoding |
|------|--------|---------|----------|
| `out/<mod-id>.mod` | `WriteModDescriptor` | `name = "<ModName>"` newline `path = "mod/<ModId>"` | Plain (`File.WriteAllText`, UTF-8 without BOM) |
| `out/<mod-id>/descriptor.mod` | `WriteModDescriptor` | Identical to the above | Plain |
| `out/<mod-id>/setup/provinces/02_shattered_world.txt` | `WriteProvinces` | One randomised province block per habitable province | UTF-8 **with** BOM (`StreamWriter` + `Encoding.UTF8`) |
| `out/<mod-id>/setup/countries/<Id>.txt` | `WriteCountryFiles` | `color = rgb { r g b }`, blank line, `ship_names = { ... }`, for generated countries | UTF-8 with BOM |
| `out/<mod-id>/setup/countries/countries.txt` | `WriteCountriesDefinitionIndexFile` | Four fixed entries then one line per country | UTF-8 with BOM |
| `out/<mod-id>/localization/english/SW_countries_l_english.yml` | `WriteCountriesLocalisationFile` | `l_english:` then ` Id:0 "Name"` for non-vanilla countries | UTF-8 with BOM |
| `out/<mod-id>/setup/main/02_shattered_world_countries.txt` | `WriteCountrySetupFile` | `country = { countries = { ... } }` | UTF-8 with BOM |
| `out/<mod-id>/events/flavor_sel.txt` | `WriteEventsFile` | A `grinsel.2` province event | UTF-8 with BOM |
| `out/<mod-id>/common/defines/00_defines.txt` | `WriteDefinesFile` | `NGame = { START_DATE = "450.10.1" }` | UTF-8 with BOM |

### Notes on individual writers

- **`WriteProvinces`** writes one block per city, keyed by province id, with a name comment, the managed attributes (`barbarian_power`, `civilization_value`, `culture`, `province_rank`, `religion`, `trade_goods`), the city's preserved lines, and one `amount` sub-block per positive population type. The file is named `02_shattered_world.txt` so that it loads alongside (and after) the vanilla `00_*` province files rather than replacing them.
- **`WriteCountryFiles`** writes `ship_names` from `country.ShipNames` for generated countries only; vanilla country files already exist in the base game and their paths are reused in the index.
- **`WriteCountriesDefinitionIndexFile`** always begins with `REB`, `PIR`, `BAR`, and `MER` pointing at the vanilla files, then appends every country in `GetCountries()` order.
- **`WriteCountriesLocalisationFile`** filters `!c.IsVanilla`, so only generated countries are localised; vanilla names already exist in the base game.
- **`WriteCountrySetupFile`** writes, per country: a comment with the name; for vanilla countries two compatibility comments; `government`; `diplomatic_stance` only when non-blank; `primary_culture`; `religion`; `centralization` only when `>= 0`; `capital`; the country's preserved lines; and `own_control_core = { <capital> }`.
- **`WriteEventsFile`** emits `create_pop = slaves` five times using `NuciExtensions.Repeat`, tied to a `num_of_cities = 1` and `is_capital = yes` trigger.
- **`WriteDefinesFile`** moves the start date to `450.10.1`.

### Newline normalisation

`NormaliseFileContent` ensures the content starts and ends with `Environment.NewLine`. `WriteFile` applies it and then `File.WriteAllText`. `WriteUnicodeFile` writes the content verbatim through a `StreamWriter` with `Encoding.UTF8`, which emits a BOM.

## Data

| Direction | Data |
|-----------|------|
| In | `entityManager.GetCities()`, `entityManager.GetCountries()`, `settings.ModName`, `settings.ModId`, `settings.ModDirectoryPath`, `settings.ModFilePath` |
| Out | Nine files across two locations |

## State

No retained state; each call rewrites everything from scratch.

## Dependencies

`NuciExtensions.Repeat`, `System.IO`, `System.Text.Encoding`.

## Dependants

`ShatteredWorldGenerator` only.

## Configuration

Uses the derived paths and the mod name. Nothing else is configurable: file names, the event body, the fixed index entries, and the start date are hard-coded.

## Failures

| Condition | Result |
|-----------|--------|
| Output directory not writable | `UnauthorizedAccessException` after the previous output was deleted |
| `GetCities` throws (loader failure) | Exception during the first write; partial directory exists |
| A city or country property is `null` | `null` is interpolated as an empty string, producing an empty attribute value |
| Ship names empty | `ship_names = {` followed by a blank line and `}` |

## Invariants

- The output directory and descriptor are deleted first, so stale files never survive a run.
- `descriptor.mod` and `<mod-id>.mod` are byte-identical.
- Every country appears in `countries.txt` and in the setup file; only non-vanilla countries appear in the localisation file and receive a country file.
- `centralization` is omitted exactly when the level is negative, matching the `Country` sentinel of `-1`.
- The literal `VANILLA COUNTRY - REQUIRED FOR COMPATIBILITY` comment is written for every vanilla country, and `ModWriterTests` asserts it.
- Every written file carries a UTF-8 BOM, which `ModWriterTests` verifies byte-wise.

## Tests

`ImperatorShatteredWorldGenerator.IntegrationTests/Service/ModWriterTests.cs` renders into a temporary working directory and asserts: the directory structure, both descriptor files and their exact content, discarding previous output, the province file for a city, preserved lines, the population blocks, the header-only case, generated country files, the absence of vanilla country files, the definition index with vanilla and generated paths, localisation inclusion and exclusion, every setup field, preserved lines, the centralisation omission, the vanilla compatibility comment, the empty-country closing blocks, the event file and its five slave pops, the defines file, and the BOM on every written file.

## Integration

`ModWriter` is where in-memory state becomes a game artefact. Any change to the field selection here is immediately visible in the game and is the most expensive class of change to validate, because no test can run the game itself.

## Modification impact

- Changing a path or file name requires a matching change in the loader only if the loader also reads that file (it does not).
- Adding a country attribute requires updating `WriteCountrySetupFile` and possibly `WriteCountryFiles`.
- Changing the province file name affects whether it merges with or replaces the vanilla province files.
- Changing the setup file name affects load order against the vanilla `00_default.txt`.