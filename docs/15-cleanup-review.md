# Repository Cleanup Review

This review records structure findings without moving files for aesthetics or deleting developer-local state.

## Structure Assessment

- `backend/src` follows the four project boundaries.
- `backend/tests` separates unit and SQL integration tests.
- `backend/tools/CareTrack.DemoSeeder` belongs with backend code because it references Domain and Infrastructure and is part of the solution.
- root `tools/CareTrack.AuthTestClient` is a standalone Node/Entra diagnostic aid. Moving it would add churn without clarifying dependencies.
- numbered documentation retains a useful product-to-operations order; focused portfolio documents use descriptive names.
- existing product-demo images are referenced by the landing page or README; none is dead.
- the two GitHub Actions deployment workflows match the active Azure hosts; no obsolete deployment definition was identified.
- the established `Persistance` folder spelling is intentionally preserved.

## Cleanup Performed

- Replaced the scaffold Angular README with project-specific commands and a link to the complete local guide.
- Replaced stale phase/planned-frontend documentation with current deployed-state documentation.
- Added a documentation index and focused workflow, decisions, recruiter, local-development, portfolio, and cleanup documents.
- Did not remove or move runtime, migration, screenshot, authentication-tool, or deployment files.

## Generated / Ignored Local State Observed

Ignore rules correctly cover build output and local secrets. The working directory contains ignored examples such as `artifacts/`, backend `bin/` and `obj/`, frontend `dist/`, `.angular/`, `node_modules/`, test results, local development settings, and authentication-client local files.

These were not deleted because they may support the developer's current workflow. They are untracked and do not affect the public repository. Published backend artifacts can copy local configuration files; delete them manually when no longer needed and never upload them outside a controlled deployment process.

## Recommendations Not Performed

- Delete ignored build/test artifacts periodically using explicit, verified paths.
- Review the standalone authentication client once it no longer provides diagnostic value.
- Add a Markdown-link checker to CI if documentation churn increases.
- Consider repository-managed disposable SQL only if cross-platform integration setup becomes a priority.
- Keep screenshots current when material UI changes occur; all currently documented recruiter views now have real captures.
- Consider an end-to-end browser suite for both recruiter paths.
- Do not rename `Persistance` merely to fix spelling; it would create broad path/namespace churn without behavior change.
