# Country Creation

## Eligibility

A city can become the capital of a generated country only when all of the following hold:
1. `IsHabitable` — culture, religion, trade good, name, and province are all non-blank.
2. It is not the capital of an existing country (which, at this stage, means any vanilla country).
3. Its `NameId` does not equal the `Name` of any existing country.

Cities that satisfy these conditions form the pool. Cities sharing a `NameId` are collapsed to one randomly chosen representative so that the localisation file cannot contain duplicate keys.

## Selection

`RandomCountriesCount` identifiers are drawn uniformly at random from the pool without replacement. Each drawn city becomes a capital, and the country is generated immediately, which means the *next* draw sees the country already in the manager and would reject its identifier on collision.

If the pool empties before the requested count is reached, the run fails with `InvalidOperationException`; nothing is written.

## Generated identity

| Attribute | Value |
|-----------|-------|
| Identifier | Three letters derived from the capital's `NameId` when possible, otherwise random |
| Name | The capital's `NameId`, written verbatim to the localisation file |
| Culture, religion | Copied from the capital city at generation time |
| Government | Random vanilla government |
| Diplomatic stance | Random vanilla stance |
| Centralisation | Random within the configured range |
| Capital | The city's identifier |
| Colour | Three independent draws in `0..255` |
| Ship names | The six default names |

## Identifier derivation rules

1. Normalise the name: remove punctuation, remove spaces, upper-case.
2. If fewer than three characters remain, use a random identifier.
3. Otherwise scan ordered character triples `(i, j, k)` with `i < j < k` and take the first triple that is not reserved, not already used, and consists only of `A`–`Z` and `0`–`9`.
4. If no triple qualifies, generate random triples until one is valid.

Consequences:
- Names with repeated structures yield predictable tags: `Cornova` → `COR`, and on collision `CON`.
- Non-ASCII letters survive normalisation in some cases and then fail the allowed-character test, so the next triple is tried; `Çupișan` → `UPI`.
- Names whose letters cannot form a valid unused tag fall back to random generation, which consumes extra random draws and therefore shifts the stream.
- Reserved tags `REB`, `PIR`, `BAR`, and `MER` are never produced; the writer always emits them as fixed index entries pointing at the vanilla files.

## Resulting map

Every generated country owns exactly one province through `own_control_core = { <capital> }`. Vanilla countries keep their capital only. The world therefore consists exclusively of one-province countries, plus any provinces that no country claims.

## Failure surface

| Situation | Outcome |
|-----------|---------|
| Requested more countries than eligible names | `InvalidOperationException`; no mod written |
| Zero vacatable cities but a non-zero count | Same exception on the first draw |
| `--random-countries 0` | Vanilla countries only; no generated countries, no localisation entries |
| Identifier space exhausted for random fallback | Unbounded loop (theoretical) |