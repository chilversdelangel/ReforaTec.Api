---
name: reforatec-slice
description: >-
  Guides the creation and scaffolding of new Vertical Slice endpoints in ReforaTec.Api.
  Use this skill when asked to create, add, or implement a new API feature, use-case, or endpoint.
---

# Vertical Slice Scaffolding Guide

This skill provides the official procedure and standardized templates for implementing new features in `ReforaTec.Api` following the Vertical Slice Architecture.

## 1. Directory Structure

Every use case resides in its own isolated folder under `src/ReforaTec.Api/Features/{Domain}/{UseCase}/`:

```text
src/ReforaTec.Api/Features/{Domain}/{UseCase}/
  ├── {UseCase}.cs       # IEndpoint implementation (MapEndpoint + HandleRequest)
  ├── Request.cs         # Input DTO record
  ├── Response.cs        # Output DTO record (omit if 204 NoContent)
  ├── Handler.cs         # Pure business logic returning ErrorOr<T>
  ├── Validator.cs       # FluentValidation AbstractValidator<Request>
  └── ErrorCodes.cs      # Domain error string constants (kebab-case)
```

## 2. Implementation Steps

1. **Input & Output DTOs (`Request.cs`, `Response.cs`)**:
   - Must be `internal sealed record`.
   - Any client timestamp must use `DateTimeOffset` to preserve timezones.
   - Never expose EF Core database entities in responses.

2. **Validation (`Validator.cs`)**:
   - Must inherit `AbstractValidator<Request>`.
   - Must be `internal sealed class`.
   - Prefer built-in validators (`NotEmpty()`, `MaximumLength()`, `GreaterThan()`). Avoid custom `.WithMessage()` unless strictly necessary.

3. **Domain Errors (`ErrorCodes.cs`)**:
   - Must be `internal static class`.
   - Error code pattern: `"entity/kebab-case"` (e.g., `"tree/not-found"`, `"service/already-exists"`).

4. **Business Handler (`Handler.cs`)**:
   - Must be `internal static class`.
   - Authenticated endpoints must accept `UserIdentity user` directly.
   - Enforce tenant isolation on all queries: `TenantId == user.TenantId`.
   - Read-only queries must use `.AsNoTracking()`.
   - Convert `DateTimeOffset` to `.UtcDateTime` before saving to entities.
   - Return `ErrorOr<Response>` (or `ErrorOr<Success>`). Never throw business exceptions.

5. **Endpoint Wiring (`{UseCase}.cs`)**:
   - Must implement `IEndpoint` with `internal sealed class`.
   - Inject `ICurrentUserService` in `HandleRequest` for protected routes.
   - Extract `var identity = currentUser.GetIdentity();` and return `identity.Errors.ToProblem()` if invalid.
   - Declare OpenAPI metadata: `.WithName()`, `.WithTags()`, `.WithSummary()`, `.WithDescription()`, `.Produces<Response>()`, `.ProducesProblem()`.

6. **Build Verification**:
   - Run `dotnet build` to verify zero errors and zero warnings.

## 3. Reference Templates

Ready-to-use boilerplate templates are available in the `resources/templates/` directory:
- [Request.cs.template](./resources/templates/Request.cs.template)
- [Response.cs.template](./resources/templates/Response.cs.template)
- [Validator.cs.template](./resources/templates/Validator.cs.template)
- [ErrorCodes.cs.template](./resources/templates/ErrorCodes.cs.template)
- [Handler.cs.template](./resources/templates/Handler.cs.template)
- [Endpoint.cs.template](./resources/templates/Endpoint.cs.template)
