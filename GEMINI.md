# ReforaTec.Api — Engineering Standards & Architecture Guidelines

## 1. Project Context
- **Scale:** Multi-tenant tree care and environmental tracking system across educational campuses.
- **Consumers:** Android mobile apps (Students, Inspectors) and Web Portal (Coordinators, Admins).
- **Dual Purpose:** Production-ready stability for real-world deployment and technical portfolio showcase.

## 2. Tech Stack
- **Runtime:** .NET 10 / C# 13
- **Data & ORM:** PostgreSQL + EF Core 10 (Npgsql)
- **Validation:** FluentValidation 12
- **Error Handling:** ErrorOr 2
- **Mapping:** Mapster 10
- **Authentication:** Microsoft.AspNetCore.Authentication.JwtBearer 10

## 3. Architecture
- **Pattern:** Vertical Slice Architecture (one self-contained folder per use case).
- **Endpoints:** Minimal APIs with `IEndpoint` auto-registration (no Controllers, no MediatR, no CQRS).
- **Feature Folder Structure:**
  ```text
  src/Features/{Domain}/{UseCase}/
    ├── {UseCase}.cs       (MapEndpoint + HandleRequest wiring)
    ├── Request.cs         (Input DTO record)
    ├── Response.cs        (Output DTO record, if applicable)
    ├── Handler.cs         (Pure business logic, returns ErrorOr<T>)
    ├── Validator.cs       (FluentValidation AbstractValidator<Request>)
    └── Errors.cs          (Pre-constructed Error objects: NotFound, Forbidden, Conflict)
  ```
- **Domain Aggregate Cohesion:** Operations on child entities or subresources strictly scoped to an Aggregate Root (e.g., tree maintenance services, measurement photos) belong inside the Aggregate's feature folder (`Features/Trees/`), preserving domain cohesion and avoiding premature top-level folder fragmentation.

## 4. Mandatory Invariants
- **Error Flow:** ALL business errors MUST return `ErrorOr<T>`. Never throw exceptions for expected business flow.
- **Error Codes:** Standard string pattern `"entity/kebab-case"` (e.g., `"tree/not-found"`, `"auth/unauthorized"`).
- **Resource Authorization Violations:** Accessing or operating on resources not actively assigned or owned within the tenant MUST return `Error.Forbidden` (`StatusCodes.Status403Forbidden`).
- **Tenant Isolation:** Every query and mutation on tenant-scoped entities MUST enforce `TenantId == user.TenantId`. Never trust client-provided tenant identifiers.
- **Identity Extraction:** Use `ICurrentUserService` and `UserIdentity` (resolved from JWT claims). Never perform ad-hoc database queries to `Users` just to discover `TenantId`.
- **JWT Claim Conventions:** Custom JWT claims MUST use standard snake_case names (e.g., `"tenant_id"`). Avoid short abbreviations reserved by Microsoft (such as `"tid"`, `"oid"`) to prevent unintended transformations by ASP.NET Core `DefaultInboundClaimTypeMap`.
- **DTO Boundary:** Endpoints MUST NEVER return EF Core database entities. Always map to immutable `Response` DTO records.
- **Unique Text Fields:** Unique text properties MUST implement `INormalizable` (handled by `AppDbContext`).
- **Soft Delete:** Entities implementing soft delete must always filter out deleted records (`!x.IsDeleted`).
- **User-Scoped Routing Hierarchy:** All endpoints operating on or projecting data for the currently authenticated user MUST use the canonical `/users/me/...` route hierarchy (e.g., `GET /users/me/profile`, `GET /users/me/trees`, `PATCH /users/me`). Never expose UI-centric endpoints at the root level (e.g., `/users/profile`), and avoid redundant secondary alias routes or `.ExcludeFromDescription()`.
- **Security:** Never leak user existence in authentication flows (OWASP User Enumeration Prevention). Store only SHA-256 hex hashes of refresh tokens.
- **Language:** ALL code, identifiers, XML docs, comments, git commits, and documentation in English.

## 5. Code Conventions
- **Encapsulation:** DTOs (`Request`, `Response`), `Validator`, and `Handler` classes MUST be `internal sealed`.
- **Timezones:** DTOs accepting timestamps MUST use `DateTimeOffset` to preserve client timezones. Database entities store `.UtcDateTime` (`timestamp with time zone`).
- **Validation:** Use FluentValidation built-in validators; avoid redundant custom `.WithMessage()` overrides.
- **Read Performance:** All read-only database queries MUST use `.AsNoTracking()`.
- **LINQ Style & Syntax:** Always use **Fluent Method Syntax** (`context.Entity.AsNoTracking().Where(...).Select(...)`) with chained methods. Never use SQL-like Query Syntax (`from ... join ... select`). When relating entities without direct EF Core navigation properties, prefer subquery expressions using `IQueryable.Contains(...)` (e.g., `activeTreeIds.Contains(record.TreeId)`), which PostgreSQL compiles to high-performance semi-joins (`Nested Loop Semi Join`) with early exit.
- **Handler Query Decomposition:** Handlers executing multiple queries, aggregations, or complex lookups MUST decompose each independent operation into private static helper methods, keeping the top-level `Handle` orchestrator clean, flat, and immediately readable.
- **EF Core Configuration:** Convention-over-configuration. Configure only exceptions (`MaxLength`, `IsFixedLength`). Base entities inherit `AuditableEntity` or `CreatableEntity`.
- **Control Flow:** Follow official .NET conventions:
  - Always use braces `{}` on separate lines (Allman style) for control blocks (`if`, `foreach`, `while`), avoiding single-line brace-less statements.
  - Favor early exits and guard clauses to minimize nesting and maintain flat indentation.
  - Leverage modern pattern matching (`is null`, `is not null`, type patterns) and switch expressions (`=>`) over multi-branch `if-else` ladders.
  - Use expression-bodied members (`=>`) for concise single-statement methods and read-only properties.
  - Always use descriptive lambda parameter names in LINQ queries and FluentValidation rules (`tree => tree.Id`, `assignment => assignment.UserId`, `request => request.ServiceTypeId`), strictly avoiding single-letter abbreviations (`x =>`, `t =>`).
  - Identity resolution result variable MUST be named `identityResult` (`var identityResult = currentUser.GetIdentity(); if (identityResult.IsError)`).
- **OpenAPI Documentation:** Every endpoint MUST declare `.WithName()`, `.WithTags()`, `.WithSummary()`, `.WithDescription()`, and explicit `.Produces<T>()` status codes.
- **Integration Testing:** Every vertical slice endpoint must be covered by integration tests adhering to the 4-layer slice testing matrix (*Security, Validation, Business Errors, Happy Path*) via the `reforatec-integration-tests` skill.
- **Build Health:** Must verify zero compiler warnings and zero errors via `dotnet build` at the completion of each task.

## 6. Git Workflow
- **Commit Discipline:** Atomic Commits (one logical, self-contained change per commit).
- **Format:** Conventional Commits (`feat`, `fix`, `refactor`, `chore`, `docs`, `test`, `perf`, `build`, `ci`).
- **Pull Requests:** Adhere strictly to `.github/PULL_REQUEST_TEMPLATE.md`.

## 7. Design Governance & Review Gate
- **Architectural Justification:** Explain the "Why" and evaluate trade-offs under three prisms (*Performance, Maintainability, Security*) before proposing implementations.
- **Socratic Alignment:** Guide architectural choices through structured exploration rather than opaque assumptions.
- **Authorization Gate:** WAIT for explicit user authorization before creating or modifying codebase files.
