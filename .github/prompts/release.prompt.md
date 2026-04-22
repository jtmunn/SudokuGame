# Release & Tag

Prepare a new release for this project. Follow these steps in order:

## 0. Pre-release checks

Before proceeding, ask the user whether to run the build and unit tests now:
- If yes, run the build task (`dotnet build`) and all unit tests. If either fails, stop and fix before continuing.
- If the user confirms they have already passed build + tests (or wants to skip), proceed without running them.

## 1. Gather unreleased changes

- Run `git log --oneline` to see commits since the last tag (use `git describe --tags --abbrev=0` to find it).
- Summarize the meaningful changes (skip merge commits, changelog-only commits, and version bumps).

## 2. Determine the new version

- Read the current version from `Directory.Build.props` (`ApplicationDisplayVersion`).
- Based on the changes, suggest the next version (patch for fixes/minor improvements, minor for new features, major for breaking changes).
- Ask the user to confirm the version number before proceeding.

## 3. Audit documentation

Review the docs for drift caused by the unreleased changes. Cross-reference the change list from step 1 against:

- `README.md`
- `docs/DEVELOPERS.md` — architecture diagram, project list, build commands, dependency table, design decisions
- `docs/CONSTANTS_REFERENCE.md` — must match the current code
- `docs/TROUBLESHOOTING.md`, `docs/INSTALLATION.md`, `docs/WINDOW_SIZE_PERSISTENCE.md`, `docs/DIFFICULTY_ALGORITHM_RESEARCH.md` — only if relevant areas changed
- `.github/copilot-instructions.md` — if architecture, conventions, or critical rules changed

Report which docs (if any) are stale and ask the user whether to:
- **Fix in this release** (preferred — docs ship with the code that made them stale), or
- **Defer** and note as a follow-up.

If fixing, make the doc updates now so they're included in the release commit.

## 4. Update version and changelog

- Update `Directory.Build.props`: bump `ApplicationDisplayVersion` and increment `ApplicationVersion` by 1.
- Add a new section to `CHANGELOG.md` under the `---` separator, above the previous release, following the existing format:
  ```
  ## [X.Y.Z] - YYYY-MM-DD

  ### Added/Changed/Deprecated/Removed/Fixed/Security
  - Description of change
  ```
- Use today's date. Use only the six [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) categories listed above.

## 5. Review

- Show the user a summary of all changes made (version bump + changelog entry + any doc updates).
- **Stop and wait** for the user to review and approve before continuing.

## 6. Commit, tag, and push

Only after the user approves:

- Stage all changes: `git add -A`
- Commit with message: `Release vX.Y.Z - brief summary` (single-line, no multi-line messages)
- Create tag: `git tag vX.Y.Z`
- Ask the user if they want to push (commit + tag to origin), which will trigger the automated build and release workflow.
- If yes: `git push origin main --tags`
