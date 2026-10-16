---
name: reforatec-git-workflow
description: >-
  Guides the end-to-end Git and GitHub lifecycle in ReforaTec: analyzing status and diffs for atomic commits,
  pushing feature branches, authoring pull requests adhering to PULL_REQUEST_TEMPLATE.md, watching CI checks,
  and executing post-merge repository hygiene. Use this skill whenever asked to commit, push, create a PR,
  prepare commits, inspect git status for staging, or clean up after merging.
---

# ReforaTec Git & GitHub Lifecycle Runbook

This skill defines the standardized 3-phase workflow for version control and pull request management in `ReforaTec.Api`.

---

## Phase 1: Working Tree Analysis & Atomic Commits

Each commit MUST represent a single, logical, and self-contained change following the **Atomic Commits** discipline and **Conventional Commits** in English.

### Step 1: Inspect Changes
```bash
# Check working tree status
git status

# Inspect exact changes to plan atomic groupings
git diff
```

### Step 2: Group Changes Logically (Atomic Commits Principle)
Do NOT stage all files in a single indiscriminate `git add .` commit. Every commit MUST represent a single cohesive purpose and be capable of compiling independently.

Group changes so that each commit addresses a specific logical concern. Examples of common groupings include (non-exhaustive):
- **Cross-Cutting / Infrastructure**: Middleware, authentication, JWT claim handling, error mapping.
- **Vertical Slice Feature**: An endpoint and its dedicated slice files (`Features/{Domain}/{UseCase}/`).
- **Targeted Bug Fix**: Correcting an error code, regex, edge case, or validation rule.
- **Database / Schema**: EF Core entities, configurations, seed data, or migrations.
- **Guidelines & Tooling**: Updates to `GEMINI.md`, skills, templates, or workspace rules (`.agents/`).
- **Automated Tests**: Unit or integration test suites for a specific feature.
- **Refactoring**: Code restructuring, renaming, or cleanup without functional changes.

### Step 3: Stage and Commit
- Format: `type(scope): concise imperative description in English`
- Permitted types: `feat`, `fix`, `refactor`, `chore`, `docs`, `test`, `perf`, `build`, `ci`.
- Examples:
  - `feat(trees): implement CreateTreeService endpoint`
  - `fix(security): rename tenant claim to tenant_id to avoid inbound claim mapping`
  - `fix(errors): map ErrorType.Forbidden to 403 Forbidden in ErrorMappingExtensions`
  - `chore(agent): update slice templates and architecture guidelines`

```bash
# Stage specific files for the atomic change
git add <file1> <file2>
git commit -m "type(scope): description"
```

---

## Phase 2: Push, PR Creation & CI Verification

Every change merged into `main` MUST go through a Pull Request and pass CI validation.

### Step 1: Push Branch to Remote
```bash
git push -u origin <branch-name>
```

### Step 2: Inspect Recent PR History & Author Pull Request
Before drafting the PR, inspect recent merged PRs to adhere to the established tone, depth, and formatting style of the repository:

```bash
# List recent merged PRs and inspect the latest one for style reference
gh pr list --state merged --limit 3
gh pr view <recent-pr-number>
```

Then create the PR strictly adhering to `.github/PULL_REQUEST_TEMPLATE.md`:

```bash
gh pr create --base main --head <branch-name> \
  --title "type(scope): concise description" \
  --body "## 📝 Summary
[Brief description of the change and business value.]

## 🛠️ Changes
- [x] [Specific change 1]
- [x] [Specific change 2]

## 🧠 Why this approach?
- **Performance:** [Evaluation under performance prism.]
- **Maintainability:** [Evaluation under maintainability prism.]
- **Security:** [Evaluation under security prism.]

## ✅ Validation
- [x] Code compiles (\`dotnet build\` with 0 errors).
- [x] Endpoints tested via \`curl\` with Bearer JWT tokens.
- [x] Database schema and queries verified via Podman / psql."
```

### PR Conciseness & Formatting Rules:
- **Checkbox Syntax:** Always use `- [x]` in markdown source for changes and validation items.
- **Flat Single-Level Bullets:** Strictly avoid deep nested bullet points, multi-tiered indentations, or walls of text. Keep each item to one crisp, self-contained sentence.
- **Punchy Summary:** 1-2 sentences explaining what was implemented and its direct business value.
- **Three Prisms Rationale:** Deliver clear, high-signal rationale under Performance, Maintainability, and Security without verbose padding.

### Step 3: Monitor CI Pipeline
```bash
gh pr checks <pr-number> --watch
```
Ensure all checks pass (`CI/Build & Verify`) before proceeding to merge.

---

## Phase 3: Post-Merge Repository Hygiene

Once the PR has been merged on GitHub (either by the user or via `gh pr merge`), immediately clean up the local environment:

### Step 1: Switch to `main` and Pull
```bash
git checkout main
git pull
```

### Step 2: Prune Remote Tracking Branches
```bash
git fetch --prune
```

### Step 3: Delete Merged Local Branch
```bash
git branch -d <branch-name>
```

### Step 4: Verify Preserved Stashes
```bash
# Verify that user WIP stashes (e.g. integration tests) were not dropped
git stash list
```

---

## Summary Checklist

| Action | Command / Standard |
| :--- | :--- |
| **Commit Language** | English only (`type(scope): description`). |
| **Commit Discipline** | Atomic (one logical change per commit). |
| **PR Template** | Must follow `.github/PULL_REQUEST_TEMPLATE.md`. |
| **Prisms Requirement** | Explain "Why" under Performance, Maintainability, Security. |
| **CI Gate** | Must verify all checks pass (`gh pr checks <pr> --watch`). |
| **Clean Up** | `checkout main` $\rightarrow$ `pull` $\rightarrow$ `fetch --prune` $\rightarrow$ `branch -d`. |
