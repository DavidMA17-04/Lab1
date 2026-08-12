## Why

The `HackerRank1` WebAPI currently mixes controllers, EF Core `DbContext`, entities, DTOs, services, JWT helpers, and migrations in a single project with inconsistent namespaces (`LibraryService.WebAPI.*` vs `HackerRank1.*`). Application services depend directly on `LibraryContext`, so business orchestration is coupled to persistence. Several contract-required CRUD operations still throw `NotImplementedException`. This change migrates the solution to **Clean Architecture** so dependencies point **inward** toward Domain, Infrastructure implements Application abstractions, and the public HTTP behavior remains intact—distinct from the separate N-Layer migration on another branch.

## What Changes

- Split the monolith into four projects with Clean Architecture responsibilities:
  - **Domain** — `Library` and `Book` entities only; zero project references; no EF/ASP.NET/Npgsql/JWT packages.
  - **Application** — simple application service interfaces/implementations, repository abstractions, DTOs, `ITokenService` abstraction, auth application service; depends **only** on Domain. No JWT packages, no concrete token implementation, no `JwtSettings`.
  - **Infrastructure** — `LibraryContext`, repository implementations, EF migrations, concrete `TokenService` (JWT), and `JwtSettings` (technical auth/token options); depends on Application (and Domain).
  - **WebAPI** — controllers, `Program.cs`/`Startup.cs` composition root, JWT bearer/CORS/Swagger pipeline, configuration binding; may reference Infrastructure **only** for composition/host initialization, not for controller use-case flow.
- Enforce the Dependency Rule via project references:
  - Allowed: `Domain` (none); `Application → Domain`; `Infrastructure → Application` (+ Domain); `WebAPI → Application + Infrastructure`.
  - Forbidden: `Domain → *`; `Application → Infrastructure|WebAPI`; `Infrastructure → WebAPI`; circular refs; N-Layer `WebAPI → BusinessLogic → DataAccess → Entities`.
- Controllers SHALL call Application abstractions only. They SHALL NOT reference `LibraryContext`, EF types, concrete repositories, or other Infrastructure services.
- Request flow: `HTTP → Controller → Application Service → Repository Abstraction → Infrastructure Repository → DbContext → Database`.
- Keep Application simple (service + repository interfaces/implementations). No MediatR, AutoMapper, FluentValidation, CQRS, or generic-repository frameworks.
- Complete only stubbed CRUD required by the documented contract; do not redesign unrelated endpoint behavior.
- Preserve public HTTP contract exactly (routes, payloads, DTO semantics, status codes, login token response, auth pipeline availability, Swagger).
- Relocate `20260528004745_InitialCreate` to Infrastructure (namespace updates allowed); migration **operations and resulting schema** remain semantically unchanged—no schema churn.
- Integration tests use isolated SQLite (or equivalent) persistence and SHALL NOT run destructive/state-changing CRUD against shared Supabase.
- Scope protection: do not modify unrelated files/behavior except as required for Clean boundaries, stubbed-CRUD completion, DI/refs, or compile/runtime after relocation.
- **BREAKING** assembly layout only: `HackerRank1.csproj` superseded by `LibraryService.WebAPI` (+ class libraries). External HTTP contract is **not** breaking.
- Stay on .NET 8 / EF Core 8.0.2 / Npgsql 8.0.2 / existing compatible test-stack versions (relocate packages; do not upgrade/downgrade unnecessarily).

## Capabilities

### New Capabilities
- `clean-architecture`: Clean Architecture project boundaries, inward-only dependencies, Application-owned abstractions (services + repositories + `ITokenService`), Infrastructure adapters (EF + JWT impl + `JwtSettings`), WebAPI composition-only Infrastructure usage, preserved HTTP/auth/Swagger behavior, functional documented CRUD, isolated integration tests, and EF migration/schema preservation.

### Modified Capabilities
- None (this branch has no existing `openspec/specs/` capabilities).

## Impact

- **Code**: `HackerRank1/` relocated into Domain/Application/Infrastructure/WebAPI; dead `IntegrationTest/` removed; solution graph rewritten. Unrelated modernization/renames out of scope.
- **APIs**: Externally compatible contract preserved. Only previously stubbed contract operations gain implementations.
- **Dependencies**: Explicit allowed/forbidden project refs as above. IdentityModel/JWT packages in Infrastructure (and JwtBearer in WebAPI); Application has no JWT infrastructure packages.
- **Tests**: Single `LibraryService.Integration.Test` against WebAPI with isolated DB; never mutates shared Supabase.
- **Database**: `20260528004745_InitialCreate` remains the only migration; `has-pending-model-changes` must be none; Supabase schema/data intact.
- **Credentials**: Secrets stay in host config; never copied into planning artifacts, logs, or generated docs.
- **Versions**: .NET 8, EF Core 8.0.2, Npgsql 8.0.2, existing compatible test packages.
