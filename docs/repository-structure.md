# Repository Structure

## Solution

`ImperatorShatteredWorldGenerator.slnx` (the XML solution format) contains three projects:

| Project | Path | Role |
|---------|------|------|
| Production | `ImperatorShatteredWorldGenerator/ImperatorShatteredWorldGenerator.csproj` | Console executable; `net10.0` |
| Unit tests | `ImperatorShatteredWorldGenerator.UnitTests/ImperatorShatteredWorldGenerator.UnitTests.csproj` | Isolated verification with Moq and deterministic random doubles |
| Integration tests | `ImperatorShatteredWorldGenerator.IntegrationTests/ImperatorShatteredWorldGenerator.IntegrationTests.csproj` | File-system verification against temporary directories |

Both test projects reference the production project and set `ExcludeFromCodeCoverage`, `ImplicitUsings=disable`, `Nullable=enable`, and `LangVersion=latest`.

## Production Project Layout

```text
ImperatorShatteredWorldGenerator/
├── Program.cs                         # composition root and argument parsing
├── Configuration/
│   └── GeneratorSettings.cs           # settings, defaults, derived paths
├── DataAccess/
│   └── DataObjects/
│       └── CityEntity.cs              # CSV row shape (NuciDAL EntityBase)
└── Service/
    ├── IEntitiesLoader.cs  IEntityManager.cs  ICityGenerator.cs
    ├── ICountryGenerator.cs  IModWriter.cs  IRandomNumberGenerator.cs
    ├── IShatteredWorldGenerator.cs
    ├── EntitiesLoader.cs              # vanilla file parsing
    ├── EntityManager.cs               # lazy in-memory caches
    ├── CityGenerator.cs               # city randomisation
    ├── CountryGenerator.cs            # country generation and identifier allocation
    ├── ModWriter.cs                   # mod directory and file serialisation
    ├── RandomNumberGenerator.cs       # seeded System.Random wrapper
    ├── ShatteredWorldGenerator.cs     # stage orchestration
    ├── Mapping/
    │   └── CityMapping.cs             # CityEntity <-> City translation
    └── Models/
        ├── City.cs                    # domain model with derived members
        └── Country.cs                 # domain model with defaults
```

Every service is `sealed`, and every collaborator is consumed through an `I`-prefixed interface located beside its implementation.

## Test Project Layout

```text
ImperatorShatteredWorldGenerator.UnitTests/
├── SequenceRandom.cs  StubRandom.cs     # deterministic Random subclasses
├── Configuration/GeneratorSettingsTests.cs
├── DataAccess/DataObjects/CityEntityTests.cs
├── Service/
│   ├── CityGeneratorTests.cs  CountryGeneratorTests.cs
│   ├── EntityManagerTests.cs  RandomNumberGeneratorTests.cs
│   ├── ShatteredWorldGeneratorTests.cs
│   └── Models/{CityTests.cs, CountryTests.cs}

ImperatorShatteredWorldGenerator.IntegrationTests/
├── TemporaryDirectory.cs                # throw-away directory with relative-path helpers
├── WorkingDirectoryScope.cs             # sets and restores Environment.CurrentDirectory
└── Service/{EntitiesLoaderTests.cs, ModWriterTests.cs}
```

## Repository Root

| Path | Purpose |
|------|---------|
| `README.md` | Capabilities, usage, configuration, installation, development, documentation index |
| `ARCHITECTURE.md` | System context, components, processes, contracts, extension points |
| `CONTRIBUTING.md` | Contribution workflow, prerequisites, testing, documentation expectations |
| `CODE_OF_CONDUCT.md` | Conduct standards |
| `SECURITY.md` | Supported versions and vulnerability reporting |
| `PRIVACY.md` | Personal-data handling |
| `LICENSE` | Licence terms |
| `package.sh` | Release publication and packaging script |
| `.github/workflows/dotnet.yml` | Continuous integration |
| `.github/FUNDING.yml` | Funding metadata |
| `.vscode/{tasks.json,launch.json}` | Editor build, publish, watch, and debug definitions |
| `.gitignore` | Ignores build output and `out/*` (the generated mod) |

## Naming Conventions Observed

- Interfaces: `I` prefix, declared next to the implementation (`Service/IEntityManager.cs`).
- Namespaces mirror folders (`ImperatorShatteredWorldGenerator.Service.Mapping`).
- Mapping is expressed as internal extension methods in a `static class` (`CityMapping`).
- Test names follow `Given…_When…_Then…`, with shared arrangements extracted into `SetUp` and private builders.

See [Components](./components/README.md) for the behavioural description of each unit.