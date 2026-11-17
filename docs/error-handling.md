# Error Handling

## Posture

There is no error-handling layer. No production method contains `try`, `catch`, `finally`, or a retry. Failures propagate to `Program.Main`, terminate the process with a non-zero exit code, and print a stack trace to standard error. The completion banner is not printed when a failure occurs.

The rationale is recorded in [Design Decisions](./design-decisions.md#d12--no-error-handling-layer): a failed run has no meaningful partial artefact, because output is prepared and written in one pass and a partial mod could mislead the game.

## Exception taxonomy by origin

| Exception | Thrown by | Typical cause |
|-----------|-----------|---------------|
| `ArgumentException` | `Program.GetRequiredArgumentValue` | `--dir` or `--name` absent or blank |
| `FormatException` | `Program.GetIntegerArgumentValue` (`int.Parse`) | Non-numeric value for a numeric option |
| `FormatException` | `CityMapping.ToServiceModel` (`int.Parse`) | Non-numeric `Civilization`/`Barbarian` column in `province_setup.csv`, or an empty `rgb` capture in a country file |
| `SerializationException` | `CsvRepository<CityEntity>` | Malformed CSV row |
| `FileNotFoundException` | `File.ReadAllLines`/`ReadAllText`/`new CsvRepository` | Missing `setup.txt`, `countries.txt`, a referenced country file, or `religions/00_default.txt` |
| `DirectoryNotFoundException` | `DirectoryInfo.GetFiles`/`File.ReadAllLines` | Missing game root, `cultures`, `governments`, or `diplomatic_stances` |
| `NullReferenceException` | `EntityManager.UpdateCity`/`AddCountry` | Mutation before any read |
| `NullReferenceException` | `CityGenerator`, `CountryGenerator` | Unknown city identifier producing `null` from `GetCity` |
| `NullReferenceException` | `GeneratorSettings.ModId` | `ModName` unset |
| `InvalidOperationException` | `NuciExtensions.GetRandomElement` | Empty culture, religion, government, stance, or candidate-city collection |
| `ArgumentException` | `Dictionary.Add` via `AddCountry` | Duplicate country identifier |
| `ArgumentException` | `Dictionary` construction via `ToDictionary` | Duplicate city or country identifier in the source data |
| `ArgumentOutOfRangeException` | `Random.Next` via `rng.Get` | `min > max`, or `max == int.MaxValue` overflow |
| `UnauthorizedAccessException` | `Directory.CreateDirectory`, `File.Write*` | Output path not writable, or the directory is read-only |

## Failure matrix by stage

| Stage | Failure | Disk state |
|-------|---------|------------|
| Argument parsing | Missing mandatory, non-numeric | Untouched |
| City loading | Missing or malformed CSV | Untouched |
| City randomisation | Empty culture/religion, degenerate range | Untouched |
| Country generation | Pool exhausted, empty government/stance list | Untouched (writer not reached) |
| Capital regeneration | Unknown capital identifier | Untouched |
| Directory preparation | Unwritable output | Previous output already deleted |
| File writing | Disk full, permissions | Partial mod present |

## Deliberate non-failures

| Situation | Behaviour | Rationale |
|-----------|-----------|-----------|
| Missing `province_setup.csv` | Empty city collection, no exception | Treated as an empty world; the writer emits a header-only CSV |
| No `rgb` block in a country file | Colour stays `0 0 0` when the index line is absent; if the index line exists but the block is missing, `int.Parse` on an empty capture throws `FormatException` | Follows `Regex` group values verbatim |
| No localised country name | `Name` stays `null`; `setup.txt` shows an empty comment | Vanilla-only concern; the game reads localisation itself |
| Vanilla `centralization` present | Level stays `-1`, so the line is omitted from the generated `setup.txt` | Current behaviour; see ambiguities |
| Unknown city or country identifier requested | `null` from `EntityManager` | Lets callers detect absence without exceptions |

## Recovery

| Failure | Recovery |
|---------|----------|
| Any pre-write failure | Correct the input or arguments and re-run; nothing to clean up |
| Write failure | Delete `out/<mod-id>/` and `out/<mod-id>.mod`, then re-run |
| Exhausted country pool | Reduce `--random-countries` or accept more candidate cities |
| Unwritable output | Run from a writable working directory |

## Diagnostics available

Only the stack trace. There are no logs, no error counters, and no deep context such as the seed when a mid-generation failure occurs — except that the seed banner is printed *before* generation, so the seed is recoverable from the console even when the run fails. See [Logging](./logging.md).

## Recommendations for future changes

If error handling is added, preserve these properties:
- Do not write a partial mod; prepare output in a temporary directory and move it into place.
- Do not swallow exceptions from the vanilla parse; a silently empty entity set produces a valid-looking but meaningless mod.
- Keep the seed banner before generation so a failed run remains reproducible.