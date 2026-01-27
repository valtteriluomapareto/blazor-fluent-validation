# Entra ID SSO Plan (App.Host + App.Ui.Client + App.Api)

This plan is tailored to the current solution structure:
- `App.Host` is the recommended entry point and maps both UI and API endpoints.
- `App.Ui` is the server host shell for the Blazor Web App.
- `App.Ui.Client` contains the interactive WASM pages and shared form components.
- `App.Api` provides the Minimal API endpoints that are mapped by `App.Host`.

## Goals And Success Criteria
- Users can sign in with Microsoft Entra ID (Azure AD) via SSO.
- The authenticated user identity is available in:
  - Server-rendered UI (`App.Ui`/`App.Host`)
  - WASM UI (`App.Ui.Client`)
  - API endpoints (`App.Api`)
- API endpoints can be protected with authorization policies.
- Local development and E2E tests remain workable via a test auth mode.

## Recommended Architecture Decision
Use `App.Host` as the SSO boundary and primary runtime:
- Configure OpenID Connect (OIDC) web app auth + cookies in `App.Host`.
- Protect API endpoints with authorization (and optionally accept bearer tokens later).
- Propagate authentication state to the WASM client using Blazor auth state serialization.

Why this fits the repo:
- E2E already boots `App.Host`.
- `App.Host` already maps API + UI together, which simplifies cookie-based auth.
- It keeps Entra complexity out of the client where possible.

## Phase 0: Entra ID App Registration Setup
Status: Not started.

Create (at least) one Entra app registration for the web app host.

Minimum setup for the web app registration:
1. Register an application for the web app.
2. Add redirect URIs (use the host launch settings):
   - `https://localhost:5001/signin-oidc`
   - `https://localhost:5001/signout-callback-oidc`
3. Create a client secret for local/dev usage.
4. Record:
   - Tenant ID
   - Client ID
   - Client secret

Optional but recommended (especially if API will be called by other clients):
- Create a second app registration for the API and expose scopes (for bearer tokens).

## Phase 1: Wire Entra ID Auth In `App.Host`
Status: Not started.

### 1. Add packages
In `src/App.Host/App.Host.csproj`:
- `Microsoft.Identity.Web`
- `Microsoft.Identity.Web.UI`

### 2. Add configuration
Add an `AzureAd` section in `src/App.Host/appsettings.json`:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "SET_VIA_USER_SECRETS_OR_ENV",
    "CallbackPath": "/signin-oidc",
    "SignedOutCallbackPath": "/signout-callback-oidc"
  }
}
```

Security note:
- Do not commit a real client secret. Use user secrets or environment variables.

### 3. Configure services
In `src/App.Host/Program.cs`:
1. Add auth + authorization services.
2. Enable Blazor auth state cascading + serialization.

Planned shape:
- Authentication:
  - OIDC + cookies via Microsoft Identity Web.
- Authorization:
  - `AddAuthorization()` with at least a default authenticated policy.
- Blazor auth state:
  - `AddCascadingAuthenticationState()`
  - `AddAuthenticationStateSerialization()`

### 4. Configure middleware
In `src/App.Host/Program.cs`, before endpoint mapping:
1. `app.UseAuthentication();`
2. `app.UseAuthorization();`

### 5. Add explicit login/logout endpoints
Add endpoints in `App.Host` for predictable sign-in/out flows:
- `GET /auth/login?returnUrl=/path`
  - Issues a challenge.
- `POST /auth/logout?returnUrl=/`
  - Signs out locally and from Entra.

This makes it easy for both server and WASM UI to trigger auth flows.

## Phase 2: Protect API Endpoints In `App.Api`
Status: Not started.

### 1. Require authorization by default for API routes
Update `src/App.Api/ApiModule.cs` to:
- Create a route group for `/api`.
- Apply `.RequireAuthorization()` at the group level.
- Keep a small allow-anonymous surface if needed (for health/auth bootstrap).

Example intent (not exact code):
- `var api = app.MapGroup("/api").RequireAuthorization();`
- Map existing endpoints on `api`.

### 2. Decide how `App.Api` authenticates when run standalone
There are two good options:

Option A (fastest, good enough for now)
- Treat standalone `App.Api` as a secondary runtime.
- Require auth only when run via `App.Host`.
- Document that Entra SSO is supported via `App.Host`.

Option B (more complete)
- Add bearer token auth to `App.Api` so it can run independently.
- Later, add a policy scheme that accepts cookie (hosted) or bearer (standalone/3rd-party).

Recommendation:
- Start with Option A to reduce moving parts, then add bearer in a follow-up.

## Phase 3: Flow Auth State Into `App.Ui.Client`
Status: Not started.

### 1. Deserialize auth state in the WASM client
In `src/App.Ui.Client/Program.cs`:
- Add:
  - `builder.Services.AddAuthorizationCore();`
  - `builder.Services.AddCascadingAuthenticationState();`
  - `builder.Services.AddAuthenticationStateDeserialization();`

This is the Blazor Web App pattern that allows the server-authenticated user
to be known by the WASM client.

### 2. Ensure cookies flow on client API calls
Because the WASM client calls same-origin endpoints hosted by `App.Host`,
ensure browser credentials are included for fetch requests.

Planned adjustment:
- Configure the WASM `HttpClient` to include credentials where needed.

## Phase 4: UI Integration (Login, Logout, Authorization UX)
Status: Not started.

### 1. Add login/logout UI affordances
Add small, explicit sign-in/out controls in shared layout:
- Server layout: `src/App.Ui/Components/Layout/`
- Client pages can also link to `/auth/login`.

### 2. Use `AuthorizeView` and/or `[Authorize]`
In relevant pages/components:
- Wrap authenticated-only content with `AuthorizeView`.
- Add `[Authorize]` on pages that require sign-in.

## Phase 5: Testing And Developer Experience
Status: Not started.

### 1. Add a test/dev auth mode (important for E2E)
Real Entra flows are not reliable in CI/E2E. Add a test auth path:
- A simple development/test authentication scheme (header or query-based).
- Gate it behind environment checks and explicit config flags.

Recommended approach:
1. Add a lightweight test auth handler in `App.Host`.
2. Enable it only when `DOTNET_ENVIRONMENT=Development` or a config flag is set.
3. Provide a predictable endpoint like:
   - `POST /auth/test-login?user=alice`

### 2. Update tests
- API tests:
  - Add a test auth scheme in `App.Api.Tests` or host them through `App.Host`.
  - Verify 401/403 behavior and authenticated access.
- E2E tests:
  - Use test auth mode to sign in without Entra.
  - Then validate authorized pages and API calls.

## Suggested Implementation Order (Low-Risk Path)
Status: Not started.

1. Phase 1: wire Entra auth in `App.Host` only.
2. Phase 3: deserialize auth state in `App.Ui.Client`.
3. Phase 4: add login/logout UX and protect one or two pages.
4. Phase 2: protect `/api` routes (start with a minimal allowlist).
5. Phase 5: add test auth mode and update API/E2E tests.

## Open Questions To Resolve Early
Status: Open.

1. Should API endpoints require auth by default, or only specific ones?
2. Do we need `App.Api` to be Entra-protected when run standalone?
3. Will we model authorization via:
   - Entra groups
   - App roles
   - A simple “authenticated user” policy (initially)

## Definition Of Done (SSO)
Status: Not started.

- `dotnet run --project src/App.Host` supports Entra sign-in.
- An authenticated user is visible via `AuthorizeView` in both server and WASM contexts.
- At least one `/api/*` endpoint is protected and returns 401 when anonymous.
- API + E2E tests can run without real Entra using test auth mode.

