# Orchestration

## Purpose

`ShatteredWorldGenerator` owns the pipeline: it decides the stage order, performs the vacancy filtering for new countries, and delegates all substance to the generators and the writer.

## Location

`ImperatorShatteredWorldGenerator/Service/ShatteredWorldGenerator.cs` — a `public sealed class` implementing `IShatteredWorldGenerator`.

## Entry

`Generate()`.

## Execution

```text
Generate()
├── GenerateCities()
├── GenerateCountries()
├── GenerateCapitals()
└── modWriter.CreateMod()
```

### `GenerateCities`

For each `id` in `entityManager.GetCityIds()`: `city = cityGenerator.GenerateCity(id)` then `entityManager.UpdateCity(city)`.

The identifier list is materialised with `ToList()` inside `GetCityIds`, so iterating a snapshot while `UpdateCity` mutates the dictionary is safe.

### `GenerateCountries`

Returns immediately when `settings.RandomCountriesCount == 0`.

Otherwise it builds the candidate pool:

```text
cities
  .Where(city => city.IsHabitable
                 && existing countries contain no country whose CapitalId == city.Id
                 && existing countries contain no country whose Name == city.NameId)
  .GroupBy(city => city.NameId)
  .Select(group => group.ToList().GetRandomElement(rng.Randomiser))
  .Select(city => city.Id)
```

Then, `RandomCountriesCount` times: draw a random identifier from the pool with `GetRandomElement`, call `countryGenerator.GenerateCountry(id)`, `entityManager.AddCountry(country)`, and remove the identifier from the pool.

Three filters are therefore applied:
- `IsHabitable` — a city lacking culture, religion, trade good, name, or province is never a candidate.
- Not already a capital — because vanilla countries are loaded first, their capital cities are excluded.
- Not named after an existing country — prevents two countries with the same localisation key.

The `GroupBy(NameId)` step collapses synonyms (cities sharing a name) to one representative per name, chosen at random from the group, so the localisation file cannot contain duplicate keys.

### `GenerateCapitals`

For each country in `entityManager.GetCountries()`: `capital = cityGenerator.GenerateCapital(country)` then `entityManager.UpdateCity(capital)`. Because new countries were added to the same dictionary, their capitals are regenerated too.

## Data

| Direction | Data |
|-----------|------|
| In | `IEntityManager`, `ICityGenerator`, `ICountryGenerator`, `IModWriter`, `IRandomNumberGenerator`, `GeneratorSettings` |
| Out | Mutated entity caches and a written mod |

## State

No state of its own; effects are confined to the manager and the file system.

## Dependencies

All five collaborators plus settings and `NuciExtensions.GetRandomElement`.

## Dependants

`Program` only.

## Configuration

`RandomCountriesCount` is the only setting read here; all others reach the generators through their own injections.

## Failures

| Condition | Result |
|-----------|--------|
| `RandomCountriesCount` greater than the candidate pool | `InvalidOperationException` from `GetRandomElement` once the pool is empty. Countries drawn before the failure remain in the manager, so a partially built world exists in memory but nothing is written |
| No vacant cities and a non-zero count | The pool is empty at the first draw, so the same `InvalidOperationException` |
| `RandomCountriesCount` negative | Loop body never runs; zero countries |
| Any collaborator throws | Propagates; the writer is not reached, so no directory is created or deleted |

Both failure modes are asserted by `ShatteredWorldGeneratorTests` (`GivenNoVacantCities_…` and `GivenMoreCountriesThanVacantCities_…`).

## Invariants

- Stage order is fixed and asserted by `GivenAWorldGeneration_WhenGenerating_ThenCitiesCountriesCapitalsAndTheModAreProducedInOrder`, which records the sequence `cities`, `countries`, `capitals`, `mod`.
- Every vanilla country is preserved; the orchestrator never removes one.
- Vanilla capitals participate in the capital stage, so their culture and religion are rewritten to match their country.
- No city is used as the capital of two generated countries, because a drawn identifier is removed from the pool.
- Country generation reads the manager after each addition, so collision checks in `CountryGenerator` see the accumulating set.

## Tests

`ImperatorShatteredWorldGenerator.UnitTests/Service/ShatteredWorldGeneratorTests.cs` covers city regeneration, the empty-city case, the zero-country setting, the configured country count, name de-duplication, exclusion of vanilla capitals, exclusion by country name, exclusion of uninhabitable cities, both pool-exhaustion failures, capital regeneration, and the stage order.

## Integration

The orchestrator is the only place where the three generation concerns meet. Its vacancy filter is the sole enforcement of the "one country per capital and per name" rule.

## Modification impact

- Reordering the stages breaks the capital/culture agreement and must not be done casually.
- Changing the filter changes which cities become countries and therefore the entire world composition.
- Adding a stage requires a new collaborator and a new registration in `Program`.