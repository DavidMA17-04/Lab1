## Purpose

Defines the classic N-Layer structure of the Library solution (Api, BusinessLogic, DataAccess, Entities) and guarantees that the external HTTP contract is preserved while previously stubbed CRUD operations become fully functional.

## Requirements

### Requirement: Layered project organization
The solution SHALL be organized into separate projects representing the Api (Presentation), BusinessLogic, DataAccess, and Entities layers, with project references flowing downward only (Api → BusinessLogic → DataAccess → Entities). DataAccess SHALL NOT reference BusinessLogic. No project SHALL depend on a layer above it.

#### Scenario: Each layer exists as its own project
- **WHEN** the solution is opened
- **THEN** it contains distinct projects for Entities, DataAccess, BusinessLogic, and Api layers, and each file lives in the project owning its concern

#### Scenario: Dependencies point downward
- **WHEN** inspecting project references
- **THEN** no layer references a layer above it, Entities references no other solution project, and DataAccess does not reference BusinessLogic

### Requirement: Namespace alignment
Each project SHALL use a namespace matching its layer name so types are identifiable by assembly and namespace.

#### Scenario: Namespaces follow the layer
- **WHEN** compiling any layer project
- **THEN** its public types are declared in namespaces matching the owning layer (e.g., `*.Entities`, `*.BusinessLogic`, `*.DataAccess`, `*.Api`) rather than mixed prefixes such as `HackerRank1.*` and `LibraryService.WebAPI.*`

### Requirement: Preserved HTTP contract
The external REST contract of the service SHALL remain unchanged from its documented behavior: same routes, same JSON payload shapes, and same status codes.

#### Scenario: Library endpoints unchanged
- **WHEN** a client calls any `/api/libraries...` endpoint
- **THEN** the route, request/response JSON shape, and status codes match the documented contract exactly

#### Scenario: Book endpoints unchanged
- **WHEN** a client calls any `/api/libraries/{libraryId}/books...` endpoint
- **THEN** the route, request/response JSON shape, and status codes match the documented contract exactly

### Requirement: Complete CRUD behavior
The service SHALL expose fully functional CRUD for libraries and books, replacing the previously unimplemented `NotImplementedException` operations. After the refactor the API SHALL remain executable (build, test, and serve documented endpoints).

#### Scenario: Delete a library
- **WHEN** a client sends `DELETE /api/libraries/{libraryId}` and the library exists
- **THEN** the library is removed and the response is `204 No Content`
- **AND** when the library does not exist, the response is `404 Not Found`

#### Scenario: Add a book to a library
- **WHEN** a client sends `POST /api/libraries/{libraryId}/books` and the library exists
- **THEN** the book is added to that library and the response is `201 Created`
- **AND** when the library does not exist, the response is `404 Not Found`

#### Scenario: List books in a library
- **WHEN** a client sends `GET /api/libraries/{libraryId}/books`
- **THEN** the response is `200 OK` with the list of books belonging to that library (empty when the library has no books)
- **AND** when the library does not exist, the response is `404 Not Found`

### Requirement: Dependency injection via composition root
All internal services and repositories SHALL be registered through the Api layer's composition root so controllers depend only on interfaces.

#### Scenario: Services resolved from the container
- **WHEN** the application starts and controllers are constructed
- **THEN** they receive service and repository implementations through constructor injection of interfaces, and no controller instantiates its dependencies directly

### Requirement: Single integration test project
The solution SHALL contain exactly one integration test project that exercises the Api through its public HTTP surface against an isolated test database.

#### Scenario: Tests run against the public contract
- **WHEN** the test suite runs
- **THEN** all API contract and CRUD scenarios pass against the layered solution, and no duplicate test projects remain

### Requirement: Migration and schema preservation
The existing `InitialCreate` migration and the Supabase database schema SHALL remain intact and unmodified by this refactor; tables must not be recreated, renamed, or dropped.

#### Scenario: Migration list is unchanged
- **WHEN** `dotnet ef migrations list` is run against the layered solution
- **THEN** the only migration is `Initial Create` / `InitialCreate`, with the same tables (`Libraries`, `Books`, and the book→library FK) and no new/renamed/dropped schema objects
- **AND** `dotnet ef migrations has-pending-model-changes` reports no pending model changes

#### Scenario: Schema stays intact
- **WHEN** the solution is compiled and the app runs its startup `Migrate()`
- **THEN** the existing Supabase tables are found as-is with no data loss, recreation, rename, or drop

### Requirement: Credentials not exposed
The refactor SHALL NOT write the Supabase password or any other credential into generated files, logs, documentation, or commit contents.

#### Scenario: No secrets in artifacts
- **WHEN** any generated file or planning artifact is inspected
- **THEN** it contains no live credential value, and connection strings reference config keys (not hard-coded secret values)

### Requirement: Swagger preserved
The Api SHALL continue to expose working Swagger documentation after the refactor.

#### Scenario: Swagger UI available
- **WHEN** the app runs in development and `/swagger` is visited
- **THEN** Swagger UI loads and lists the documented endpoints (`/login`, `/api/libraries...`, `/api/libraries/{libraryId}/books...`)
