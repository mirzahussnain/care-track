# Clinical Risk (Portfolio Awareness)

> **Portfolio disclaimer:** CareTrack is not a certified clinical system and does not claim compliance with DCB0129, DCB0160, or any other clinical-safety standard. It uses synthetic data only.

## Current Hazard Awareness

| Hazard | Potential consequence | Current / Planned Controls |
|---|---|---|
| Wrong patient selected | Information associated with the wrong patient | unique patient records; explicit patient identifiers; persistent patient context on detail/workflow screens |
| Unauthorized access | Sensitive information exposed | Microsoft Entra authentication; named authorization policies; role + scope checks; least privilege |
| Spoofed clinical-note author | Incorrect attribution of clinical documentation | `CreatedBy` derived server-side from authenticated Entra object ID |
| Referral accidentally completed | Outstanding work becomes less visible | explicit state machine; invalid-transition rejection; completion prerequisites |
| Duplicate patient/referral/appointment identifiers | Duplicate or ambiguous work | duplicate reference validation and persistence constraints |
| Concurrent editing | One update overwrites another | optimistic concurrency for patient update paths |
| Concurrent appointment scheduling | Double booking | serializable scheduling transaction and deterministic concurrency integration test |
| Appointment/referral state mismatch | Inconsistent workflow | application-layer cross-aggregate orchestration and transactions |
| Clinical note orphaning | Documentation detached from encounter | foreign-key relationship and restricted appointment deletion |
| Sensitive clinical content in logs | Confidentiality exposure | logging avoids clinical-note content; unexpected errors return generic details |
| Important referral state not noticed | Workflow delay | structured states; dashboard attention queues; filterable referral/appointment views |

## Implemented Safety-Oriented Engineering Practices
- explicit workflow transitions rather than free-form status assignment
- application exceptions for invalid state transitions
- database referential integrity
- concurrency handling
- transactional cross-aggregate operations
- centralized exception handling
- explicit authorization policies
- synthetic data only

## Implemented UI / Operational Controls

- persistent patient identity context on detailed workflows
- explicit, state-aware workflow actions rather than free-form status editing
- dashboard/queue and status visualization
- conflict and stale-update error presentation
- role-aware navigation backed by API enforcement
- prominent synthetic-data warnings for public and authenticated demo use

Future hardening for real-world use would require formal hazard analysis, accessibility and security assessment, operational monitoring, incident processes, and clinical governance—not only additional UI controls.

## Scope Boundary
These controls demonstrate engineering awareness only. They are not evidence of formal hazard analysis, clinical safety case approval, certification, or production fitness.
