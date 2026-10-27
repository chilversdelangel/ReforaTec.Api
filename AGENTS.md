# ReforaTec.Api — Agent Guidelines & Engineering Standards

## 1. Project Context & Documentation Hub
* **Scale:** Multi-tenant environmental tracking and tree care platform across educational campuses (TecNM / ITCM).
* **Consumers:** Android mobile apps (Students, Inspectors) and Web Portal (Coordinators, Admins).
* **Reference Hub (Cold Memory):**
  * Product Domain & Personas: [`docs/PRODUCT.md`](docs/PRODUCT.md)
  * Technical Architecture & Ingress: [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md)
  * Database & Persistence Invariants: [`docs/DATABASE.md`](docs/DATABASE.md)
  * Testing Guidelines & Fixtures: [`tests/ReforaTec.Api.IntegrationTests/AGENTS.md`](tests/ReforaTec.Api.IntegrationTests/AGENTS.md)
  * Subagent Topology & Parallel Execution: [`docs/SUBAGENTS.md`](docs/SUBAGENTS.md)
  * Contribution & Git Workflow: [`CONTRIBUTING.md`](CONTRIBUTING.md)

---

## 2. Tech Stack & Repository Map
* **Runtime:** .NET 10 / C# 13
* **Data & ORM:** PostgreSQL 18 + EF Core 10 (Npgsql)
* **Validation:** FluentValidation 12
* **Error Handling:** ErrorOr 2
* **Mapping:** Mapster 10
* **Authentication:** Microsoft.AspNetCore.Authentication.JwtBearer 10

### Monorepo Structure:
```text
ReforaTec/
  ├── src/ReforaTec.Api/
  │    ├── Database/                   # AppDbContext, Migrations, Seeding
  │    ├── Entities/                   # Domain Entities & Enums
  │    ├── Features/{Domain}/{UseCase} # Vertical Slices (Endpoints & Logic)
  │    └── Infrastructure/             # Security, Filters, OpenApi, Mapping
  ├── tests/ReforaTec.Api.IntegrationTests/
  │    ├── Features/{Domain}/          # Integration test classes
  │    └── Fixtures/                   # Testcontainers, Data Object Mothers
  └── docs/                            # Deep domain & architectural specs
```

---

## 3. Mandatory Invariants
* **Error Flow:** ALL business errors MUST return `ErrorOr<T>`. Never throw exceptions for expected business flow.
* **Error Codes:** Standard string pattern `"entity/kebab-case"` (e.g. `"tree/not-found"`, `"auth/unauthorized"`).
* **Tenant Isolation:** Every query and mutation on tenant-scoped entities MUST enforce `TenantId == user.TenantId`. Never trust client-provided tenant identifiers.
* **Identity Resolution:** Use `ICurrentUserService` and `UserIdentity` (resolved from JWT claims). Result variable MUST be named `identityResult`:
  ```csharp
  var identityResult = currentUser.GetIdentity();
  if (identityResult.IsError)
  {
      return identityResult.Errors.ToProblem();
  }
  ```
* **Resource Authorization:** Accessing or operating on resources not actively assigned or owned within the tenant MUST return `Error.Forbidden` (`StatusCodes.Status403Forbidden`).
* **JWT Claims:** Custom JWT claims MUST use standard snake_case names (`tenant_id`, `sub`, `role`). Never use Microsoft-reserved short abbreviations (`tid`, `oid`).
* **DTO Boundary:** Endpoints MUST NEVER return EF Core database entities. Always map to immutable `Response` DTO records.
* **Unique Text Fields:** Unique text properties MUST implement `INormalizable` (handled by `AppDbContext`).
* **Soft Delete:** Entities implementing soft delete MUST filter out deleted records (`!x.IsDeleted`).
* **User-Scoped Routing:** Endpoints operating on or projecting data for the authenticated user MUST use the `/users/me/...` route hierarchy (e.g. `GET /users/me/profile`, `PATCH /users/me`).
* **Security:** Never leak user existence in authentication flows (OWASP User Enumeration Prevention).
* **Language:** ALL code, identifiers, XML docs, comments, git commits, and documentation in English.

---

## 4. Code Conventions
* **Encapsulation:** DTOs (`Request`, `Response`) and `Validator` classes MUST be `internal sealed`. Handlers MUST be `internal static`.
* **Timezones:** DTOs accepting timestamps MUST use `DateTimeOffset`. Database entities store `.UtcDateTime`.
* **Read Performance:** All read-only database queries MUST use `.AsNoTracking()`.
* **LINQ Style:** Always use **Fluent Method Syntax** (`context.Entity.AsNoTracking().Where(...).Select(...)`). Never use Query Syntax (`from ... join ... select`). For unmapped relations, use subqueries with `IQueryable.Contains(...)`.
* **Handler & Method Decomposition:** Handlers executing multiple queries, complex middleware configurations, or deeply nested lambdas MUST decompose each independent operation into private static helper methods.
* **Domain Ubiquitous Language over Framework Jargon:**
  * Model identifiers and class/record names after the domain purpose and user intent, NEVER by blindly parroting low-level framework artifacts (e.g. use `MaxAllowedAttempts` over `PermitLimit`, `attemptTime` over `now`, `RateLimitState` over `OtpAttempt`, `WindowDuration` over raw `Window`).
  * Variables MUST describe what they represent; strictly avoid vague pronouns or generic nouns (`state`, `current`, `updated`, `obj`, `data`). Use explicit contextual names (`currentState`, `updatedState`, `rateLimitState`).
* **Temporal Consistency (Single Time Snapshot):**
  * When executing operations that evaluate or record timestamps across multiple checks, capture time **EXACTLY ONCE** at the entry point of the pipeline (`var attemptTime = DateTimeOffset.UtcNow;`).
  * Pass this immutable snapshot as an explicit parameter (`attemptTime`) to pure domain calculation methods (`HasReset(attemptTime)`, `GetSecondsUntilReset(attemptTime)`).
  * NEVER make multiple unsynchronized calls to `DateTimeOffset.UtcNow` across a request pipeline, and NEVER hide calls to `UtcNow` inside record properties or getters.
* **Configuration Time Unit Suffixes (`appsettings.json`):**
  * Time durations in `appsettings.json` and options POCO classes MUST be scalar integers with explicit unit suffixes (`*Minutes`, `*Hours`, `*Days`) (e.g., `OtpEmailWindowMinutes: 5`, `AccessTokenExpirationHours: 3`).
  * NEVER use raw string-based `TimeSpan` formats (e.g. `"00:05:00"`) in configuration, preventing parsing ambiguities and .NET TimeSpan formatting traps (e.g., `"05:00"` parsing as 5 hours instead of 5 minutes).
* **Control Flow & Formatting:**
  * Always use braces `{}` on separate lines (Allman style) for control blocks (`if`, `foreach`, `while`).
  * Favor early exits and guard clauses to minimize indentation (strictly maximum 2-3 indentation levels; no arrow-shaped callback pyramids).
  * Use expression-bodied members (`=>`) for concise single-statement methods and properties.
  * Always use descriptive lambda parameter names (`tree => tree.Id`, `request => request.ServiceTypeId`), strictly avoiding single-letter abbreviations (`x =>`, `t =>`).
* **OpenAPI Documentation:** Every endpoint MUST declare `.WithName()`, `.WithTags()`, `.WithSummary()`, `.WithDescription()`, and explicit `.Produces<T>()` status codes.

---

## 5. Anti-Patterns (What NOT to do)
* **DO NOT** use traditional MVC Controllers, MediatR, or CQRS libraries. Keep use cases self-contained in Minimal APIs.
* **DO NOT** use LINQ SQL Query Syntax (`from ... select`).
* **DO NOT** use single-letter lambda parameter names (`x =>`, `t =>`, `e =>`).
* **DO NOT** write deeply nested lambdas, callback hell, or arrow-shaped code pyramids exceeding 2-3 indentation levels; decompose into private static helpers.
* **DO NOT** throw exceptions for expected business errors; use `ErrorOr<T>`.
* **DO NOT** return tuples, structs, or `out` parameters containing artificial dummy/sentinel values (e.g., `return (false, 0)`) for non-applicable states. Keep methods single-responsibility: return explicit domain records/results, and evaluate conditional metrics (`GetSecondsUntilReset`) on demand only when applicable.
* **DO NOT** use raw `out var` parameters in business/application logic; prefer explicit return types, pattern matching, or domain records.
* **DO NOT** query `DateTimeOffset.UtcNow` repeatedly across a method or hide it inside domain records/structs; capture a single immutable `attemptTime` snapshot at the pipeline entry.
* **DO NOT** name domain classes or properties after third-party library abstractions when ubiquitous domain names exist.
* **DO NOT** modify test assertions in `tests/` to force a failing test to pass during implementation phases.

---

## 6. Deterministic Verification Commands

```bash
# Verify build health with zero warnings and zero errors
dotnet build --disable-build-servers

# Run full integration test suite (Testcontainers PostgreSQL 18 in RAM)
dotnet test --no-build

# Run a specific feature test class
dotnet test --no-build --filter "FullyQualifiedName~{UseCase}Tests"
```

---

## 7. Multi-Agent Collaboration & Authorization Gate
* **Primary Agent (Coordinator):** WAIT for explicit user authorization before modifying codebase files or launching subagents.
* **Delegated Subagents:** Once authorized by the user, subagents execute autonomously within their assigned boundaries (see [`docs/SUBAGENTS.md`](docs/SUBAGENTS.md) for full personas, system prompts, and parallel execution protocol):
  * **Test Architect Agent (Phase 1 Red):** Operates strictly within `tests/ReforaTec.Api.IntegrationTests/`.
  * **Implementer Agent (Phase 2 Green):** Operates strictly within `src/ReforaTec.Api/`.
  * **Reviewer Agent (Phase 3 Refactor):** Read-only inspection against this file and zero-warning verification.

---

## 8. Git & Contribution Workflow
* **Commit Discipline:** Atomic Commits (one logical, self-contained change per commit).
* **Format:** Strict single-line Conventional Commits in English (`type(scope): description`). NEVER include multi-line bodies or bullet points in git commit messages; all detailed rationale and prism evaluations belong exclusively in the GitHub Pull Request description.
* **PR Standards:** Adhere strictly to [`.github/PULL_REQUEST_TEMPLATE.md`](.github/PULL_REQUEST_TEMPLATE.md) with flat single-level bullets, `- [x]` syntax, and rationale evaluated under the 3 Prisms (*Performance, Maintainability, Security*).
* **CI Quality Gate:** All GitHub Actions checks must pass (`gh pr checks <pr> --watch`) before merging.
* **Merge Method:** Context-dependent, always passing `--delete-branch`. Use standard merge commit (`gh pr merge --merge --delete-branch`) for multi-commit feature branches to preserve atomic commit history. Use squash and merge (`gh pr merge --squash --delete-branch`) for single-commit or cohesive documentation/chore/hotfix branches to maintain a clean linear history without redundant merge bubbles.
* **Canonical Runbook:** Consult [`CONTRIBUTING.md`](CONTRIBUTING.md) and [`reforatec-git-workflow`](.agents/skills/reforatec-git-workflow/SKILL.md) for full branch lifecycle and post-merge hygiene.
