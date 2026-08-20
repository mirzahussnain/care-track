# Testing Strategy

## Current Status
Backend automated testing is implemented and includes unit and SQL Server-backed integration coverage.

Frontend and Playwright testing remain planned for Phase 6/7.

## Unit Testing
xUnit unit tests cover areas including:
- domain rules
- workflow transitions
- application services
- validation and exception translation
- current-user dependent application behavior using test doubles

Unit tests remain isolated from Microsoft Entra and SQL Server where appropriate.

## Integration Testing
Integration tests exercise the real ASP.NET Core application host and SQL Server persistence.

Coverage includes:
- API behavior
- database persistence
- workflow behavior
- validation
- pagination/search
- concurrency
- transaction rollback
- exception/status-code mapping
- authentication
- authorization policies
- route-to-policy wiring
- clinical-note authenticated ownership

## Deterministic Test Authentication
External Entra authentication is replaced in integration tests with a deterministic authentication handler.

Reusable test identities model:
- Clinician
- ReferralCoordinator
- Administrator
- basic authenticated user

Tests explicitly create authenticated clients with the minimum required scope/role.

Anonymous clients remain anonymous so 401 behavior can be verified.

## Authorization Testing Layers

### Policy Tests
Verify that policy definitions behave correctly for:
- valid role + scope
- missing scope
- missing role
- wrong role

### Route-Level Tests
Verify that real API endpoints are attached to the intended policy.

Examples include:
- patient read → `ClinicianAccess`
- appointment creation → `ReferralManagement`
- appointment check-in → `ClinicianAccess`
- clinical note read → `ClinicianAccess`

## Security-Sensitive Clinical Note Tests
Tests verify:
- authenticated user becomes `CreatedBy`
- client-supplied `createdBy` cannot override identity
- anonymous caller receives 401
- insufficient role receives 403

## SQL Server Integration
Integration tests use the SQL Server provider rather than replacing relational behavior with SQLite.

This is important for:
- SQL Server concurrency behavior
- constraints
- transactions
- migrations
- provider-specific behavior

## Regression Strategy
After focused changes:
1. run relevant unit/integration test subset
2. run the full integration suite
3. run unit tests
4. build the solution

Exact discovered test totals may change as coverage grows; successful sign-off is based on zero failures rather than a fixed historic count.

## Planned Frontend Testing
Phase 6:
- Angular component tests
- Angular service tests
- authentication/interceptor tests
- route guard tests

## Planned End-to-End Testing
Later Playwright scenarios may cover:

```text
Sign in
→ create/search patient
→ create referral
→ triage/assign
→ schedule appointment
→ clinician workflow
→ clinical note
→ verify role boundaries
```
