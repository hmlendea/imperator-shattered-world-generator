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

## `centralization` is never loaded from vanilla files

`EntitiesLoader.LoadCountries` walks *backwards* from the `capital` line and stops at the first match of each pattern. The centralisation guard is `country.CentralisationLevel == 0` with an additional non-blank check, and the integration test asserts that a `centralization = 42` line yields a model value of `-1`. In other words, vanilla centralisation is discarded and the writer consequently omits the `centralization` line for vanilla countries.

The test name (`…ThenTheCentralisationLevelIsLeftUnset`) documents the behaviour as intended, but the loader contains the code to read the value, so the intent is ambiguous. Options: parse it correctly (changes output), or remove the dead parsing (clarifies intent).

## CSV header order disagrees with row order

`WriteProvincesSetup` writes the header `…;Citizens;Freedmen;Slaves;Tribesmen;…` and each row as `Citizens, Freemen, Tribesmen, Slaves`. The row order matches `CityEntity` and therefore the repository, so the file is self-consistent for a positional reader but contradicts its own header. Either the header or the row order should change; the row order is what the loader expects back.

## `ToServiceModel` and `ToDataObject` assign `TribesmenCount` twice

Both mapping methods set `TribesmenCount` twice in a row. The second assignment is redundant but harmless because the value is identical. It is a candidate for a small cleanup with no behavioural impact.

## UTF-8 normalisation is discarded

`WriteUnicodeFile` computes `NormaliseFileContent(content)` but writes the original `content`. The resulting files therefore depend on each writer producing its own leading/trailing newline. The tests only assert trailing newlines for the plain-text files, so the discrepancy is not currently caught. Either assign the normalised value or remove the unused local.

## Unused local in `GenerateCountryId`

`GenerateCountryId` computes `normalisedCapitalName` and never uses it; the actual work happens in `GenerateCapitalIdBasedOnName`. Dead code only.

## `CsvRepository.Add` misuse in `ModWriter`

`WriteProvincesSetup` creates a repository over the target file and calls `Add` for each city, then writes the file content manually. The `Add` calls may create or truncate the file before the manual write. On a fresh directory the effect is invisible; on an existing file it interacts with the earlier delete. The repository is effectively redundant and could be removed.

## Editor tasks point at stale paths

`.vscode/tasks.json` builds `${workspaceFolder}/ImperatorShatteredWorldGenerator.csproj` and `.vscode/launch.json` expects `bin/Debug/net10.0/ImperatorShatteredWorldGenerator.dll` at the workspace root. Both predate the solution restructure that moved the project into `ImperatorShatteredWorldGenerator/`. The tasks therefore fail as written.

## Country count ceiling is empirical

The README states that more than roughly 1,900 countries could not be generated without the game crashing. No code enforces a limit and no test records the basis. The figure should either be traced to a reproducible experiment or softened.

## Government and stance extraction is over-broad

`LoadGovernmentIds`, `LoadDiplomaticStanceIds`, and `LoadReligionIds` accept **any** block header at column zero, including nested blocks whose names begin at that indentation in some vanilla files. The fixtures only exercise top-level blocks. A wrong identifier would surface as an invalid `government =` value in `setup.txt`, which the game may reject without a clear error.

## Culture extraction depends on exact indentation

`LoadCultureIds` requires exactly two tabs and an empty body (`{}`). A vanilla file that uses spaces or a body with a modifier would yield no cultures for that file, and the failure is silent until `GetRandomElement` throws on an empty list.

## No validation that `min <= max`

No option pair is validated. A user who passes `--city-population-min 12 --city-population-max 4` receives an `ArgumentOutOfRangeException` from deep inside generation rather than a clear configuration error.

## Where documentation intentionally stops

- The number of countries the game accepts is a game-engine property, not a repository property.
- `System.Random`'s exact sequence is an implementation detail of the runtime, so reproducibility is stated per runtime version rather than absolutely.
- The repository contains no HTTP surface, so no API reference or API usage examples exist.