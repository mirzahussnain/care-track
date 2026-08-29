# API Design

The ASP.NET Core REST API groups endpoints by patients, referrals, appointments, Clinical Notes, current-user metadata, and health. Business endpoints declare named authorization policies; application/domain failures use centralized Problem Details handling.

## Authorization Policies

Every named business policy requires an authenticated user and delegated `access_as_user` scope.

| Policy | Additional role requirement |
| --- | --- |
| `ApiAccess` | none |
| `ClinicianAccess` | `Clinician` |
| `ReferralManagement` | `ReferralCoordinator` or `Clinician` |
| `AdministrativeAccess` | `Administrator` |

`Administrator` is not a universal bypass. No current endpoint uses `AdministrativeAccess` because no administrative feature has been implemented.

## Compact Role / Endpoint Matrix

| Area | Clinician | Referral Coordinator | Administrator only |
| --- | --- | --- | --- |
| `GET /api/me` | Yes | Yes | Yes |
| Full patient search/detail | Yes | No | No |
| Referral-safe patient lookup/summary | Yes | Yes | No |
| Patient create/update | Yes | Yes | No |
| Referral read/workflow/history | Yes | Yes | No |
| Appointment create | Yes | Yes | No |
| Appointment search/detail/workflow | Yes | No | No |
| Clinical Note create/read/update | Yes | No | No |
| Health endpoints | Anonymous | Anonymous | Anonymous |

This matrix reflects API policies, not just visible Angular navigation.

## Patients

| Method | Route | Policy |
| --- | --- | --- |
| GET | `/api/patients` | `ClinicianAccess` |
| GET | `/api/patients/{id}` | `ClinicianAccess` |
| GET | `/api/patients/referral-lookup` | `ReferralManagement` |
| GET | `/api/patients/{id}/referral-summary` | `ReferralManagement` |
| POST | `/api/patients` | `ReferralManagement` |
| PUT | `/api/patients/{id}` | `ReferralManagement` |

The referral-safe lookup contracts expose only ID, patient reference, full name, and date of birth. They allow a Referral Coordinator to choose a patient for referral work without broadening the full patient-list policy.

Patient updates require the response `rowVersion` to be returned as Base64. Invalid Base64 produces `400`; a stale row version becomes a `409 Concurrency Conflict` rather than silently overwriting a newer update.

## Referrals

All referral routes use `ReferralManagement`.

| Method | Route | Purpose |
| --- | --- | --- |
| GET | `/api/referrals` | paged/filterable search |
| GET | `/api/referrals/{id}` | detail |
| GET | `/api/referrals/{id}/history` | ordered workflow history |
| GET | `/api/referrals/assignment-targets` | configured canonical team names |
| POST | `/api/referrals` | create Draft referral |
| POST | `/api/referrals/{id}/submit` | Draft → Submitted |
| POST | `/api/referrals/{id}/start-triage` | Submitted → AwaitingTriage |
| POST | `/api/referrals/{id}/triage-assessment` | record priority/note without changing status |
| POST | `/api/referrals/{id}/request-more-information` | AwaitingTriage → MoreInformationRequired |
| POST | `/api/referrals/{id}/resubmit` | MoreInformationRequired → Submitted |
| POST | `/api/referrals/{id}/accept` | AwaitingTriage → Accepted |
| POST | `/api/referrals/{id}/reject` | AwaitingTriage → Rejected |
| POST | `/api/referrals/{id}/assign` | Accepted → Assigned |
| POST | `/api/referrals/{id}/reassign` | replace target while Assigned |
| POST | `/api/referrals/{id}/complete` | explicitly complete an eligible InProgress referral |

Referral status, priority, and history-event enums serialize as numbers. There is no generic status-update route and no referral-cancellation route. `Cancelled` exists in the enum but has no current domain transition.

Assignment targets come from `ReferralAssignment:Targets`; they are canonical clinical-team names, not Entra identities or a persistent team directory.

## Appointments

| Method | Route | Policy |
| --- | --- | --- |
| POST | `/api/appointments` | `ReferralManagement` |
| GET | `/api/appointments` | `ClinicianAccess` |
| GET | `/api/appointments/{id}` | `ClinicianAccess` |
| POST | `/api/appointments/{id}/check-in` | `ClinicianAccess` |
| POST | `/api/appointments/{id}/start` | `ClinicianAccess` |
| POST | `/api/appointments/{id}/complete` | `ClinicianAccess` |
| POST | `/api/appointments/{id}/cancel` | `ClinicianAccess` |
| POST | `/api/appointments/{id}/did-not-attend` | `ClinicianAccess` |

Creation validates referral/patient ownership and scheduling eligibility, rejects duplicate references and overlapping active time slots, and can advance an Assigned referral to Scheduled atomically. Starting an appointment can advance a Scheduled referral to InProgress. Appointment completion does not complete the referral.

## Clinical Notes

All Clinical Note routes require `ClinicianAccess`.

| Method | Route |
| --- | --- |
| POST | `/api/appointments/{appointmentId}/clinical-notes` |
| GET | `/api/clinical-notes/{id}` |
| GET | `/api/appointments/{appointmentId}/clinical-notes` |
| PUT | `/api/clinical-notes/{id}` |

Creation accepts appointment ID and content; `CreatedBy` is always derived from the authenticated Entra object ID through `ICurrentUser`. Client-supplied ownership is not part of the contract. Updating content preserves the original creator value. There is no delete endpoint.

## Current User and Demo Metadata

`GET /api/me` uses `ApiAccess` and returns the stable object ID, display name, username, role claims, and `isDemoAccount`. The demo flag is calculated by matching the authenticated object ID against the configured application directory. It is presentation metadata for the banner and does not satisfy a scope, role, policy, or ownership check.

## Health Endpoints

| Method | Route | Authorization | Meaning |
| --- | --- | --- | --- |
| GET | `/api/health` | anonymous | SQL-independent process liveness |
| GET | `/api/health/ready` | anonymous | database connectivity readiness; sanitized `200`/`503` |

OpenAPI is mapped only in Development.

## Problem Details and Status Codes

Known application exceptions are converted by the global exception handler to RFC-style Problem Details:

| Status | Meaning in CareTrack |
| --- | --- |
| `400 Bad Request` | invalid argument/contract value, such as unavailable assignment target or malformed row version |
| `404 Not Found` | requested patient, referral, appointment, or note does not exist |
| `409 Conflict` | duplicate/conflicting operation, stale concurrency token, scheduling collision, or invalid workflow transition |
| `500 Internal Server Error` | unexpected failure with generic client detail and sanitized server logging |

Authentication/authorization failures are generated by the ASP.NET Core authentication pipeline:

- `401 Unauthorized`: no usable authentication was supplied, including an absent/invalid token.
- `403 Forbidden`: the token authenticated the user, but the delegated scope or required role is missing.

Invalid state transitions deliberately return `409`, for example starting a Scheduled appointment without check-in or accepting a referral that is not AwaitingTriage.

## Design Principles

- resource-oriented grouping plus endpoint-specific workflow commands
- API authorization as the source of truth
- explicit request/response contracts and server-owned security fields
- pagination, filtering, and deterministic sorting for list endpoints
- cancellation-token propagation
- explicit transactions for cross-aggregate consistency
- generic public failure detail and sanitized operational logging
