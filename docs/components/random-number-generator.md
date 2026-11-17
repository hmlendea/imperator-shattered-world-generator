# Random Number Generator

## Purpose

`RandomNumberGenerator` wraps a `System.Random` seeded from `GeneratorSettings.Seed` and exposes an inclusive integer range helper.

## Location

`ImperatorShatteredWorldGenerator/Service/RandomNumberGenerator.cs` — a `public sealed class` implementing `IRandomNumberGenerator`.

## Entry

| Member | Behaviour |
|--------|-----------|
| `Randomiser` (property) | The underlying `Random`, constructed with `settings.Seed` |
| `Get(int min, int max)` | Returns `max` when `min == max`; otherwise `Randomiser.Next(min, max + 1)` |

## Execution

Construction is the only state change: `Randomiser = new Random(settings.Seed)`. `Get` is otherwise pure over the current `Random` state.

## Data

| Direction | Data |
|-----------|------|
| In | `Seed` from settings; the two range bounds |
| Out | A pseudo-random integer within `[min, max]` |

## State

Mutable PRNG state inside the wrapped `Random`. Nothing else.

## Dependencies

`System.Random` and `GeneratorSettings`.

## Dependants

- `CityGenerator` — population, culture, religion, level draws, and the pop-class draws via the `Randomiser` property.
- `CountryGenerator` — government, stance, centralisation, colour, plus `Randomiser.Next(...)` for random identifier characters and `GetRandomElement` calls.
- `ShatteredWorldGenerator` — city selection via `GetRandomElement(rng.Randomiser)`.

## Configuration

`GeneratorSettings.Seed` only. When the seed is omitted, `GeneratorSettings` assigns `new Random().Next()`, so the printed seed is the only record of the run.

## Failures

| Condition | Result |
|-----------|--------|
| `min > max` with `min != max` | `ArgumentOutOfRangeException` from `Random.Next` |
| `max == int.MaxValue` | `max + 1` overflows to `int.MinValue`; `Random.Next` throws |
| Negative bounds where `min == max` | Returned unchanged, which may violate a documented range assumption in a caller |

## Invariants

- A fixed seed produces the same sequence for the same .NET runtime version.
- `Get` is inclusive at both ends when `min < max`; `RandomNumberGeneratorTests` verifies both bounds are reachable and that all values fall inside the range over 256 and 1024 draws.
- `Get(x, x)` never touches the random state, so a degenerate range does not desynchronise the stream.

## Instance semantics — important

`IRandomNumberGenerator` is registered with `AddTransient`, so **each consumer receives its own instance and therefore its own `Random` seeded with the same value**:

| Consumer | Own `Random` seeded from `settings.Seed` |
|----------|------------------------------------------|
| `CityGenerator` | Yes |
| `CountryGenerator` | Yes |
| `ShatteredWorldGenerator` | Yes |

The observable consequences:
- City draws and country draws come from independent streams, so changing the number of cities does not shift the first country's government.
- `ShatteredWorldGenerator`'s city-selection draws and `CountryGenerator`'s attribute draws are independent, so selection order and attribute assignment do not interfere.
- Determinism is preserved because every stream is re-seeded identically and each consumer's call order is stable.

This contradicts the root `ARCHITECTURE.md` statement that "a single seeded `System.Random` instance is shared across all generators". See [Ambiguities and Open Questions](../ambiguities-and-open-questions.md#the-random-stream-is-not-actually-shared).

## Tests

`ImperatorShatteredWorldGenerator.UnitTests/Service/RandomNumberGeneratorTests.cs` covers seeding equality across two instances, the equal-bounds short-circuit, bound containment across four ranges, reachability of both bounds, identical sequences for identical seeds, differing sequences for differing seeds, and the `4..5` inclusive case.

Two test doubles support the generator tests:
- `StubRandom` returns `nextValue % maximumValue` (or a modulo over the range), so a single constant drives every draw.
- `SequenceRandom` cycles a supplied array, so exact draw sequences can be scripted.

## Integration

Because each consumer owns a stream, the writer never draws randomness and the `ModWriter` output is a pure function of the generated entities.

## Modification impact

- Changing the registration to `AddSingleton` genuinely shares the stream, which changes every generated world for a given seed and must be accompanied by updated expectations.
- Replacing `System.Random` with a different algorithm preserves the interface but breaks reproducibility across versions.
- Adding a method to `IRandomNumberGenerator` requires updating both test doubles if they are used as implementations; they derive from `Random` instead, so only `Randomiser` overrides matter.