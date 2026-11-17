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
3. `WriteProvincesSetup()`
4. `WriteCountryFiles()`
5. `WriteCountriesDefinitionIndexFile()`
6. `WriteCountriesLocalisationFile()`
7. `WriteSetupFile()`
8. `WriteEventsFile()`
9. `WriteDefinesFile()`

### Directory preparation

Creates `out/<mod-id>/common/countries`, `out/<mod-id>/common/defines`, `out/<mod-id>/events`, and `out/<mod-id>/localization`. Before creating, it recursively deletes `out/<mod-id>` when present and deletes `out/<mod-id>.mod` when present. The delete-then-create order means a failure after deletion leaves no output at all.

### Files

| File | Writer | Content | Encoding |
|------|--------|---------|----------|
| `out/<mod-id>.mod` | `WriteModDescriptor` | `name = "<ModName>"` newline `path = "mod/<ModId>"` | Plain (`File.WriteAllText`, UTF-8 without BOM) |
| `out/<mod-id>/descriptor.mod` | `WriteModDescriptor` | Identical to the above | Plain |
| `out/<mod-id>/common/province_setup.csv` | `WriteProvincesSetup` | Header line, then one line per city | UTF-8 **with** BOM (`StreamWriter` + `Encoding.UTF8`) |
| `out/<mod-id>/common/countries/<Id>.txt` | `WriteCountryFiles` | `color = rgb { r g b }`, blank line, `ship_names = { … }` | UTF-8 with BOM |
| `out/<mod-id>/common/countries.txt` | `WriteCountriesDefinitionIndexFile` | Four fixed entries then one line per country | UTF-8 with BOM |
| `out/<mod-id>/localization/SW_countries_l_english.yml` | `WriteCountriesLocalisationFile` | `l_english:` then ` Id:0 "Name"` for non-vanilla countries | UTF-8 with BOM |
| `out/<mod-id>/common/setup.txt` | `WriteSetupFile` | `country = { countries = { … } }` | Plain |
| `out/<mod-id>/events/flavor_sel.txt` | `WriteEventsFile` | A `grinsel.2` province event | UTF-8 with BOM |
| `out/<mod-id>/common/defines/00_defines.txt` | `WriteDefinesFile` | `NGame = { START_DATE = "001.10.1" }` | UTF-8 with BOM |

### Notes on individual writers

- **`WriteProvincesSetup`** creates a `CsvRepository<CityEntity>` over the target path and also calls `repo.Add(city.ToDataObject())` for each city, then writes the file content manually with a hand-formatted row. The repository instance is not used for output — the string concatenation is authoritative. The resulting CSV row order is `Id, CultureId, ReligionId, TradeGoodId, Citizens, Freemen, Tribesmen, Slaves, Civilization, Barbarian, NameId, ProvinceId`, whereas the header names the columns `…;Citizens;Freedmen;Slaves;Tribesmen;…`. Because the repository writes directly to the same path, the file may be created by `Add` before `WriteUnicodeFile` overwrites it; on a fresh output directory the repository path does not exist until `Add` writes it.
- **`WriteCountryFiles`** writes `ship_names` from `country.ShipNames`, which is the six-name default for every generated country.
- **`WriteCountriesDefinitionIndexFile`** always begins with `REB`, `PIR`, `BAR`, and `MER` pointing at the vanilla files, then appends every country in `GetCountries()` order.
- **`WriteCountriesLocalisationFile`** filters `!c.IsVanilla`, so only generated countries are localised; vanilla names already exist in the base game.
- **`WriteSetupFile`** writes, per country: a comment with the name; for vanilla countries two compatibility comments; `government`; `diplomatic_stance` only when non-blank; `primary_culture`; `religion`; `centralization` only when `>= 0`; `capital`; and `own_control_core = { <capital> }`.
- **`WriteEventsFile`** emits `create_pop = slaves` five times using `NuciExtensions.Repeat`, tied to a `num_of_cities = 1` and `is_capital = yes` trigger.
- **`WriteDefinesFile`** moves the start date to `001.10.1`.

### Newline normalisation

`NormaliseFileContent` ensures the content starts and ends with `Environment.NewLine`. `WriteFile` applies it and then `File.WriteAllText`. `WriteUnicodeFile` computes the normalised content but writes the **original** `content` variable to the stream — the normalisation is discarded for BOM files. In practice the writers already end their content with a newline, and the tests assert trailing newlines for descriptor and `setup.txt` (both plain) while asserting only the BOM for the UTF-8 files.

## Data

| Direction | Data |
|-----------|------|
| In | `entityManager.GetCities()`, `entityManager.GetCountries()`, `settings.ModName`, `settings.ModId`, `settings.ModDirectoryPath`, `settings.ModFilePath` |
| Out | Nine files across two locations |

## State

No retained state; each call rewrites everything from scratch.

## Dependencies

`NuciDAL.Repositories` (`CsvRepository<CityEntity>`), `NuciExtensions.Repeat`, `System.IO`, `System.Text.Encoding`, `CityMapping`.

## Dependants

`ShatteredWorldGenerator` only.

## Configuration

Uses the derived paths and the mod name. Nothing else is configurable: file names, the event body, the fixed index entries, and the start date are hard-coded.

## Failures

| Condition | Result |
|-----------|--------|
| Output directory not writable | `UnauthorizedAccessException` after the previous output was deleted |
| `GetCities` throws (loader failure) | Exception during the first write; partial directory exists |
| A city or country property is `null` | `null` is interpolated as an empty string, producing an empty CSV field or an empty localisation name |
| Ship names empty | `ship_names = {` followed by a blank line and `}` |

## Invariants

- The output directory and descriptor are deleted first, so stale files never survive a run.
- `descriptor.mod` and `<mod-id>.mod` are byte-identical.
- Every country appears in `countries.txt` and in `setup.txt`; only non-vanilla countries appear in the localisation file.
- `centralization` is omitted exactly when the level is negative, matching the `Country` sentinel of `-1`.
- The literal `VANILLA COUNTRY - REQUIRED FOR COMPATIBILITY` comment is written for every vanilla country, and `ModWriterTests` asserts it.
- Files carrying non-ASCII content are emitted with a UTF-8 BOM, which `ModWriterTests` verifies byte-wise.

## Tests

`ImperatorShatteredWorldGenerator.IntegrationTests/Service/ModWriterTests.cs` renders into a temporary working directory and asserts: the directory structure, both descriptor files and their exact content, discarding previous output, the province CSV header and row for a single city, the header-only case, the country file, the definition index, localisation inclusion and exclusion for generated and vanilla countries, all `setup.txt` fields, the stance and centralisation omissions, the vanilla compatibility comment, the empty-country closing blocks, the event file and its five slave pops, the defines file, trailing newlines for plain files, and the BOM on every Unicode file.

## Integration

`ModWriter` is where in-memory state becomes a game artefact. Any change to the field selection here is immediately visible in the game and is the most expensive class of change to validate, because no test can run the game itself.

## Modification impact

- Changing a path or file name requires a matching change in the loader only if the loader also reads that file (it does not).
- Adding a country attribute requires updating `WriteSetupFile` and possibly `WriteCountryFiles`.
- Fixing the `WriteUnicodeFile` normalisation would change the trailing-newline behaviour of BOM files and would require new assertions.
- Fixing the CSV column order changes the generated `province_setup.csv`, which is read positionally by the game.