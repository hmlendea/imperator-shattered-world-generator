# Entity Manager

## Purpose

`EntityManager` is the in-memory entity store and the only component that triggers `IEntityLoader`. It hides the file system from the generators and the writer and provides the mutation entry points used during generation.

## Location

`ImperatorShatteredWorldGenerator/Service/EntityManager.cs` — a `public sealed class` implementing `IEntityManager`.

## Entry

| Member | Behaviour |
|--------|-----------|
| `GetCity(string id)` | Loads all cities on first use; returns the instance or `null` when the identifier is unknown |
| `GetCities()` | Returns `cities.Values` (insertion order of the source CSV) |
| `GetCityIds()` | `GetCities().Select(x => x.Id).ToList()` |
| `UpdateCity(City city)` | `cities[city.Id] = city` |
| `AddCountry(Country country)` | `countries.Add(country.Id, country)` |
| `GetCountry(string id)` | Loads all countries on first use; returns the instance or `null` |
| `GetCountries()` | Returns `countries.Values` |
| `GetCultureIds()`, `GetReligionIds()`, `GetGovernmentIds()`, `GetDiplomaticStanceIds()` | Loads and caches a `List<string>` per collection |

## Execution

Every read member uses the same lazy pattern:

```text
if (cache is null) cache = loader.Load…()
return cached value
```

`GetCities` and `GetCity` share `cities`; `GetCountries` and `GetCountry` share `countries`. The identifier collections each have their own field.

The two mutation members access the backing field directly:
- `UpdateCity` writes into `cities` without ensuring it is initialised.
- `AddCountry` calls `countries.Add` without ensuring it is initialised.

This is intentional in the pipeline, because the orchestrator always reads before writing: `GenerateCities` starts from `GetCityIds()` and `GenerateCountries` starts from `GetCities()`/`GetCountries()`. Called out of order, both throw `NullReferenceException`, which `EntityManagerTests` asserts explicitly (`GivenANewlyConstructedManager_WhenUpdatingACity_ThenTheUpdateFails` and the country equivalent).

`AddCountry` additionally throws `ArgumentException` if a country with the same identifier already exists, because `Dictionary.Add` rejects duplicates. The generator's identifier allocation is what prevents that.

## Data

| Direction | Data |
|-----------|------|
| In | Domain models supplied by the loader; mutated models supplied by the generators |
| Out | The same model instances, by reference |

Returning the live instance matters: `CityGenerator.GenerateCity` mutates the object returned by `GetCity` and then `UpdateCity` re-stores that same reference, so the write is effectively a no-op registration of an already-mutated object.

## State

| Field | Populated by | Lifetime |
|-------|--------------|----------|
| `cities` | `loader.LoadCities()` | Process |
| `countries` | `loader.LoadCountries()` | Process |
| `cultureIds`, `religionIds`, `governmentIds`, `diplomaticStanceIds` | respective loader methods | Process |

State is never invalidated, so a long-running process would observe stale vanilla data; irrelevant for a batch run.

## Dependencies

`IEntityLoader` only. `EntityManager` has no direct file-system knowledge.

## Dependants

`ShatteredWorldGenerator` (orchestration and vacancy filtering), `CityGenerator`, `CountryGenerator`, `ModWriter`.

## Configuration

None. Behaviour is determined entirely by the loader and by call order.

## Failures

| Condition | Result |
|-----------|--------|
| `UpdateCity` or `AddCountry` before any read | `NullReferenceException` |
| `AddCountry` with a duplicate identifier | `ArgumentException("An item with the same key has already been added…")` |
| Loader throws | Propagates unchanged; the cache remains `null` |
| Unknown city or country identifier | `null` (not an exception) |

## Invariants

- Each collection is loaded at most once per process (`EntityManagerTests` verifies `Times.Once` for every loader method).
- Repeated identifier-collection reads return the same instance (`Is.SameAs` assertions).
- The dictionaries are keyed by `City.Id` and `Country.Id`; an entity with a duplicated key makes `ToDictionary` throw `ArgumentException` while caching.
- `GetCities()` and `GetCountries()` expose live dictionary value collections; modifying them during enumeration would throw `InvalidOperationException`. `GenerateCountries` avoids that by building a separate list first and adding countries only through `AddCountry`, while iterating `entityManager.GetCountries()` inside the LINQ filter is a *nested* enumeration of the same collection — safe only because no mutation occurs during filtering.

## Tests

`ImperatorShatteredWorldGenerator.UnitTests/Service/EntityManagerTests.cs` covers the two out-of-order mutation failures, city and country retrieval including the missing-identifier case, single-load caching, identifier lists, cache-preserving updates and additions, and the empty-collection cases.

## Integration

`EntityManager` is the single point where stage state is shared. Every generator reads through it and the writer reads through it, so a cache that is not populated before writing produces an empty mod rather than an error.

## Modification impact

- Adding a cache requires a new field, a new interface member, and a loader method.
- Making the mutations initialise their caches would change documented behaviour and require updating `EntityManagerTests`.
- Changing the returned collection types affects enumeration semantics for `ShatteredWorldGenerator.GenerateCountries` and `ModWriter`.