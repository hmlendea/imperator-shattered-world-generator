# Design Decisions

Each decision records the choice, the evidence, the rationale, and the consequence.

## D1 — Single console pipeline rather than a library plus host

**Choice:** One executable project containing configuration, data access, and services.
**Evidence:** `ImperatorShatteredWorldGenerator.csproj` has `<OutputType>Exe</OutputType>`; there is no library project in the solution.
**Rationale:** The work is a one-shot transformation with no reuse surface; a library split would add packaging overhead with no consumer.
**Consequence:** Contributing requires a game installation to exercise the code end to end, and the only test entry points are the two test projects.

## D2 — Dependency injection with a single composition root

**Choice:** `Microsoft.Extensions.DependencyInjection` with all registrations in `Program.Main`.
**Evidence:** Eight `Add…` calls in `Program`; every service takes interfaces in its constructor.
**Rationale:** Lets the unit tests substitute collaborators with Moq and keeps construction out of the services.
**Consequence:** Adding a component requires editing `Program`, and the DI lifetimes silently define the random-stream topology (see D7).

## D3 — Lazy, self-populating caches in `EntityManager`

**Choice:** Load on first read, cache for the process lifetime, no invalidation.
**Evidence:** Every getter follows `if (field is null) field = loader.Load…()`.
**Rationale:** Avoids loading data a run may not need (for example identifiers when no countries are requested) and keeps the loader free of cache concerns.
**Consequence:** Mutation before any read throws `NullReferenceException`; the orchestration order is the implicit contract. Documented in `EntityManagerTests`.

## D4 — Regular-expression parsing instead of a game-format parser

**Choice:** Hard-coded regexes per vanilla file.
**Evidence:** Thirteen `const string …RegexPattern` declarations across `EntitiesLoader` and `CountryGenerator`.
**Rationale:** The Paradox text format is informal and regex line scanning was the shortest path.
**Consequence:** Changed vanilla formatting can silently yield blanks rather than failing; the backward scan in `LoadCountries` is fragile and currently leaves `CentralisationLevel` unset.

## D5 — Countries preserved, cities rewritten

**Choice:** Never remove a vanilla country.
**Evidence:** `LoadCountries` sets `IsVanilla = true`; `ModWriter.WriteCountrySetupFile` emits a comment stating removal could break the game.
**Rationale:** Vanilla events reference the tags and fail once enough in-game years pass.
**Consequence:** The mod cannot produce a world without the vanilla powers; not configurable.

## D6 — Capital regeneration as a distinct third stage

**Choice:** Re-run city randomisation for capitals after countries exist, then overwrite culture and religion from the country.
**Evidence:** `GenerateCapitals` loops all countries; `GenerateCapital` calls `GenerateCity` first.
**Rationale:** A country's declared culture and religion must match its capital, and capitals need a distinct population.
**Consequence:** Capitals consume double draw budgets and the stream advances more than the visible output suggests.

## D7 — Transient random number generator

**Choice:** `AddTransient<IRandomNumberGenerator, RandomNumberGenerator>()`.
**Evidence:** `Program.Main` registration; each of `CityGenerator`, `CountryGenerator`, and `ShatteredWorldGenerator` receives its own instance.
**Rationale:** Not documented in the repository; the effect is that each consumer has an independent, identically seeded stream.
**Consequence:** Reproducible and *more* robust than a single shared stream, because a change in one stage does not shift another. It contradicts the root `ARCHITECTURE.md` wording; see [Ambiguities](./ambiguities-and-open-questions.md#the-random-stream-is-not-actually-shared).

## D8 — Delete-then-recreate output

**Choice:** `Directory.Delete(…, true)` and `File.Delete(…)` before writing.
**Evidence:** `ModWriter.PrepareDirectoryStructure`.
**Rationale:** Guarantees no stale files survive, which matters because file names are derived from generated identifiers.
**Consequence:** Previous output is unrecoverable and no atomic swap or temporary directory is used.

## D9 — Hand-formatted file content instead of a serialiser

**Choice:** String concatenation with `Environment.NewLine` for every generated file.
**Evidence:** Every `Write…` method builds a `fileContent` string.
**Rationale:** The game formats are not JSON/XML and the layout includes comments and deliberate ordering.
**Consequence:** Newline handling must be explicit (`NormaliseFileContent`), and the UTF-8 helper currently discards the normalised value.

## D10 — Test doubles deriving from `Random`

**Choice:** `StubRandom` (constant) and `SequenceRandom` (cyclic) both override `Next(int)`, `Next(int,int)`, and `NextDouble()`.
**Evidence:** `ImperatorShatteredWorldGenerator.UnitTests/StubRandom.cs` and `SequenceRandom.cs`.
**Rationale:** The production code calls `Randomiser.Next` directly in some places and `rng.Get` in others, so a mock of `IRandomNumberGenerator` alone would be insufficient; substituting the `Random` instance covers both paths.
**Consequence:** Tests can script exact draw sequences but must account for modulo `%` semantics rather than true range mapping.

## D11 — Working-directory-relative output

**Choice:** Derive paths from `Environment.CurrentDirectory`.
**Evidence:** `GeneratorSettings.ModDirectoryPath`.
**Rationale:** Keeps the tool portable for self-contained archives extracted anywhere.
**Consequence:** Output location depends on where the process is started; `WorkingDirectoryScope` exists solely to control this in tests.

## D12 — No error-handling layer

**Choice:** Let exceptions propagate.
**Evidence:** No `try`/`catch` in the production project.
**Rationale:** A failed run has no meaningful partial result to preserve, so surfacing the stack trace is the most informative outcome.
**Consequence:** Operations that ignore a failure (like the unused `normalisedCapitalName` local) are only detectable by reading code, not at runtime.

## D13 — No structured logging

**Choice:** Two `Console.WriteLine` calls.
**Evidence:** `Program.Main`.
**Rationale:** The tool is short-lived and interactive.
**Consequence:** No timings, no per-stage counts, and no diagnostic trail; see [Logging](./logging.md).