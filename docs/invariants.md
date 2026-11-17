# Invariants

Constraints that must remain true unless a change is explicitly intended to alter them. Each entry states the invariant, the enforcing code, and the verification.

## Game compatibility

| Invariant | Enforced by | Verified by |
|-----------|-------------|-------------|
| Every vanilla country is present in `setup.txt` | `ShatteredWorldGenerator` never removes; `ModWriter.WriteSetupFile` writes all countries | `ModWriterTests.GivenAVanillaCountry_WhenCreatingTheMod_ThenACompatibilityNoticeIsWritten` |
| Vanilla countries are restricted to their capital | `own_control_core = { <capital> }` written for every country | `ModWriterTests.GivenAGeneratedCountry_WhenCreatingTheMod_ThenTheSetupContainsItsDefinition` |
| Reserved tags `REB`, `PIR`, `BAR`, `MER` are never generated | `CountryGenerator.DisallowedCountryIds` | `CountryGeneratorTests` (two reserved-name cases) |
| `countries.txt` always lists the four reserved entries first | `WriteCountriesDefinitionIndexFile` | `ModWriterTests.GivenCountries_WhenCreatingTheMod_ThenTheDefinitionIndexListsTheDefaultEntriesAndEveryCountry` |
| Files with non-ASCII content carry a UTF-8 BOM | `WriteUnicodeFile` | `ModWriterTests.GivenACreatingModRequest_WhenCreatingTheMod_ThenEveryUnicodeFileStartsWithAByteOrderMark` |
| The `province_setup.csv` header is emitted verbatim | `WriteProvincesSetup` | `ModWriterTests` header assertions |

## Output identity

| Invariant | Enforced by | Verified by |
|-----------|-------------|-------------|
| `ModId` is lower-case, punctuation-free, diacritic-free, hyphenated | `GeneratorSettings.ModId` | `GeneratorSettingsTests` (six cases) |
| Output lives at `out/<ModId>/` relative to the working directory | `GeneratorSettings.ModDirectoryPath` | `GeneratorSettingsTests`, `ModWriterTests` |
| Descriptor path is `<ModDirectoryPath>.mod` | `GeneratorSettings.ModFilePath` | `GeneratorSettingsTests` |
| `descriptor.mod` and `<ModId>.mod` are byte-identical | Both written from one `fileContent` | `ModWriterTests` |
| Previous output for the same mod is fully discarded | `PrepareDirectoryStructure` | `ModWriterTests.GivenAnExistingModDirectory_…` |

## Generation semantics

| Invariant | Enforced by | Verified by |
|-----------|-------------|-------------|
| A city's final `TotalPopulation` equals the drawn amount | `SetCityPopulation` loop | `CityGeneratorTests` totals and the 64-iteration range check |
| A capital's culture and religion equal its country's | `GenerateCapital` overwrite | `CityGeneratorTests.GivenACapital_…` |
| Population replacement discards prior distributions | Zeroing before the loop | `CityGeneratorTests.GivenAnExistingPopulation_…` and the early-return test |
| Civilisation level lies within `[CityBarbarianLevelMin, CityCivilisationLevelMax]` | `GenerateCity` | `CityGeneratorTests` bounds test |
| Barbarian level lies within `[CityBarbarianLevelMin, CityBarbarianLevelMax]` | `GenerateCity` | same |
| No generated country uses an existing or reserved tag | `IsCapitalIdValid` | `CountryGeneratorTests` collision cases |
| No capital city hosts two countries | Pool removal in `GenerateCountries` | `ShatteredWorldGeneratorTests.GivenACityUsedAsAVanillaCapital_…` |
| No two countries share a name | Name filter plus `GroupBy(NameId)` | `ShatteredWorldGeneratorTests.GivenCitiesSharingAName_…` |
| Countries are created only from habitable cities | `IsHabitable` filter | `ShatteredWorldGeneratorTests.GivenAnInhabitableCity_…` |
| Stage order is cities → countries → capitals → write | `Generate` | `ShatteredWorldGeneratorTests` ordering test |

## Configuration

| Invariant | Enforced by | Verified by |
|-----------|-------------|-------------|
| `--dir` and `--name` are mandatory | `GetRequiredArgumentValue` | Not covered by tests |
| Documented defaults are the constructor values | `GeneratorSettings` constructor | `GeneratorSettingsTests` (three default tests) |
| `CentralisationLevel == -1` means "unset" | `Country` constructor and `WriteSetupFile` guard | `CountryTests`, `ModWriterTests` (two `TestCase(-1/-42)` cases) |
| `RandomCountriesCount == 0` produces no generated countries | Early return in `GenerateCountries` | `ShatteredWorldGeneratorTests` |

## Determinism

| Invariant | Enforced by | Verified by |
|-----------|-------------|-------------|
| A fixed seed reproduces a run | Seeded `Random` per consumer | `RandomNumberGeneratorTests` seed and sequence tests |
| `Get(min, max)` is inclusive at both ends | `Randomiser.Next(min, max + 1)` | `RandomNumberGeneratorTests` reachability test |
| `Get(x, x)` does not advance the stream | Early return | `RandomNumberGeneratorTests` equal-bounds tests |

## Cache semantics

| Invariant | Enforced by | Verified by |
|-----------|-------------|-------------|
| Each collection is loaded at most once | Null-check guard | `EntityManagerTests` `Times.Once` assertions |
| Repeated identifier reads return the same instance | `List<string>` caching | `EntityManagerTests` `Is.SameAs` assertions |
| Unknown identifiers return `null`, not an exception | Explicit `ContainsKey` check | `EntityManagerTests` missing-identifier tests |

## Structural

| Invariant | Enforced by |
|-----------|-------------|
| No network access anywhere in the production code | Absence of any networking package or call |
| No file outside the working-directory `out/` tree is written | Only `ModWriter` writes, and only under `settings.ModDirectoryPath` and `settings.ModFilePath` |
| The game installation is never modified | Only reads under `settings.GameDirectoryPath` |
| Production code never references a test project | Project references point from tests to production only |