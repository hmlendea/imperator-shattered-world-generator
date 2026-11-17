# FAQ

## What does the generator actually change?

It rewrites every habitable city's population, culture, religion, civilisation level, and barbarian level, keeps every vanilla country restricted to its capital, and creates new one-province countries from vacant cities. See [World Randomisation](./behaviour/world-randomisation.md).

## Why are vanilla countries always kept?

Because the base game's events reference those country tags and fail once enough in-game years pass if a tag is missing. The restriction is deliberate and not configurable; the generated `setup.txt` even carries a comment explaining it.

## Can I remove a vanilla country?

Not through the generator. Hand-editing the output is possible but reintroduces the failure the generator avoids.

## Where does the mod go?

To `out/<mod-id>/` and `out/<mod-id>.mod` relative to the process working directory. `<mod-id>` is the mod name with punctuation and diacritics removed, spaces replaced by hyphens, and lower-cased. See [Configuration](./configuration.md#derived-values).

## The generator destroyed my previous world.

`ModWriter` deletes `out/<mod-id>/` and `out/<mod-id>.mod` before writing. Use a different `--name` to keep multiple worlds, or copy the directory away first. See [State and Persistence](./state-and-persistence.md#output-lifecycle).

## How do I regenerate exactly the same world?

Re-use the printed seed with the same command line and the same game installation. See [Reproducibility](./behaviour/reproducibility.md).

## Does the seed guarantee identical output after a game patch?

No. A patch changes the vanilla lists and file order, which changes every draw.

## Why do two of my cities produce countries with the same name?

They should not. Cities sharing a `NameId` are collapsed to one representative before selection. If duplicates appear, capture the seed and report a defect.

## Why does the country name look like an identifier (`Newport`) instead of a translated name?

For generated countries the name is the capital city's `NameId` written verbatim to `SW_countries_l_english.yml`. No translation table is included.

## Why is every country exactly one province?

The generated `setup.txt` assigns `own_control_core = { <capital> }` and nothing else, so no country owns any province beyond its capital.

## Why is the barbarian level always zero?

Both `--city-barbarian-min` and `--city-barbarian-max` default to `0`. Pass a positive maximum.

## Why does `--city-civilisation-min` have no effect?

The setting is parsed but never consumed; the civilisation draw uses the barbarian minimum as its lower bound. See [Ambiguities](./ambiguities-and-open-questions.md#civilisation-minimum-is-ignored).

## Why did generation fail with `InvalidOperationException`?

More countries were requested than there are eligible capitals. Reduce `--random-countries`. See [Country Creation](./behaviour/country-creation.md#failure-surface).

## Can I run two generations at once?

Not for the same mod name: the writer deletes the shared output directory. Different mod names write to different directories but still rely on the same working directory.

## Does it need an internet connection?

No. It performs no network communication at all.

## Does it collect data?

No. No telemetry, analytics, diagnostics, or update checks. See [PRIVACY.md](../PRIVACY.md).

## Which .NET version do I need?

The SDK that matches `net10.0`. Published self-contained archives bundle the runtime, so an installed SDK is only needed to build from source.

## How many countries can the game handle?

Empirically fewer than about 1,900; beyond that the game was reported to crash at startup. No limit is enforced in code.

## Why are files written with a BOM?

The game's text parsers require a UTF-8 BOM on files containing non-ASCII identifiers. `setup.txt` and the descriptors are plain UTF-8.

## How do I add my own generation rule?

Follow [Change Guide](./change-guide.md) and preserve draw-count discipline so determinism stays predictable.

## Where are the tests?

Two projects: unit tests for services and models, integration tests for the loader and writer. See [Testing](./testing.md).