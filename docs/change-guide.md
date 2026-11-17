# Change Guide

Recipes for common modifications. Each entry names the files to change, the tests to update, and the documents to synchronise.

## Add or rename a command-line option

1. `Program.cs` — add a `string[]` field, register it in `LoadSettings`, and assign it from `GetIntegerArgumentValue` or `GetRequiredArgumentValue`.
2. `Configuration/GeneratorSettings.cs` — add the property and, when it has a default, set it in the constructor.
3. `ImperatorShatteredWorldGenerator.UnitTests/Configuration/GeneratorSettingsTests.cs` — assert the default.
4. Consume the value where it is needed (`CityGenerator`, `CountryGenerator`, `ShatteredWorldGenerator`, `ModWriter`).
5. Synchronise [Configuration](./configuration.md) and the README option table.
6. If the option renames an existing one, treat it as a breaking change for scripts and document it.

## Add a city attribute

1. `Service/Models/City.cs` — add the property.
2. `DataAccess/DataObjects/CityEntity.cs` — add the matching property in the position the CSV column occupies.
3. `Service/Mapping/CityMapping.cs` — map in both directions.
4. `Service/EntitiesLoader.cs` — only if parsing is not the CSV repository path (it is).
5. `Service/ModWriter.cs` — add the column to the header and to the row string.
6. Update `CityEntityTests`, `CityTests` if it has behaviour, and the `ModWriterTests` expected row.
7. Synchronise [Data Model](./data-model.md) and [Invariants](./invariants.md).

Breaking risk: the CSV column order is positional for the game. Appending at the end is safer than inserting.

## Add a country attribute

1. `Service/Models/Country.cs` — add the property with a neutral default.
2. `Service/CountryGenerator.cs` — assign it in `GenerateCountry`, being explicit about draw order.
3. `Service/EntitiesLoader.cs` — parse it in the backward-scan block when vanilla values matter.
4. `Service/ModWriter.cs` — write it in `WriteSetupFile` or `WriteCountryFiles`.
5. Update `CountryTests`, `CountryGeneratorTests`, both integration fixtures, and any seed-sensitive expectation.
6. Synchronise [Data Model](./data-model.md).

## Change the randomisation rules

1. Modify `CityGenerator` or `CountryGenerator`.
2. Count the draws you add or remove: any change reorders that consumer's stream and alters every later value.
3. Update the deterministic expectations in the corresponding unit test.
4. Do not add a draw to a different consumer's stream expecting determinism to be unaffected; each consumer owns a separate `Random`.
5. Synchronise [Reproducibility](./behaviour/reproducibility.md) when the guarantee changes.

## Change the identifier algorithm

1. `Service/CountryGenerator.cs` — `GenerateCountryId`, `GenerateCapitalIdBasedOnName`, `GenerateRandomCapitalId`, `IsCapitalIdValid`.
2. `CountryGeneratorTests` — the eight derivation cases and five fall-back cases.
3. Check `ModWriter` because the tag appears in the file name, in `countries.txt`, and in the localisation key.
4. Synchronise [Country Creation](./behaviour/country-creation.md) and [Invariants](./invariants.md).

## Change the mod identifier or output paths

1. `Configuration/GeneratorSettings.cs` — `ModId`, `ModDirectoryPath`, `ModFilePath`.
2. `GeneratorSettingsTests` — the six `ModId` cases and both path assertions.
3. `ModWriterTests` — every expected path.
4. Synchronise [Configuration](./configuration.md), [Quick Start](./quick-start.md), and [Invariants](./invariants.md).

## Add or change a generated file

1. `Service/ModWriter.cs` — add a `Write…` method, call it from `CreateMod`, and choose the correct helper: `WriteFile` (UTF-8 without BOM) or `WriteUnicodeFile` (UTF-8 with BOM).
2. If a directory is required, add it to `PrepareDirectoryStructure`.
3. `ModWriterTests` — assert existence, content, encoding, and trailing newline.
4. Synchronise [Mod Emission](./flows/mod-emission-flow.md) and [Integrations](./integrations.md).

## Change parsing of a vanilla file

1. `Service/EntitiesLoader.cs` — the relevant `Load…` method.
2. Add a case to `EntitiesLoaderTests` that mirrors a realistic snippet of the vanilla file, including indentation characters, because every pattern is indentation-sensitive.
3. Validate against a real installation; the fixtures are synthetic and cannot detect a wrong indentation assumption.
4. Synchronise [Integrations](./integrations.md).

## Add a new service

1. Declare `IX.cs` in `Service/`.
2. Implement `X` as a `sealed class`.
3. Register in `Program.Main` with an explicit lifetime, and consider the determinism consequence of that lifetime.
4. Unit-test with Moq collaborators; add an integration test only if it touches the file system.
5. Update [Components](./components/README.md), [Architecture](./architecture.md), and [Dependencies](./dependencies.md).

## Adjust the dependency set

1. Edit the `.csproj` `PackageReference` entries.
2. Follow the `update-nuget-packages` skill: baseline the tests, update per project, validate.
3. Re-run `dotnet test` and, for a major upgrade, re-check `EntitiesLoaderTests` and `ModWriterTests` because they assert library-dependent behaviour (CSV ordering, extension semantics).
4. Synchronise [Dependencies](./dependencies.md) and the README dependency table.

## Change CI or packaging

1. `.github/workflows/dotnet.yml` — follow the canonical workflow template for the repository type.
2. `package.sh` — the preview trimming package is temporary and mutates the project file; keep the cleanup symmetrical.
3. Synchronise [Build and Deployment](./build-and-deployment.md) and the README GitHub Actions table.

## Safe-change checklist

- [ ] Identifying whether the change alters draw counts
- [ ] Updating every affected test in both test projects
- [ ] Running `dotnet build --no-restore` and `dotnet test --no-build`
- [ ] Checking whether a compatibility invariant is affected
- [ ] Updating this documentation tree and any affected README section
- [ ] Not committing generated `out/`, `bin/`, or `obj/` content