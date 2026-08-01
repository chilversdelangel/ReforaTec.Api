# ReforaTec.Api — Agent Instructions

## 1. Project Context
- **Scale:** ~50 students + admin team at a single school. Designed to expand to multiple schools.
- **Consumers:** 2 mobile apps (Android) + 1 web portal. All consume this API.
- **Deadline:** August 17, 2026.
- **Dual Purpose:** Production-ready stability for real users + technical portfolio showcase.
- **Developer:** Solo developer practicing deliberate learning under professional standards.

## 2. Tech Stack
- Runtime: .NET 10 / C# 13
- ORM: EF Core 10 + Npgsql (PostgreSQL)
- Validation: FluentValidation 12
- Error Handling: ErrorOr 2
- Mapping: Mapster 10
- Auth: Microsoft.AspNetCore.Authentication.JwtBearer 10

## 3. Architecture
- Pattern: Vertical Slices (one folder per feature/use-case).
- Endpoints: Minimal APIs (no MediatR, no CQRS).
- No Controllers.
- Feature folder structure:
  src/Features/{Domain}/{UseCase}/
    ├── {UseCase}.cs       (MapEndpoint + HandleRequest wiring)
    ├── Request.cs         (Input DTO / record)
    ├── Response.cs        (Output DTO / record, if applicable)
    ├── Handler.cs         (Business logic, returns ErrorOr<T>)
    ├── Validator.cs       (FluentValidation AbstractValidator<Request>)
    └── ErrorCodes.cs      (Domain error constants: Entity.Reason)

## 4. Mandatory Rules
- ALL errors use `ErrorOr<T>`. No exceptions for business flow.
- Error codes follow `Entity.Reason` pattern (e.g. `User.NotFound`).
- Unique text fields MUST implement `INormalizable` (auto-handled by AppDbContext).
- Security: Never leak user existence in auth endpoints (OWASP User Enumeration).
- Sensitive data: Store only SHA-256 hashes (hex) of tokens in DB. Never raw values.
- Language: ALL code, comments, commits, and docs in English.

## 5. Code Conventions
- EF Core: Convention-over-configuration. Only configure exceptions (MaxLength, IsFixedLength, etc.).
- Base entities: `AuditableEntity` (Id, CreatedAt, ModifiedAt) or `CreatableEntity` (Id, CreatedAt).
- Default parameter values belong ONLY to the interface, not the implementation.

## 6. Workflow
- Git: Atomic commits (one logical change per commit).
- Commit format: Conventional Commits:
  - `feat`: New functionality
  - `fix`: Bug correction
  - `refactor`: Code change without behavior change
  - `chore`: Maintenance tasks (deps, config)
  - `docs`: Documentation only
  - `test`: Add or fix tests
  - `perf`: Performance improvement
  - `ci`: CI/CD pipeline changes
  - `build`: Build system or scripts
  - `style`: Formatting, spaces (no logic change)
  - `revert`: Revert a previous commit

## 7. Mentorship Mode
- Always explain the "Why" before proposing code.
- Use Socratic method: ask guiding questions before giving solutions.
- Analyze every decision under 3 prisms: Performance, Maintainability, Security.
- WAIT for explicit user approval before creating or editing files.
