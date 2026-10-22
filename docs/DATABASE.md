# ReforaTec — Database & Persistence Invariants

## 1. Engine & ORM
* **Database Engine:** PostgreSQL 18
* **ORM:** Entity Framework Core 10 (Npgsql.EntityFrameworkCore.PostgreSQL)
* **Migrations:** Code-First EF Core Migrations located in `src/ReforaTec.Api/Database/Migrations/`

---

## 2. Entity Invariants & Base Types

### Base Entity Hierarchy
* **`CreatableEntity`:** Entities that track creation metadata (`CreatedAtUtc`, `CreatedBy`).
* **`AuditableEntity`:** Extends `CreatableEntity` with mutation metadata (`UpdatedAtUtc`, `UpdatedBy`, `IsDeleted`).
* **Soft Delete:** Entities implementing soft delete MUST filter out deleted records in all queries (`!x.IsDeleted`).

### Normalization Pattern (`INormalizable`)
Every text property with a unique constraint or lookup behavior (e.g., `Email`, `InstitutionName`, `Folio`) MUST implement `INormalizable`.
* The entity exposes a canonical normalized property (e.g., `Email` $\to$ `NormalizedEmail`).
* `AppDbContext.SaveChangesAsync` automatically executes `.Normalize()` on all tracked `INormalizable` entries before persisting.
* Handlers perform lookups using the `.ToNormalized()` extension on incoming input DTOs.

### Timezones & Timestamps
* **DTO Boundary:** DTOs accepting timestamps MUST use `DateTimeOffset` to preserve client timezones.
* **Database Persistence:** Entities store `.UtcDateTime` (`timestamp with time zone` in PostgreSQL). Never store naive `DateTime` values.

---

## 3. LINQ Standards & Performance

### Mandatory Fluent Method Syntax
Always use **Fluent Method Syntax** with chained methods. Never use SQL-like Query Syntax (`from ... join ... select`).

```csharp
// CORRECT: Fluent Method Syntax
var activeTrees = await context.Trees
    .AsNoTracking()
    .Where(tree => tree.TenantId == tenantId && !tree.IsDeleted)
    .Select(tree => new Response(...))
    .ToListAsync(cancellationToken);

// FORBIDDEN: Query Syntax
var activeTrees = await (from tree in context.Trees ... select ...);
```

### Read-Only Performance
All read-only queries MUST explicitly declare `.AsNoTracking()` to bypass EF Core change tracking overhead.

### Semi-Joins without Navigation Properties
When relating entities without direct EF Core navigation properties, prefer subquery expressions using `IQueryable.Contains(...)`:

```csharp
var assignedTreeIds = context.UserCaresForTrees
    .Where(assignment => assignment.UserId == userId && assignment.EndDate == null)
    .Select(assignment => assignment.TreeId);

var trees = await context.Trees
    .AsNoTracking()
    .Where(tree => assignedTreeIds.Contains(tree.Id))
    .ToListAsync(cancellationToken);
```
PostgreSQL compiles this to high-performance `Nested Loop Semi Join` operations with early exit.

### Query Decomposition Rule
Handlers executing multiple queries, aggregations, or complex lookups MUST decompose each independent operation into private static helper methods, keeping the top-level `Handle` orchestrator flat and readable.
