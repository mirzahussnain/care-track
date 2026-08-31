# Portfolio Summary

## One-Line Description

CareTrack is a deployed Angular and ASP.NET Core clinical referral workflow portfolio application with Microsoft Entra RBAC, Azure SQL persistence, explicit state orchestration, and a recruiter-ready synthetic demo.

## Two-Line CV Version

Built and deployed a full-stack healthcare workflow portfolio application using Angular 22, ASP.NET Core .NET 10, EF Core, Azure SQL, and Microsoft Entra ID. Implemented policy-based RBAC, explicit referral/appointment state machines, retry-safe transactions, health/readiness checks, CI/CD, and a guarded synthetic demo environment.

## CV Bullet Version

- Designed an Angular and ASP.NET Core system using Clean Architecture to manage synthetic patient, referral, appointment, and Clinical Note workflows.
- Integrated Microsoft Entra ID/MSAL with delegated API scopes, app roles, named authorization policies, and deterministic 401/403 integration coverage.
- Implemented EF Core/SQL Server persistence, optimistic concurrency, serializable overlap protection, and retry-safe cross-aggregate transaction verification.
- Removed appointment-list request/query fan-out with a narrow SQL Server view mapped as an EF Core keyless operational read model.
- Deployed independent frontend/API pipelines to Azure Static Web Apps and App Service with Azure SQL serverless, liveness/readiness endpoints, sanitized logging, and a guarded recruiter demo reset.

## LinkedIn Project Description

CareTrack is my independent full-stack healthcare workflow portfolio project. It connects synthetic patient registration, referral triage and assignment, appointment scheduling and clinical workflow, and Clinical Notes in an Angular 22 interface backed by ASP.NET Core .NET 10, EF Core, and Azure SQL. Microsoft Entra ID and MSAL provide real OAuth/OIDC sign-in, while API policies enforce role and scope boundaries for Clinician and Referral Coordinator users. I also built retry-safe transactions, optimistic concurrency, separate liveness/readiness checks, sanitized production logging, GitHub Actions delivery, and a guarded shared-demo reset. The live recruiter experience uses synthetic data only and makes no claim of clinical certification or real healthcare use.

## GitHub Repository Description

Angular + ASP.NET Core referral workflow portfolio app with Entra RBAC, EF Core/Azure SQL, CI/CD, health checks, and a synthetic recruiter demo.

## Interview Explanation: 30 Seconds

CareTrack is a deployed portfolio system for the workflow around referrals: register a synthetic patient, triage and assign a referral, schedule and progress an appointment, and record Clinical Notes. The Angular SPA signs users in with Microsoft Entra, while the .NET API remains authoritative for role and scope checks. The engineering focus is explicit state machines, cross-aggregate transactions, concurrency/retry safety, and a recruiter demo that uses the real auth path without any real healthcare data.

The appointment list originally caused client-side enrichment fan-out, producing many patient and referral lookups per page. I introduced a narrow SQL Server operational view mapped as an EF Core keyless read model so the list is served from one joined read path while keeping transactional workflow logic in the domain and application layers.

## Interview Explanation: Two Minutes

I built CareTrack to show more than CRUD. Domain owns referral and appointment invariants; Application orchestrates cross-aggregate use cases; Infrastructure implements EF Core repositories and transactions; and API handles HTTP, Entra validation, policies, Problem Details, and health checks.

Appointment creation verifies that patient and referral match, checks eligibility, protects the overlap query with a serializable transaction, creates the appointment, and advances an Assigned referral to Scheduled atomically. Because SQL retries can encounter an ambiguous commit, the transaction abstraction clears tracked state and verifies a persisted marker before repeating work. Starting an appointment can similarly advance a Scheduled referral to InProgress. Completing the appointment deliberately does not close the referral; explicit referral completion checks all related appointments first.

Angular uses MSAL Authorization Code Flow with PKCE. The API requires the delegated scope plus Clinician or Referral Coordinator roles according to the route; Administrator is not a superuser. Demo identities are real restricted Entra accounts, and `isDemoAccount` controls only a synthetic-data banner. Production runs on Azure Static Web Apps, App Service, and Azure SQL serverless through GitHub Actions. A guarded manual tool resets the shared deterministic dataset.

## Key Technical Achievements

- Angular 22 responsive, role-aware UI with MSAL and an accessible recruiter credential flow
- ASP.NET Core .NET 10 organized into Domain, Application, Infrastructure, and API boundaries
- referral and appointment state machines with explicit transition endpoints and history
- application-layer referral/appointment orchestration with EF Core and SQL Server
- patient optimistic concurrency and scheduling overlap protection
- deterministic synthetic dataset and migration-preserving reset tool
- independent Azure frontend/backend GitHub Actions pipelines

## Reliability and Security Achievements

- delegated `access_as_user` scope plus named role policies on every business endpoint
- API-derived Clinical Note creator identity and no local credential store
- authorization tests covering anonymous, missing-scope, wrong-role, and valid-role callers
- retry-safe transactions with persisted-state verification and deadlock translation
- separate SQL-independent liveness and SQL-backed readiness endpoints
- sanitized production errors/logging, restricted CORS, HTTPS, and secrets outside source

## Honest Scope Boundary

CareTrack uses synthetic data and shared demo identities. It is not used by the NHS or another healthcare provider, is not clinically certified, and has no measured production-scale, penetration-test, or formal accessibility-audit evidence.
