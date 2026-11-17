# Index

## Repository Summary

Imperator Shattered World Generator is a .NET 10 console application that reads a local Imperator: Rome installation, randomises every habitable city, restricts the vanilla countries to their capitals, generates additional countries from vacant cities, and writes the result as a self-contained mod directory and descriptor. It performs no network communication, collects no data, and exposes no HTTP API; its only public contracts are the command line and the generated mod files.

## Root Document Map

- [README.md](../README.md) — capabilities, usage, configuration, installation, development, and documentation index.
- [ARCHITECTURE.md](../ARCHITECTURE.md) — system context, components, processes, contracts, and extension points.
- [SECURITY.md](../SECURITY.md) — supported versions and vulnerability reporting.
- [PRIVACY.md](../PRIVACY.md) — personal-data handling.
- [CONTRIBUTING.md](../CONTRIBUTING.md) — contribution workflow and expectations.
- [CODE_OF_CONDUCT.md](../CODE_OF_CONDUCT.md) — conduct standards.
- [LICENSE](../LICENSE) — licensing terms.

## Documentation Catalogue

### Orientation
- [Repository Overview](./repository-overview.md) — purpose, scope, audiences, entry points, and principal constraints.
- [Repository Structure](./repository-structure.md) — solution, project layout, test layout, root files, naming conventions.
- [Quick Start](./quick-start.md) — obtain, run, and verify a first generation.
- [FAQ](./faq.md) — recurring questions about behaviour and output.

### Architecture
- [Architecture](./architecture.md) — high-level architecture that complements the root `ARCHITECTURE.md`.
- [Design Decisions](./design-decisions.md) — the choices behind the pipeline, caching, identifier allocation, and writer behaviour.
- [Invariants](./invariants.md) — behaviour that must remain true for the mod to load.
- [Dependencies](./dependencies.md) — NuGet packages, framework, tooling, and external game data.
- [Concurrency and Scheduling](./concurrency-and-scheduling.md) — the single-threaded, deterministic execution model.
- [Data Model](./data-model.md) — `City`, `Country`, `CityEntity`, derived members, and the vanilla-to-mod field mapping.
- [State and Persistence](./state-and-persistence.md) — in-memory caches, output lifecycle, and overwrite semantics.
- [Configuration](./configuration.md) — every command-line option, alias, default, and validation rule.
- [Integrations](./integrations.md) — the Imperator: Rome installation, the mod loader contract, and IDE tooling.

### Components
- [Components Index](./components/README.md) — map of the component deep dives.
- [Composition Root](./components/composition-root.md) — `Program` argument handling and service registration.
- [Generator Settings](./components/generator-settings.md) — `GeneratorSettings` defaults and derived paths.
- [Entities Loader](./components/entities-loader.md) — vanilla file paths, regular expressions, and parse failure modes.
- [Entity Manager](./components/entity-manager.md) — lazy caches, mutation entry points, and lifecycle hazards.
- [City Generator](./components/city-generator.md) — population, culture, religion, civilisation, and barbarian randomisation.
- [Country Generator](./components/country-generator.md) — identity derivation, identifier allocation, and attribute assignment.
- [Random Number Generator](./components/random-number-generator.md) — seeding and inclusive range semantics.
- [Mod Writer](./components/mod-writer.md) — directory preparation and per-file serialisation.
- [Orchestration](./components/orchestration.md) — `ShatteredWorldGenerator` stage sequencing and vacancy filtering.
- [Domain Models and Mapping](./components/domain-models-and-mapping.md) — `City`, `Country`, and `CityMapping`.

### Behaviour
- [Behaviour Index](./behaviour/README.md) — user-visible behaviours.
- [World Randomisation](./behaviour/world-randomisation.md) — what changes in a generated world.
- [Country Creation](./behaviour/country-creation.md) — which cities become countries and under which identity.
- [Reproducibility](./behaviour/reproducibility.md) — how a seed reproduces a world and where determinism ends.

### Flows
- [Flows Index](./flows/README.md) — end-to-end execution flows.
- [Startup and Generation](./flows/startup-and-generation.md) — from process start to a completed mod.
- [City Randomisation Flow](./flows/city-randomisation-flow.md) — the city stage in detail.
- [Country Generation Flow](./flows/country-generation-flow.md) — vacancy filtering, drawing, and registration.
- [Capital Regeneration Flow](./flows/capital-regeneration-flow.md) — why capitals are regenerated last.
- [Mod Emission Flow](./flows/mod-emission-flow.md) — directory preparation and file-by-file writing.

### Development and Operations
- [Build and Deployment](./build-and-deployment.md) — restore, build, test, publish, packaging, and continuous integration.
- [Testing](./testing.md) — test projects, fixtures, doubles, covered behaviour, and gaps.
- [Change Guide](./change-guide.md) — safe modification recipes for common areas.
- [Error Handling](./error-handling.md) — exception taxonomy, failure points, and recovery behaviour.
- [Logging](./logging.md) — the console output that constitutes the only observability signal.
- [Security](./security.md) — trust boundaries and defensive posture, complementing the root `SECURITY.md`.
- [Troubleshooting](./troubleshooting.md) — symptoms, causes, and remedies.
- [Ambiguities and Open Questions](./ambiguities-and-open-questions.md) — defects, discrepancies, and unresolved aspects.
- [Documentation Maintenance](./documentation-maintenance.md) — ownership and refresh procedure for this tree.

## Navigation Aids

### Start Here
- Newcomer: [Repository Overview](./repository-overview.md) then [Quick Start](./quick-start.md).
- First code change: [Architecture](./architecture.md) then [Change Guide](./change-guide.md).

### Deep Dive
- Component owner: [Components Index](./components/README.md), then the specific component page.
- Behaviour owner: [Behaviour Index](./behaviour/README.md) and [Invariants](./invariants.md).

### Flows
- Debugger: [Startup and Generation](./flows/startup-and-generation.md), then the failing stage flow.

## Maintenance Metadata

| Attribute | Value |
|-----------|-------|
| Last reviewed | 2026-11-17 |
| Owner | Repository maintainer (`hmlendea`) |
| Coverage | Production project in full; both test projects in full; build, packaging, and continuous integration |
| Known gaps | The count of newly generated countries the game tolerates is empirical only; no coverage measurement is configured |
| Absent by design | No HTTP API reference and no API usage examples, because the application exposes no HTTP surface |