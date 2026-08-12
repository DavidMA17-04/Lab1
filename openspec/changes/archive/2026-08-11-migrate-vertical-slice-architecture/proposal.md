## Why

The lab requires a third architecture branch (`lab1/verticalSlice`) that reorganizes the Library API around **use cases** instead of technical layers. N-Layer groups by Controllers/Services/Data; Clean Architecture protects Domain behind dependency inversion. Vertical Slice Architecture (VSA) keeps endpoint, request/response, and logic for each feature together so changes stay localized.

## What Changes

- Replace the monolithic `HackerRank1` project with a single `LibraryService.Api` host organized as `Features/` + `Common/`.
- Place only shared persistence and JWT settings under `Common/`.
- Organize HTTP operations as feature slices (Auth/Login, Libraries CRUD slices, Books CRUD slices) with module-level `Shared/` services where sibling slices reuse logic.
- Preserve the existing HTTP contract, EF `InitialCreate` migration semantics, Swagger, and .NET 8 / EF 8.0.2 / Npgsql 8.0.2 stack.
- Complete only documented stubbed CRUD (library delete; book add/update/delete).
- Keep integration tests on isolated SQLite; never mutate shared Supabase from tests.
- Do **not** introduce MediatR, CQRS frameworks, multi-project N-Layer/Clean graphs, or unrelated redesign.

## Capabilities

### New Capabilities

- `vertical-slice`: Vertical Slice project organization (Features + Common), slice cohesion rules, minimal shared kernel, HTTP/migration/test/secret preservation for the Library API.

### Modified Capabilities

- (none)

## Impact

- Solution layout: one API project (`LibraryService.Api`) instead of the monolith folder layout.
- Controllers and services relocate under `Features/{Module}/{UseCase}/` (and `Shared/` within a module).
- `LibraryContext`, entities, migrations, and `JwtSettings` move to `Common/`.
- Integration test project retargets `LibraryService.Api` with SQLite isolation (same contract coverage as prior branches).
- OpenSpec planning artifacts for this change; main specs synced at archive time.
