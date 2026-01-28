# ADR-002: Dual-Mode Blazor UI (Server + WASM)

## Status

Accepted

## Date

2026-01

## Context

Blazor offers multiple hosting models:
- **Blazor Server**: Components run on the server, UI updates via SignalR
- **Blazor WebAssembly (WASM)**: Components run in the browser via .NET in WebAssembly
- **Blazor Web App (unified)**: .NET 8+ model supporting both with per-component render mode selection

We needed to choose a hosting model that balances:
- Fast initial load and SEO (server rendering)
- Rich interactivity without server round-trips (client-side)
- Code sharing between modes
- Demo flexibility for showcasing both approaches

## Decision

Adopt **Blazor Web App with dual-mode support**:

1. **`App.Ui`**: Server host providing the shell, static assets, and server-rendered pages
2. **`App.Ui.Client`**: WebAssembly client assembly with shared pages and components
3. **`App.Host`**: Combined host that runs UI + API together for demos and E2E tests

Pages can declare their render mode explicitly:
- `@rendermode InteractiveServer` — Server-side interactivity
- `@rendermode InteractiveWebAssembly` — Client-side interactivity
- `@rendermode InteractiveAuto` — Server on first load, WASM after download

Most demo pages use `InteractiveWebAssembly` to showcase client-side validation.

## Consequences

### Positive

- **Flexibility**: Can demonstrate both server and client rendering in the same app
- **Code reuse**: Components in `App.Ui.Client` work in both modes
- **Fast iteration**: Server mode avoids WASM rebuild delays during development
- **Progressive enhancement**: Auto mode provides fast first paint then upgrades

### Negative

- **Complexity**: Two projects (`App.Ui` + `App.Ui.Client`) instead of one
- **Service registration**: Services must be registered in both hosts
- **Debugging**: Different debugging experience between server and WASM

### Neutral

- WASM download size is acceptable for forms-heavy applications
- SignalR connection management adds operational considerations for server mode

## Alternatives Considered

### Alternative 1: Blazor Server Only

All interactivity via SignalR.

**Why rejected**: Requires persistent connection, latency for every interaction, not suitable for offline scenarios, doesn't demonstrate WASM validation patterns.

### Alternative 2: Blazor WebAssembly Standalone

Pure client-side SPA with separate API.

**Why rejected**: Slower initial load, no server-side rendering for SEO, can't demonstrate server render mode.

### Alternative 3: Blazor MAUI Hybrid

Desktop/mobile app with embedded Blazor.

**Why rejected**: Out of scope; this is a web-focused validation demo.

## References

- [Blazor render modes documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/render-modes)
- [specs/architecture-plan.md](../../specs/architecture-plan.md) — App.Ui and App.Ui.Client sections
- [docs/blazor-guide.md](../blazor-guide.md) — Blazor fundamentals guide
