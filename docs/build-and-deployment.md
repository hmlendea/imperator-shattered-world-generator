# Build and Deployment

## Prerequisites

| Tool | Version | Source |
|------|---------|--------|
| .NET SDK | 10.0 | `<TargetFramework>net10.0</TargetFramework>` |
| `zip` | Any | Required by `package.sh` |
| Imperator: Rome installation | Any vanilla version | Required to run the generator, not to build it |

## Restore, build, test

```bash
dotnet restore
dotnet build --no-restore
dotnet test --no-build
```

`dotnet restore` covers all three projects through `ImperatorShatteredWorldGenerator.slnx`. Tests require a build first because the CI and README both use `--no-build`.

## Editor tasks

`.vscode/tasks.json` declares three shell tasks:

| Label | Command |
|-------|---------|
| `build` | `dotnet build ${workspaceFolder}/ImperatorShatteredWorldGenerator.csproj` |
| `publish` | `dotnet publish ${workspaceFolder}/ImperatorShatteredWorldGenerator.csproj` |
| `watch` | `dotnet watch run ${workspaceFolder}/ImperatorShatteredWorldGenerator.csproj` |

The paths assume a flat layout; after the solution restructure the project lives in the `ImperatorShatteredWorldGenerator/` subdirectory, so the tasks as written do not resolve. `.vscode/launch.json` similarly expects `bin/Debug/net10.0/ImperatorShatteredWorldGenerator.dll` at the workspace root. See [Ambiguities and Open Questions](./ambiguities-and-open-questions.md#editor-tasks-point-at-stale-paths).

## Publication

### Manual

```bash
dotnet publish -c Release -r linux-x64 -o out-linux-x64 --self-contained=true
dotnet publish -c Release -r osx-x64   -o out-osx-x64   --self-contained=true
dotnet publish -c Release -r win-x64   -o out-win-x64   --self-contained=true
```

Self-contained publication bundles the runtime, so an archive needs no installed SDK on the target machine.

### Scripted: `package.sh`

```bash
bash ./package.sh <version>
```

Behaviour, in order:
1. Derive `APP_NAME` from the `origin` remote URL (the repository name).
2. Require a version argument; exit `1` when absent.
3. `dotnet add package Microsoft.Packaging.Tools.Trimming --version 1.1.0-preview1-26619-01` — a temporary preview package used to enable trimming.
4. For each of `win-x64`, `linux-x64`, `osx-x64`: `dotnet publish -c Release -r <arch> -o bin/Release/publish-script-output/<arch> --self-contained=true /p:TrimUnusedDependencies=true /p:LinkDuringPublish=true`, then ZIP the output directory to `bin/Release/<app>_<version>_<arch>.zip`.
5. `dotnet remove package Microsoft.Packaging.Tools.Trimming` and delete `bin/Release/publish-script-output`.

Notes:
- The script mutates the project file temporarily (`dotnet add`/`dotnet remove`), so an interrupted run can leave the package reference behind.
- `bin/Release` accumulates the three ZIP archives; the publish directory is removed.
- `out/*` and `bin/`/`obj/` are git-ignored.

## Continuous integration

`.github/workflows/dotnet.yml`:

| Step | Detail |
|------|--------|
| Trigger | `push` and `pull_request` to `master` |
| Runner | `ubuntu-latest` |
| .NET version detection | Extracts `<TargetFramework>` from the production `.csproj` with `sed`, strips the `net` prefix, and fails the step when the value is empty |
| SDK setup | `actions/setup-dotnet@v5` with `<version>.x` |
| Restore | `dotnet restore` |
| Build | `dotnet build --no-restore` |
| Test | `dotnet test --no-build --verbosity normal` |

No workflow publishes a release, uploads artefacts, or runs a security scan.

## Deployment target

| Concern | Design |
|---------|--------|
| Deployment unit | A ZIP archive per platform from `package.sh`, or the per-platform `dotnet publish` output |
| Distribution | GitHub releases (badge and installation instructions reference the latest release) |
| Installation | Extract; run from the directory in which output should appear |
| Environments | One: the operator machine. No staging, no production service |
| Configuration at deployment | Command-line arguments only |
| Health checks | None |
| Observability | Console output only |
| Backup | Operator-managed copy of `out/<mod-id>/` |
| Rollback | Delete the generated directory and re-run, or restore the copied one |

## Versioning and release flow

Versioning is expressed through Git tags and release archives; the assembly version is not set explicitly in the project file. `package.sh` embeds the supplied version in archive names.