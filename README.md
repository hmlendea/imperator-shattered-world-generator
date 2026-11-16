[![Latest Release](https://img.shields.io/github/v/release/hmlendea/imperator-shattered-world-generator)](https://github.com/hmlendea/imperator-shattered-world-generator/releases/latest)
[![Build Status](https://github.com/hmlendea/imperator-shattered-world-generator/actions/workflows/dotnet.yml/badge.svg)](https://github.com/hmlendea/imperator-shattered-world-generator/actions/workflows/dotnet.yml)
[![License](https://img.shields.io/github/license/hmlendea/imperator-shattered-world-generator)](https://github.com/hmlendea/imperator-shattered-world-generator/blob/master/LICENSE)

# Imperator Shattered World Generator

A .NET console application that rewrites the vanilla Imperator: Rome map into a shattered world mod by randomising cities and generating new countries.

Every habitable city receives a randomised population, culture, religion, civilisation level, and barbarian level. The vanilla countries keep their capitals but lose all other regions, and additional countries are created from vacant cities, each with a generated name, government, diplomatic stance, and centralisation level. The result is written to a mod directory together with its descriptor, localisation, and event files.

## 📑 Table of Contents

- [Capabilities](#capabilities)
- [Usage](#usage)
- [Known Limitations](#known-limitations)
- [System Requirements](#system-requirements)
- [Installation](#installation)
  - [Manual Installation](#manual-installation)
- [Configuration](#configuration)
  - [Command-Line Options](#command-line-options)
- [Privacy and Data](#privacy-and-data)
- [Development](#development)
  - [Requirements](#requirements)
  - [Setup](#setup)
  - [Build](#build)
  - [Run](#run)
  - [Test](#test)
  - [Release](#release)
  - [Dependencies](#dependencies)
- [GitHub Actions](#github-actions)
- [Project Structure](#project-structure)
  - [Directories](#directories)
- [Architecture](#architecture)
- [Contributing](#contributing)
- [Security](#security)
- [Support](#support)
- [Project Engagement](#project-engagement)
- [License](#license)

## ✨ Capabilities

- Randomises the population, culture, religion, civilisation level, and barbarian level of every habitable vanilla city.
- Restricts every vanilla country to its capital region and aligns the capital's culture and religion with its country.
- Generates random countries from vacant cities, including names, governments, diplomatic stances, and centralisation levels.
- Accepts a fixed random seed so that a generated world can be reproduced exactly.
- Writes a complete mod directory, consisting of the mod descriptor, provinces setup, country files, localisation, and events.

## 🚀 Usage

Generate a mod by supplying the Imperator: Rome installation directory and the desired mod name:

```bash
dotnet run --project ImperatorShatteredWorldGenerator/ImperatorShatteredWorldGenerator.csproj -- --dir "$HOME/.local/share/Steam/steamapps/common/ImperatorRome" --name "Shattered World"
```

The generator loads the vanilla game files, applies the randomisation, and writes the result to `out/<mod-identifier>/` together with the `<mod-identifier>.mod` descriptor in the working directory, where `<mod-identifier>` is the mod name converted to a lower-case hyphenated identifier.

## ⚠️ Known Limitations

- All vanilla countries are preserved regardless of settings, because removing them causes the vanilla events to fail. This cannot be altered through configuration.
- Generating very many countries can produce a mod that Imperator: Rome fails to load or that crashes during play, and more than approximately 1,900 countries could not be generated without the game crashing at startup.
- The output mod directory and mod descriptor files are deleted and regenerated on every run, so previous output for the same mod is overwritten.

## 🖥️ System Requirements

| Component | Minimum | Recommended |
|-----------|---------|-------------|
| Imperator: Rome | A vanilla installation to read the source game files from | N/A |
| Operating system | 64-bit Linux, macOS, or Windows | N/A |

## 📦 Installation

[![Obtain it from GitHub](https://raw.githubusercontent.com/hmlendea/readme-assets/master/badges/stores/github.png)](https://github.com/hmlendea/imperator-shattered-world-generator/releases)

### Manual Installation

1. Download the archive for your operating system and processor architecture from the [latest release](https://github.com/hmlendea/imperator-shattered-world-generator/releases/latest). The published release provides `linux-x64`, `osx-x64`, and `win-x64` archives.
2. Extract the archive to a local directory.
3. Execute the generator from the extraction directory so that the generated `out/` directory is created where you expect it.

## ⚙️ Configuration

The generator is configured entirely through command-line arguments. The game directory and the mod name are mandatory, while every remaining argument falls back to a default value.

### Command-Line Options

| Option | Value | Default | Description |
|--------|-------|---------|-------------|
| `-d`, `--dir`, `--game`, `--imperator` | Path to the Imperator: Rome installation | — | Required. For example `~/.local/share/Steam/steamapps/common/ImperatorRome`. |
| `-n`, `--name` | Mod name | — | Required. The output directory name is derived from it. |
| `-s`, `--seed` | Integer seed | Random | Seed used by the random number generator. |
| `--capital-population`, `--capital-pops` | Integer | `10` | Population of capital cities. |
| `--city-population-min`, `--city-pops-min` | Integer | `4` | Minimum population of regular cities. |
| `--city-population-max`, `--city-pops-max` | Integer | `12` | Maximum population of regular cities. |
| `--city-civilisation-min`, `--city-civ-min` | Integer | `0` | Minimum civilisation level of cities. |
| `--city-civilisation-max`, `--city-civ-max` | Integer | `20` | Maximum civilisation level of cities. |
| `--city-barbarian-min`, `--city-barb-min` | Integer | `0` | Minimum barbarian level of cities. |
| `--city-barbarian-max`, `--city-barb-max` | Integer | `0` | Maximum barbarian level of cities. |
| `--country-centralisation-min`, `--country-cent-min` | Integer | `0` | Minimum centralisation level of countries. |
| `--country-centralisation-max`, `--country-cent-max` | Integer | `100` | Maximum centralisation level of countries. |
| `--random-countries`, `--new-countries`, `--rng-countries` | Integer | `500` | Number of new countries to generate. A value of `0` keeps only the vanilla countries. |

## 🛡️ Privacy and Data

Imperator Shattered World Generator runs entirely on your own machine and does not collect, transmit, or store personal data. It performs no analytics, diagnostics, telemetry, or update checks.

For the detailed description of how the application handles privacy and personal data, see the [Privacy Statement](./PRIVACY.md).

The application reads a local Imperator: Rome installation and writes the generated mod to the local working directory. Every value it handles is either a local filesystem path or a command-line configuration value, and all data remains under your direct control.

| Data | Purpose | Storage | Retention | Optional |
|------|---------|---------|-----------|----------|
| Game installation path | Locating the vanilla game files to read | Not stored by the application | N/A | No |
| Mod name and generation settings | Deriving the output directory and generating the mod | Not stored by the application | N/A | No |
| Generated mod files | The output mod directory and descriptor | Local working directory, under `out/<mod-identifier>/` | Until you delete them or run the generator again | No |

## 🛠️ Development

### Requirements

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- An Imperator: Rome installation, to read the vanilla game files from

### Setup

Clone the repository and restore its dependencies:

```bash
git clone https://github.com/hmlendea/imperator-shattered-world-generator.git
cd imperator-shattered-world-generator
dotnet restore
```

### Build

```bash
dotnet build --no-restore
```

### Run

```bash
dotnet run --project ImperatorShatteredWorldGenerator/ImperatorShatteredWorldGenerator.csproj -- --dir "$HOME/.local/share/Steam/steamapps/common/ImperatorRome" --name "Shattered World"
```

### Test

```bash
dotnet test --no-build
```

The solution contains two test projects. `ImperatorShatteredWorldGenerator.UnitTests` covers the configuration, models, and services in isolation, while `ImperatorShatteredWorldGenerator.IntegrationTests` exercises the file-system based loaders and the mod writer against a temporary game directory.

### Release

Publish self-contained builds for each supported platform and package them into release archives:

```bash
dotnet publish -c Release -r linux-x64 -o out-linux-x64 --self-contained=true
dotnet publish -c Release -r osx-x64 -o out-osx-x64 --self-contained=true
dotnet publish -c Release -r win-x64 -o out-win-x64 --self-contained=true
```

The repository also contains `package.sh`, which performs the same publication for `win-x64`, `linux-x64`, and `osx-x64`, packages each output directory into a ZIP archive under `bin/Release`, and cleans up afterwards:

```bash
bash ./package.sh <version>
```

**Note:** `package.sh` temporarily adds the preview `Microsoft.Packaging.Tools.Trimming` package before publishing. Review the script before running it in your environment.

### Dependencies

| Package | Version | Scope | Purpose |
|---------|---------|-------|---------|
| `Microsoft.Extensions.DependencyInjection` | `11.0.0` | Runtime | Constructs and resolves the application services. |
| `NuciCLI.Arguments` | `1.0.3` | Runtime | Parses the command-line arguments. |
| `NuciDAL` | `4.3.1` | Runtime | Provides the file-based data access and repository abstractions. |
| `NuciExtensions` | `6.0.3` | Runtime | Provides string and collection extension methods. |

## ⚙️ GitHub Actions

| Workflow | Purpose | What it does |
|----------|---------|--------------|
| [ .NET ](https://github.com/hmlendea/imperator-shattered-world-generator/blob/master/.github/workflows/dotnet.yml) | Continuous integration | Determines the .NET version from the production project, sets up the corresponding SDK, restores dependencies, builds the solution, and runs the test step on pushes and pull requests to `master`. |

## 🗂️ Project Structure

The solution contains a single console project, whose namespaces are organised into configuration, data access, and service folders, together with a unit test project and an integration test project.

### Directories

| Directory | Purpose |
|-----------|---------|
| `ImperatorShatteredWorldGenerator/Configuration` | Generator settings and their defaults. |
| `ImperatorShatteredWorldGenerator/DataAccess/DataObjects` | Entities loaded from the game files. |
| `ImperatorShatteredWorldGenerator/Service` | Generation, entity management, and mod writing services. |
| `ImperatorShatteredWorldGenerator/Service/Mapping` | Mappings between entities and domain models. |
| `ImperatorShatteredWorldGenerator/Service/Models` | City and country domain models. |
| `ImperatorShatteredWorldGenerator.UnitTests` | Unit tests for the configuration, models, and services. |
| `ImperatorShatteredWorldGenerator.IntegrationTests` | Tests for the file-system based loaders and the mod writer. |
| `.github/workflows` | GitHub Actions workflow definitions. |

## 🏗️ Architecture

Imperator Shattered World Generator is a single-process console pipeline in which a dependency-injected composition root loads the vanilla game data, randomises it through dedicated city and country generators, and writes the result as a self-contained mod.

See the [Architecture Documentation](./ARCHITECTURE.md) for the system context, principal components, runtime flows, ownership boundaries, dependencies, constraints, and extension points.

## 🤝 Contributing

You are welcome to submit any suggestion, feedback, or modification to this project.

When doing so, please:
- Maintain cross-platform compatibility
- Preserve the existing public contract unless a breaking change is intentional
- Submit focused pull requests that conform to the existing code style
- Maintain your branch synchronised with `master`
- Revise the documentation when functionality changes
- Properly test all modifications, including edge cases and error conditions
- Include tests for additional or modified functionality
- Raise a new [issue](https://github.com/hmlendea/imperator-shattered-world-generator/issues) for problems or suggestions

## 🔒 Security

Imperator Shattered World Generator runs entirely on your own machine, performs no network communication, and collects no personal data, so its security surface is limited to parsing the vanilla game files and writing the generated mod.

For information on reporting security vulnerabilities, see the [Security Policy](./SECURITY.md).

## 💬 Support

For support, please [open an issue](https://github.com/hmlendea/imperator-shattered-world-generator/issues).

## 💝 Project Engagement

Discovered a problem with Imperator Shattered World Generator or have a suggestion for it? [Open an issue](https://github.com/hmlendea/imperator-shattered-world-generator/issues)!

If you find this project useful, consider starring ⭐️ it on GitHub!

## 📄 License

This project is being distributed under the `GNU General Public License v3.0`.
See [LICENSE](./LICENSE) for additional information.
