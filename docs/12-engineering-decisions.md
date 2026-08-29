# Engineering Decisions

These notes summarize the practical choices behind CareTrack and the trade-offs worth discussing in an interview.

## 1. Microsoft Entra ID instead of custom authentication

Identity lifecycle, passwords, MFA, token issuance, and app-role assignment stay with an established identity provider. CareTrack can focus on workflow authorization and avoids building a sensitive credential system that adds little portfolio value.

## 2. API authorization is authoritative

Angular hides or adapts controls for clarity, but browser state can be altered. Every business endpoint therefore declares a named policy requiring the delegated scope and appropriate app role. UI role checks improve experience; they never create permission.

## 3. Demo users are real Entra identities

The recruiter path exercises the same MSAL redirect, token validation, scope, role claims, and API policies as any other user. A local authentication bypass would make the easiest demo path the least representative one.

## 4. Demo metadata grants nothing

`isDemoAccount` is calculated server-side from the authenticated object ID and displayed as a synthetic-data banner. Policies do not inspect it. Compromising or changing presentation metadata cannot elevate access.

## 5. Public demo credentials are limited to restricted synthetic identities

Credentials displayed on a public landing page are acceptable here only because these accounts are dedicated to a shared portfolio environment, have narrow app-role assignments, and can access synthetic data only. They must not be reused elsewhere, granted tenant privileges, or treated as a pattern for real clinical users. Passwords are intentionally omitted from repository documentation.

## 6. Azure SQL serverless for portfolio hosting

Serverless Azure SQL preserves real SQL Server/EF Core behavior while reducing idle hosting cost for an intermittently used demo. The trade-off is cold-start latency, which is why readiness may briefly fail while the database resumes.

## 7. Separate liveness and readiness

Liveness answers “is the API process responding?” without depending on SQL. Readiness answers “can this instance serve database-backed work?” Keeping them separate prevents a database pause from being mistaken for a dead process and supports platform routing if the plan is upgraded.

## 8. Referral completion stays explicit

Completing an appointment is not identical to closing the wider referral. The explicit referral operation validates all related appointments and makes outstanding work less likely to disappear through an incidental state change.

## 9. Cross-aggregate orchestration belongs in Application

Appointment creation/start can also change referral state. Application services coordinate repositories and transactions, while each entity remains responsible for its own legal transitions. This avoids coupling Domain entities to persistence or making controllers transaction scripts.

## 10. Retry-safe transaction verification

SQL transient retry can encounter an ambiguous commit: the client loses confirmation even though the database committed. CareTrack clears tracked entities and verifies persisted markers before retrying, reducing duplicate or partially repeated cross-aggregate work. Deadlock victims are translated to a retryable concurrency response.

## 11. Demo seeding is explicit, never startup behavior

Startup seeding would make an application restart destructive and could race with active visitors. The separate tool requires the exact database name, no pending migrations, a connection supplied through the environment, and exact human confirmation. It preserves migration history.

## 12. One shared synthetic database

A shared database keeps portfolio hosting simple and lets both demo roles see the same workflow. The accepted limitation is visitor interference. The landing page states that changes are shared, and an operator can restore the deterministic baseline manually. Per-visitor tenancy or scheduled reset would add infrastructure not justified by current demo usage.
