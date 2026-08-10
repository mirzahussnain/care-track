# Planned API Design

This document lists **planned** REST endpoints only. No implementation exists at this stage.

## Planned Endpoints
### Patients
- Planned: `GET /api/patients`
- Planned: `GET /api/patients/{id}`
- Planned: `POST /api/patients`

### Referrals
- Planned: `GET /api/referrals`
- Planned: `GET /api/referrals/{id}`
- Planned: `POST /api/referrals`
- Planned: `POST /api/referrals/{id}/submit`
- Planned: `POST /api/referrals/{id}/triage`
- Planned: `POST /api/referrals/{id}/assign`
- Planned: `POST /api/referrals/{id}/complete`
- Planned: `GET /api/referrals/{id}/history`

### Dashboard and Audit
- Planned: `GET /api/dashboard`
- Planned: `GET /api/audit`

## Planned REST Design Considerations
- HTTP status codes
- validation
- Problem Details
- pagination
- filtering
- authorization
- versioning considerations
