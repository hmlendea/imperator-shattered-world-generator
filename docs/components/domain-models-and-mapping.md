# Domain Models and Mapping

## Purpose

The models hold the in-memory state that generation mutates and that the writer serialises. The mapping class is the only translator between the CSV data object and the domain model.

## Location

| Type | Path |
|------|------|
| `City` | `ImperatorShatteredWorldGenerator/Service/Models/City.cs` |
| `Country` | `ImperatorShatteredWorldGenerator/Service/Models/Country.cs` |
| `CityEntity` | `ImperatorShatteredWorldGenerator/DataAccess/DataObjects/CityEntity.cs` |
| `CityMapping` | `ImperatorShatteredWorldGenerator/Service/Mapping/CityMapping.cs` |

Field-by-field detail is in [Data Model](../data-model.md); this page covers behaviour and boundaries.

## Execution

All three types are passive data carriers. The only executable members are:
- `City.TotalPopulation` — computed sum, evaluated on every read.
- `City.IsHabitable` — five `IsNullOrWhiteSpace` checks joined with `&&`, evaluated on every read (the orchestrator calls it once per city per filter pass).
- `Country`'s constructor — assigns a fresh six-element `ShipNames` list and `CentralisationLevel = -1`.

`CityMapping` provides three internal extension methods, reachable only inside the production assembly (it is a non-public `static class` with `internal` members) and used by `EntitiesLoader` (data object → model) and `ModWriter` (model → data object).

## Data

| Type | Storage | Notes |
|------|---------|-------|
| `City` | Memory only | Shared by reference between manager, generators, and writer |
| `Country` | Memory only | Only cities have a persisted data object; countries are written by direct string formatting |
| `CityEntity` | CSV row | Level fields are strings, mapped through `int.Parse`/`ToString` |

## State

`City` and `Country` are mutable and are mutated in place. Because `EntityManager` stores and returns the same references, a generator that mutates a city mutates the stored instance.

## Dependencies

`City` and `Country` depend on nothing but `System.Collections.Generic`. `CityEntity` depends on `NuciDAL.DataObjects.EntityBase`. `CityMapping` depends on both model namespaces.

## Dependants

`EntityManager`, `EntitiesLoader`, `CityGenerator`, `CountryGenerator`, `ModWriter`, `ShatteredWorldGenerator`, and every test fixture.

## Configuration

None.

## Failures

| Condition | Result |
|-----------|--------|
| `CityEntity.CivilizationLevel` or `BarbarianLevel` not numeric | `FormatException` in `ToServiceModel` |
| `City` fields `null` during `ToDataObject` | No failure; `null` becomes an empty CSV field for strings, and `ToString()` is safe for `int` |
| `Country.ShipNames` set to `null` externally | `WriteCountryFiles` throws `NullReferenceException` through `string.Join` |

## Invariants

- `City.Id` and `Country.Id` are the dictionary keys; both must be non-null and unique for the manager's dictionary construction to succeed.
- `City.IsHabitable` is purely string-based; a city with zero population is still habitable.
- `Country.CentralisationLevel` uses `-1` as the "unset" sentinel, which the writer uses to decide whether to emit the line.
- `Country.ShipNames` is never `null` after construction.
- Mapping preserves every field in both directions, except that the two level fields change representation.

## Tests

| Fixture | Coverage |
|---------|----------|
| `Service/Models/CityTests.cs` | `TotalPopulation` summation across four cases; `IsHabitable` true for a complete city; false for each of the five attributes when `null`, empty, blank, or whitespace |
| `Service/Models/CountryTests.cs` | Default ship names and their identity, six names, per-instance collection, `CentralisationLevel == -1`, `IsVanilla == false`, zero colour, all identifiers unset |
| `DataAccess/DataObjects/CityEntityTests.cs` | Default nulls and zeros, and round-trip retention of every assigned value |

Mapping itself is covered indirectly: the loader tests assert the mapped model, and the writer tests assert the serialised row.

## Integration

Because mapping is internal and confined to the production assembly, an external consumer (a test project) cannot call it; both directions are therefore only verified through the loader and writer integration tests.

## Modification impact

- Adding a city attribute requires `CityEntity`, `CityMapping` in both directions, `ModWriter.WriteProvincesSetup`, and possibly the CSV column order.
- Adding a country attribute requires only `Country` and `ModWriter`, because countries have no data object.
- Removing `IsHabitable` checks would allow cities without culture or name into country generation, producing countries with blank identity and duplicate localisation keys.