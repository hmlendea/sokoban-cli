# Privacy and Personal Data

This document describes how the SokobanCLI terminal application handles personal data, as of 16 November 2026. SokobanCLI is a locally executed console game distributed as a .NET application: it runs entirely within the user's own terminal session, operates no service or network endpoint, and the project maintainers receive no data from the software.

## 📑 Table of Contents

- [What This Document Covers](#what-this-document-covers)
- [Data We Handle](#data-we-handle)
  - [Data Provided to the Application](#data-provided-to-the-application)
  - [Data Generated or Collected by the Application](#data-generated-or-collected-by-the-application)
  - [Data Received from Integrations](#data-received-from-integrations)
- [Processing and Use](#processing-and-use)
- [Storage, Retention, and Deletion](#storage-retention-and-deletion)
- [External Processing and Integrations](#external-processing-and-integrations)
- [Document Changes](#document-changes)
- [Contact](#contact)

## 🔎 What This Document Covers

This document describes how SokobanCLI at https://github.com/hmlendea/sokoban-cli manages personal data. It covers the application conduct and verified integrations described below.

## 📥 Data We Handle

### Data Provided to the Application

- No personal data is requested. The application reads only keystrokes during play, which it uses to move the player within the current level and does not retain, log, or transmit.

### Data Generated or Collected by the Application

- No personal data is generated or collected automatically. The application emits no telemetry, analytics, diagnostics, crash reports, or update checks, and it contains no logging or network code. Gameplay state such as the current level, player position, and move count is held only in memory for the lifetime of the running process.

### Data Received from Integrations

- No personal data is received from integrations or third parties. The application has no network, authentication, payment, advertising, or third-party software development kit integration.

## 🧭 Processing and Use

The application processes the data described above for these verified functions:
- Local gameplay rendering — keystrokes read from the console, used in memory to move the player, push boxes, count moves, and advance levels

## 🗄️ Storage, Retention, and Deletion

The application stores no personal data and creates no configuration, progress, cache, or log files. There is no database, no local settings store, and no persistence of gameplay state: level progress and move counts exist only in memory and are discarded when the level changes or the process exits, so there is nothing to retain, export, or delete. The only files read by the application are the level definitions under `Levels/` and the screen definitions under `Screens/` that ship with it, which are read-only content and contain no personal data.

## 🔗 External Processing and Integrations

The application has no built-in external data transfer. It does not contact any server, and no external service participates in its operation at runtime.

| Service or integration | Purpose | Data involved | Configuration or documentation |
|-----------------------|---------|---------------|--------------------------------|
| None | No external processing or integration is performed by the application | None | — |

## 🛡️ Data Protection and Security

The application applies no remote data-processing safeguards because it transmits and stores no personal data. Its only local data inputs are the level and screen definition files that ship with it, which are read-only content resolved relative to the process working directory; screen definitions are deserialised and drive screen creation, so those files should be treated as trusted content and protected from local tampering. Console output is written directly to the terminal the user started, and no data leaves that session.

The original maintainers are not responsible for changes, modifications, or privacy issues introduced by downstream forks.

## 🔄 Document Changes

Update this document when application data processes, storage, integrations, or deployment responsibilities change. The current version is published at https://github.com/hmlendea/sokoban-cli/blob/master/PRIVACY.md.

## 📬 Contact

For questions about application data handling, contact the project maintainers. For a self-hosted instance, contact the instance operator, unless the project explicitly manages the request. Include no additional information to identify the deployment or data process, because the application processes no personal data; do not send passwords, access tokens, or other secrets.
