# Security

## Current Status
Core backend security controls are implemented for the current portfolio scope.

CareTrack uses synthetic data only and does not claim production healthcare certification or regulatory compliance.

## Implemented Principles
- Microsoft Entra authentication
- delegated `access_as_user` API scope
- explicit CareTrack application roles
- named authorization policies
- least privilege
- no universal Administrator bypass
- HTTPS redirection
- server-derived authenticated ownership
- input validation
- centralized exception handling
- generic unexpected-error responses
- SQL Server referential integrity
- optimistic concurrency
- transactional scheduling protections
- deterministic authorization tests
- no committed secrets
- limited anonymous API surface
- development-only OpenAPI exposure

## Authorization Boundary
Business endpoints require explicit named policies.

Intentionally anonymous:
- `/api/health` (SQL-independent liveness)
- `/api/health/ready` (database readiness with sanitized output)

Development-only:
- OpenAPI endpoint(s)

The default template `/weatherforecast` endpoint was removed as unnecessary public surface.

## Trusted Identity
CareTrack uses the authenticated Microsoft Entra object ID as the stable user identifier for server-derived ownership.

The application does not trust:
- email address as a durable security identifier
- display name
- client-supplied `createdBy`
- arbitrary user IDs in request bodies

## Role Model
- `Clinician` — clinical reads/workflows and clinical notes
- `ReferralCoordinator` — referral administration and scheduling
- `Administrator` — future system administration only

A valid login alone does not grant business access. Policies require both delegated scope and appropriate application role.

## Patient Access Boundary
Patients are not authenticated actors in v1.

A future patient portal would require:
- patient-to-Entra identity mapping
- resource-level ownership checks
- patient-safe API contracts
- explicit privacy/consent design

A simple `Patient` role would not be sufficient.

## Identity Administration
User account lifecycle, passwords, MFA, and app-role assignment remain Microsoft Entra responsibilities.

A future CareTrack admin dashboard could integrate with Microsoft Graph for selected staff-access operations, but such permissions would be introduced only with a defined requirement and threat model.

## Logging
Production logging records only HTTP method, matched route template, status, trace ID, sanitized exception category, SQL error number, and retry-exhausted state where applicable. Raw route values, request data, exception messages, provider details, clinical content, tokens, credentials, and unnecessary identifiers are excluded.

Unexpected server failures return generic client-facing details.

## Secrets
Credentials and environment-specific identifiers must remain outside Git.

The temporary development authentication client uses local environment configuration and must not commit secrets or tokens.

## Recruiter Demo Boundary

Public demo credentials are acceptable only for the dedicated, restricted Entra identities connected to this shared synthetic environment. They must not be reused for another service, granted tenant-wide privileges, or given access to real data.

The API derives `isDemoAccount` from authenticated object IDs for a visible banner. That metadata is not an authorization requirement or grant; the normal delegated scope and app-role policies remain authoritative.

## Privacy & Governance Awareness
The project demonstrates awareness of:
- UK GDPR principles
- confidentiality
- data minimisation
- least privilege
- secure handling of potentially sensitive information
- auditability

These are portfolio engineering considerations rather than claims of formal compliance.
