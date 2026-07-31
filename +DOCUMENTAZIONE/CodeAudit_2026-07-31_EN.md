# Code Audit — Gestionale (API + DATAACCESS + DOMAIN + Blazor Client)

## Context

Full codebase review requested: is it better to keep improving existing code, or move on to new features? Two parallel review passes scanned backend (API/DATAACCESS/DOMAIN) and frontend (Blazor Client). Findings below. Recent commits show an active refactor already in progress (generic CRUD abstraction `XpoCrudServiceBase`), so this audit lands mid-refactor — some findings are "finish what you started" items.

No test project exists anywhere in the solution (backend or frontend).

## Findings — Backend (API / DATAACCESS / DOMAIN)

**Security / correctness (fix first, small effort):**
- `API/appsettings.json:18` — placeholder JWT secret committed. Must move to env var/user-secrets before any real deploy.
- `DATAACCESS/Datacontext/DataSeeder.cs:45` — seeded admin has hardcoded password `Admin123!`, no forced change.
- `DATAACCESS/Services/TokenBlacklistService.cs:38,59-81` — fire-and-forget task with empty `catch{}`, swallows pruning failures silently.
- `API/helpers/XpoSerilogSink.cs:70-77` — same silent-swallow pattern for logging sink failures.
- `API/Controllers/AuthController.cs:63-81` — `Logout` no-ops (still 204) when JWT has no `jti`, token never revoked.
- `API/Controllers/AuthController.cs` (`Login`) — no rate limiting/lockout, brute-forceable.
- `MappingProfile.cs:155-171` / `ProjectService.cs:44-47` — invalid FK throws `InvalidOperationException` mapped to HTTP 409, should be 400/422 (validation, not conflict).

**Architecture (finish the refactor):**
- `DeleteGuard.cs` dead code — never called, duplicated inline in `XpoCrudServiceBase.cs:102-108`.
- `WorkLogServices.cs`, `LogService.cs`, `ReportService.cs` never migrated to `XpoCrudServiceBase` — still hand-rolled CRUD.
- `WorkLogAdminService`/`WorkLogUserService` duplicate near-identical CRUD, differ only by ownership check.
- `XpoCrudServiceBase.cs:44-61` — `GetAllAsync`/`GetByIdAsync` wrap sync XPO calls in `Task.FromResult`, no real async I/O (leaky abstraction across all 5 consumers).
- `WorklogController.cs:112-120,148-156` — duplicated Admin→User DTO mapping between Create/Update.

**Maintainability:**
- `AuthService.cs:39-70` — `LoginAsync` returns a 7-element tuple, should be a record.
- `ProjectController.cs:53-93` — missed formatting/refactor pass done elsewhere.
- Hardcoded Italian error strings scattered per-service, not centralized.
- `EmployeeService` has friendly uniqueness pre-check, `Company` doesn't — inconsistent UX on duplicate names.
- `Program.cs:22-23` — SQLite connection string duplicated as inline fallback vs appsettings.json.

**Positive:** new `XpoCrudServiceBase` abstraction is applied correctly on 5 services. No SQL injection, no CORS misconfig, no `.Result`/`.Wait()`/`async void` misuse in backend.

## Findings — Frontend (Blazor Client)

**Security:**
- `Services/TokenStorageService.cs:26,31` — JWT + claims in `localStorage`, readable by any XSS. Consider in-memory + silent refresh, or at minimum `sessionStorage`.
- `wwwroot/index.html:12` — bootstrap-icons CDN link missing SRI (`integrity`/`crossorigin`), inconsistent with Bootstrap CSS above it.
- No CSP meta tag anywhere.
- `ReportDownloadModal.razor:106-108` — unsanitized `_fileName` passed to `a.download`.
- Client-side role checks are UI-only — confirm backend re-validates roles per-endpoint independently (it does, per backend review, but worth a note).

**Correctness:**
- `Dashboard.razor.cs:66-72` — `GetCurrentUserId()` blocks with `.GetAwaiter().GetResult()` instead of awaiting.
- `MainLayout.razor:340-347` — `System.Timers.Timer.Elapsed` calls `InvokeAsync` with no disposed-guard, races with `Dispose()`.
- `ReportDataApiClient.cs:24-28` — missing try/catch around HTTP call, inconsistent with `WorkLogApiClient`'s pattern.
- `Dashboard.razor.cs:113,154` — `async void` handlers on service events, fragile.
- `FilterStateService.cs:161-186` — `LoadLookupsAsync` has no `CancellationToken`, can't supersede overlapping calls (sibling `ReloadLookupsAsync` already does this correctly).
- Failed lookup reloads only `Console.Error.WriteLine`, no user-visible feedback.

**Architecture:**
- `CompanyApiClient/EmployeeApiClient/ProjectApiClient/StatusApiClient/TypeApiClient` — 5x near-identical CRUD wrapper, same shape as the backend's pre-refactor duplication. Natural candidate for a generic `CrudApiClient<TResponse,TCreate,TUpdate>`.
- `Admin/Services/{Companies,Statuses,Types}/*.razor.cs` — same open/close-modal/save/delete/reload skeleton copy-pasted per entity.
- Two parallel DTO hierarchies for the same entities (e.g. `EmployeeResponse` vs `EmployeeResponseDto`).
- Loading/error UI blocks hand-copied across Admin pages.
- 4 inconsistent state-management patterns in use (pub/sub services, cascading auth state, prop-drilling, local fields), no documented convention.

**Maintainability:**
- API route strings hardcoded across 9 files, no `ApiRoutes` constants class (despite `Constants/RoleNames` already existing as a pattern).
- Page routes duplicated between `Sidebar.razor` and each page's `@page` directive.
- `RoleNames.cs` is a hand-maintained mirror of a backend enum, no compile-time sync check.

**Positive:** no hardcoded secrets, no raw-HTML/`MarkupString` XSS vectors, open-redirect protection in `Login.razor` correct, `IDisposable` hygiene solid elsewhere.

## Recommendation

Codebase is in decent shape mid-refactor, not a mess. Before new features:

1. **Quick security fixes (hours, do first regardless of what's next):** JWT secret out of appsettings, seeded admin password, `Logout` jti gap, login rate limiting, CDN SRI, CSP header, filename sanitization.
2. **Finish the CRUD refactor (both layers)** — mirror the backend `XpoCrudServiceBase` pattern into a frontend `CrudApiClient<T>` + shared Admin page base. This is the same shape of work already validated once (bb061b0/c7493d8), low risk, pays down the largest duplication block on both sides.
3. **Then new features** — foundation will be more consistent to build on, and route/DTO drift won't compound.

Skip: full test suite retrofit (real project, separate scope decision), tuple→record and other pure style nits (bundle into the refactor pass above, not standalone work).

## Decision

Reviewed on 2026-07-31: report only, no action taken at that time. This document stands as reference for prioritization whenever work resumes.
