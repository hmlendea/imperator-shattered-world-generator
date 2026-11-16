# Privacy and Personal Data

This document describes how Imperator Shattered World Generator at https://github.com/hmlendea/imperator-shattered-world-generator manages personal data. It covers the command-line application, the data it processes, where that data is stored, and what third-party services if any are involved. Last updated: 16 November 2026.

## 📑 Table of Contents

- [What This Document Covers](#what-this-document-covers)
- [Self-Hosted Deployments](#self-hosted-deployments)
- [Data We Handle](#data-we-handle)
  - [Data Provided to the Application](#data-provided-to-the-application)
  - [Data Generated or Collected by the Application](#data-generated-or-collected-by-the-application)
  - [Data Received from Integrations](#data-received-from-integrations)
- [Processing and Use](#processing-and-use)
- [Storage, Retention, and Deletion](#storage-retention-and-deletion)
- [External Processing and Integrations](#external-processing-and-integrations)
- [User Controls and Requests](#user-controls-and-requests)
- [International Transfers](#international-transfers)
- [Children](#children)
- [Data Protection and Security](#data-protection-and-security)
- [Document Changes](#document-changes)
- [Contact](#contact)

## 🔎 What This Document Covers

This document describes how Imperator Shattered World Generator at https://github.com/hmlendea/imperator-shattered-world-generator manages personal data. It covers the application conduct and verified integrations described below. Where the software is self-hosted, the instance operator may have separate responsibilities described below.

## 🏠 Self-Hosted Deployments

The application is a command-line tool that users and organisations run locally on their own machines. The instance operator controls the game installation directory, the local working directory, the output directory, log output to the console, and any backups.

Imperator Shattered World Generator does not send data from the local instance to the project maintainers or to any external service. There is no telemetry, update check, crash reporting, or monitoring integration. All data remains on the local device.

## 📥 Data We Handle

### Data Provided to the Application

- The path to the Imperator: Rome installation directory (`--dir` / `--game` / `--imperator`). This is a local filesystem path on the user's machine.
- The mod name (`--name`). This is a human-chosen name used to derive the output directory and mod identifier.
- The random seed (`--seed`) and optional generation parameters (`--capital-population`, `--city-population-min/max`, `--city-civilisation-min/max`, `--city-barbarian-min/max`, `--country-centralisation-min/max`, `--random-countries`). These are configuration values supplied as command-line arguments.
- No user accounts, credentials, email addresses, or other personal data are requested.

### Data Generated or Collected by the Application

- The output mod directory and the mod descriptor file in the working directory. These contain the generated city, country, localisation, and event data derived from the vanilla game files and the supplied random seed.
- No server logs, access logs, audit logs, telemetry, crash reports, or update-check records are generated or collected automatically.

### Data Received from Integrations

- The application reads the vanilla Imperator: Rome game files from the directory supplied by the user. These are game data files on the user's local disk.
- No data is received from third-party integrations, remote APIs, or online services.

## 🧭 Processing and Use

The application processes the data described above for these verified functions:
- Reading vanilla game files — Game installation path provided by the user
- Randomising city, country, and localisation data — Random seed and generation parameters provided by the user
- Writing the generated mod files — Mod name provided by the user; output directory on local disk

## 🗄️ Storage, Retention, and Deletion

- Game files read from the local Imperator: Rome installation are not modified, moved, or deleted.
- The output mod directory and mod descriptor file are written to the user's local working directory. They persist until the user deletes them or runs the generator again, in which case the output directory and descriptor are deleted and recreated.
- No data is stored on any server, cloud service, or remote system. The application does not maintain any external database, cache, or backup.

## 🔗 External Processing and Integrations

Imperator Shattered World Generator has no built-in external data transfer. It does not integrate with any third-party services, APIs, analytics platforms, or online repositories during execution. The only external interaction is the GitHub-hosted repository and release files, which are accessed manually by the user downloading the application or its source.

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| None | No external services are used during execution | None | Not applicable |

## ⚙️ User Controls and Requests

All data is under the user's direct control. The `--dir` argument determines which game files are read. The output is written to the current working directory under the `out/<mod-identifier>/` subfolder. Users may inspect, move, or delete the output files at any time. No account, token, or credential management is required.

## 🌍 International Transfers

No international data transfers occur. The application runs entirely on the user's local machine. The only location data is processed or stored is the user's local filesystem.

## 🧒 Children

The application does not collect any data from users. No age-restricted features apply. There is no registration or personal data collection from users of any age.

## 🛡️ Data Protection and Security

Because the application collects no personal data and performs no network transfer, its data-handling surface is limited to the local filesystem paths that the operator supplies. Operators remain responsible for updates, secrets, access controls, backups, network exposure, and log protection on the machine that runs the generator. No absolute security guarantee is made.

The original maintainers are not responsible for changes, modifications, or privacy issues introduced by downstream forks.

## 🔄 Document Changes

Update this document when application data processes, storage, integrations, or deployment responsibilities change. The current version is published at https://github.com/hmlendea/imperator-shattered-world-generator/blob/master/PRIVACY.md.

## 📬 Contact

For questions about application data handling, contact the project maintainers through the repository issue tracker at https://github.com/hmlendea/imperator-shattered-world-generator/issues. For a self-hosted instance, contact the instance operator, unless the project explicitly manages the request. Include the mod name and the command-line arguments in use, if relevant; do not send passwords, access tokens, or other secrets.