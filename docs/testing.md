# Testing

## Organisation

| Project | Purpose | Path |
|---------|---------|------|
| `ImperatorShatteredWorldGenerator.UnitTests` | Verify configuration, models, and services in isolation with Moq and deterministic `Random` doubles | `ImperatorShatteredWorldGenerator.UnitTests/` |
| `ImperatorShatteredWorldGenerator.IntegrationTests` | Verify file-system readers and the writer against temporary directories | `ImperatorShatteredWorldGenerator.IntegrationTests/` |

Both use NUnit (`[TestFixture]`, `[Test]`, `[TestCase]`), Moq for interfaces, and disable implicit usings and enable nullable reference types. Both set `ExcludeFromCodeCoverage`.

## Fixtures

| Fixture | Tests (attributes) | Covers |
|---------|--------------------|--------|
| `Configuration/GeneratorSettingsTests` | 13 | Seed range, unset mandatory values, all defaults, six `ModId` cases, both derived paths |
| `DataAccess/DataObjects/CityEntityTests` | 4 | Defaults and round-trip retention |
| `Service/Models/CityTests` | 12 | `TotalPopulation` summation, `IsHabitable` for five attributes × four blank forms |
| `Service/Models/CountryTests` | 7 | Defaults: ship names, sentinel, colour, identity |
| `Service/RandomNumberGeneratorTests` | 9 | Seeding, inclusive bounds, equal ranges, sequence equality and difference |
| `Service/EntityManagerTests` | 21 | Out-of-order mutations, retrieval, single-load caching, identifier lists, updates and additions, empty sets |
| `Service/CityGeneratorTests` | 12 | Capital alignment, population replacement and early return, distribution, ranges, culture/religion, level bounds |
| `Service/CountryGeneratorTests` | 15 | Identity, eight name-derivation cases, five fall-back cases, collisions, random selections, ranges, defaults |
| `Service/ShatteredWorldGeneratorTests` | 13 | Stage outputs, filter rules, both pool failures, ordering |
| `Service/EntitiesLoaderTests` (integration) | 20 | City CSV mapping and failures, culture filtering, directory scans, government/stance/religion extraction, country identity, colour, centralisation, multi-country |
| `Service/ModWriterTests` (integration) | 17 | Directory structure, descriptors, discard behaviour, every file's content, omissions, BOM, trailing newlines |

Approximately 124 unit-test attribute occurrences and 49 integration-test attribute occurrences; `[TestCase]` rows expand the executed count well beyond those numbers.

## Test doubles

| Double | Project | Behaviour |
|--------|---------|-----------|
| `StubRandom` | UnitTests | Every draw returns `nextValue % maximum` (or `minimum + nextValue % range`) |
| `SequenceRandom` | UnitTests | Cycles a supplied integer array, wrapping at the end; falls back to `0` when empty |
| `TemporaryDirectory` | IntegrationTests | Creates a GUID-named temp directory and exposes `GetPath`, `WriteFile`, `ReadFile`, `FileExists`, `DirectoryExists`, and recursive disposal |
| `WorkingDirectoryScope` | IntegrationTests | Sets `Environment.CurrentDirectory` in the constructor and restores it on dispose, so `GeneratorSettings`'s derived paths resolve into the temp directory |

`WorkingDirectoryScope` mutates process-wide state, so integration tests must not run in parallel inside the same process while a scope is active. No `[Parallelizable]` attribute is present.

## Coverage gaps

| Area | Status |
|------|--------|
| `Program` argument parsing and normalisation | Untested; includes the `-d/-n/-s` alias mapping and `--opt=value` splitting |
| `CityMapping` | Only indirectly through loader and writer tests |
| `CountryGenerator.GenerateCountryId`'s unused `normalisedCapitalName` | Not observable |
| Real vanilla file formats | Fixtures are synthetic; a changed game layout is not detected |
| `package.sh` | Untested |
| Actual game loading of the produced mod | Impossible to automate here |

## Running

```bash
dotnet test --no-build
dotnet test --filter "FullyQualifiedName~CityGeneratorTests"
```

## Adding tests

- Unit tests: mock `IEntityManager`, `ICityGenerator`, `ICountryGenerator`, `IModWriter`, and `IRandomNumberGenerator`; when a test needs scripted randomness, set `rng.SetupGet(g => g.Randomiser).Returns(new SequenceRandom(…))`.
- Integration tests: create a `TemporaryDirectory`, populate the required `game/...` files, construct `EntitiesLoader` with `GameDirectoryPath`, and dispose the directory in `[TearDown]`.
- Writer tests additionally require a `WorkingDirectoryScope` around the temporary directory because output paths derive from the working directory.
- Follow the `Given…_When…_Then…` naming convention and prefer shared canonical fixture values.