# Ambiguities and Open Questions

Discrepancies between code, tests, documentation, and intent, recorded so that future work does not rediscover them.

## Civilisation minimum is ignored

`CityGenerator.GenerateCity` draws the civilisation level with:

```csharp
city.CivilizationLevel = rng.Get(settings.CityBarbarianLevelMin, settings.CityCivilisationLevelMax);
```

`CityCivilisationLevelMin` is parsed, defaulted, and asserted by `GeneratorSettingsTests`, but never read. The lower bound is therefore the *barbarian* minimum. The README documents `--city-civilisation-min` as "Minimum civilisation level of cities", which the code does not honour.

Resolution options: use `CityCivilisationLevelMin` (changes every generated world), or remove the option (breaking). The choice must be recorded in the README and in [Configuration](./configuration.md).

## The random stream is not actually shared

The root `ARCHITECTURE.md` states that "a single seeded `System.Random` instance is shared across all generators through `IRandomNumberGenerator`". In practice `IRandomNumberGenerator` is registered `AddTransient`, so `CityGenerator`, `CountryGenerator`, and `ShatteredWorldGenerator` each receive an instance that constructs its own `Random(seed)`.

Consequences are benign (determinism holds, stages are isolated) but the documented mental model is wrong, and reasoning about "the next draw" across components will mislead. Either the documentation or the registration should change; changing the registration alters every generated world.

## `centralization` parsing tolerates both value forms

`EntitiesLoader.LoadCountries` reads the `centralization` attribute from the country block and applies it when it parses as an integer. Vanilla centralisation is therefore loaded and re-emitted, rather than discarded. The `Country` default of `-1` still marks an unset value, so a country whose block omits the attribute keeps the sentinel and the writer omits the line.

## Province definition CSV has ragged field counts

`LoadProvinceNames` reads `game/map_data/definition.csv` through `NuciDAL.IO.CsvFile<T>`, which requires every row to have exactly as many fields as the entity declares. The vanilla file has 23, 24, or 25 fields depending on the row, so the loader first writes a temporary normalised copy with the leading `id;r;g;b;name` columns of each non-comment row, reads that, and deletes it. This is correct but allocates a temporary file per run; a streaming row reader would avoid it.

## Culture extraction depends on exact indentation

## Unused local in `GenerateCountryId`

`GenerateCountryId` computes `normalisedCapitalName` and never uses it; the actual work happens in `GenerateCapitalIdBasedOnName`. Dead code only.

`LoadCultureIds` locates the top-level `culture = { }` block in each group file and collects its two-tab entries. A vanilla file whose group block is named differently, or that nests the subcultures at a different indentation, would yield no cultures for that file, and the failure is silent until `GetRandomElement` throws on an empty list.

## Editor tasks point at stale paths

`.vscode/tasks.json` builds `${workspaceFolder}/ImperatorShatteredWorldGenerator.csproj` and `.vscode/launch.json` expects `bin/Debug/net10.0/ImperatorShatteredWorldGenerator.dll` at the workspace root. Both predate the solution restructure that moved the project into `ImperatorShatteredWorldGenerator/`. The tasks therefore fail as written.

## Country count ceiling is empirical

The README states that more than roughly 1,900 countries could not be generated without the game crashing. No code enforces a limit and no test records the basis. The figure should either be traced to a reproducible experiment or softened.

## Government and stance extraction is over-broad

`LoadGovernmentIds`, `LoadDiplomaticStanceIds`, and `LoadReligionIds` accept **any** block header at column zero, including nested blocks whose names begin at that indentation in some vanilla files. The fixtures only exercise top-level blocks. A wrong identifier would surface as an invalid `government =` value in the setup file, which the game may reject without a clear error.

## Province definition CSV has ragged field counts

`LoadProvinceNames` reads `game/map_data/definition.csv` through `NuciDAL.IO.CsvFile<T>`, which requires every row to have exactly as many fields as the entity declares. The vanilla file has 23, 24, or 25 fields depending on the row, so the loader first writes a temporary normalised copy with the leading `id;r;g;b;name` columns of each non-comment row, reads that, and deletes it. This is correct but allocates a temporary file per run; a streaming row reader would avoid it.

## Non-habitable provinces are left unchanged

Only provinces with a non-blank `province_rank`, `culture`, `religion`, and `trade_goods`, plus a name in the definition CSV, are rewritten. Impassable and unnamed provinces keep their vanilla values because the mod's province file merges with the vanilla files per province id. This relies on the game merging rather than replacing province entries; a game version that replaced them would wipe the untouched provinces.

## No validation that `min <= max`

No option pair is validated. A user who passes `--city-population-min 12 --city-population-max 4` receives an `ArgumentOutOfRangeException` from deep inside generation rather than a clear configuration error.

## Where documentation intentionally stops

- The number of countries the game accepts is a game-engine property, not a repository property.
- `System.Random`'s exact sequence is an implementation detail of the runtime, so reproducibility is stated per runtime version rather than absolutely.
- The repository contains no HTTP surface, so no API reference or API usage examples exist.