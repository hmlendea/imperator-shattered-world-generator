# Entities Loader

## Purpose

`EntitiesLoader` reads the vanilla Imperator: Rome files and converts them into domain models. It owns every path, regular expression, and parsing rule; nothing else in the application reads the game directory.

## Location

`ImperatorShatteredWorldGenerator/Service/EntitiesLoader.cs` — a `public sealed class` implementing `IEntityLoader`, constructed with `GeneratorSettings`.

## Entry

Six methods, all invoked through `IEntityManager` on first cache access:

| Method | Vanilla source |
|--------|----------------|
| `LoadCities()` | `game/common/province_setup.csv` via `CsvRepository<CityEntity>` |
| `LoadCountries()` | `game/common/setup.txt`, `game/localization/english/countries_l_english.yml`, `game/common/countries.txt`, and each referenced country file |
| `LoadReligionIds()` | `game/common/religions/00_default.txt` |
| `LoadCultureIds()` | every `*.txt` under `game/common/cultures` |
| `LoadGovernmentIds()` | every `*.txt` under `game/common/governments` |
| `LoadDiplomaticStanceIds()` | every `*.txt` under `game/common/diplomatic_stances` |

All paths are rooted at `settings.GameDirectoryPath`.

## Execution

### Cities

`new CsvRepository<CityEntity>(citiesFilePath).GetAll().ToServiceModels()`. The repository maps columns positionally onto `CityEntity`, which is why the entity declares `Id` first and the level fields as strings. A file that lacks the expected column count raises `SerializationException` (asserted by `EntitiesLoaderTests`). A missing file yields an empty sequence rather than throwing.

### Countries

`LoadCountries` performs a line scan of `setup.txt`:
1. Find a line matching `capital\s*=\s*([0-9]*)` at three-tab indentation. Each such line declares the end of one country block.
2. Walk **backwards** from that line until each attribute is found, filling only fields that are still blank:
   - `^\t\t([A-Z]*)\s*=` → `Id` (walking backwards stops at the first match, i.e. the block header)
   - `^\t\t\tgovernment\s*=\s*(.*)` → `GovernmentId`
   - `^\t\t\tdiplomatic_stance\s*=\s*(.*)` → `DiplomaticStanceId`
   - `^\t\t\tprimary_culture\s*=\s*([^ #]*)` → `CultureId`
   - `^\t\t\treligion\s*=\s*(.*)` → `ReligionId`
   - `^\t\t\tcentralization\s*=\s*([0-9]*)` → `CentralisationLevel`, only when the previous value is `0` and the capture is non-blank
   - `capital` → `CapitalId`
3. Mark `IsVanilla = true`.
4. Resolve the display name by scanning `countries_l_english.yml` for `^\s*<Id>:.*\s"([^"]*)"` and taking the first capture.
5. Resolve the colour by scanning `countries.txt` for `^\s*<Id>\s*=\s*"([^"]*)"`, reading that file, and matching `rgb\s*{\s*(\d*)\s*(\d*)\s*(\d*)\s*}`. If no index line matches, the colour stays `0 0 0`.

Consequence of the backward walk: attributes are read from the lines *above* the `capital` line only. Because `centralization` must appear above `capital` in the vanilla layout, and because the guard is `CentralisationLevel == 0`, a country whose declared centralisation is literally `0` may be re-parsed from an earlier value or left at the model default. The integration test `GivenAVanillaCountryWithACentralisationValue_WhenLoadingCountries_ThenTheCentralisationLevelIsLeftUnset` records that a value of `42` in the fixture does **not** reach the model, leaving `-1`. See [Ambiguities and Open Questions](../ambiguities-and-open-questions.md#centralization-is-never-loaded-from-vanilla-files).

### Identifier collections

Each identifier loader walks the relevant directory, reads only `*.txt`, and applies one regular expression per file:

| Collection | Pattern | Semantics |
|------------|---------|-----------|
| Religions | `^([^#\s]*)\s*=\s*{` | Any block header on a line |
| Cultures | `^\t\t([^\s]*)\s*=\s*{\s*}` | Only *empty* blocks at exactly two tabs, i.e. the culture group entries with no body |
| Governments | `^([^#\s]*)\s*=\s*{` | Any block header |
| Diplomatic stances | `^([^#\s]*)\s*=\s*{` | Any block header |

The culture pattern deliberately excludes non-empty blocks; `EntitiesLoaderTests` verifies that `non_empty = { color = ... }` is skipped while `municipal = {}` and `britannic = {} #deprecated` are included.

## Data

| Direction | Data |
|-----------|------|
| In | Files under `settings.GameDirectoryPath` |
| Out | `City`, `Country`, and `IEnumerable<string>` identifier collections |

## State

Stateless and idempotent per call: each call re-reads the file system. Caching is the responsibility of `EntityManager`. Because `LoadCountries` re-reads the localisation and index files once per country, its cost is `O(countries × lines)` — noticeable on a full vanilla dataset.

## Dependencies

`NuciDAL.Repositories` (`CsvRepository<CityEntity>`), `System.IO`, `System.Text.RegularExpressions`, `GeneratorSettings`, `CityMapping`.

## Dependants

Only `EntityManager`. The loader is never called directly by a generator or by the writer.

## Configuration

Depends on `GameDirectoryPath` only. Directory layout is hard-coded to the vanilla structure.

## Failures

| Condition | Exception |
|-----------|-----------|
| Game root missing | `DirectoryNotFoundException` (from `File.ReadAllLines` or `DirectoryInfo.GetFiles`) |
| Religions directory present but `00_default.txt` absent | `FileNotFoundException` |
| `setup.txt` absent | `FileNotFoundException` |
| Cultures, governments, or diplomatic-stances directory absent | `DirectoryNotFoundException` |
| `province_setup.csv` absent | Empty city sequence, no exception |
| `province_setup.csv` malformed | `SerializationException` |
| Country file referenced by `countries.txt` absent | `FileNotFoundException` |
| `rgb { … }` block absent from a referenced country file | `FormatException` from parsing an empty capture, or an `ArgumentOutOfRangeException` on the match groups |

## Invariants

- Returned collections are domain models, never data objects; mapping happens inside the loader.
- `IsVanilla` is always `true` for loaded countries.
- `CentralisationLevel` remains at the model default (`-1`) for loaded countries in practice.
- Culture loading returns only empty-block identifiers, because those are the culture groups the game exposes for assignment.

## Tests

`ImperatorShatteredWorldGenerator.IntegrationTests/Service/EntitiesLoaderTests.cs` covers city mapping for every column, multi-row files, comment skipping, malformed CSV, every missing-file and missing-directory case, culture filtering, multi-file directory scanning, non-text file exclusion, government and stance block extraction, religion extraction, country identity and localised name, country colour, the unset centralisation behaviour, multiple countries, a missing localised name, and a country absent from the index.

## Integration

The loader is the sole reader of external input, so any vanilla file-format change regresses here first. A changed vanilla layout silently produces partial data (blank identifiers) rather than failing, which then propagates into empty culture or religion assignment.

## Modification impact

- Adding a parsed country field requires editing the backward-scan block, `Country`, and possibly `ModWriter`.
- Changing a directory or file name affects only this class, but must be cross-checked against a real installation.
- Adding a new identifier collection requires a new interface member, a new `EntityManager` cache, and a new caller.
- Any change to the backward-scan logic must be validated against a real vanilla `setup.txt` because the fixture is synthetic.