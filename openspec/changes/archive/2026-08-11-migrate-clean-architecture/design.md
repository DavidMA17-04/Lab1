## Context

Branch `lab1/clean` starts from the monolith `HackerRank1` (.NET 8 Web API): mixed namespaces (`LibraryService.WebAPI.*` / `HackerRank1.*`), entities co-located with `LibraryContext`, services depending directly on EF `DbContext`, JWT helper as a static class, stubbed CRUD (`LibrariesService.Delete`, `BooksService.Add/Update/Delete`), duplicate integration test projects, and EF Core 8 + Npgsql against Supabase with migration `20260528004745_InitialCreate`.

This design implements **Clean Architecture** (Dependency Rule inward), not N-Layer (`WebAPI → BLL → DAL → Entities`), and not Vertical Slice/CQRS/MediatR.

## Goals / Non-Goals

**Goals:**
- Four projects with explicit allowed references (see Decision 1).
- Dependency inversion: Application owns repository + `ITokenService` abstractions; Infrastructure implements them.
- Application stays free of JWT packages and concrete token/settings types.
- Controllers depend only on Application abstractions; WebAPI→Infrastructure is composition-only.
- Preserve exact HTTP contract; complete only documented stubbed CRUD.
- Preserve `20260528004745_InitialCreate` semantically; no schema churn.
- Integration tests use isolated persistence (SQLite), never shared Supabase for CRUD.
- Keep .NET 8 / EF 8.0.2 / Npgsql 8.0.2 / existing compatible test stack.

**Non-Goals:**
- No MediatR, AutoMapper, FluentValidation, CQRS, one-class-per-use-case ceremony, generic repository frameworks.
- No Vertical Slice / Onion / Hexagonal beyond Clean’s four layers.
- No API redesign or unrelated cleanup/modernization/renaming.
- No rewrite of hardcoded `admin`/`1234` auth rule (relocate only).
- No unnecessary package version changes.

## Decisions

**1. Final project dependency graph (mandatory)**

Allowed:

```
Domain              → (no solution project)
Application         → Domain
Infrastructure      → Application + Domain
WebAPI              → Application + Infrastructure   (Infrastructure = composition/host init only)
Integration.Test    → WebAPI (+ types needed for test doubles)
```

Forbidden (must not appear as project references or controller usings of Infrastructure concretes for request handling):

```
Domain → Application | Infrastructure | WebAPI
Application → Infrastructure | WebAPI
Infrastructure → WebAPI
any circular reference
WebAPI → BusinessLogic → DataAccess → Entities   (N-Layer; other branch)
```

**2. JwtSettings placement (chosen)**

`JwtSettings` is technical auth/token configuration, **not** Domain and **not** Application.

**Decision:** place `JwtSettings` in **Infrastructure** (e.g. `Infrastructure/Auth/JwtSettings.cs`).

Rationale: the concrete `TokenService` consumes issuer/audience/secret; WebAPI binds `IConfiguration` → `JwtSettings` and registers it for JwtBearer middleware and `TokenService`. Application never references `JwtSettings` or signing secrets.

`ITokenService` stays framework-independent in Application, e.g. `string GenerateToken(User user)` (settings injected only into Infrastructure `TokenService`).

**3. JWT implementation outside Application**

```
Application     → ITokenService (abstraction only; no IdentityModel packages)
Infrastructure  → TokenService (JwtSecurityTokenHandler, signing, claims, credentials)
WebAPI          → registers TokenService for ITokenService at composition root
```

Application MUST NOT depend on Infrastructure to obtain tokens.

**4. WebAPI → Infrastructure is composition-only**

WebAPI may reference Infrastructure to register `LibraryContext`, repositories, `TokenService`, and bind `JwtSettings`.

Controllers SHALL NOT inject or `new`:
- `LibraryContext`
- EF Core types
- concrete repositories
- Infrastructure `TokenService` (use `ITokenService`)
- other persistence-specific Infrastructure types

Normal flow:

```
HTTP → Controller → Application Service → I*Repository → Infrastructure Repository → DbContext → Database
```

Login:

```
HTTP → AuthController → IAuthenticationService + ITokenService
```

**5. Simple Application structure (no overengineering)**

Acceptable layout:

```
Application/Interfaces/ILibrariesService.cs, IBooksService.cs, IAuthenticationService.cs
Application/Interfaces/ILibraryRepository.cs, IBookRepository.cs, ITokenService.cs
Application/Services/LibrariesService.cs, BooksService.cs, AuthenticationService.cs
Application/DTOs/...
```

Do not invent one handler class per use case, MediatR, CQRS, AutoMapper, or FluentValidation unless a requirement strictly forces it (none do).

**6. Layer file mapping**

| Current | Target |
|---|---|
| `Data/Library`, `Book` | `Domain/Entities/` |
| `DTO/*` | `Application/DTOs/` |
| `Entities/JwtSettings` | `Infrastructure/Auth/JwtSettings.cs` (not Application/Domain) |
| Service interfaces + impls | `Application/Interfaces/` + `Application/Services/` |
| New repo + `ITokenService` interfaces | `Application/Interfaces/` |
| `LibraryContext`, Migrations | `Infrastructure/Persistence/` (+ `Migrations/`) |
| Repository impls | `Infrastructure/Persistence/Repositories/` |
| `TokenGenerator` → `TokenService` | `Infrastructure/Auth/` |
| Controllers, Program, Startup, appsettings | `WebAPI/` |

**7. Dependency inversion for persistence**
- Application services use `ILibraryRepository` / `IBookRepository` only—never `LibraryContext`.
- Infrastructure repositories implement those interfaces over EF.

**8. Domain purity**
- Entities are POCOs without EF/ASP.NET/Npgsql/JWT package references.
- Fluent `OnModelCreating` in Infrastructure preserves `Libraries`/`Books`, identity, and cascade FK semantics of the existing model.

**9. HTTP contract preservation (exact)**
- Same routes, request/response JSON, DTO semantics, status codes, login token response shape, Swagger surface, and currently working CRUD behavior.
- Only implement operations currently stubbed with `NotImplementedException` that the documented contract requires.
- Do not “improve” unrelated endpoint behavior.
- Books list (and contract tests) remain usable without requiring Bearer; JWT pipeline stays configured (documented pragmatic decision).

**10. EF migration: organizational move vs semantic identity**

Allowed:
- Relocate migration files to Infrastructure.
- Update C# namespaces and designer/snapshot CLR type names to Domain entities.
- Set `MigrationsAssembly` to Infrastructure.

Not allowed (schema churn):
- Changing `Up`/`Down` operations, table/column names/types, PKs/FKs/constraints/relationships.
- Adding a new migration for the refactor.
- Recreating/renaming/dropping tables or data in Supabase.

After apply: `dotnet ef migrations list` shows only `20260528004745_InitialCreate`; `has-pending-model-changes` reports none.

**11. Integration tests must not touch shared Supabase**
- Keep SQLite in-memory (or equivalent isolated) replacement for `LibraryContext`.
- Integration tests SHALL never execute destructive or state-changing CRUD against the shared Supabase database.
- Skip Npgsql `Migrate()` when the provider is not Npgsql.

**12. Scope protection**
Files/config/behavior/endpoints outside Clean migration needs SHALL NOT be modified unnecessarily. Changes are limited to: Clean boundaries, documented stubbed CRUD, DI/project references, and preserving compile/runtime after relocation.

**13. Versions**
.NET 8; EF Core 8.0.2; Npgsql 8.0.2; JwtBearer 8.0.16 and existing compatible test packages. Relocate packages between projects as needed; do not upgrade/downgrade without cause.

## Risks / Trade-offs

- **Model drift after entity move** → Re-point snapshot type names; gate on `has-pending-model-changes`.
- **Confusion with N-Layer** → Inward refs + Application-owned ports are mandatory differentiators.
- **Controllers bypassing Application** → Spec/tasks forbid Infrastructure types in controllers despite WebAPI project reference.
- **Bearer double-registration in tests** → Avoid double `UseStartup`; override services carefully.
- **Auth vs anonymous contract** → Prefer documented contract/tests; JWT remains available.
- **Accidental Supabase writes in tests** → Enforce SQLite swap + verification checklist item.

## Migration Plan

1. Create projects; wire **only** allowed references; scope NuGet.
2. Domain entities; Application DTOs/interfaces/services (no JwtSettings/JWT packages).
3. Infrastructure: DbContext, repos, migrations (semantic preserve), `JwtSettings`, `TokenService`.
4. WebAPI host/controllers; composition root registers Infrastructure concretes; controllers use Application only.
5. Adapt integration tests to isolated DB; delete dead test project; update solution.
6. Run full verification checklist.

## Migration-Preservation Strategy

1. Move `20260528004745_InitialCreate*.cs` + snapshot; namespace → `LibraryService.Infrastructure.Migrations`.
2. Keep `Up`/`Down` operations byte-compatible in schema effect (same tables/columns/FKs/constraints).
3. Update designer/snapshot entity CLR names to `LibraryService.Domain.Entities.*`; keep Npgsql identity annotations.
4. Baseline and post-check: `migrations list` + `has-pending-model-changes`.
5. Rollback = git revert; no DB undo if schema untouched.

## File / Folder Inventory

```
LibraryService.Domain/
  Entities/Library.cs, Book.cs
LibraryService.Application/
  DTOs/LibraryForm.cs, BookForm.cs, User.cs
  Interfaces/ILibrariesService.cs, IBooksService.cs, IAuthenticationService.cs,
             ILibraryRepository.cs, IBookRepository.cs, ITokenService.cs
  Services/LibrariesService.cs, BooksService.cs, AuthenticationService.cs
LibraryService.Infrastructure/
  Auth/JwtSettings.cs, TokenService.cs
  Persistence/LibraryContext.cs
  Persistence/Repositories/LibraryRepository.cs, BookRepository.cs
  Persistence/Migrations/*
LibraryService.WebAPI/
  Controllers/*, Program.cs, Startup.cs, appsettings*.json, Properties/
LibraryService.Integration.Test/   (SQLite isolated; IntegrationTest/ deleted)
```

**Deleted:** `HackerRank1/` (superseded), `IntegrationTest/`.
**Modified:** `HackerRank1.sln` project list/refs.

## Verification Steps

1. `dotnet restore`
2. `dotnet build`
3. `dotnet test`
4. Verify project references match Decision 1 (allowed/forbidden).
5. Confirm Domain has no EF Core, ASP.NET Core, Npgsql, JWT infrastructure, or persistence packages/usings.
6. Confirm Application does not reference Infrastructure or WebAPI and has no JWT/IdentityModel packages.
7. Confirm Application services use repository abstractions, not `LibraryContext`.
8. Confirm Infrastructure implements Application repository and `ITokenService` abstractions; `JwtSettings` lives in Infrastructure.
9. Confirm WebAPI controllers depend on Application abstractions only (no Infrastructure concretes / `LibraryContext` / EF in controllers).
10. `dotnet ef migrations list`
11. Confirm only `20260528004745_InitialCreate` exists.
12. `dotnet ef migrations has-pending-model-changes`
13. Confirm no pending model changes.
14. Start `LibraryService.WebAPI` successfully.
15. Confirm startup migration handling finds existing schema without recreate/alter.
16. Verify Swagger loads.
17. Exercise documented HTTP contract (routes, payloads, status codes, login, CRUD) unchanged except newly completed stubs.
18. Confirm integration tests use isolated persistence and do not modify shared Supabase.
19. Confirm no credentials/secrets were introduced into generated or tracked planning artifacts.

## Open Questions

- None blocking apply; books endpoints remain ungated by Bearer for contract/test parity (JWT pipeline still registered).
