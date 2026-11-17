# Composition Root

## Purpose

`Program` is the only place that knows the concrete implementations. It converts command-line text into `GeneratorSettings`, registers the service graph, resolves `IShatteredWorldGenerator`, and reports progress on the console.

## Location

`ImperatorShatteredWorldGenerator/Program.cs` — a single non-public `class Program` with `Main`, `LoadSettings`, `RegisterArguments`, `NormaliseShortArguments`, `GetArgumentValue`, `GetRequiredArgumentValue`, and `GetIntegerArgumentValue`.

## Entry

`Program.Main(string[] args)`, invoked by the runtime. `Main` is `private static void`; it takes no return code, so every successful run exits with `0` and every unhandled exception exits non-zero.

## Execution

1. `LoadSettings(args)` — normalise short aliases, register the 13 argument names, parse, populate settings.
2. Build a `ServiceCollection` with:
   - `AddSingleton(settings)`
   - `AddTransient<IRandomNumberGenerator, RandomNumberGenerator>()`
   - `AddSingleton<IEntityLoader, EntitiesLoader>()`
   - `AddSingleton<IEntityManager, EntityManager>()`
   - `AddSingleton<ICityGenerator, CityGenerator>()`
   - `AddSingleton<ICountryGenerator, CountryGenerator>()`
   - `AddSingleton<IModWriter, ModWriter>()`
   - `AddSingleton<IShatteredWorldGenerator, ShatteredWorldGenerator>()`
3. Resolve `IShatteredWorldGenerator`.
4. `Console.WriteLine($"Generating a shattered world using the seed '{settings.Seed}'...")`.
5. `generator.Generate()`.
6. `Console.WriteLine("Done!")`.

## Data

| Direction | Data |
|-----------|------|
| In | `string[]` argument tokens |
| Out | A fully populated `GeneratorSettings` singleton and a built `IServiceProvider` |
| Console | Exactly two lines: the seed banner and the completion banner |

## State

No retained state beyond the local container, which is never disposed explicitly. Registration order is irrelevant because dependency injection resolves the graph on demand.

## Dependencies

`NuciCLI.Arguments` (`ArgumentParser`, `ArgumentsCollection`), `Microsoft.Extensions.DependencyInjection`, and all production services.

## Dependants

None. `Program` is the top of the dependency graph and is not referenced by the production code; the tests never invoke `Main`.

## Configuration

Every option name and alias is declared here as a `string[]` field. Adding an option requires a new field, an `RegisterArguments` call, and an `settings.X = GetIntegerArgumentValue(...)` (or `GetRequiredArgumentValue`) assignment. See [Configuration](../configuration.md).

## Failures

| Condition | Result |
|-----------|--------|
| Mandatory option missing or blank | `ArgumentException` from `GetRequiredArgumentValue`; nothing is loaded or written |
| Integer option unparseable | `FormatException` from `int.Parse` |
| Any service throws during `Generate` | Exception escapes `Main`; the completion banner is not printed |
| `GetService<IShatteredWorldGenerator>()` returns `null` | Would throw `NullReferenceException` at `generator.Generate()`; unreachable while the registration exists |

## Invariants

- `GeneratorSettings` is registered as a singleton, so every consumer shares one instance and therefore one `Seed`.
- `IRandomNumberGenerator` is transient but its only state (`Random`) is seeded from that singleton, so the graph contains exactly one random stream per resolve. Because exactly one component resolves it… in fact `CityGenerator`, `CountryGenerator`, and `ShatteredWorldGenerator` each receive the transient instance; since `RandomNumberGenerator` creates a new `Random(seed)` per construction, **each injected consumer owns a separate `Random` seeded identically**. See [Random Number Generator](./random-number-generator.md) for the consequence.

## Tests

Not covered directly. Argument normalisation and parsing are untested; the option table is verified indirectly only through `GeneratorSettings` defaults. This is the largest untested surface in the project. See [Testing](../testing.md).

## Integration

`Program` is the only element aware of both the command line and the service graph, which is why every extension recipe in [Change Guide](../change-guide.md) starts by naming it.

## Modification impact

- Adding or renaming a setting affects `Program`, `GeneratorSettings`, `configuration.md`, and the README option table.
- Changing a lifetime (for example making `IRandomNumberGenerator` a singleton) changes determinism semantics and must be reviewed against [Reproducibility](../behaviour/reproducibility.md).
- Adding console output changes the only observability signal; see [Logging](../logging.md).