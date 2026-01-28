# Docs Index

## Guides

| Document | Purpose |
|----------|---------|
| [new-form-guide.md](new-form-guide.md) | Step-by-step walkthrough for adding a new form: contracts, validation, API, UI, and tests. Start here when implementing a feature. |
| [blazor-guide.md](blazor-guide.md) | Blazor fundamentals for developers new to the framework: components, lifecycle, data binding, testing, routing, and project conventions. |
| [blazilla-usage.md](blazilla-usage.md) | How to use FluentValidation with Blazor forms via Blazilla: sync vs async validation, rule sets, and EditContext integration. |
| [server-validation-errors.md](server-validation-errors.md) | End-to-end guide for handling server validation errors in the UI: error contract, localization, ValidationMessageStore, and testing patterns. |
| [validate-on-blur.md](validate-on-blur.md) | Optional "touched" validation: show errors when users leave empty required fields. Includes implementation guide for adding to other components. |

## Reference

| Document | Purpose |
|----------|---------|
| [coverage-summary.md](coverage-summary.md) | Latest test coverage snapshot with per-assembly breakdown, identified gaps, and prioritized improvement suggestions. |

## Architecture Decisions

| Document | Purpose |
|----------|---------|
| [decisions/](decisions/README.md) | Architecture Decision Records (ADRs) documenting key technical decisions and their rationale. |

Key decisions:
- [ADR-001](decisions/ADR-001-validation-first-architecture.md) — Why validation rules are the single source of truth
- [ADR-002](decisions/ADR-002-dual-mode-blazor-ui.md) — Why we support both Server and WASM rendering
- [ADR-003](decisions/ADR-003-error-codes-for-localization.md) — Why error codes are the contract for localization
- [ADR-004](decisions/ADR-004-local-vs-server-rule-sets.md) — Why validation is split into Local and Server rule sets
- [ADR-005](decisions/ADR-005-layered-project-structure.md) — Why the solution uses layered project boundaries

## See also

- [README.md](../README.md) — Project quickstart and command reference.
- [AGENTS.md](../AGENTS.md) — AI/agent context with all conventions and commands.
- [specs/architecture-plan.md](../specs/architecture-plan.md) — Design rationale, project dependencies, and remaining work items.
