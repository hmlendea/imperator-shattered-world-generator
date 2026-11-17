# Troubleshooting

## Argument problems

| Symptom | Cause | Remedy |
|---------|-------|--------|
| `Missing required argument: --dir` | Game path absent or blank | Pass `--dir`, `-d`, `--game`, or `--imperator` |
| `Missing required argument: --name` | Mod name absent or blank | Pass `--name` or `-n` |
| `FormatException` at startup | A numeric option received a non-numeric value | Check every integer option |
| `-d` or `-n` ignored | The aliases do not exist for that option | Use the long form; only `-d`, `-n`, and `-s` exist |
| Value containing spaces truncated | The value was not quoted | Quote it: `--name "Shattered World"` |

## Loading problems

| Symptom | Cause | Remedy |
|---------|-------|--------|
| `DirectoryNotFoundException` mentioning `game/setup` or `game/common` | `--dir` points at the wrong level | Point at the directory that directly contains `game/` |
| `FileNotFoundException` for `religions/00_default.txt` | Non-vanilla or heavily modded installation, or an unexpected game version | Verify the file exists |
| `FileNotFoundException` for `setup/main/00_default.txt` or `setup/countries/countries.txt` | Unexpected game version or incomplete installation | Verify the files exist |
| Empty world (no cities) | `map_data/definition.csv` absent, or `setup/provinces` absent | Verify the installation |
| `InvalidOperationException` about an empty sequence | No cultures or religions were loaded | Verify `game/common/cultures/**` and `religions/00_default.txt` |
| Reload after a game patch produces odd values | Parsing is format-based | Inspect the loader logic against the new file layout |

## Generation problems

| Symptom | Cause | Remedy |
|---------|-------|--------|
| `InvalidOperationException` during country generation | Requested more countries than there are vacant, uniquely named, habitable cities | Reduce `--random-countries` |
| `ArgumentOutOfRangeException` | A minimum exceeds its maximum | Fix the range; nothing validates this for you |
| No new countries at all | `--random-countries 0` | Omit the option (default `500`) or pass a positive value |
| Cities all have barbarian level `0` | Default `CityBarbarianLevelMin`/`Max` are both `0` | Pass `--city-barbarian-max` |
| Civilisation levels below the configured minimum | `CityCivilisationLevelMin` is not used | See [Ambiguities](./ambiguities-and-open-questions.md#civilisation-minimum-is-ignored) |

## Output problems

| Symptom | Cause | Remedy |
|---------|-------|--------|
| `out/` created in an unexpected place | Paths derive from the working directory | Run the binary from the intended directory |
| Previous output missing | `ModWriter` deletes and recreates on every run | Copy `out/<mod-id>/` before re-running |
| `UnauthorizedAccessException` | Output directory not writable | Run from a writable directory |
| Partial mod present | The writer failed mid-way | Delete `out/<mod-id>` and `out/<mod-id>.mod`, then re-run |
| No BOM on descriptor files | Only `WriteUnicodeFile` emits a BOM; the descriptors do not | Expected behaviour |

## In-game problems

| Symptom | Cause | Remedy |
|---------|-------|--------|
| Mod not listed | The descriptor is not in the game's mod directory, or `path` does not match the folder | Place `out/<mod-id>.mod` and `out/<mod-id>/` per the game's mod layout |
| Game crashes at startup | Too many countries, or a malformed generated file | Reduce `--random-countries`; inspect `setup/main/02_shattered_world_countries.txt` and `setup/countries/countries.txt` |
| Missing country names | Localisation only covers generated countries | Expected: vanilla names come from the base game |
| Duplicate or blank names | A city with a blank `NameId` entered the pool | Should be prevented by `IsHabitable`; report a defect with the seed |
| Country tags rejected after years | Vanilla countries were removed from a hand-edited setup | Do not remove vanilla countries; the generator never does |

## Reproduction

Always capture the printed seed and the full command line. With both, plus the same game version, any defect is reproducible.

## Diagnosing without logs

The application emits two lines. To narrow a failure:
1. Run with `--random-countries 0` to isolate loading and the city stage.
2. Inspect `out/<mod-id>/setup/provinces/02_shattered_world.txt` to confirm the city stage produced data.
3. Inspect `out/<mod-id>/setup/main/02_shattered_world_countries.txt` to confirm every country is present.
4. Set breakpoints using the table in [Startup and Generation](./flows/startup-and-generation.md#where-to-set-breakpoints).