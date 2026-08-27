# API Design

## Current Status

The ASP.NET Core REST API is implemented for the current backend scope.

Business endpoints use explicit named authorization policies. Errors are returned through centralized Problem Details handling.

## Patients

| Method | Route                                 | Policy               |
| ------ | ------------------------------------- | -------------------- |
| GET    | `/api/patients`                       | `ClinicianAccess`    |
| GET    | `/api/patients/{id}`                  | `ClinicianAccess`    |
| GET    | `/api/patients/referral-lookup`       | `ReferralManagement` |
| GET    | `/api/patients/{id}/referral-summary` | `ReferralManagement` |
| POST   | `/api/patients`                       | `ReferralManagement` |
| PUT    | `/api/patients/{id}`                  | `ReferralManagement` |

## Referrals

| Method | Route                                          | Policy               |
| ------ | ---------------------------------------------- | -------------------- |
| POST   | `/api/referrals`                               | `ReferralManagement` |
| POST   | `/api/referrals/{id}/submit`                   | `ReferralManagement` |
| POST   | `/api/referrals/{id}/start-triage`             | `ReferralManagement` |
| POST   | `/api/referrals/{id}/accept`                   | `ReferralManagement` |
| POST   | `/api/referrals/{id}/request-more-information` | `ReferralManagement` |
| POST   | `/api/referrals/{id}/reject`                   | `ReferralManagement` |
| POST   | `/api/referrals/{id}/resubmit`                 | `ReferralManagement` |
| POST   | `/api/referrals/{id}/triage-assessment`        | `ReferralManagement` |
| POST   | `/api/referrals/{id}/assign`                   | `ReferralManagement` |
| POST   | `/api/referrals/{id}/reassign`                 | `ReferralManagement` |
| POST   | `/api/referrals/{id}/complete`                 | `ReferralManagement` |
| GET    | `/api/referrals/{id}/history`                  | `ReferralManagement` |
| GET    | `/api/referrals/assignment-targets`            | `ReferralManagement` |
| GET    | `/api/referrals`                               | `ReferralManagement` |
| GET    | `/api/referrals/{id}`                          | `ReferralManagement` |

### Referral frontend prerequisite contracts

`GET /api/patients/referral-lookup` accepts `search`, `page` (default `1`), and `pageSize` (default `20`). It uses fixed `lastName asc` ordering and returns the standard paging metadata with patient items limited to:

```json
{
  "id": "guid",
  "patientReference": "PAT-001",
  "fullName": "Amina Khan",
  "dateOfBirth": "1988-04-12"
}
```

`GET /api/patients/{id}/referral-summary` returns the same four-field item. The existing full patient GET/search routes remain protected by `ClinicianAccess`.

`GET /api/referrals/assignment-targets` returns `{ "items": ["Cardiology Team A"] }`. Values come from the ordered `ReferralAssignment:Targets` configuration array. Startup trims values and rejects an empty list, blank values, values over 200 characters, and case-insensitive duplicates. Assign/reassign trims the submitted name, matches it case-insensitively, persists the exact canonical configured value, and returns `400 Bad Request` for an unavailable value. These values are clinical-team names, not user identities or Entra object IDs; there is no persistent team directory.

Referral status, priority, and history-event enums continue to serialize as numbers. Workflow mutation remains endpoint-specific; there is no generic status mutation or cancellation endpoint.

## Appointments

| Method | Route                                   | Policy               |
| ------ | --------------------------------------- | -------------------- |
| POST   | `/api/appointments`                     | `ReferralManagement` |
| POST   | `/api/appointments/{id}/check-in`       | `ClinicianAccess`    |
| POST   | `/api/appointments/{id}/start`          | `ClinicianAccess`    |
| POST   | `/api/appointments/{id}/complete`       | `ClinicianAccess`    |
| POST   | `/api/appointments/{id}/cancel`         | `ClinicianAccess`    |
| POST   | `/api/appointments/{id}/did-not-attend` | `ClinicianAccess`    |
| GET    | `/api/appointments/{id}`                | `ClinicianAccess`    |
| GET    | `/api/appointments`                     | `ClinicianAccess`    |

## Clinical Notes

| Method | Route                                              | Policy            |
| ------ | -------------------------------------------------- | ----------------- |
| POST   | `/api/appointments/{appointmentId}/clinical-notes` | `ClinicianAccess` |
| GET    | `/api/clinical-notes/{id}`                         | `ClinicianAccess` |
| GET    | `/api/appointments/{appointmentId}/clinical-notes` | `ClinicianAccess` |
| PUT    | `/api/clinical-notes/{id}`                         | `ClinicianAccess` |

## Infrastructure Endpoint

| Method | Route               | Authorization                                       |
| ------ | ------------------- | --------------------------------------------------- |
| GET    | `/api/health`       | anonymous liveness; does not query SQL              |
| GET    | `/api/health/ready` | anonymous readiness; verifies database connectivity |

OpenAPI is mapped only in the Development environment.

## Error Handling

Centralized exception handling maps known application/domain failures to consistent HTTP responses, including:

- 400 Bad Request
- 404 Not Found
- 409 Conflict
- 500 Internal Server Error

Unexpected failures return generic server-error details rather than exposing internals.

## API Design Principles

- explicit request and response contracts
- appropriate HTTP status codes
- centralized Problem Details
- pagination and filtering for search endpoints
- deterministic sorting
- explicit named authorization policies
- no client-controlled security-sensitive ownership fields
- cancellation-token propagation
