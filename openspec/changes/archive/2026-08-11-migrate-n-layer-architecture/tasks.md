## 1. Project scaffolding

- [x] 1.1 Create `LibraryService.Entities`, `LibraryService.DataAccess`, `LibraryService.BusinessLogic`, and `LibraryService.Api` projects with root namespaces matching their layer name
- [x] 1.2 Add project references flowing downward only: DataAccess → Entities; BusinessLogic → DataAccess; Api → BusinessLogic, DataAccess, and Entities; DataAccess must not reference BusinessLogic
- [x] 1.3 Scope NuGet packages to their owning project (EF Core/Npgsql → DataAccess; JwtBearer/Swagger/Newtonsoft → Api; Entities stays free of EF/framework packages; BusinessLogic stays free of EF packages)
- [x] 1.4 Move Api files (`Controllers/`, `Program.cs`, `Startup.cs`, `appsettings*.json`, `Properties/`) into `LibraryService.Api` with namespaces updated to `LibraryService.Api.*`
- [x] 1.5 Update `HackerRank1.sln` to reference the new projects and remove the old `HackerRank1` / dead `IntegrationTest` projects

## 2. Entities layer

- [x] 2.1 Move `Library` and `Book` into `LibraryService.Entities` (namespace `LibraryService.Entities.Models`), keeping them free of EF package references
- [x] 2.2 Move `JwtSettings` into `LibraryService.Entities.Settings` as a plain POCO
- [x] 2.3 Move `BookForm`, `LibraryForm`, and `User` DTOs into `LibraryService.Entities.DTO`
- [x] 2.4 Configure entity keys/relationships via fluent `OnModelCreating` in DataAccess (or keep annotations without EF packages) so the schema is unchanged

## 3. DataAccess layer (DAL)

- [x] 3.1 Move `LibraryContext` into `LibraryService.DataAccess.Data`, using fluent configuration for `Libraries`/`Books` as needed (namespace `LibraryService.DataAccess`)
- [x] 3.2 Define repository interfaces `ILibraryRepository` and `IBookRepository` in DataAccess covering Get/Add/Update/Delete for libraries and books
- [x] 3.3 Implement `LibraryRepository` and `BookRepository` in DataAccess over `LibraryContext`
- [x] 3.4 Move migrations (`Migrations/*`, `LibraryContextModelSnapshot`) into `LibraryService.DataAccess` and re-point type references to `LibraryService.Entities` models
- [x] 3.5 Verify tables keep exact names (`Libraries`, `Books`) and the `Book→Library` FK cascade, with `Id` as identity
- [x] 3.6 Verify `Migrate()` runs against the existing schema with no schema changes (no new migration generated, no table recreation/rename/drop)

## 4. BusinessLogic layer (BLL)

- [x] 4.1 Define service interfaces `ILibrariesService`, `IBooksService`, `IAuthenticationService`, and `ITokenService` in `LibraryService.BusinessLogic`
- [x] 4.2 Implement `LibrariesService` depending on `ILibraryRepository` (not the DbContext), completing the `Delete` operation
- [x] 4.3 Implement `BooksService` depending on `IBookRepository`, completing `Add`, `Update`, and `Delete`
- [x] 4.4 Map entities ↔ DTOs inside the services (manual 1:1 mapping; controllers bind DTO forms)
- [x] 4.5 Move `AuthenticationService` and `TokenGenerator` (as `ITokenService` / `TokenService`) into `LibraryService.BusinessLogic`

## 5. Api composition root

- [x] 5.1 Rebuild `Startup.cs`/`Program.cs` as the composition root registering `JwtSettings` (singleton), auth (JwtBearer), CORS, Swagger, controllers, and the EF/Npgsql `DbContext`
- [x] 5.2 Register `ILibraryRepository`/`IBookRepository` (DataAccess), `ILibrariesService`/`IBooksService`/`IAuthenticationService`/`ITokenService` (BusinessLogic) via DI so controllers depend only on interfaces
- [x] 5.3 Rewrite `LibrariesController` to bind `LibraryForm`, call `ILibrariesService`, and implement `DELETE /api/libraries/{libraryId}` (`204` on success, `404` when missing)
- [x] 5.4 Rewrite `BooksController` to bind `BookForm`, call `IBooksService`, and implement `POST` (`201`/`404`), `PUT`, `DELETE`, and `GET` (`200`/`404`) for `/api/libraries/{libraryId}/books`
- [x] 5.5 Update `AuthController` to `/login` via `IAuthenticationService` + `ITokenService` preserving the existing token response shape
- [x] 5.6 Confirm the connection string is only read from configuration in `appsettings*.json` and that no credentials are hard-coded or echoed into any file, log, or commit

## 6. Tests

- [x] 6.1 Consolidate the two integration-test projects into `LibraryService.Integration.Test` targeting the Api project; delete `IntegrationTest/`
- [x] 6.2 Update test imports/namespaces to the new DTOs/entities and the SQLite in-memory `LibraryContext` swap
- [x] 6.3 Ensure tests exercise the full contract: add book (`201`/`404`), list books (`200`/`404`), delete library (`204`/`404`) against the layered solution
- [x] 6.4 Add coverage for the newly completed CRUD (book update, book delete, library delete missing case)

## 7. Verification

- [x] 7.1 Record the migration baseline before moving: run `dotnet ef migrations list` and `dotnet ef migrations has-pending-model-changes` against the current project (expect `Initial Create` and no pending changes)
- [x] 7.2 Run `dotnet build` on the full solution and confirm no compile errors
- [x] 7.3 Run `dotnet test` (integration suite) and confirm all contract + CRUD scenarios pass
- [x] 7.4 Run `dotnet ef migrations list` against `LibraryService.Api` and confirm exactly one migration, `Initial Create` / `InitialCreate`, unchanged, with no pending model changes
- [x] 7.5 Launch the app and manually exercise every documented endpoint in Swagger (`/login`, `/api/libraries...`, `/api/libraries/{libraryId}/books...`), confirming routes, payloads, and status codes match `HackerRank1/README.md` and the API remains functional
