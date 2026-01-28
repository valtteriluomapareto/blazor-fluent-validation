# Architecture Decision Records (ADRs)

This folder contains Architecture Decision Records that document significant technical decisions made in this project.

## What is an ADR?

An ADR captures a single architectural decision along with its context and consequences. They help future maintainers understand *why* decisions were made, not just *what* was decided.

## ADR Status

| Status | Meaning |
|--------|---------|
| **Proposed** | Under discussion, not yet accepted |
| **Accepted** | Decision has been made and is in effect |
| **Deprecated** | No longer applies but kept for historical context |
| **Superseded** | Replaced by a newer ADR (link to replacement) |

## Index

| ADR | Title | Status | Date |
|-----|-------|--------|------|
| [ADR-001](ADR-001-validation-first-architecture.md) | Validation-First Architecture | Accepted | 2026-01 |
| [ADR-002](ADR-002-dual-mode-blazor-ui.md) | Dual-Mode Blazor UI (Server + WASM) | Accepted | 2026-01 |
| [ADR-003](ADR-003-error-codes-for-localization.md) | Error Codes for Localization | Accepted | 2026-01 |
| [ADR-004](ADR-004-local-vs-server-rule-sets.md) | Local vs Server Validation Rule Sets | Accepted | 2026-01 |
| [ADR-005](ADR-005-layered-project-structure.md) | Layered Project Structure | Accepted | 2026-01 |

## Creating a New ADR

1. Copy `_template.md` to `ADR-NNN-short-title.md`
2. Fill in all sections
3. Add an entry to the index above
4. Submit for review

## References

- [ADR GitHub organization](https://adr.github.io/)
- [Michael Nygard's article on ADRs](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)
