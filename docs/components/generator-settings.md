# Generator Settings

## Purpose

`GeneratorSettings` carries every tunable value, supplies the documented defaults, and derives the mod identifier and the two output paths.

## Location

`ImperatorShatteredWorldGenerator/Configuration/GeneratorSettings.cs` — a `public sealed class` with auto-properties, three derived getter-only properties, and a constructor that sets the defaults.

## Entry

Constructed once by `Program.LoadSettings` and registered as a singleton. Constructed directly by every test fixture that needs settings.

## Execution

### Defaults assigned in the constructor

| Property | Default |
|----------|---------|
| `Seed` | `new Random().Next()` — a fresh random seed per construction |
| `CapitalPopulation` | `10` |
| `CityPopulationMin` / `CityPopulationMax` | `4` / `12` |
| `CityCivilisationLevelMin` / `CityCivilisationLevelMax` | `0` / `20` |
| `CityBarbarianLevelMin` / `CityBarbarianLevelMax` | `0` / `0` |
| `CountryCentralisationLevelMin` / `CountryCentralisationLevelMax` | `0` / `100` |
| `RandomCountriesCount` | `500` |
| `GameDirectoryPath`, `ModName` | Not set (`null`); mandatory on the command line |

### Derived properties

| Property | Implementation |
|----------|----------------|
| `ModId` | `ModName.RemovePunctuation().RemoveDiacritics().Replace(" ", "-").ToLower()` — the two `Remove…` helpers come from `NuciExtensions` |
| `ModDirectoryPath` | `Path.Combine(Environment.CurrentDirectory, "out", ModId)` |
| `ModFilePath` | `ModDirectoryPath + ".mod"` |

`ModId` throws `NullReferenceException` when `ModName` is `null`, so any consumer touching it before the mandatory argument is applied would fail. The command line guarantees it is set first.

Worked examples (each asserted by `GeneratorSettingsTests`):

| Mod name | `ModId` |
|----------|---------|
| `Shattered World` | `shattered-world` |
| `NEW WORLD` | `new-world` |
| `Shattered  World` | `shattered--world` |
| `Çupișan's World!` | `cupisans-world` |
| `Țări Frumoase` | `tari-frumoase` |

## Data

Pure in-memory configuration; nothing is read from or written to disk.

## State

Mutable public setters exist on every scalar, so the object is mutable during argument loading and immutable by convention afterwards. No component writes to it after `LoadSettings` returns.

## Dependencies

`NuciExtensions` for `RemovePunctuation` and `RemoveDiacritics`; `System.IO.Path` and `System.Environment` for the derived paths.

## Dependants

`Program` (creation), `EntitiesLoader` (game path), `CityGenerator` (population and level bounds), `CountryGenerator` (centralisation bounds), `RandomNumberGenerator` (seed), `ModWriter` (name, directory, descriptor path, `ModId`), `ShatteredWorldGenerator` (`RandomCountriesCount`).

## Configuration

This class *is* the configuration model. See [Configuration](../configuration.md) for the command-line surface.

## Failures

| Condition | Result |
|-----------|--------|
| `ModName` unset when `ModId` is read | `NullReferenceException` |
| `CityPopulationMin > CityPopulationMax` | Deferred failure inside `Random.Next` when a city is generated |
| `CapitalPopulation` negative | `rng.Get` returns the negative value only if `min == max`; otherwise the population loop simply does not execute and all counts stay `0` |

## Invariants

- `ModId`, `ModDirectoryPath`, and `ModFilePath` are computed, never assigned, so they cannot drift out of sync with `ModName`.
- The default `Seed` is random per construction, so two `GeneratorSettings` instances never share a seed by accident; `RandomNumberGeneratorTests` relies on explicit seeds instead.
- `CityCivilisationLevelMin` is stored and defaulted but never consumed by generation. See [Ambiguities and Open Questions](../ambiguities-and-open-questions.md#civilisation-minimum-is-ignored).

## Tests

`ImperatorShatteredWorldGenerator.UnitTests/Configuration/GeneratorSettingsTests.cs` verifies the seed range, the unset mandatory values, every default, the six `ModId` transformations, and both derived paths.

## Integration

`ModId` is the join point between the configuration layer and the writer: `ModWriter.WriteModDescriptor` embeds `settings.ModId` in `path = "mod/<mod-id>"`, and `PrepareDirectoryStructure` creates that directory, so a change in the derivation affects both the folder and the descriptor target.

## Modification impact

- Renaming a property requires touching `Program`, the consuming service, `GeneratorSettingsTests`, and the README option table.
- Changing a default is a behavioural change for every invocation that omits the option; update the README, [Configuration](../configuration.md), and the corresponding default test.
- Adding a derived path requires checking `ModWriter` for hard-coded equivalents.