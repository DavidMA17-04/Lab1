## 1. Scaffold

- [x] 1.1 Create `LibraryService.Api` (net8.0 Web) and update `HackerRank1.sln` to include it + integration tests
- [x] 1.2 Add NuGet packages matching current versions (JwtBearer 8.0.16, EF 8.0.2, Npgsql 8.0.2, Design, Swashbuckle, Newtonsoft) without version churn
- [x] 1.3 Confirm layout target is Features + Common in one host (no N-Layer/Clean multi-project graph)

## 2. Common

- [x] 2.1 Move `LibraryContext` and extract `Library`/`Book` into `Common/Persistence`
- [x] 2.2 Move `JwtSettings` into `Common/Settings`
- [x] 2.3 Move `20260528004745_InitialCreate` + designer + snapshot into `Common/Persistence/Migrations` with namespace/type updates only; keep schema ops identical
- [x] 2.4 Confirm no new migration and no schema semantic changes

## 3. Features — Auth

- [x] 3.1 Create `Features/Auth/Login/` with AuthController, User, TokenResponse, AuthenticationService, TokenGenerator
- [x] 3.2 Preserve `/login` behavior (admin/1234 → token; invalid → Unauthorized)

## 4. Features — Libraries

- [x] 4.1 Create `Features/Libraries/Shared` with `ILibrariesService` / `LibrariesService` (implement Delete)
- [x] 4.2 Create slice folders GetLibraries, GetLibraryById, CreateLibrary, UpdateLibrary, DeleteLibrary with partial controller actions
- [x] 4.3 Preserve library routes/status codes including Delete 204/404

## 5. Features — Books

- [x] 5.1 Create `Features/Books/Shared` with `IBooksService` / `BooksService` (implement Add/Update/Delete)
- [x] 5.2 Create slice folders GetBooksByLibrary, AddBook, UpdateBook, DeleteBook with partial controller actions and BookForm where needed
- [x] 5.3 Preserve books contract (list 200/404, add 201/404, update/delete as documented)

## 6. Host composition

- [x] 6.1 Move Program/Startup/appsettings/Properties into `LibraryService.Api`; register DI, JwtBearer, CORS, Swagger, `AddDbContext` + MigrationsAssembly
- [x] 6.2 Skip `Migrate()` when provider is not Npgsql
- [x] 6.3 Remove obsolete `HackerRank1` and dead `IntegrationTest/` when ready

## 7. Tests

- [x] 7.1 Retarget `LibraryService.Integration.Test` to `LibraryService.Api`
- [x] 7.2 Use SQLite in-memory; never mutate shared Supabase for CRUD
- [x] 7.3 Avoid double JWT scheme registration; ensure contract tests pass (add book, list books, delete library, update/delete book)

## 8. Verification

- [x] 8.1 `dotnet restore` and `dotnet build` — no errors
- [x] 8.2 `dotnet test` — all pass
- [x] 8.3 Confirm Features + Common organization and no forbidden multi-project layer graph
- [x] 8.4 `dotnet ef migrations list` — only InitialCreate; has-pending-model-changes — none
- [x] 8.5 Swagger/login/libraries smoke OK; secrets not in planning artifacts; no out-of-scope redesign
