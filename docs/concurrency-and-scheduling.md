# Concurrency and Scheduling

## Summary

The application is entirely single-threaded and synchronous. There is no `async`, no `Task`, no `Thread`, no timer, no queue, and no scheduler. Every method returns void or a plain collection; the whole run completes on the entry thread before the process exits.

## Evidence

| Concern | Finding |
|---------|---------|
| `async`/`await` | None in the production project |
| `Task`, `Thread`, `Parallel` | None |
| Locks, `Interlocked`, `Concurrent*` collections | None |
| Background services, timers | None |
| Shared mutable state | `EntityManager` dictionaries, mutated only from the generation thread |
| Shared `Random` | One instance per consumer, never accessed concurrently |

## Why single-threaded is safe for the design

- `System.Random` is not thread-safe. Because generation is sequential and each consumer owns its own instance, no data race is possible. Parallelising the city stage would make both the shared instances unsafe *and* the draw order non-deterministic.
- `EntityManager` exposes live `Dictionary` value collections. Nested enumeration and mutation during the same loop would throw `InvalidOperationException`; the orchestrator deliberately builds a list before adding countries.
- `ModWriter` performs sequential I/O; the delete-then-create sequence would be unsafe if concurrent runs targeted the same mod name.

## Concurrency-related hazards for future changes

| Change | Hazard |
|--------|--------|
| Parallelising `GenerateCities` | Determinism lost; `Random` corruption; `Dictionary` mutation races |
| Making `IRandomNumberGenerator` a singleton while adding parallelism | Definite corruption, since `Random` is not thread-safe |
| Running two processes against the same mod name | Interleaved deletes and writes; output corrupted |
| Adding asynchronous I/O to `ModWriter` | `CreateMod` would need to become `async`, forcing changes through `IShatteredWorldGenerator` and `Program` |

## Scheduling

There is no in-process scheduling. The *external* schedules are:
- Manual runs by an operator.
- The CI workflow on push and pull request to `master` (`.github/workflows/dotnet.yml`).

## Resource use

| Resource | Behaviour |
|----------|-----------|
| Memory | All cities and countries are held in dictionaries for the process lifetime; peak memory scales with the vanilla dataset plus generated countries |
| CPU | Dominated by the `O(cities × countries)` predicate inside the candidate filter and by repeated file reads in `LoadCountries` |
| Disk | Nine output files per run plus the read cost of the whole vanilla dataset |
| Network | None |

## Determinism interaction

Determinism relies on three properties that concurrency would break: a fixed seed, a fixed draw order, and a fixed enumeration order. Any change to collection type (for example using a `HashSet` or a concurrent dictionary) alters enumeration order and therefore output.