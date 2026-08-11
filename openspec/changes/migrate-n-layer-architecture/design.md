## Context

See proposal.md "Why" for the motivation behind this migration.

Current single `HackerRank1` WebAPI project mixes layers in one assembly with inconsistent `LibraryService.WebAPI.*` / `HackerRank1.*` namespaces, defines `Book`/`Library` entities inside `Data/LibraryContext.cs`, has no repository layer (services talk directly to `LibraryContext`), stubs several CRUD methods with `NotImplementedException`, and ships two near-duplicate integration-test projects (only `LibraryService.Integration.Test`, net8, is in the `.sln`; `IntegrationTest/` net6 is dead). EF Core 8 + Npgsql targets a Supabase Postgres DB; the runtime runs `Migrate()` on startup.

This change uses **classic N-Layer** (technical layers, dependencies downward), not Clean Architecture (Domain/Application/Infrastructure with inward dependency inversion). Clean will be a separate change on `lab1/clean`.

## Goals / Non-Goals

**Goals:**
- Split the solution into four projects: `LibraryService.Entities`, `LibraryService.BusinessLogic`, `LibraryService.DataAccess`, `LibraryService.Api`, with references flowing **downward only**.
- Give each assembly a canonical namespace matching its layer (`LibraryService.Entities`, `LibraryService.BusinessLogic`, `LibraryService.DataAccess`, `LibraryService.Api`).
- Introduce a repository layer in DataAccess so BusinessLogic services depend on repository abstractions in DataAccess, not the EF `DbContext` directly.
- Complete the stubbed CRUD (library delete; book add/update/delete) so the API remains fully functional.
- Consolidate to a single integration-test project that drives the Api over HTTP.
- Keep the public HTTP contract (routes, JSON shapes, status codes) identical to today.

**Non-Goals:**
- No new API endpoints, no request/response shape changes.
- No database schema changes (existing migration/model preserved).
- Not reworking the hardcoded admin/1234 credential check in `AuthenticationService` (out of scope; noted as debt).
- No Clean-style dependency inversion (DataAccess must not reference BusinessLogic).
- No change to the auth design beyond relocating its types.

## Decisions

**1. Project layout and namespaces**
Four projects. Namespaces match layer names (optionally with `.*` subfolders).

```
LibraryService.Entities        (no project refs, no EF/framework deps)
LibraryService.DataAccess      → Entities
LibraryService.BusinessLogic   → DataAccess → Entities
LibraryService.Api             → BusinessLogic, DataAccess, Entities
LibraryService.Integration.Test → Api
```

Api references DataAccess so the composition root can register `DbContext` and repository implementations. DataAccess does **not** reference BusinessLogic.

**Decision:** Shared models and DTOs live in Entities; DbContext stays in DataAccess.
`Library`, `Book`, DTOs (`LibraryForm`, `BookForm`, `User`), and `JwtSettings` move to `LibraryService.Entities` as POCOs (no EF package). `[Key]` may remain as `System.ComponentModel.DataAnnotations` or be replaced by fluent `OnModelCreating` in DataAccess — schema must stay unchanged. `LibraryContext` stays in `LibraryService.DataAccess.Data`.

**Decision:** Repository interfaces and implementations live in DataAccess.
- `ILibraryRepository` / `IBookRepository` and their EF implementations sit in DataAccess.
- BusinessLogic services reference those repository types via the DataAccess project reference (classic N-Layer downward call).
- Rationale: technical DAL owns persistence contracts; avoids Clean-style “ports in Application / adapters in Infrastructure”.

**Decision:** Controllers bind DTOs; services own entity mapping.
Controllers take `BookForm`/`LibraryForm`/`User` DTOs (Entities) and return DTO/entity shapes consistent with today’s JSON; services map as needed. Field names match 1:1; mapping is manual (no mapper library). Rationale: removes “controller binds entity from Data folder” smell while preserving the HTTP contract.

**Decision:** Composition root stays in the Api project.
`Program.cs`/`Startup.cs` in `LibraryService.Api` registers JWT settings, auth (`JwtBearer`), EF/Npgsql, DI, CORS, Swagger, controllers/middleware. It registers repositories (DataAccess), services (BusinessLogic), and `ITokenService`. Controllers only receive interfaces.

**Decision:** Auth/JWT behavior preserved, but contract wins.
`/login` and token generation stay (moved to BusinessLogic). The current `GET /api/libraries/{libraryId}/books` carries `[Authorize]`, but the specified contract and existing integration tests call it without a token. Keep CRUD endpoints anonymous for this change; JWT remains available for later. Recorded as a deliberate behavior note (see Risks).

**Decision:** Migration re-home, not regenerate.
EF migrations and `LibraryContextModelSnapshot` move into `LibraryService.DataAccess` and re-point type refs to `LibraryService.Entities` models. Schema unchanged; regenerate only if designer refs cannot be cleanly re-pointed.

**Decision:** One test project, SQLite in-memory.
Keep `LibraryService.Integration.Test` (net8, xUnit + FluentAssertions via `WebApplicationFactory`), delete `IntegrationTest/`. Swap `LibraryContext` for SQLite in-memory, seed, and run contract scenarios unauthenticated. Update namespaces to the new projects.

## Risks / Trade-offs

- [Contract/auth mismatch] Existing `[Authorize]` on GET books conflicts with anonymous `200`/`404`. → Keep endpoints public for this change; gate later if needed.
- [Migration type refs] Moving entities changes type refs in `Migrations/*.Designer.cs`. → Re-point to Entities types; verify `Migrate()`; regenerate only if needed.
- [Assembly rename] Renaming `HackerRank1` alters test/`WebApplicationFactory<Program>` binding. → Update tests and Program namespace together.
- [Confusion with Clean naming] Using Domain/Application/Infrastructure would blur the lab. → Classic Api/BLL/DAL/Entities names are mandatory for this branch.

## Migration Plan

1. Create the four projects + solution references (Entities ← DataAccess ← BusinessLogic ← Api) with namespaces.
2. Move models/DTOs/settings → Entities; create repository interfaces + implementations in DataAccess; implement services in BusinessLogic (complete stubs, map DTO flows).
3. Move `LibraryContext` and migrations → DataAccess.
4. Move controllers, `Program.cs`/`Startup` composition root, config, CORS, auth, Swagger → Api; register everything via DI.
5. Reconcile tests; delete duplicate test project; point tests at Api and run.
6. Run build + full test suite; smoke-test Swagger.

## Migration-Preservation Strategy

Goal: never recreate, rename, or drop the existing Supabase `Libraries`/`Books` tables, and keep the `InitialCreate` migration intact.

1. **Move, don't regenerate.** Files under `HackerRank1/Migrations/` move to `LibraryService.DataAccess/Migrations/` with namespace updated to `LibraryService.DataAccess.Migrations`.
2. **Keep entity type names stable.** `Library` and `Book` keep class and table names. Fluent config (if used) must preserve `Libraries`/`Books`, `Book.LibraryId` FK cascade, and identity `Id`.
3. **Re-point designer type refs.** Context/entity type names in designer/snapshot update to new namespaces; table/column definitions do NOT change.
4. **No new migration** unless designer refs cannot be fixed cleanly; prefer move-as-is.
5. **Checkpoint before moving.** Baseline with `dotnet ef migrations list` / `has-pending-model-changes`; re-check after the move.
6. **Rollback is a file restore.** `git revert` restores layout; no DB undo needed if schema untouched.

## File / Folder Inventory

**Created (new projects):**

```
LibraryService.Entities/Models/Library.cs
LibraryService.Entities/Models/Book.cs
LibraryService.Entities/DTO/LibraryForm.cs
LibraryService.Entities/DTO/BookForm.cs
LibraryService.Entities/DTO/User.cs
LibraryService.Entities/Settings/JwtSettings.cs
LibraryService.BusinessLogic/Interfaces/*.cs
LibraryService.BusinessLogic/Services/*.cs
LibraryService.DataAccess/Data/LibraryContext.cs
LibraryService.DataAccess/Repositories/ILibraryRepository.cs
LibraryService.DataAccess/Repositories/IBookRepository.cs
LibraryService.DataAccess/Repositories/LibraryRepository.cs
LibraryService.DataAccess/Repositories/BookRepository.cs
LibraryService.DataAccess/Migrations/*.cs
LibraryService.Api/Controllers/*.cs, Program.cs, Startup.cs,
  appsettings.json, appsettings.Development.json, Properties/
```

**Moved** — all source paths listed in proposal.md inventory.
**Deleted** — `HackerRank1/` (superseded), `IntegrationTest/` (dead net6 duplicate).
**Modified** — `HackerRank1.sln`, each `.csproj`, namespaces in every moved file.

## Verification Steps

1. `dotnet build` — no compile errors.
2. `dotnet test` (integration suite) — contract + CRUD scenarios pass.
3. `dotnet ef migrations list` — exactly one migration, `InitialCreate` / `Initial Create`, unchanged.
4. `dotnet ef migrations has-pending-model-changes` — no pending changes (schema preserved).
5. Manual Swagger smoke test of every documented endpoint (see `HackerRank1/README.md`).

Rollback: `git revert` of the relocate+complete-CRUD commit. No DB schema changes.

## Open Questions

- Whether `[Authorize]` should later be re-applied on books endpoints (future change; out of scope here).
