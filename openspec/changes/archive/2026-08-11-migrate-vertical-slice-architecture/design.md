## Context

Branch `lab1/verticalSlice` starts from the monolith (`HackerRank1`). Prior branches delivered N-Layer and Clean Architecture. This change reorganizes by **use case** in one host project.

## Goals / Non-Goals

**Goals**

- Single `LibraryService.Api` with `Features/` + `Common/`.
- One folder per HTTP use case; module `Shared/` for reused services.
- Preserve HTTP contract, migration semantics, Swagger, package versions.
- Complete stubbed library delete and book add/update/delete.
- SQLite-isolated integration tests.

**Non-Goals**

- MediatR, FluentValidation pipeline, CQRS frameworks.
- Multi-project Clean/N-Layer graphs on this branch.
- Unrelated API redesign or schema changes.

## Decisions

1. **Host name:** `LibraryService.Api` (replaces `HackerRank1`).
2. **Controllers:** one controller type per module (`LibrariesController`, `BooksController`, `AuthController`) implemented as **partial classes**, with each action file living under its use-case folder (matches the lab folder guide).
3. **Entities:** extract `Library`/`Book` next to `LibraryContext` under `Common/Persistence`.
4. **DI:** register module Shared services + auth/token helpers in `Startup`; use `AddDbContext` (not pool) for test replacement; migrate only when provider is Npgsql.
5. **Books slices:** add Add/Update/Delete beyond the guide’s Get-only example so stubs and tests pass.
6. **No cross-slice calls:** Libraries/Books slices collaborate via Shared services and `LibraryContext`, not via other slice endpoints.

## Target tree

```text
LibraryService.Api/
  Program.cs, Startup.cs, appsettings*.json, Properties/
  Common/
    Persistence/ LibraryContext.cs, Library.cs, Book.cs, Migrations/
    Settings/ JwtSettings.cs
  Features/
    Auth/Login/   AuthController, User, TokenResponse, AuthenticationService, TokenGenerator
    Libraries/
      GetLibraries/, GetLibraryById/, CreateLibrary/, UpdateLibrary/, DeleteLibrary/
      Shared/ LibrariesService + ILibrariesService
    Books/
      GetBooksByLibrary/, AddBook/, UpdateBook/, DeleteBook/
      Shared/ BooksService + IBooksService
```

## File mapping (monolith → VSA)

| Current | New location |
|---------|--------------|
| `Data/LibraryContext.cs` (+ entities) | `Common/Persistence/` |
| `Migrations/*` | `Common/Persistence/Migrations/` |
| `Entities/JwtSettings.cs` | `Common/Settings/` |
| Auth controller/service/helper/User | `Features/Auth/Login/` |
| Libraries controller actions | `Features/Libraries/{UseCase}/` partials |
| `Services/LibraryService.cs` | `Features/Libraries/Shared/` |
| Books controller/service/BookForm | `Features/Books/{UseCase}/` + Shared |

## Risks / Trade-offs

- Partial controllers keep routing attributes consistent while colocating actions in slice folders.
- Small duplication risk mitigated by module `Shared/` only.
- Startup still knows about Shared services (composition root); that is acceptable for VSA without MediatR.

## Migration plan

1. Scaffold `LibraryService.Api`; update solution.
2. Move Common persistence/settings/migrations.
3. Create Features folders; implement partial controllers + Shared services; complete stubs.
4. Wire Startup/DI; remove monolith.
5. Retarget integration tests to SQLite isolation.
6. Verify build, tests, EF, Swagger, contract smoke.

## Verification checklist

- `dotnet restore` / `dotnet build` / `dotnet test` (all pass).
- Project is single API host with Features + Common (no N-Layer/Clean multi-project graph).
- `dotnet ef migrations list` → only `20260528004745_InitialCreate`; no pending model changes.
- Startup migrate safe on existing Npgsql schema; skipped for SQLite tests.
- Swagger + login + libraries smoke OK.
- Tests do not hit Supabase for CRUD.
- No secrets in OpenSpec artifacts.
