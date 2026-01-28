# ADR-005: Layered Project Structure

## Status

Accepted

## Date

2026-01

## Context

As the solution grew, we needed a project structure that:
- Enforces clear dependency boundaries
- Prevents accidental coupling between layers
- Enables independent testing of each layer
- Supports both combined and separate deployment models

Without explicit boundaries, it's easy for UI code to accidentally depend on integration details, or for validators to reach into API infrastructure.

## Decision

Adopt a **layered project structure** with explicit dependency direction:

```
┌─────────────────────────────────────┐
│           Host Layer                │
│         (App.Host)                  │
└─────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────┐
│       Presentation Layer            │
│    (App.Ui, App.Ui.Client)          │
└─────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────┐
│       Application Layer             │
│          (App.Api)                  │
└─────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────┐
│          Core Layer                 │
│  (App.Validation, App.Integrations) │
└─────────────────────────────────────┘
                 │
                 ▼
┌─────────────────────────────────────┐
│       Foundation Layer              │
│ (App.Contracts, App.Abstractions)   │
└─────────────────────────────────────┘
```

**Key rules:**
1. Dependencies flow downward only
2. Foundation projects have no internal dependencies
3. `App.Validation` cannot reference `App.Api` or `App.Integrations`
4. `App.Ui`/`App.Ui.Client` cannot reference `App.Integrations`
5. `App.Integrations` implements interfaces from `App.Abstractions`

## Consequences

### Positive

- **Clear boundaries**: Each project has a single responsibility
- **Testable in isolation**: Can test validators without API, API without UI
- **Deployment flexibility**: Can split into separate services if needed
- **Onboarding clarity**: New developers understand where code belongs

### Negative

- **More projects**: 8 source projects vs. a monolithic approach
- **Reference management**: Must explicitly add project references
- **Indirection**: Interfaces in `App.Abstractions` add a layer of abstraction

### Neutral

- Project count is manageable for a validation-focused solution
- Visual Studio/Rider handle multi-project solutions well

## Alternatives Considered

### Alternative 1: Monolithic Single Project

All code in one project with folder-based organization.

**Why rejected**: No compile-time enforcement of boundaries, easy to create accidental dependencies, harder to test layers in isolation.

### Alternative 2: Vertical Slice Architecture

Organize by feature (e.g., `Features/SampleForm/`) with all layers together.

**Why rejected**: Good for large applications, but overkill for this validation-focused solution. Layered approach better matches our cross-cutting validation concerns.

### Alternative 3: Onion/Clean Architecture with More Layers

Separate Domain, Application, Infrastructure, and Presentation layers.

**Why rejected**: This solution has no domain entities or complex business logic. The validation-first model doesn't need full DDD layering.

## References

- [specs/architecture-plan.md](../../specs/architecture-plan.md) — Project Dependency Diagram
- [AGENTS.md](../../AGENTS.md) — Project structure overview
- [ADR-001](ADR-001-validation-first-architecture.md) — Validation-First Architecture
