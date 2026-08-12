# clean-architecture Specification

## Purpose

Defines Clean Architecture boundaries for the Library solution so Domain stays framework-free, Application owns simple service/repository/`ITokenService` abstractions without JWT implementation details, Infrastructure adapts EF persistence and concrete JWT issuance (including `JwtSettings`), and WebAPI remains the HTTP composition root—while preserving the existing public API contract, migration semantics, and isolating integration tests from shared Supabase.

## Requirements

### Requirement: Clean Architecture project organization
The solution SHALL be organized into separate projects for Domain, Application, Infrastructure, and WebAPI with this allowed reference graph: Domain references no solution project; Application → Domain; Infrastructure → Application and Domain; WebAPI → Application and Infrastructure. The following references are forbidden: Domain → Application/Infrastructure/WebAPI; Application → Infrastructure/WebAPI; Infrastructure → WebAPI; any circular project reference; and N-Layer-style WebAPI → BusinessLogic → DataAccess → Entities.

#### Scenario: Each layer exists as its own project
- **WHEN** the solution is opened
- **THEN** it contains distinct Domain, Application, Infrastructure, and WebAPI projects, and each type lives in the project matching its responsibility

#### Scenario: Dependency Rule enforced by project references
- **WHEN** inspecting project references
- **THEN** only the allowed graph above is present and all forbidden edges are absent

### Requirement: Namespace alignment
Each project SHALL use namespaces matching its architectural layer so types are identifiable by assembly and namespace after migration.

#### Scenario: Namespaces follow the layer
- **WHEN** compiling any layer project
- **THEN** public types use namespaces such as `LibraryService.Domain`, `LibraryService.Application`, `LibraryService.Infrastructure`, and `LibraryService.WebAPI` rather than mixed legacy prefixes for relocated types

### Requirement: Dependency inversion for persistence
Application services SHALL depend on repository abstractions defined in Application. Infrastructure SHALL provide Entity Framework Core implementations of those abstractions. Application SHALL NOT reference `LibraryContext`, EF Core packages, or Infrastructure types.

#### Scenario: Application does not depend on DbContext
- **WHEN** inspecting Application project source and package references
- **THEN** no Application type references `LibraryContext`, EF Core packages, or Infrastructure types

#### Scenario: Infrastructure implements repository contracts
- **WHEN** the composition root registers persistence
- **THEN** concrete repository types from Infrastructure are bound to Application repository interfaces

### Requirement: JWT abstraction without Application coupling
Application MAY define a framework-independent `ITokenService` abstraction. Application SHALL NOT contain the concrete JWT implementation and SHALL NOT reference JWT infrastructure packages solely to generate tokens. The concrete token implementation (including `JwtSecurityTokenHandler`, signing credentials, claims construction, and related behavior) SHALL live in Infrastructure and SHALL be registered by WebAPI at the composition root.

#### Scenario: Token packages stay out of Application
- **WHEN** inspecting Application package references and source
- **THEN** Application has no IdentityModel/JWT token-implementation packages and no concrete token generator type

#### Scenario: Infrastructure provides token implementation
- **WHEN** login requires a token
- **THEN** WebAPI resolves `ITokenService` to an Infrastructure implementation registered at the composition root

### Requirement: JwtSettings placement
`JwtSettings` SHALL NOT live in Domain or Application. `JwtSettings` SHALL live in Infrastructure as technical auth/token configuration. WebAPI SHALL bind host configuration to `JwtSettings` and register it for middleware and Infrastructure token issuance. Application SHALL NOT depend on issuer, audience, or signing-secret configuration types.

#### Scenario: Application does not reference JwtSettings
- **WHEN** inspecting Application source
- **THEN** no Application type references `JwtSettings` or signing-secret configuration classes

### Requirement: WebAPI composition-only use of Infrastructure
WebAPI MAY reference Infrastructure for dependency registration and host initialization. Controllers SHALL depend on Application-layer abstractions/services and SHALL NOT directly depend on `LibraryContext`, EF Core, concrete repository implementations, Infrastructure token services (concrete), or other persistence-specific Infrastructure types. The normal request flow SHALL be HTTP → Controller → Application Service → Application Repository Abstraction → Infrastructure Repository → DbContext → Database.

#### Scenario: Controllers depend on Application abstractions only
- **WHEN** controllers are constructed
- **THEN** they receive Application service interfaces (and `ITokenService` where needed) through DI, not Infrastructure concretes or `LibraryContext`

### Requirement: Simple Application design
Application SHALL use a straightforward service-and-repository interface/implementation structure. The migration SHALL NOT introduce MediatR, CQRS, AutoMapper, FluentValidation, generic repository frameworks, or one-class-per-use-case ceremony unless an existing requirement strictly requires it (none do for this change).

#### Scenario: Application layout remains simple
- **WHEN** inspecting the Application project
- **THEN** libraries/books/auth are expressed as service interfaces and implementations plus repository abstractions, without MediatR/CQRS handler proliferation

### Requirement: Preserved HTTP contract
This migration is architectural, not an API redesign. Existing routes, request payloads, response payloads, DTO semantics, status codes, authentication pipeline availability, login token response shape, Swagger/OpenAPI surface, and currently working CRUD behavior SHALL remain externally compatible. Only operations currently stubbed with `NotImplementedException` and required by the documented contract SHALL gain their missing implementation. Unrelated endpoint behavior SHALL NOT be “improved” as part of this migration.

#### Scenario: Library endpoints unchanged
- **WHEN** a client calls any `/api/libraries...` endpoint
- **THEN** the route, request/response JSON shape, and status codes match the documented contract

#### Scenario: Book endpoints unchanged
- **WHEN** a client calls any `/api/libraries/{libraryId}/books...` endpoint
- **THEN** the route, request/response JSON shape, and status codes match the documented contract

#### Scenario: Login unchanged
- **WHEN** a client posts to `/login` with valid or invalid credentials
- **THEN** the response semantics remain Unauthorized or a token payload equivalent to the existing `TokenResponse` shape

### Requirement: Complete documented stubbed CRUD only
The service SHALL implement previously unimplemented contract-required CRUD (library delete; book add/update/delete as documented) and remain executable after the migration, without changing unrelated working behavior.

#### Scenario: Delete a library
- **WHEN** a client sends `DELETE /api/libraries/{libraryId}` and the library exists
- **THEN** the library is removed and the response is `204 No Content`
- **AND** when the library does not exist, the response is `404 Not Found`

#### Scenario: Add a book to a library
- **WHEN** a client sends `POST /api/libraries/{libraryId}/books` and the library exists
- **THEN** the book is added and the response is `201 Created`
- **AND** when the library does not exist, the response is `404 Not Found`

#### Scenario: List books in a library
- **WHEN** a client sends `GET /api/libraries/{libraryId}/books`
- **THEN** the response is `200 OK` with the books for that library (empty list when none)
- **AND** when the library does not exist, the response is `404 Not Found`

### Requirement: Semantic EF migration preservation
The existing `20260528004745_InitialCreate` migration MAY be relocated to Infrastructure and MAY have namespace/type-name updates required by the architectural move. Migration operations and the resulting database schema SHALL remain semantically unchanged: no altered table/column names or types, PKs, FKs, relationships, or constraints; no unnecessary new migration; no recreate/rename/drop of existing Supabase schema or data solely due to the refactor.

#### Scenario: Migration list unchanged
- **WHEN** `dotnet ef migrations list` is run against the WebAPI startup project
- **THEN** the only migration is `20260528004745_InitialCreate`
- **AND** `dotnet ef migrations has-pending-model-changes` reports no pending model changes

#### Scenario: Startup migrate is safe
- **WHEN** the app starts against the existing database
- **THEN** migration handling finds the existing schema without recreating or altering it and existing data remains available

### Requirement: Integration tests isolated from shared Supabase
The solution SHALL contain exactly one integration test project that exercises WebAPI through its public HTTP surface against an isolated test persistence environment (SQLite in-memory or equivalent). Integration tests SHALL never execute destructive or state-changing CRUD tests against the shared Supabase database.

#### Scenario: Contract tests pass on isolated persistence
- **WHEN** the test suite runs
- **THEN** documented contract and CRUD scenarios pass using isolated persistence and no duplicate dead test projects remain

#### Scenario: Shared Supabase is not mutated by tests
- **WHEN** integration tests execute
- **THEN** they do not perform state-changing CRUD against the shared Supabase connection

### Requirement: Scope protection
Files, configuration, behavior, endpoints, and functionality outside the scope required for the Clean Architecture migration SHALL NOT be modified unnecessarily. Existing working functionality SHALL be preserved unless modification is explicitly required to establish Clean boundaries, complete documented stubbed CRUD, adapt DI/project references, or preserve compilation and runtime after relocation.

#### Scenario: No unrelated redesign
- **WHEN** reviewing the migration diff
- **THEN** changes are limited to architectural relocation/wiring and required stub completions, without unrelated cleanup or API redesign

### Requirement: Technology and version preservation
The solution SHALL remain on .NET 8, EF Core 8.0.2, Npgsql 8.0.2, and existing compatible test-stack versions. Package relocation between projects is allowed; unnecessary upgrades or downgrades are not.

#### Scenario: Package versions preserved
- **WHEN** inspecting PackageReferences after migration
- **THEN** EF Core/Npgsql remain 8.0.2 (or the same previously established versions) and are located in the correct projects

### Requirement: Credentials not exposed
The migration SHALL NOT write Supabase passwords, JWT secrets, or other live credentials into generated planning artifacts, logs, documentation, or newly introduced tracked content beyond existing host configuration files already used for local development.

#### Scenario: No secrets in planning artifacts
- **WHEN** OpenSpec artifacts for this change are inspected
- **THEN** they contain no live credential values and refer to configuration keys rather than hardcoded secrets

### Requirement: Swagger preserved
WebAPI SHALL continue to expose working Swagger documentation in Development after the migration.

#### Scenario: Swagger UI available
- **WHEN** the app runs in Development and `/swagger` is visited
- **THEN** Swagger UI loads and lists the documented endpoints
