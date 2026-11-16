# Security Policy

This document describes the security policy for Imperator Shattered World Generator, including supported versions, vulnerability reporting, scope, and disclosure expectations. The application is a self-hosted console tool that runs entirely on the operator's machine, performs no network communication, and collects no personal data.

## 📑 Table of Contents

- [Supported Versions](#supported-versions)
- [Reporting a Vulnerability](#reporting-a-vulnerability)
- [Scope](#scope)
- [Disclosure Policy](#disclosure-policy)
- [Safe Harbour](#safe-harbour)
- [Recognition](#recognition)

## 🛡️ Supported Versions

Use this table to indicate which project versions currently receive security maintenance.

| Version | Distribution Channel | Supported |
|---------|--------------------|-----------|
| Latest version | GitHub Releases | ✅ |
| Latest version | Unofficial third-party distribution channels | ❌ |
| Preceding versions | Any distribution channel | ❌ |

## 🚨 Reporting a Vulnerability

Please do not disclose suspected vulnerabilities publicly before maintainers have had an opportunity to validate and remediate them.

To report a vulnerability:
- [GitHub Security Advisories](https://github.com/hmlendea/imperator-shattered-world-generator/security/advisories)
- Contact the maintainers directly

## 📌 Scope

The original maintainers are not responsible for changes, modifications, or security issues introduced by downstream forks.

The subsequent report categories are in scope for this repository:
- Parsing of vanilla game files in `ImperatorShatteredWorldGenerator/Service/EntitiesLoader.cs`
- Mod file generation and path handling in `ImperatorShatteredWorldGenerator/Service/ModWriter.cs`
- Output directory and descriptor derivation in `ImperatorShatteredWorldGenerator/Configuration/GeneratorSettings.cs`
- Command-line argument handling in `ImperatorShatteredWorldGenerator/Program.cs`

The subsequent categories are out of scope unless explicitly stated to the contrary:
- Vulnerabilities in the Imperator: Rome game or its engine
- Vulnerabilities in third-party dependencies (`Microsoft.Extensions.DependencyInjection`, `NuciCLI.Arguments`, `NuciDAL`, `NuciExtensions`)
- Operating system, filesystem, or runtime (.NET) vulnerabilities
- Malicious or tampered game files supplied by the operator
- Supply-chain attacks on distribution channels

## 📢 Disclosure Policy

This project follows coordinated disclosure:
1. Vulnerabilities are investigated privately.
2. A remediation plan is prepared and validated.
3. Public disclosure is published after a fix, mitigation, or agreed risk decision is available.
4. Credit is attributed in accordance with reporter preference and project policy.

## 🧾 Safe Harbour

If your investigation is conducted in good faith, confined to authorised scope, and disclosed responsibly, the maintainers will not pursue action for policy-compliant activity.

## 🙏 Recognition

We appreciate responsible disclosure. Reporters who desire public attribution may be acknowledged in release notes, advisories, or a dedicated acknowledgements section.