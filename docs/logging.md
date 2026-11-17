# Logging

## Framework

None. The application uses `System.Console` directly and has no logging library, no abstraction, no sinks, no levels, and no configuration. There is no `Microsoft.Extensions.Logging` reference.

## Emitted output

Exactly two lines on standard output, written by `Program.Main`:

| Order | Message | Timing |
|-------|---------|--------|
| 1 | `Generating a shattered world using the seed '<seed>'...` | Before `generator.Generate()` |
| 2 | `Done!` | After `generator.Generate()` returns |

Nothing else is written to standard output. Library code (`EntitiesLoader`, `ModWriter`, `CityGenerator`, `CountryGenerator`, `EntityManager`, `ShatteredWorldGenerator`) is completely silent, including on success paths that discard work (for example an early return when `RandomCountriesCount == 0`).

## Error output

Unhandled exceptions are reported by the runtime on standard error with a full stack trace. There is no custom message, no wrapping, and no per-stage context.

## Levels

Not applicable. The two messages correspond informally to informational start and completion. There is no debug, warning, error, or fatal level, and therefore no way to raise verbosity.

## Structured fields

None. The seed is interpolated into prose rather than emitted as a field, so it cannot be parsed reliably beyond a prefix match on `Generating a shattered world using the seed '`.

## Correlation

None. A run is one process with one output stream; there is no correlation identifier, no trace identifier, and no span.

## Redaction and masking

Nothing is redacted, but nothing sensitive is emitted:
- The seed is not secret.
- The game installation path and mod name are not printed.
- No personal data is handled.

Adding output that includes the game path or the working directory would expose local file-system information; prefer omitting both.

## Retention and rotation

Standard output is not persisted by the application. Retention depends entirely on the operator's shell redirection.

## Sampling and performance

Not applicable. Two console writes per run have no measurable cost.

## Audit logging

None. There is no record of which settings produced which world beyond the printed seed and the mod content itself.

## Observability gaps

| Gap | Consequence |
|-----|-------------|
| No counts of loaded cities or countries | A silently empty load is indistinguishable from a successful one until the mod is inspected |
| No per-stage progress | A long `LoadCountries` scan looks like a hang |
| No timing | Performance regressions (for example the `O(cities × countries)` filter) are invisible |
| No warnings for ignored settings | The unused `CityCivilisationLevelMin` cannot be noticed at runtime |
| No summary of the generated world | Operators cannot confirm the requested country count was met |
| No log of the output paths | The operator must infer the location from the working directory |

## If logging is added

Follow the repository's `csharp.logging` conventions for `NuciLog`: an `Operation` concept, keyed `LogInfoKey` fields, and masking of sensitive values. Minimum useful additions:
- the resolved game directory and output directory,
- the number of cities loaded and the number of countries generated,
- stage timings,
- a warning when a configured setting has no effect.