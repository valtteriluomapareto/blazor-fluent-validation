# Blazor Guide (for non-Blazor devs)

## Purpose
A quick, practical overview of how Blazor works, when to use server vs WebAssembly, and how to build components safely and predictably.

## How Blazor works (mental model)
- You write Razor components (`.razor`) that mix HTML and C#.
- Blazor renders components into a DOM representation and applies diffs to the browser.
- Components are re-rendered when state changes (similar to React/Vue reactivity, but driven by C# state + render pipeline).
- Routing is component-based (pages are components with `@page` directives).

## Blazor Server vs WebAssembly
### Blazor Server (interactive server render mode)
- UI logic runs on the server; the browser receives UI diffs over a real-time connection.
- Faster initial load; no large WASM download.
- Requires a persistent connection; latency impacts UX.
- Server holds per-user state in memory (scalability considerations).

### Blazor WebAssembly (WASM)
- UI logic runs in the browser; app downloads the .NET runtime + assemblies.
- Works offline-ish after load; reduced server CPU usage per user.
- Larger initial download; slower cold start on low-end devices.
- Must call APIs for server resources (no direct server-side access).

### Data flow differences (Server vs WASM)
- Server: UI events go over SignalR, server executes the handler and streams DOM diffs back.
- WASM: UI events execute in the browser; any server interaction is explicit HTTP calls to APIs.
- For demos, WASM often feels closer to a “traditional SPA” because you can see network requests in the browser.

### When to consider switching from Server to WASM
- You need offline capability or ultra-low latency interactions.
- You expect huge concurrent users and want to offload server CPU/memory.
- You want to avoid sticky sessions/SignalR scale-out complexity.
- Your app is mostly client-driven and API-centric.

### When to stay on Server
- You want minimal client downloads and fast first render.
- You need access to server-only resources without additional APIs.
- You expect enterprise networks with strict client security policies.
- You prefer simpler deployment and smaller client footprint.

### Dual-mode (Server + WASM)
- A Blazor Web App can host both modes side-by-side.
- Pages/components opt into a render mode with `@rendermode`.
- Use `InteractiveServer` for server, `InteractiveWebAssembly` for WASM, or `InteractiveAuto` to let the runtime choose.
- The host must reference the client project and enable WebAssembly render mode plus `AddAdditionalAssemblies(...)` for routing.

## How components look and behave
### Basic component
```razor
@page "/counter"

<h3>Counter</h3>

<p>Current count: @_count</p>

<button class="btn btn-primary" @onclick="Increment">Click me</button>

@code {
    private int _count = 0;

    private void Increment()
    {
        _count++;
    }
}
```

### Parameters (props)
```razor
<MyWidget Title="Hello" Count="@count" OnSave="HandleSave" />
```

```csharp
[Parameter] public string Title { get; set; } = "";
[Parameter] public int Count { get; set; }
[Parameter] public EventCallback OnSave { get; set; }
```

### Child content (slots)
```razor
<Card>
    <h4>Header</h4>
    <p>Body content here.</p>
</Card>
```

```csharp
[Parameter] public RenderFragment? ChildContent { get; set; }
```

### Cascading parameters (context injection)
```csharp
[CascadingParameter] public AppState AppState { get; set; } = default!;
```

## Rendering and lifecycle
### Rendering
- Blazor re-runs the component render method when state changes.
- Blazor diffs the resulting render tree and updates the DOM.
- Use `StateHasChanged()` to force a re-render when state changes outside event callbacks.

### Lifecycle methods
```csharp
protected override void OnInitialized() { }
protected override async Task OnInitializedAsync() { }
protected override void OnParametersSet() { }
protected override async Task OnParametersSetAsync() { }
protected override void OnAfterRender(bool firstRender) { }
protected override async Task OnAfterRenderAsync(bool firstRender) { }
```

Guidance:
- Use `OnInitialized*` for setup and DI.
- Use `OnParametersSet*` when parameters change.
- Use `OnAfterRender*` for DOM/JS interop (guard with `firstRender`).

## Events and data binding
- Event handlers are C# methods; async is supported.
- Two-way binding uses `@bind`.

```razor
<input @bind="_name" />
```

```razor
<input @bind-value="_name" @bind-value:event="oninput" />
```

## Forms and validation
- Blazor uses `EditForm` + `EditContext`.
- Validation can be built-in (DataAnnotations) or custom (FluentValidation).
- In this repo, FluentValidation is used; see `docs/blazilla-usage.md` and `src/App.Ui.Client/Pages/SampleFormValidationWasm.razor`.

## DI and services
- Use constructor injection for services in `.razor.cs` (code-behind) or `@inject` in `.razor`.
- Prefer scoped services for per-user state (server) and singleton for stateless services.

```razor
@inject HttpClient Http
```

## JavaScript interop
- Use JS interop for browser-only APIs (clipboard, local storage, etc.).
- Prefer minimal JS surface and keep the .NET boundary narrow.

## Comparing Blazor to React/Vue
### Component model
- React/Vue use JavaScript/TypeScript; Blazor uses C# with Razor.
- Props vs parameters: same concept, different syntax.
- Slots vs `RenderFragment`: similar pattern for composition.

### Rendering
- React/Vue use VDOM diffing in the browser.
- Blazor Server does diffing on the server and streams DOM diffs to the client.
- Blazor WASM does diffing in the browser (C# runtime).

### State and reactivity
- React state updates trigger re-render; Vue uses reactive proxies.
- Blazor re-renders when component state changes in event handlers or when you call `StateHasChanged()`.

### Data fetching
- React/Vue typically fetch in `useEffect`/`mounted`.
- Blazor uses `OnInitializedAsync` or `OnParametersSetAsync`.

## Practical gotchas
- Avoid long-running synchronous work in UI events; use async.
- Be careful with large objects in component state (rendering can be expensive).
- In Server mode, keep per-user state small; memory usage scales with users.
- In WASM, watch initial bundle size and avoid heavy dependencies.
- Use `@key` in lists when items can reorder.

```razor
@foreach (var item in Items)
{
    <ItemRow @key="item.Id" Item="item" />
}
```

## Repo-specific notes
- UI host is in `src/App.Ui/` (and `src/App.Host/` for combined UI + API).
- Server pages live in `src/App.Ui/Components/Pages/`.
- WASM pages live in `src/App.Ui.Client/Pages/`.
- Layouts live in `src/App.Ui/Components/Layout/`.
- This repo is set up for dual-mode; see `src/App.Ui/Program.cs` and `src/App.Host/Program.cs`.
- Demo routes: `/sample-form` (WASM, with `/sample-form-wasm` as an alias), plus `/complex-form` and `/tabbed-form` (WASM).

## Open questions / clarifications
- Do you want guidance on testing Blazor components (bUnit, integration tests)?
- Should we add a section on routing and navigation (`NavLink`, `NavigationManager`)?
- Any team conventions to codify (naming, folder patterns, component prefixes)?
