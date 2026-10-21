# ReforaTec.Api.IntegrationTests — Testing Invariants & Guidelines

## 1. Scope & Isolation
This directory houses the end-to-end integration test suite using **xUnit**, **Testcontainers (PostgreSQL 18)**, and **Respawn**.

* **Container Isolation:** Every test run boots an ephemeral PostgreSQL 18 container in RAM via `ReforaTecWebApplicationFactory`.
* **State Reset:** Respawn resets database tables between tests (ignoring `__EFMigrationsHistory` and lookup seeds).
* **Deterministic Fixtures:** Use modular fixture methods in `Fixtures/Data/` (`IntegrationTestBase.*.cs`). One file = one table = one direct INSERT helper.

---

## 2. The 4-Layer Testing Matrix

Every vertical slice test class MUST test from the HTTP perimeter down to the database using the 4 layers in strict order:

```text
┌────────────────────────────────────────────────────────┐
│  Layer 1: Security & Multi-Tenant Isolation            │  401 / Multi-Tenant / 403
├────────────────────────────────────────────────────────┤
│  Layer 2: Input Validation (FluentValidation)          │  400 ValidationProblem
├────────────────────────────────────────────────────────┤
│  Layer 3: Domain & Business Errors (Errors.cs)         │  404 NotFound / 409 Conflict
├────────────────────────────────────────────────────────┤
│  Layer 4: Happy Path & State Persistence               │  200 / 201 / 204 + DB Verification
└────────────────────────────────────────────────────────┘
```

If a layer does not apply (e.g. anonymous endpoint), document it with a single-line comment: `// Security: N/A — anonymous endpoint`.

---

## 3. Method Naming & Structure Conventions

### Naming Pattern:
```text
{UseCase}_{Condition}_Should{Outcome}
```
* **Examples:**
  * `GetUserProfile_WhenUnauthenticated_ShouldReturnUnauthorized`
  * `GetUserProfile_WhenUserBelongsToForeignTenant_ShouldReturnNotFound`
  * `RequestOtp_WithInvalidEmail_ShouldReturnBadRequest`

### AAA Pattern:
Every test method MUST clearly separate the three phases with comments:
```csharp
// Arrange
...

// Act
...

// Assert
...
```

---

## 4. Assertion Standards & Helpers

* **Problem Details Assertions:** Use the extension methods in `Fixtures/Assertions/ProblemDetailsAssertions.cs`:
  ```csharp
  // Business Error (Layer 3)
  await response.ShouldBeProblemAsync(HttpStatusCode.NotFound, Errors.UserNotFound(nonExistentUser.Id));

  // Validation Error (Layer 2)
  await response.ShouldBeValidationProblemAsync("PropertyName");
  ```
* **State Persistence (Layer 4):** Use `QueryDbAsync` to inspect PostgreSQL directly:
  ```csharp
  var userInDb = await QueryDbAsync(db => db.Users.FirstOrDefaultAsync(u => u.Id == user.Id));
  userInDb!.DisplayName.Should().Be("Updated Name");
  ```

---

## 5. Multi-Agent Role Boundaries
* **Test Architect Agent (Phase 1 Red):** May create and modify files inside `tests/ReforaTec.Api.IntegrationTests/`. **Strictly forbidden** from modifying any file inside `src/`.
* **Implementer Agent (Phase 2 Green):** May modify files in `src/`. **Strictly forbidden** from altering assertions or loosening checks inside `tests/` to force a pass.
