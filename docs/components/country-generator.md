# Country Generator

## Purpose

`CountryGenerator` creates a new country from a vacant capital city: it derives the identifier and name, inherits culture and religion, and draws the government, diplomatic stance, centralisation level, and colour.

## Location

`ImperatorShatteredWorldGenerator/Service/CountryGenerator.cs` — a `public sealed class` implementing `ICountryGenerator`.

## Entry

`GenerateCountry(string capitalCityId)`.

## Execution

1. `capital = entityManager.GetCity(capitalCityId)`.
2. `country.Id = GenerateCountryId(capital.NameId)`.
3. `country.Name = capital.NameId` — the city's name *identifier*, not a localised name.
4. `country.CultureId = capital.CultureId`, `country.ReligionId = capital.ReligionId`.
5. `country.GovernmentId = entityManager.GetGovernmentIds().GetRandomElement(rng.Randomiser)`.
6. `country.DiplomaticStanceId = entityManager.GetDiplomaticStanceIds().GetRandomElement(rng.Randomiser)`.
7. `country.CentralisationLevel = rng.Get(min, max)` from the country centralisation range.
8. `country.CapitalId = capital.Id`.
9. Three draws from `rng.Get(0, 255)` for red, green, and blue.
10. Ship names and `IsVanilla = false` remain at the `Country` constructor defaults.

### Identifier derivation

`GenerateCountryId(name)`:
1. Compute a normalised name: `RemovePunctuation()`, remove spaces, upper-case. (A first computation assigns to `normalisedCapitalName`, which is then unused.)
2. Try `GenerateCapitalIdBasedOnName(name)`:
   - Normalise again.
   - Return `null` when the normalised name is shorter than three characters.
   - Enumerate every ordered triple of character positions `i < j < k` and return the first combination that passes `IsCapitalIdValid`.
3. On `null`, fall back to `GenerateRandomCapitalId()`, which loops until `IsCapitalIdValid` accepts an identifier built from three random characters drawn from `ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789` using `rng.Randomiser.Next(length)` directly (not `rng.Get`).

`IsCapitalIdValid(id)` requires:
- non-blank,
- not one of `REB`, `PIR`, `BAR`, `MER`,
- `entityManager.GetCountry(id) is null` — that is, not already used,
- every character allowed (which also excludes digits produced by punctuation removal from a name, unless the digit is a permitted character).

Because the triple enumeration is ordered, the derived identifier is deterministic for a given name and current country set: `Cornova` yields `COR`, and if `COR` is taken it yields `CON`. `CountryGeneratorTests` pins several cases including `Çupișan` → `UPI`, `C+ornova` → `COR`, `Rome2` → `ROM`, and `Mercia` → `MEC`.

## Data

| Direction | Data |
|-----------|------|
| In | Capital city, government and stance identifier lists, centralisation bounds |
| Out | A new `Country` instance; no mutation of the capital |

## State

No state of its own; the identifier uniqueness check reads through `IEntityManager`, which the orchestrator keeps up to date with `AddCountry`.

## Dependencies

`IEntityManager`, `IRandomNumberGenerator`, `GeneratorSettings`, `NuciExtensions` (`RemovePunctuation`, `GetRandomElement`, `All`).

## Dependants

`ShatteredWorldGenerator.GenerateCountries` only.

## Configuration

| Setting | Effect |
|---------|--------|
| `CountryCentralisationLevelMin`, `CountryCentralisationLevelMax` | Range for the centralisation draw |
| — | The colour range is hard-coded to `0..255` |

## Failures

| Condition | Result |
|-----------|--------|
| Capital identifier unknown | `NullReferenceException` on `capital.NameId` |
| Government or stance list empty | `InvalidOperationException` from `GetRandomElement` |
| Random identifier search cannot terminate (all 36³ tags used) | Infinite loop; the reserved-tag check cannot exhaust a 46 656-entry space in practice, but it is unbounded |
| Duplicate identifier reaching `AddCountry` | `ArgumentException` from `Dictionary.Add`; prevented by `IsCapitalIdValid` |

## Invariants

- Reserved identifiers are never produced: `REB`, `PIR`, `BAR`, `MER`.
- An identifier already present in the manager is never produced.
- The country's name is the capital's `NameId` and the localisation file writes it verbatim, so two countries whose capitals share a name cannot both be created (the orchestrator filters those cities out first).
- Culture and religion are always copied from the capital *after* the city stage, so they reflect the drawn values.
- The identifier is exactly three characters.

## Tests

`ImperatorShatteredWorldGenerator.UnitTests/Service/CountryGeneratorTests.cs` covers identity and location, eight name-to-identifier cases, five fall-back cases (including the reserved `REB`, `MER`, an empty name, and whitespace), the next-combination behaviour on collision, random fall-back on collision, random government and stance selection, centralisation range, colour range, and the default ship names and vanilla flag.

## Integration

The generator does not register the country; registration is the orchestrator's responsibility, which keeps the "generate" and "store" responsibilities separate and lets the unit tests observe generated countries without side effects.

## Modification impact

- Changing the identifier algorithm changes the `countries.txt` index, every country file name, and the localisation lines simultaneously; all three are derived from `Country.Id`.
- Adding a reserved identifier must be mirrored in the writer's fixed `countries.txt` header if the tag needs a file, and in `invariants.md`.
- Adding an `rng` draw reorders the stream and invalidates seed-based expectations.