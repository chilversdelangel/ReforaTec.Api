# Contributing to ReforaTec

Thank you for contributing to **ReforaTec.Api**! This document outlines our repository standards, Git workflow, commit discipline, and Pull Request lifecycle for both human developers and autonomous AI agents.

---

## 1. Branch Naming Conventions

All development occurs on short-lived feature or maintenance branches branched from `main`. Branch names must follow the format:

```text
<type>/<kebab-case-description>
```

### Allowed Prefixes:
* `feat/`: New endpoint, domain logic, or business feature (e.g. `feat/create-measurement`).
* `fix/`: Bug fix in validation, authorization, error mapping, or data persistence (e.g. `fix/tenant-claim-mapping`).
* `refactor/`: Code reorganization, method decomposition, or cleanup without functional changes (e.g. `refactor/decompose-tree-queries`).
* `test/`: New integration tests, modular fixtures, or testing infrastructure (e.g. `test/update-user-profile-matrix`).
* `docs/`: Documentation updates (`docs/`, `AGENTS.md`, READMEs) (e.g. `docs/context-engineering-architecture`).
* `chore/`: Tooling, CI/CD pipelines, package dependencies, or repository hygiene (e.g. `chore/dockerignore-test-exclusions`).

---

## 2. Commit Discipline & Format

### The Atomic Commits Principle
Each commit MUST represent a **single, logical, and self-contained change**:
* **Never** bundle unrelated changes in a single indiscriminate `git add .` commit.
* Each commit must leave the repository in a **working, compilable state** (`dotnet build` succeeds with zero errors).
* Separate infrastructure/schema changes, feature logic, and tests into dedicated commits when appropriate.

### Conventional Commits Standard
Commit messages must strictly follow the Conventional Commits specification in **English** and be **strictly single-line**:

```bash
git commit -m "type(scope): concise imperative description in English"
```

* **Single-Line Invariant:** NEVER include multi-line bodies, second `-m` flags, or bullet points in the git commit message. All detailed architectural rationale, change breakdowns, and the 3 Prisms belong exclusively in the **GitHub Pull Request description**.
* **Types:** `feat`, `fix`, `refactor`, `chore`, `docs`, `test`, `perf`, `build`, `ci`.
* **Scope:** The affected domain or layer (e.g. `trees`, `users`, `auth`, `infra`, `database`, `agents`).
* **Imperative mood:** "implement endpoint", "fix query error" (not "implemented" or "fixing").

---

## 3. Pre-Commit Quality Verification

Before committing changes, verify the solution health locally:

```bash
# 1. Verify build with zero compiler warnings and zero errors
dotnet build --disable-build-servers

# 2. Run the integration test suite
dotnet test --no-build
```

---

## 4. Pull Request Standards

Every change merged into `main` must pass through a Pull Request and adhere strictly to [`.github/PULL_REQUEST_TEMPLATE.md`](.github/PULL_REQUEST_TEMPLATE.md):

1. **Title:** Match Conventional Commits (e.g. `feat(users): implement UpdateUserProfile endpoint`).
2. **Body:** Complete all template sections:
   * 📝 **Summary:** Business value and problem solved.
   * 🛠️ **Changes:** Bulleted list of implemented components.
   * 🧠 **Why this approach?:** Architectural trade-offs and rationale (evaluated under *Performance, Maintainability, Security*).
   * ✅ **Validation:** Confirmation of build health and integration test execution.
3. **CI Quality Gate:** All GitHub Actions checks (`CI / Build & Verify`) must be 100% green before merging.
4. **Merge Method:** Use **Squash and Merge** or **Rebase and Merge** to maintain a clean linear history on `main`.

---

## 5. Post-Merge Repository Hygiene

After a PR is merged into `main`:
```bash
git checkout main
git pull origin main
git branch -d <branch-name>
```
