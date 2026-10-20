---
name: reforatec-integration-tests
description: >-
  Guides the end-to-end design and authoring of integration tests for ReforaTec.Api vertical slices.
  Covers the 4-layer test matrix (Security, Validation, Business Errors, Happy Path), modular data fixtures,
  and the TDD Red-Green-Refactor multi-agent workflow. Use this skill when asked to write, design,
  or scaffold integration tests for an endpoint or feature.
---

# Vertical Slice Integration Testing Guide

This skill provides the official procedure, architectural standards, and templates for designing and writing integration tests in `ReforaTec.Api.IntegrationTests` using xUnit, Testcontainers (PostgreSQL 18), and Respawn.

---

## 1. Directory Structure & File Organization

Integration tests mirror the Vertical Slice feature structure of `src/ReforaTec.Api`:

```text
tests/ReforaTec.Api.IntegrationTests/
  ├── Features/
  │   └── {Domain}/
  │       └── {UseCase}Tests.cs                # 1 file per feature, containing the 4-layer matrix
  └── Fixtures/
      ├── Assertions/
      │   └── ProblemDetailsAssertions.cs      # Reusable ShouldBeProblemAsync & ShouldBeValidationProblemAsync
      ├── Data/
      │   ├── IntegrationTestBase.Users.cs     # 1 file = 1 table = 1 direct INSERT helper
      │   ├── IntegrationTestBase.Campaigns.cs
      │   ├── IntegrationTestBase.Trees.cs
      │   ├── IntegrationTestBase.CampaignTrees.cs
      │   ├── IntegrationTestBase.UserTrees.cs
      │   └── IntegrationTestBase.Services.cs
      ├── IntegrationTestBase.cs               # Base class with Factory, Client, Authenticate, QueryDbAsync
      ├── IntegrationTestCollection.cs         # [CollectionDefinition("Integration")]
      ├── ReforaTecWebApplicationFactory.cs    # PostgreSQL 18 container + Respawn + seed
      └── TestTenants.cs                       # Primary = 1, Foreign = 2
```

### File Sizing & Splitting Rule
* Each feature has **one test file** (`{UseCase}Tests.cs`) with sections separated by clear layer comments.
* If a test file exceeds **~300 lines** (e.g. a complex endpoint with extensive validation rules), split it using `public partial class {UseCase}Tests` into separate files by layer (`{UseCase}Tests.Security.cs`, `{UseCase}Tests.Validation.cs`, etc.).

---

## 2. The 4-Layer Slice Testing Matrix

Every vertical slice endpoint must be tested from the HTTP perimeter down to the database using the **4-Layer Matrix** in strict top-to-bottom order:

```text
┌────────────────────────────────────────────────────────┐
│  Layer 1: Security & Multi-Tenant Isolation            │  401 / Multi-Tenant / 403
├────────────────────────────────────────────────────────┤
│  Layer 2: Input Validation (FluentValidation)          │  400 ValidationProblem Details
├────────────────────────────────────────────────────────┤
│  Layer 3: Domain & Business Errors (Errors.cs)         │  404 NotFound / 409 Conflict
├────────────────────────────────────────────────────────┤
│  Layer 4: Happy Path & State Persistence               │  200 / 201 / 204 + DB Verification
└────────────────────────────────────────────────────────┘
```

If a layer does not apply to a specific endpoint, **always document it explicitly** with a single-line comment (e.g. `// Security: N/A — anonymous endpoint`).

### Layer 1: Security & Multi-Tenant Isolation
1. **Unauthenticated Access (`401 Unauthorized`):**
   - Mandatory for all protected endpoints.
   - Omit the `Authorization` header and assert `HttpStatusCode.Unauthorized`.
2. **Multi-Tenant Boundary (`404 Not Found` or `403 Forbidden`):**
   - Mandatory for all tenant-scoped queries and mutations.
   - Persist a resource or user under `TestTenants.Foreign` (`Id = 2`).
   - Authenticate with a token scoped to `TestTenants.Primary` (`Id = 1`).
   - Assert that the endpoint never leaks cross-tenant data (returns 404 or 403).
3. **Resource Ownership / Care Assignment (`403 Forbidden`):**
   - When an endpoint requires an active tree care assignment or campaign association, verify that accessing an unassigned resource returns `403 Forbidden` with the domain error code (e.g., `user/not-assigned-to-tree`).
4. **Role & Policy Authorization (`403 Forbidden`):**
   - When an endpoint is protected by a policy (e.g. `CanManageCatalogs`), verify that a student role returns `403 Forbidden`.

### Layer 2: Input Validation (FluentValidation)
- Write **1 test per validation rule** defined in `Validator.cs`.
- Use `[Theory]` and `[InlineData]` for invalid variations (e.g. `null`, empty `""`, whitespace `"   "`, malformed format).
- Test **boundary limits** for length/range rules (e.g. exactly 50 characters passes, 51 characters fails).
- Assert using the assertion helper:
  ```csharp
  await response.ShouldBeValidationProblemAsync("PropertyName");
  ```

### Layer 3: Domain & Business Errors (`Errors.cs` / `ErrorOr`)
- Write **1 test for each business error** declared in `Errors.cs`.
- Never hardcode error strings or fake `0` IDs. Always pass the actual resource or user ID under test to `Errors.{Method}(actualId)` so both the code (`type`) and description (`detail`) match:
  ```csharp
  await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.UserNotFound(nonExistentUser.Id));
  ```

### Layer 4: Happy Path & State Persistence
- Verify the successful HTTP status code (`200 OK`, `201 Created`, or `204 NoContent`).
- For read endpoints, assert the full response payload against an expected DTO using `BeEquivalentTo`:
  ```csharp
  profile.Should().BeEquivalentTo(expectedProfile);
  ```
- For mutations (POST, PATCH, DELETE), inspect the persisted state directly in PostgreSQL using `QueryDbAsync`:
  ```csharp
  var userInDb = await QueryDbAsync(db => db.Users.FirstOrDefaultAsync(u => u.Id == user.Id));
  userInDb!.DisplayName.Should().Be("Updated Name");
  ```

---

## 3. Method Naming & Structure Conventions

### Test Method Names
Must strictly follow the pattern:
```text
{UseCase}_{Condition}_Should{Outcome}
```

**Good Examples:**
* `GetUserProfile_WhenUnauthenticated_ShouldReturnUnauthorized`
* `GetUserProfile_WhenUserBelongsToForeignTenant_ShouldReturnNotFound`
* `GetUserProfile_WhenUserDoesNotExist_ShouldReturnNotFound`
* `GetUserProfile_WhenAuthenticated_ShouldReturnOkWithProfile`
* `RequestOtp_WithInvalidEmail_ShouldReturnBadRequest`

**Avoid:**
* Shortened prefixes that do not match the slice name (e.g. `GetProfile_...` instead of `GetUserProfile_...`).
* Vague outcomes (e.g. `ShouldWork` or `Test1`).

### AAA Pattern
Every test method must clearly separate the three phases with comments:
```csharp
// Arrange
...

// Act
...

// Assert
...
```

---

## 4. Modular Data Fixtures (Object Mother Pattern)

When tests need test data in PostgreSQL, use the modular fixture helpers in `tests/.../Fixtures/Data/`:

### Core Invariants:
1. **1 File = 1 Table = 1 Direct INSERT Helper:**
   - Keep each partial class file small (~40-50 lines) with a single method.
   - Zero `if` branches, zero internal queries (`AnyAsync`), and zero hidden cascades.
2. **Coherent Defaults:**
   - `int tenantId = 1` (matches `TestTenants.Primary`).
   - `DateOnly? endDate = null` (represents an actively assigned entity).
   - Relational timestamps in the past relative to execution:
     $$\text{Tree (6 months ago)} \le \text{Campaign (1 month ago)} \le \text{Assignment (today)} \le \text{Service (now)}$$
3. **No Domain Logic in Test Fixtures:**
   - Complex domain calculations (generating semester inscription codes, closing campaigns, enrollment rules) belong in `src/ReforaTec.Api`, never inside test helpers.

### Available Fixture Methods:
* `CreateUserAsync(tenantId, email, controlNumber, givenName, familyName, gender, role, isDeleted)`
* `CreateCampaignAsync(tenantId, campaignName, inscriptionCode, period, location)`
* `CreateTreeAsync(tenantId, speciesId, valueId, plantingDate, height, diameter, healthState)`
* `AddTreeToCampaignAsync(treeId, campaignId, tenantId, campaignFolio, startDate, endDate)`
* `AssignTreeToUserAsync(treeId, userId, campaignId, tenantId, startDate, endDate)`
* `CreateTreeServiceAsync(treeId, studentId, serviceTypeId, campaignId, tenantId, deviceCapturedAt)`

---

## 5. TDD Red-Green-Refactor Multi-Agent Workflow

For new feature development or collaborative agent workflows, follow the 3-phase TDD discipline:

### Phase 1: Red (Test Architect Agent)
1. Read the feature requirements and draft `{UseCase}Tests.cs` using the template.
2. Define the expected request/response contracts, validation cases, error codes, and security requirements across the 4 layers.
3. Run `dotnet test` $\rightarrow$ Tests fail (Red: compilation error or 404).

### Phase 2: Green (Software Engineer Agent)
1. Use the `reforatec-slice` skill to scaffold the 6 slice files (`Request.cs`, `Response.cs`, `Validator.cs`, `Errors.cs`, `Handler.cs`, `{UseCase}.cs`).
2. Implement the minimum logic required in `Handler.cs` and wire the endpoint.
3. Run `dotnet test` $\rightarrow$ Tests pass (Green).

### Phase 3: Refactor (Reviewer Agent)
1. Review implementation against `GEMINI.md`:
   - LINQ Fluent Syntax with descriptive parameter names.
   - Query decomposition into private static helper methods.
   - Allman brace formatting and guard clauses.
   - Zero compiler warnings and zero errors.
2. Refactor without altering public contracts; verify all tests remain green.

---

## 6. Build & Test Commands

Always verify build health and run tests without build servers:

```bash
# Verify zero compiler warnings and zero errors
dotnet build --disable-build-servers --no-restore

# Execute the integration test suite
dotnet test --no-build
```
