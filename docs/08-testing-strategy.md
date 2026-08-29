# Testing Strategy

CareTrack has automated backend unit, SQL Server integration, and Angular/Vitest coverage. The recorded baseline before this documentation phase was 249 unit tests, 198 integration tests, and 307 frontend tests passing; the production Angular and backend Release builds also passed. Counts are a recorded checkpoint, not a permanent assertion as coverage evolves.

## Backend Unit Tests

xUnit tests cover domain transitions, invariants, application services, validation, exception translation, transaction behavior through fakes, configured assignment targets, and the deterministic demo dataset/reset application. Unit tests avoid Entra and SQL where those dependencies are not part of the behavior.

## SQL Server Integration Tests

Integration tests run the real ASP.NET Core host and EF Core SQL Server provider. Coverage includes:

- API contracts, persistence, migrations, filtering, paging, and ordering
- referral and appointment workflow integration
- patient optimistic concurrency
- serializable appointment scheduling and overlap behavior
- transaction rollback, transient retry safety, and ambiguous-commit verification
- Problem Details and status-code mapping
- liveness/readiness and sanitized production logging
- demo-seeder guards, repeat resets, referential integrity, and migration-history preservation

The configured database is `CareTrackIntegrationTests` on local SQL Server. The repository does not currently provision a Docker container. Developers must keep this database disposable and separate from shared demo/production data.

## Deterministic Authentication and Authorization

Integration tests replace external Entra authentication with a deterministic scheme while retaining the production authorization policies. Test identities model anonymous, authenticated/no-role, missing-scope, Clinician, ReferralCoordinator, and Administrator callers.

Coverage verifies policy definitions and route wiring, including:

- `401` for an absent/unusable authentication context
- `403` for authenticated callers missing scope or role
- full patient reads and appointment workflow requiring `ClinicianAccess`
- referral-safe patient lookup, referral workflow, and appointment creation using `ReferralManagement`
- no implicit Administrator clinical bypass
- Clinical Note creator identity derived from the authenticated principal
- `GET /api/me` role and demo metadata without demo-based elevation

## Frontend Tests

Angular unit/component tests run with the Angular unit-test builder and Vitest. Coverage includes services and API URL construction, validation/error mapping, authentication state, role directives/navigation, page states and actions, accessible focus behavior, the recruiter credentials dialog, demo login hint/clipboard flow, and the authenticated demo banner.

## Build Verification

```powershell
dotnet build backend/CareTrack.slnx --configuration Release
dotnet test backend/tests/CareTrack.UnitTests/CareTrack.UnitTests.csproj --configuration Release
dotnet test backend/tests/CareTrack.IntegrationTests/CareTrack.IntegrationTests.csproj --configuration Release

Set-Location frontend/caretrack-web
npm test -- --watch=false
npm run build
```

The backend GitHub Actions workflow runs restore, Release build, and unit tests before publish. The frontend workflow runs `npm ci`, a production build, and artifact/path checks. The current CI does not provision SQL Server for the integration suite.

## Remaining Gaps

- no automated end-to-end browser suite
- no repository-managed disposable SQL container
- no load/performance evidence
- no formal accessibility audit or penetration test

These are explicit portfolio limitations rather than implied coverage.
