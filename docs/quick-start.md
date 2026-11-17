# Quick Start

## Prerequisites

| Requirement | Detail |
|-------------|--------|
| .NET SDK | 10.0, as declared by `<TargetFramework>net10.0</TargetFramework>` |
| Imperator: Rome | A vanilla installation containing `game/common` and `game/localization`. GOG and Steam layouts both work as long as the supplied directory is the installation root |
| Operating system | 64-bit Linux, macOS, or Windows |

## Run from source

```bash
git clone https://github.com/hmlendea/imperator-shattered-world-generator.git
cd imperator-shattered-world-generator
dotnet restore
dotnet build --no-restore
dotnet run --project ImperatorShatteredWorldGenerator/ImperatorShatteredWorldGenerator.csproj -- --dir "$HOME/.local/share/Steam/steamapps/common/ImperatorRome" --name "Shattered World"
```

The process prints the seed in use and a completion message. Output appears in the *current working directory*:

```text
out/shattered-world/          # mod directory
out/shattered-world.mod       # mod descriptor
```

## Run a published build

1. Download the archive matching the platform from the [latest release](https://github.com/hmlendea/imperator-shattered-world-generator/releases/latest).
2. Extract it anywhere.
3. Execute the binary from the directory in which the `out/` tree should be created.

## Reproduce a world

Supply the seed printed by a previous run:

```bash
dotnet run --project ImperatorShatteredWorldGenerator/ImperatorShatteredWorldGenerator.csproj -- \
  --dir "$HOME/.local/share/Steam/steamapps/common/ImperatorRome" \
  --name "Shattered World" \
  --seed 613
```

The same seed with the same game files and the same settings produces the same mod. See [Reproducibility](./behaviour/reproducibility.md).

## Verify the result

| Check | Expected |
|-------|----------|
| `out/shattered-world.mod` exists | Contains `name = "Shattered World"` and `path = "mod/shattered-world"` |
| `out/shattered-world/descriptor.mod` exists | Identical content to the descriptor above |
| `out/shattered-world/common/province_setup.csv` exists | Header row plus one row per city |
| `out/shattered-world/common/setup.txt` exists | One `countries` block containing every vanilla country and every generated country |
| Process output | `Generating a shattered world using the seed '<seed>'...` then `Done!` |

## Common first-run problems

| Symptom | Cause | Remedy |
|---------|-------|--------|
| `Missing required argument: --dir` | The game path was omitted | Pass `--dir` (or `-d`) |
| `DirectoryNotFoundException` for `game/common/...` | The supplied path is not the installation root | Point `--dir` at the directory that contains `game/` |
| Mod not visible in game | The output was written to an unexpected working directory | Run the binary from the intended location; the path derives from `Environment.CurrentDirectory` |
| Game crashes at startup | Too many generated countries | Reduce `--random-countries` |

See [Troubleshooting](./troubleshooting.md) for the complete list.