# vertical-slice Specification

## Purpose

Defines Vertical Slice Architecture for the Library API: organize by use case under Features, keep only true cross-cutting persistence and auth settings in Common, preserve the public HTTP contract and EF migration semantics, and isolate integration tests from shared Supabase.

## Requirements

### Requirement: Vertical Slice project organization
The solution SHALL expose the Library API from a single host project (`LibraryService.Api`) organized primarily by feature folders under `Features/`, with a minimal `Common/` area for shared persistence and settings. The solution SHALL NOT introduce an N-Layer BusinessLogic/DataAccess/Entities project graph or a Clean Architecture Domain/Application/Infrastructure multi-project graph for this branch.

#### Scenario: Single host with Features and Common
- **WHEN** the solution is opened
- **THEN** it contains `LibraryService.Api` with `Features/` and `Common/` as the primary organization, not Controllers/Services/Data as the top-level grouping for migrated types

### Requirement: One slice per use case
Each HTTP use case (login, get libraries, get library by id, create/update/delete library, get/add/update/delete books) SHALL have a dedicated slice folder under `Features/{Module}/{UseCase}/` that colocates the endpoint pieces and slice-specific request/response types for that use case.

#### Scenario: Libraries operations are separate slices
- **WHEN** inspecting `Features/Libraries/`
- **THEN** GetLibraries, GetLibraryById, CreateLibrary, UpdateLibrary, and DeleteLibrary each exist as their own use-case folders

#### Scenario: Books operations are separate slices
- **WHEN** inspecting `Features/Books/`
- **THEN** GetBooksByLibrary, AddBook, UpdateBook, and DeleteBook each exist as their own use-case folders

### Requirement: Minimal Common shared kernel
`Common/` SHALL contain only cross-cutting necessities: persistence (`LibraryContext`, `Library`/`Book` entities, EF migrations) and `JwtSettings`. Feature-specific DTOs, controllers, and token helpers SHALL NOT live in `Common/`.

#### Scenario: Auth helpers live in the Login slice
- **WHEN** inspecting authentication/token generation code
- **THEN** it lives under `Features/Auth/Login/` rather than a global Helpers folder in Common

### Requirement: Shared only within a module
When multiple slices in the same module reuse the same application service, that service MAY live in `Features/{Module}/Shared/`. A slice SHALL NOT call another sliceâ€™s endpoint/handler as its primary collaboration path.

#### Scenario: Libraries Shared service
- **WHEN** Libraries CRUD slices need library persistence logic
- **THEN** they use `Features/Libraries/Shared` service abstractions/implementations rather than calling other Libraries slice endpoints

### Requirement: Preserved HTTP contract
Existing routes, request/response JSON shapes, status codes, login token response shape, Swagger availability in Development, and currently working CRUD behavior SHALL remain externally compatible. Only documented stubbed operations SHALL gain missing implementations.

#### Scenario: Login unchanged
- **WHEN** a client posts to `/login` with valid or invalid credentials
- **THEN** the response remains Unauthorized or a token payload equivalent to the existing `TokenResponse` shape

#### Scenario: Library and book routes unchanged
- **WHEN** a client calls `/api/libraries...` or `/api/libraries/{libraryId}/books...`
- **THEN** routes, payloads, and status codes match the documented contract

### Requirement: Complete documented stubbed CRUD only
The service SHALL implement previously unimplemented contract-required CRUD (library delete; book add/update/delete as documented) without redesigning unrelated working behavior.

#### Scenario: Delete library
- **WHEN** `DELETE /api/libraries/{libraryId}` is sent
- **THEN** the response is `204` when the library exists and `404` when it does not

#### Scenario: Add book
- **WHEN** `POST /api/libraries/{libraryId}/books` is sent
- **THEN** the response is `201` when the library exists and `404` when it does not

#### Scenario: List books
- **WHEN** `GET /api/libraries/{libraryId}/books` is sent
- **THEN** the response is `200` with books (possibly empty) when the library exists and `404` when it does not

### Requirement: Semantic EF migration preservation
The existing `20260528004745_InitialCreate` migration MAY move under `Common/Persistence/Migrations` with namespace/CLR type updates only. Schema operations SHALL remain semantically unchanged; no unnecessary new migration; no recreate of existing Supabase schema solely due to the refactor.

#### Scenario: Migration list unchanged
- **WHEN** `dotnet ef migrations list` is run against the API startup project
- **THEN** the only migration is `20260528004745_InitialCreate` and there are no pending model changes

### Requirement: Integration tests isolated from shared Supabase
The solution SHALL contain one integration test project targeting the API host against isolated persistence (SQLite in-memory or equivalent). Tests SHALL never perform state-changing CRUD against shared Supabase.

#### Scenario: Contract tests pass in isolation
- **WHEN** the test suite runs
- **THEN** documented contract scenarios pass using isolated persistence

### Requirement: Technology and scope protection
The solution SHALL remain on .NET 8, EF Core 8.0.2, and Npgsql 8.0.2. The migration SHALL NOT introduce MediatR/CQRS frameworks or unrelated redesign. Secrets SHALL NOT be written into OpenSpec planning artifacts.

#### Scenario: Package versions preserved
- **WHEN** inspecting PackageReferences after migration
- **THEN** EF Core and Npgsql remain 8.0.2 (or the previously established versions)

