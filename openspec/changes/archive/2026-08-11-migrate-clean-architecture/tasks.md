## 1. Project scaffolding

- [x] 1.1 Create `LibraryService.Domain`, `LibraryService.Application`, `LibraryService.Infrastructure`, and `LibraryService.WebAPI` projects (net8.0) with root namespaces matching each layer
- [x] 1.2 Add **only** allowed project references: Application → Domain; Infrastructure → Application (+ Domain); WebAPI → Application + Infrastructure; Domain has none
- [x] 1.3 Confirm forbidden references are absent: Domain→Application/Infrastructure/WebAPI; Application→Infrastructure/WebAPI; Infrastructure→WebAPI; no circular refs; no N-Layer BLL/DAL graph
- [x] 1.4 Scope NuGet packages without version churn: EF Core 8.0.2 + Npgsql 8.0.2 + Design → Infrastructure; JwtBearer 8.0.16 + Swashbuckle + Newtonsoft as needed → WebAPI; IdentityModel/JWT implementation packages → Infrastructure only; Application and Domain have no EF/ASP.NET/Npgsql/JWT implementation packages
- [x] 1.5 Update `HackerRank1.sln`; remove obsolete `HackerRank1` / dead `IntegrationTest` when ready

## 2. Domain layer

- [x] 2.1 Move `Library` and `Book` into `LibraryService.Domain.Entities` as POCOs without EF/ASP.NET/Npgsql/JWT dependencies
- [x] 2.2 Ensure Domain `.csproj` has no infrastructure/framework PackageReferences listed above

## 3. Application layer (keep simple)

- [x] 3.1 Move `LibraryForm`, `BookForm`, and `User` into `Application/DTOs`
- [x] 3.2 Define `ILibraryRepository` and `IBookRepository` in `Application/Interfaces`
- [x] 3.3 Define `ILibrariesService`, `IBooksService`, `IAuthenticationService`, and framework-independent `ITokenService` in `Application/Interfaces` (no JwtSettings parameters required on Application APIs)
- [x] 3.4 Implement `LibrariesService` and `BooksService` in `Application/Services` depending on repository interfaces only (not `LibraryContext`), completing only documented stubbed CRUD
- [x] 3.5 Implement `AuthenticationService` in Application preserving existing admin/1234 behavior
- [x] 3.6 Confirm Application does **not** contain `JwtSettings`, concrete JWT implementation, IdentityModel packages, Infrastructure references, or MediatR/CQRS/AutoMapper/FluentValidation

## 4. Infrastructure layer

- [x] 4.1 Place `JwtSettings` in Infrastructure (e.g. `Infrastructure/Auth/JwtSettings.cs`), not Domain/Application
- [x] 4.2 Move `LibraryContext` into Infrastructure Persistence; fluent-map `Libraries`/`Books`, identity keys, and cascade FK to match existing schema semantics
- [x] 4.3 Implement `LibraryRepository` and `BookRepository` against `LibraryContext`
- [x] 4.4 Implement `TokenService` for `ITokenService` in Infrastructure (JwtSecurityTokenHandler, signing, claims); inject `JwtSettings` only here
- [x] 4.5 Move `20260528004745_InitialCreate` + designer + snapshot into Infrastructure; update namespaces/CLR type names only; keep `Up`/`Down` schema operations semantically identical; set MigrationsAssembly
- [x] 4.6 Confirm no new migration was added for the refactor and no table/column/FK/constraint changes were introduced

## 5. WebAPI composition root (composition-only Infrastructure usage)

- [x] 5.1 Move controllers, `Program.cs`, `Startup.cs`, `appsettings*.json`, and `Properties/` into `LibraryService.WebAPI`
- [x] 5.2 Rebuild Startup composition root: bind config → Infrastructure `JwtSettings`; register Application services; register Infrastructure repositories/`TokenService`/`DbContext`; configure JwtBearer, CORS, Swagger, controllers
- [x] 5.3 Ensure controllers depend only on Application abstractions (`ILibrariesService`, `IBooksService`, `IAuthenticationService`, `ITokenService`, DTOs)—no `LibraryContext`, EF, concrete repos, or Infrastructure services in controller constructors/usings for request handling
- [x] 5.4 Preserve `/login` token response shape and documented library/books status codes; implement only missing stubbed contract operations; do not redesign unrelated endpoints
- [x] 5.5 Read secrets only from configuration; do not echo credentials into docs/artifacts/commits

## 6. Tests (isolated from Supabase)

- [x] 6.1 Consolidate to `LibraryService.Integration.Test` targeting WebAPI; delete dead `IntegrationTest/`
- [x] 6.2 Replace persistence with SQLite in-memory (or equivalent isolated provider); ensure tests never use shared Supabase for state-changing CRUD
- [x] 6.3 Avoid double JWT scheme registration; skip Npgsql `Migrate()` when provider is not Npgsql
- [x] 6.4 Ensure contract tests pass (add book 201/404, list books 200/404, delete library 204/404) plus book update/delete coverage

## 7. Verification

- [x] 7.1 `dotnet restore`
- [x] 7.2 `dotnet build` — no errors
- [x] 7.3 `dotnet test` — all pass
- [x] 7.4 Verify project references match allowed/forbidden Dependency Rule graph
- [x] 7.5 Confirm Domain has no EF/ASP.NET/Npgsql/JWT/persistence dependencies
- [x] 7.6 Confirm Application does not reference Infrastructure/WebAPI and services do not use `LibraryContext`
- [x] 7.7 Confirm Infrastructure implements Application repository and `ITokenService` abstractions; `JwtSettings` is in Infrastructure
- [x] 7.8 Confirm WebAPI controllers depend on Application abstractions only (composition-only Infrastructure usage)
- [x] 7.9 `dotnet ef migrations list` — only `20260528004745_InitialCreate`
- [x] 7.10 `dotnet ef migrations has-pending-model-changes` — none
- [x] 7.11 Start `LibraryService.WebAPI`; startup migration handling does not recreate/alter existing schema
- [x] 7.12 Swagger loads; exercise documented HTTP contract (routes/payloads/status codes/login/CRUD) for compatibility
- [x] 7.13 Confirm integration tests used isolated persistence and did not modify shared Supabase
- [x] 7.14 Confirm no secrets were introduced into generated/tracked planning artifacts
- [x] 7.15 Confirm no unrelated out-of-scope redesign/cleanup was included
