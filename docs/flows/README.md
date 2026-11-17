# Flows Index

| Flow | Page | Covers |
|------|------|--------|
| Startup and generation | [Startup and Generation](./startup-and-generation.md) | Argument parsing through a completed mod |
| City randomisation | [City Randomisation](./city-randomisation-flow.md) | The city stage, cache warm-up, and per-city draws |
| Country generation | [Country Generation](./country-generation-flow.md) | Pool construction, draws, registration, and failures |
| Capital regeneration | [Capital Regeneration](./capital-regeneration-flow.md) | Why capitals are rewritten after countries exist |
| Mod emission | [Mod Emission](./mod-emission-flow.md) | Directory preparation and each file write |

Each flow names the exact methods involved so that a debugger can set breakpoints without searching.

## Combined sequence

```mermaid
sequenceDiagram
    participant P as Program
    participant S as ShatteredWorldGenerator
    participant M as EntityManager
    participant L as EntitiesLoader
    participant C as CityGenerator
    participant K as CountryGenerator
    participant W as ModWriter

    P->>P: LoadSettings(args)
    P->>S: Generate()
    S->>M: GetCityIds()
    M->>L: LoadCities()
    loop every city
        S->>C: GenerateCity(id)
        C->>M: GetCity / GetCultureIds / GetReligionIds
        S->>M: UpdateCity(city)
    end
    S->>M: GetCities(), GetCountries()
    M->>L: LoadCountries()
    loop RandomCountriesCount times
        S->>K: GenerateCountry(cityId)
        K->>M: GetCity / GetGovernmentIds / GetDiplomaticStanceIds / GetCountry
        S->>M: AddCountry(country)
    end
    loop every country
        S->>C: GenerateCapital(country)
        S->>M: UpdateCity(capital)
    end
    S->>W: CreateMod()
    W->>M: GetCities(), GetCountries()
    W->>W: write nine files
```

See also [Orchestration](../components/orchestration.md) for the invariants each stage preserves.