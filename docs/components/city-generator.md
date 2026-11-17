# City Generator

## Purpose

`CityGenerator` applies the randomisation rules to a single city and to a country capital.

## Location

`ImperatorShatteredWorldGenerator/Service/CityGenerator.cs` — a `public sealed class` implementing `ICityGenerator`.

## Entry

| Member | Used by | Effect |
|--------|---------|--------|
| `GenerateCity(string id)` | `ShatteredWorldGenerator.GenerateCities` | Randomises population, culture, religion, civilisation level, and barbarian level on the stored city |
| `GenerateCapital(Country country)` | `ShatteredWorldGenerator.GenerateCapitals` | Calls `GenerateCity(country.CapitalId)`, then sets a fixed population and the country's culture and religion |

## Execution

`GenerateCity(id)`:
1. `city = entityManager.GetCity(id)`.
2. `SetCityPopulation(city, settings.CityPopulationMin, settings.CityPopulationMax)` → draws `rng.Get(min, max)` then delegates to the fixed-amount overload.
3. `city.CultureId = entityManager.GetCultureIds().GetRandomElement(rng.Randomiser)`.
4. `city.ReligionId = entityManager.GetReligionIds().GetRandomElement(rng.Randomiser)`.
5. `city.CivilizationLevel = rng.Get(settings.CityBarbarianLevelMin, settings.CityCivilisationLevelMax)`.
6. `city.BarbarianLevel = rng.Get(settings.CityBarbarianLevelMin, settings.CityBarbarianLevelMax)`.
7. Returns the same instance it received.

`GenerateCapital(country)`:
1. `city = GenerateCity(country.CapitalId)` — full randomisation first.
2. `SetCityPopulation(city, settings.CapitalPopulation)`.
3. `city.CultureId = country.CultureId`, `city.ReligionId = country.ReligionId`.
4. Returns the same instance.

`SetCityPopulation(city, amountMin, amountMax)` draws a single amount and delegates. `SetCityPopulation(city, amount)`:
1. Returns immediately when `city.TotalPopulation == amount`, preserving the existing distribution.
2. Otherwise zeroes all four counts.
3. Draws `rng.Get(0, 3)` once per population point and increments `Slaves` for `0`, `Tribesmen` for `1`, `Freemen` for `2`, and `Citizens` for `3`.

## Data

| Direction | Data |
|-----------|------|
| In | `City.Id`, `Country.CapitalId`, `Country.CultureId`, `Country.ReligionId`, culture and religion identifier lists, settings ranges |
| Out | Mutated `City` instance |

## State

No state of its own. All state lives in the city instance, the manager, the settings, and the shared random stream.

## Dependencies

`IEntityManager`, `IRandomNumberGenerator`, `GeneratorSettings`, `NuciExtensions.GetRandomElement`.

## Dependants

`ShatteredWorldGenerator` only. Note that `ShatteredWorldGenerator` also resolves `IRandomNumberGenerator` for its own country-selection draws.

## Configuration

| Setting | Effect |
|---------|--------|
| `CapitalPopulation` | Exact population of every country capital |
| `CityPopulationMin`, `CityPopulationMax` | Population range for non-capital cities |
| `CityCivilisationLevelMax` | Upper bound of the civilisation draw |
| `CityBarbarianLevelMin`, `CityBarbarianLevelMax` | Barbarian draw range |
| `CityCivilisationLevelMin` | **Never read**; the civilisation draw uses `CityBarbarianLevelMin` as its lower bound |

## Failures

| Condition | Result |
|-----------|--------|
| `GetCity` returns `null` (unknown identifier) | `NullReferenceException` on `city.TotalPopulation` |
| Culture or religion list empty | `GetRandomElement` throws `InvalidOperationException` (sequence contains no elements) |
| `CityPopulationMin > CityPopulationMax` | `ArgumentOutOfRangeException` from `Random.Next` |
| `CapitalPopulation < 0` | Loop body skipped; all counts remain `0` |

## Invariants

- `TotalPopulation` after generation equals the drawn amount exactly, because the loop runs exactly that many times and each iteration increments exactly one count. `CityGeneratorTests` verifies the totals and the per-class distribution for the sequence `0,1,2,3`.
- A city whose population already equals the target keeps its existing distribution (early return). This makes population idempotent when the draw repeats a value.
- Capital generation always ends with the owning country's culture and religion, regardless of what the city stage drew.
- `TradeGoodId`, `NameId`, `ProvinceId`, and `Id` are never modified.
- Determinism depends on draw order; inserting an extra draw changes every subsequent value.

## Tests

`ImperatorShatteredWorldGenerator.UnitTests/Service/CityGeneratorTests.cs` covers capital culture/religion application, capital population, the population early return, discarding previous populations, per-class distribution, the configured population range, culture and religion selection, level bounds, identifier lookup, and a 64-iteration bound check driven by a real `RandomNumberGenerator`.

## Integration

Capitals are generated twice per run: once as ordinary cities in the city stage and once again in the capital stage. The second pass is what makes the capital's culture and religion agree with its country; without it, a vanilla capital would keep a randomly drawn culture while its country declared a different one.

## Modification impact

- Fixing the civilisation lower bound changes every generated world for a given seed.
- Changing the pop-class distribution (for example weighting) changes both determinism and the expected values in `CityGeneratorTests`.
- Any extra `rng` call reorders the whole stream and must be accompanied by updated seed-based expectations elsewhere.