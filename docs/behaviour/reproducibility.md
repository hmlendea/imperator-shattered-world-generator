# Reproducibility

## Guarantee

Given the same seed, the same command line, the same game files, and the same runtime version, a run reproduces the same mod.

Mechanism: `RandomNumberGenerator` seeds a `System.Random` with `GeneratorSettings.Seed`. Generators draw only from that stream. Nothing else in the pipeline is non-deterministic: file names derive from the pressed mod name, entity order derives from file order, and the writer performs no draws.

## What is printed

`Program.Main` writes:

```text
Generating a shattered world using the seed '<seed>'...
```

That line is the only record of the seed used when `--seed` was omitted; it should be captured for reproduction.

## Stream topology

Because `IRandomNumberGenerator` is transient, three independent streams exist, each seeded identically:

| Consumer | Draws |
|----------|-------|
| `CityGenerator` | Population amounts, pop classes, culture, religion, civilisation, barbarian |
| `CountryGenerator` | Government, stance, centralisation, colour, random identifier characters, group representatives (through `GetRandomElement`) |
| `ShatteredWorldGenerator` | Candidate pool representative per name, capital selection per country |

Consequences worth knowing:
- Changing the number of cities does not shift which government the first country receives, because the country stream is independent.
- Changing the number of countries does not shift the city randomisation.
- Changing the *country* stream (for example adding a draw) shifts every later country but leaves cities untouched.

## Where determinism ends

| Change | Effect on determinism |
|--------|----------------------|
| Different vanilla files (game patch) | Different values, because lists and file order change |
| Different settings | Different values |
| Different .NET major version | `System.Random` output is not contractually stable across versions; the same seed may differ |
| Different code with an extra draw | The remainder of that consumer's stream shifts |
| Collisions on derived identifiers | A collision forces the random fallback and consumes an extra draw, so identical names plus different country sets produce different subsequent values |
| Parallelism | None exists; the pipeline is sequential, which is what makes determinism achievable |

## Practical recipe

1. Run once, note the printed seed.
2. Preserve the exact command line and the exact game version.
3. Re-run with `--seed <value>`.

Do not expect the same output across a game patch, across a .NET major upgrade, or after a code change that alters draw counts. Update `RandomNumberGeneratorTests`, `CityGeneratorTests`, `CountryGeneratorTests`, and `ShatteredWorldGeneratorTests` when that happens.