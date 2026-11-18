# Startup and Generation

## Trigger

Process start with a command line.

## Sequence

| Step | Method | Effect |
|------|--------|--------|
| 1 | `Program.Main` | Entry |
| 2 | `Program.LoadSettings` → `NormaliseShortArguments` | `-d/-n/-s` become `--dir/--name/--seed`; `--opt=value` splits into two tokens |
| 3 | `Program.RegisterArguments` (13 calls) | Registers every option name with `NuciCLI.ArgumentParser` |
| 4 | `parser.ParseArgs`, `GetRequiredArgumentValue`, `GetIntegerArgumentValue` | Populates `GeneratorSettings`; throws on missing mandatory values or unparseable integers |
| 5 | `ServiceCollection` construction | Eight registrations; `GeneratorSettings` is a singleton |
| 6 | `GetService<IShatteredWorldGenerator>()` | Resolves the graph; all services are singletons except `IRandomNumberGenerator` |
| 7 | Console banner | Prints the effective seed |
| 8 | `ShatteredWorldGenerator.Generate` | Four stages |
| 9 | Console banner | Prints `Done!` |

## Stage 1 — cities

`GetCityIds()` triggers `EntitiesLoader.LoadCities()` and caches the dictionary. Every identifier is then regenerated and re-stored. See [City Randomisation](./city-randomisation-flow.md).

## Stage 2 — countries

Skipped entirely when `RandomCountriesCount == 0`. Otherwise the first `GetCities()`/`GetCountries()` of this stage triggers `LoadCountries()`. Candidate filtering, drawing, generation, and registration follow. See [Country Generation](./country-generation-flow.md).

## Stage 3 — capitals

`GetCountries()` now returns vanilla and generated countries alike, so both are rewritten. See [Capital Regeneration](./capital-regeneration-flow.md).

## Stage 4 — mod writing

`CreateMod()` deletes any previous output and writes nine files. See [Mod Emission](./mod-emission-flow.md).

## Failure points in order

| Failure | Stage | Result |
|---------|-------|--------|
| Missing mandatory argument | 4 | `ArgumentException`; nothing loaded, nothing written |
| Non-numeric integer argument | 4 | `FormatException` |
| Game path invalid | 8.1 | `DirectoryNotFoundException` or `FileNotFoundException` |
| Empty culture or religion list | 8.1 | `InvalidOperationException` from `GetRandomElement` |
| Country pool exhausted | 8.2 | `InvalidOperationException`; the in-memory world is partially built but nothing is written |
| Output path unwritable | 8.4 | `UnauthorizedAccessException` after the previous output was deleted |

## Where to set breakpoints

| Goal | Breakpoint |
|------|------------|
| Inspect settings | `Program.LoadSettings` return |
| Inspect the seed actually used | `ShatteredWorldGenerator.Generate` entry |
| Inspect loaded vanilla data | `EntityManager.GetCities` after the `LoadCities` assignment |
| Inspect a single city draw | `CityGenerator.GenerateCity` |
| Inspect pool composition | `ShatteredWorldGenerator.GenerateCountries` after the LINQ chain |
| Inspect a country draw | `CountryGenerator.GenerateCountry` |
| Inspect output content | `ModWriter.WriteCountrySetupFile` before `WriteUnicodeFile` |