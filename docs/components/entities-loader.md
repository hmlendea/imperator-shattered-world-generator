# Entities Loader

## Purpose

`EntitiesLoader` reads the vanilla Imperator: Rome files and converts them into domain models. It owns every path, regular expression, and parsing rule; nothing else in the application reads the game directory. Clausewitz text files are parsed directly, while the semicolon-separated province definition CSV is read through `NuciDAL.IO.CsvFile<T>`.

## Location

`ImperatorShatteredWorldGenerator/Service/EntitiesLoader.cs` — a `public sealed class` implementing `IEntityLoader`, constructed with `GeneratorSettings`.

## Entry

Six methods, all invoked through `IEntityManager` on first cache access:

| Method | Vanilla source |
|--------|----------------|
| `LoadCities()` | every `*.txt` under `game/setup/provinces`, with names from `game/map_data/definition.csv` |
| `LoadCountries()` | `game/setup/main/*.txt`, `game/localization/english/countries_l_english.yml`, `game/setup/countries/countries.txt`, and each referenced country file |
| `LoadReligionIds()` | `game/common/religions/00_default.txt` |
| `LoadCultureIds()` | every `*.txt` under `game/common/cultures` |
| `LoadGovernmentIds()` | every `*.txt` under `game/common/governments` |
| `LoadDiplomaticStanceIds()` | every `*.txt` under `game/common/diplomatic_stances` |

All paths are rooted at `settings.GameDirectoryPath`.

## Execution

### Cities

`LoadCities` reads every `*.txt` under `game/setup/provinces` in ordinal file-name order. Each file is scanned for province headers matching `^\s*([0-9]+)\s*=`; the name of each province is looked up in `game/map_data/definition.csv`, which is read through `NuciDAL.IO.CsvFile<ProvinceDefinitionEntity>` with the `;` field separator. Because the vanilla CSV rows have ragged field counts (23–25), the loader first writes a temporary normalised copy containing the first five fields of each non-comment row, reads that, and deletes it.

A province block is converted to a `City` only when it is habitable: the definition CSV supplies a non-blank name and the block declares a non-blank `province_rank`, `culture`, `religion`, and `trade_goods`. Pop sub-blocks (`citizen`, `freemen`, `slaves`, `tribesmen`) may repeat; their `amount` values are summed into the matching counters. Fractional `civilization_value` values (for example `18.5`) are truncated to an `int`. Lines that the mod does not manage (for example `terrain`, `holy_site`, `port_building`) are captured as `PreservedLines` and re-emitted by `ModWriter`.

### Countries

`LoadCountries` reads the `countries = { }` block of `game/setup/main/00_default.txt`:
1. Locate the `country = {` block, then the nested `countries = {` block.
2. For every two-tab `<TAG> = {` entry, read `government`, `diplomatic_stance`, `primary_culture`, `religion`, `centralization`, and `capital` from the block body.
3. Ignore any block without a `capital` value, because the generator cannot place a country without a capital.
4. Resolve the display name from `countries_l_english.yml` and the colour from the tag's own file referenced by `game/setup/countries/countries.txt`.
5. Keep the remaining block lines, minus the managed attributes and their sub-blocks, as `PreservedLines`.

Attributes use both `key = value` and `key=value` forms in the vanilla data, so the attribute reader splits on the first `=` and compares the trimmed key rather than matching a fixed prefix.

### Identifier collections

Each identifier loader walks the relevant directory, reads only `*.txt`, and applies one regular expression per file:

| Collection | Pattern | Semantics |
|------------|---------|-----------|
| Religions | `^([^#\s]*)\s*=\s*{` | Any block header on a line |
| Cultures | the two-tab entries inside the top-level `culture = { }` block | The subculture identifiers the game exposes for assignment |
| Governments | `^([^#\s]*)\s*=\s*{` | Any block header |
| Diplomatic stances | `^([^#\s]*)\s*=\s*{` | Any block header |

The culture loader first locates the group file's `culture = {` block and then collects its two-tab entries, so it never picks up `male_names`, `family`, or `nickname` entries. `EntitiesLoaderTests` verifies that `roman`, `venetic`, and `etruscan` are returned and that a group's `male_names` entries are excluded.

## Data

| Direction | Data |
|-----------|------|
| In | Files under `settings.GameDirectoryPath` |
| Out | `City`, `Country`, and `IEnumerable<string>` identifier collections |

## State

Stateless and idempotent per call: each call re-reads the file system. Caching is the responsibility of `EntityManager`. Because `LoadCountries` re-reads the localisation and index files once per country, its cost is `O(countries × lines)` — noticeable on a full vanilla dataset.

## Dependencies

`NuciDAL.IO` (`CsvFile<ProvinceDefinitionEntity>`), `System.IO`, `System.Text.RegularExpressions`, `GeneratorSettings`, `DataAccess.Parsing.ClausewitzText`, `CityMapping`.

## Dependants

Only `EntityManager`. The loader is never called directly by a generator or by the writer.

## Configuration

Depends on `GameDirectoryPath` only. Directory layout is hard-coded to the vanilla structure.

## Failures

| Condition | Exception |
|-----------|-----------|
| Game root missing | `DirectoryNotFoundException` (from `File.ReadAllLines` or `DirectoryInfo.GetFiles`) |
| Religions directory present but `00_default.txt` absent | `FileNotFoundException` |
| `setup/main/00_default.txt` absent | `FileNotFoundException` |
| `setup/countries/countries.txt` absent | `FileNotFoundException` |
| Cultures, governments, or diplomatic-stances directory absent | `DirectoryNotFoundException` |
| `setup/provinces` directory absent | Empty city sequence, no exception |
| `map_data/definition.csv` absent | Empty province-name map, so no cities are loaded |
| Country file referenced by `countries.txt` absent | The tag is skipped; no exception |
| `rgb { ... }` block absent from a referenced country file | Colour stays `0 0 0`; no exception |

## Invariants

- Returned collections are domain models, never data objects; mapping happens inside the loader.
- `IsVanilla` is always `true` for loaded countries.
- A city is returned only when the definition CSV names it and the block declares `province_rank`, `culture`, `religion`, and `trade_goods`.
- Culture loading returns only the subculture identifiers declared inside a group's `culture = { }` block.

## Tests

`ImperatorShatteredWorldGenerator.IntegrationTests/Service/EntitiesLoaderTests.cs` covers city mapping for a habitable province, fractional civilisation values, repeated pop blocks, multi-province files, non-habitable and unnamed provinces, preserved non-randomised lines, every missing-file and missing-directory case, culture subculture filtering, multi-file directory scanning, government and stance block extraction, religion extraction, country identity, localised name, colour, centralisation, preserved lines, a country without a capital, and province names read from the definition CSV.

## Integration

The loader is the sole reader of external input, so any vanilla file-format change regresses here first. A changed vanilla layout silently produces partial data (blank identifiers) rather than failing, which then propagates into empty culture or religion assignment.

## Modification impact

- Adding a parsed country field requires editing the country block reader, `Country`, and possibly `ModWriter`.
- Changing a directory or file name affects only this class, but must be cross-checked against a real installation.
- Adding a new identifier collection requires a new interface member, a new `EntityManager` cache, and a new caller.
- Any change to the province or country readers must be validated against a real vanilla `setup/` tree because the fixtures are synthetic.