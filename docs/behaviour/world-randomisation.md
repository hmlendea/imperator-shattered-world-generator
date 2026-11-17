# World Randomisation

## What changes

Every city present in the vanilla `province_setup.csv` is rewritten:

| Attribute | New value |
|-----------|-----------|
| Population split | Redrawn per population point; each point becomes a slave, tribesman, freeman, or citizen with equal probability |
| Total population | Random within `[CityPopulationMin, CityPopulationMax]`, or exactly `CapitalPopulation` for a capital |
| Culture | Random entry from the vanilla culture list (capitals: the owning country's culture) |
| Religion | Random entry from the vanilla religion list (capitals: the owning country's religion) |
| Civilisation level | Random within `[CityBarbarianLevelMin, CityCivilisationLevelMax]` — note the lower bound bug described in [Configuration](../configuration.md#settings-that-are-accepted-but-unused) |
| Barbarian level | Random within `[CityBarbarianLevelMin, CityBarbarianLevelMax]`, default `0..0` |

Unchanged: province identifier, trade good, name reference, and province reference.

## What happens to the vanilla countries

Every vanilla country is preserved and restricted to its capital. In the generated `setup.txt` each vanilla country keeps its identifier, government, stance (when present), culture, religion, and capital, and gains `own_control_core = { <capital> }`, which limits ownership to that one province. Two comment lines mark vanilla countries as required for compatibility.

Vanilla countries are **never removed**, because the base game's events reference them and fail after enough in-game years if the tags are missing.

## What happens to vanilla cities that are not capitals

They remain in the `province_setup.csv` with randomised attributes but belong to no country unless they become the capital of a generated country. The mod does not assign ownership beyond `own_control_core`, so the resulting political map consists of one-province countries.

## What is added

| Addition | Count | Source |
|----------|-------|--------|
| Generated countries | `RandomCountriesCount` (default `500`) | Vacant habitable cities, one per distinct name |
| Country files | One per country, vanilla and generated | `country.Id` |
| Localisation entries | One per generated country | `country.Name` |
| Event file | One | Fixed `grinsel.2` province event granting five slave pops |
| Defines file | One | Start date `001.10.1` |

## What is not changed

- Nothing outside `out/<mod-id>/` is touched; the game installation is read-only for the application.
- The vanilla mod directory is not modified; the generated mod is independent.
- No vanilla localisation is rewritten; only `SW_countries_l_english.yml` is added.

## Worked example

For a world with three vanilla countries and `--random-countries 2`:

```text
province_setup.csv  -> every city, randomised
common/countries.txt -> REB, PIR, BAR, MER, <vanilla ids…>, <2 new ids>
localization/SW_countries_l_english.yml -> l_english: + 2 entries
common/setup.txt -> vanilla countries with compatibility comments + 2 new countries
common/countries/<id>.txt -> one per country, each with colour and ship names
events/flavor_sel.txt, common/defines/00_defines.txt -> fixed content
```

## Observable limits

- A large `--random-countries` can exceed what the game loads; more than roughly 1,900 countries were reported to crash at startup.
- Because population is drawn per point with an equal split, populations cluster near an even division between the four classes.
- Every generated country is a one-province state with a random colour, so neighbouring countries can share similar colours.