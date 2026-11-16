# Security Policy

This policy describes the security objectives of SokobanCLI, how suspected vulnerabilities are reported, and which releases receive security maintenance. Reports are handled privately and in good faith, investigated before any public disclosure, and remediated before the details are published. Only the latest released version is maintained; preceding versions no longer receive security fixes.

## 📑 Table of Contents

- [Supported Versions](#supported-versions)
- [Reporting a Vulnerability](#reporting-a-vulnerability)
- [Scope](#scope)
- [Disclosure Policy](#disclosure-policy)
- [Automated Security Scanning](#automated-security-scanning)
- [Safe Harbour](#safe-harbour)
- [Recognition](#recognition)

## 🛡️ Supported Versions

Use this table to indicate which project versions currently receive security maintenance.

| Version | Distribution Channel | Supported |
|---------|--------------------|-----------|
| Latest version | GitHub Releases | ✅ |
| Latest version | Source builds from the `master` branch | ✅ |
| Latest version | Unofficial third-party distribution channels | ❌ |
| Preceding versions | Any distribution channel | ❌ |

## 🚨 Reporting a Vulnerability

Please do not disclose suspected vulnerabilities publicly before maintainers have had an opportunity to validate and remediate them.

To report a vulnerability:
- [GitHub Security Advisories](https://github.com/hmlendea/sokoban-cli/security/advisories)
- Contact the maintainers directly

## 📌 Scope

The original maintainers are not responsible for changes, modifications, or security issues introduced by downstream forks.

The subsequent report categories are in scope for this repository:
- Vulnerabilities in the SokobanCLI source code
- Vulnerabilities in the build tooling and release artefacts published by the maintainers

The subsequent categories are out of scope unless explicitly stated to the contrary:
- Vulnerabilities in third-party builds, repackaged distributions, or modified forks
- Vulnerabilities that require an already compromised local environment or terminal

## 📢 Disclosure Policy

This project follows coordinated disclosure:
1. Vulnerabilities are investigated privately.
2. A remediation plan is prepared and validated.
3. Public disclosure is published after a fix, mitigation, or agreed risk decision is available.
4. Credit is attributed in accordance with reporter preference and project policy.

## 🔍 Automated Security Scanning

This repository runs automated security scans on every push and pull request targeting `master`, and on a weekly schedule. The scan includes:
- **NuGet dependencies** are checked for known vulnerable packages, including transitive dependencies.
- **C# source code** is analysed with CodeQL code scanning.

Results are published to the [Security](https://github.com/hmlendea/sokoban-cli/security) tab and the [Actions](https://github.com/hmlendea/sokoban-cli/actions/workflows/security.yml) workflow page.

## 🧾 Safe Harbour

If your investigation is conducted in good faith, confined to authorised scope, and disclosed responsibly, the maintainers will not pursue action for policy-compliant activity.

## 🙏 Recognition

We appreciate responsible disclosure. Reporters who desire public attribution may be acknowledged in release notes, advisories, or a dedicated acknowledgements section.