# Documentation Maintenance

## Purpose

This tree exists so that a future contributor or agent can work on the generator without re-deriving its behaviour from source. Keeping it current is cheaper than re-reading the implementation.

## Ownership

The repository maintainer owns this tree. Any change to behaviour, configuration, file formats, packaging, or dependencies requires a matching documentation change in the same contribution.

## When to update what

| Change | Documents to revise |
|--------|---------------------|
| New or renamed command-line option | [Configuration](./configuration.md), [Quick Start](./quick-start.md), README option table |
| New or changed default | [Configuration](./configuration.md), [Invariants](./invariants.md), `GeneratorSettingsTests` |
| Changed city or country rule | [City Generator](./components/city-generator.md) or [Country Generator](./components/country-generator.md), [Behaviour](./behaviour/README.md), [Flows](./flows/README.md) |
| Changed identifier algorithm | [Country Creation](./behaviour/country-creation.md), [Invariants](./invariants.md) |
| Changed generated file, path, or encoding | [Mod Writer](./components/mod-writer.md), [Mod Emission](./flows/mod-emission-flow.md), [Integrations](./integrations.md) |
| Changed vanilla parsing | [Entities Loader](./components/entities-loader.md), [Integrations](./integrations.md), [Ambiguities](./ambiguities-and-open-questions.md) |
| Changed stage order | [Orchestration](./components/orchestration.md), [Architecture](./architecture.md), [Flows](./flows/README.md) |
| Changed random-stream topology | [Random Number Generator](./components/random-number-generator.md), [Reproducibility](./behaviour/reproducibility.md), [Invariants](./invariants.md) |
| New or removed service | [Components Index](./components/README.md), [Architecture](./architecture.md), [Dependencies](./dependencies.md) |
| Dependency version change | [Dependencies](./dependencies.md), README dependency table |
| CI or packaging change | [Build and Deployment](./build-and-deployment.md), README GitHub Actions table |
| New tests or fixtures | [Testing](./testing.md) |
| Fixed a defect listed in ambiguities | [Ambiguities and Open Questions](./ambiguities-and-open-questions.md) |

## Maintenance procedure

1. Identify the affected documents from the table above.
2. Re-read the current source for the area; do not edit from memory.
3. Update the document, preserving its heading structure and cross-reference style.
4. If a document was added or removed, update [Index](./INDEX.md) and the README documentation table.
5. Verify every relative link resolves to an existing file.
6. Update the *Last reviewed* row in [Index](./INDEX.md).

## Conventions

- File names are kebab-case; directories are kebab-case plural. `behaviour/` follows the repository's British spelling convention.
- Headings are sentence case within headings only where the section is descriptive prose; table headers use sentence case.
- Code symbols are written in backticks with enough namespace context to locate them.
- Repository-relative links point at real paths; no link to a non-existent file.
- Statements about behaviour must be traceable to source; where behaviour and tests disagree, record the discrepancy in [Ambiguities and Open Questions](./ambiguities-and-open-questions.md) rather than silently choosing one.

## Verification checklist

- [ ] Every relative link resolves
- [ ] Every symbol named in a document exists in the source
- [ ] Every option, default, and path matches `Program` and `GeneratorSettings`
- [ ] Every file-table entry in the writer documents a file the writer actually produces
- [ ] Every test name cited exists in a test fixture
- [ ] `Index` listing matches the actual tree

## Deliberately absent documents

| Document | Reason |
|----------|--------|
| `api-reference/` | The application exposes no HTTP surface |
| `api-usage-examples.md` | Same |
| `troubleshooting` duplication into the README | The README remains a concise index; see [Troubleshooting](./troubleshooting.md) |

## Review cadence

Re-review after every release and after any change that touches generation rules, file formats, or the random-stream topology. The coverage metadata in [Index](./INDEX.md) records the last full review.