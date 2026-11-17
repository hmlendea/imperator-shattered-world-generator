# Repository Overview

## Purpose

The repository contains **Imperator Shattered World Generator**, a .NET 10 console application that rewrites the vanilla Imperator: Rome map into a *shattered world* mod. It randomises every habitable city and generates additional countries from vacant cities, then serialises the result as a complete mod directory and descriptor.

## Scope

In scope:
- Reading a user-supplied Imperator: Rome installation (`game/setup`, `game/map_data`, `game/localization`, `game/common`) and parsing the vanilla files that describe provinces, countries, cultures, religions, governments, and diplomatic stances.
- Randomising city population, culture, religion, civilisation level, and barbarian level.
- Restricting every vanilla country to its capital city.
- Generating additional countries, each with an identifier, name, culture, religion, government, diplomatic stance, centralisation level, capital, colour, and ship names.
- Writing the mod descriptor, the `setup/provinces`, `setup/countries`, and `setup/main` files, localisation, an event file, and a defines file.

Out of scope:
- Launching the game or installing the mod into the game directory.
- Validating the generated mod against the game's own schema.
- Network access of any description; the application performs no telemetry, no update checks, and no analytics.

## Audience

| Audience | Entry point |
|----------|-------------|
| Player who wants a shattered world | [Quick start](./quick-start.md) |
| Contributor changing generation behaviour | [Change guide](./change-guide.md) |
| Developer debugging a run | [Flows](./flows/README.md) |
| Maintainer of the documentation set | [Documentation maintenance](./documentation-maintenance.md) |

## Entry Points

| Entry point | Location | Responsibility |
|-------------|----------|----------------|
| Process entry | `ImperatorShatteredWorldGenerator/Program.cs` (`Program.Main`) | Parses arguments, builds the DI container, invokes `IShatteredWorldGenerator.Generate` |
| Generation orchestration | `ShatteredWorldGenerator.Generate` | Fixed stage order: cities, countries, capitals, mod writing |
| Continuous integration | `.github/workflows/dotnet.yml` | Restore, build, test on push and pull request to `master` |
| Release packaging | `package.sh` | Self-contained publication for `win-x64`, `linux-x64`, `osx-x64` plus ZIP archives |

## Runtime Shape

A single short-lived process:

```text
command line -> Program.Main -> DI container -> ShatteredWorldGenerator.Generate
    -> load vanilla data (lazy) -> randomise cities -> generate countries
    -> regenerate capitals -> write out/<mod-id>/ and <mod-id>.mod -> exit
```

There are no hosted services, no background workers, no persistence beyond the generated mod, and no configuration files: every setting arrives on the command line.

## Principal Constraints

- Vanilla countries are always retained; removing them breaks vanilla events.
- The output directory and descriptor are deleted and recreated on every run.
- Paths are derived from the process working directory, so the working directory determines where output lands.
- The vanilla file formats are parsed by regular expressions, a Clausewitz text scanner, and a semicolon CSV reader without schema validation.

See [Invariants](./invariants.md) for the complete list and [Ambiguities and open questions](./ambiguities-and-open-questions.md) for unresolved aspects.