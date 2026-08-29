# Recruiter Demo Guide

CareTrack can be evaluated in a few minutes without publishing demo passwords in the repository.

## Start the Demo

1. Visit [https://caretrack.hussnainali.me](https://caretrack.hussnainali.me).
2. Open **Interactive Demo**.
3. Choose **Referral Coordinator** or **Clinician**.
4. Use the role-specific credentials shown in the centered dialog. The primary action copies the password and continues to Microsoft sign-in with an email login hint.
5. Complete the real Microsoft Entra sign-in flow.
6. Confirm the authenticated shell shows the **DEMO ACCOUNT · SYNTHETIC DATA ONLY** banner.

The environment is shared. Previous visitors may have changed records, and a manual reset may occur without notice. Never enter real patient, clinical, or personal information.

## Role Differences

| Role | Can explore | Intentionally unavailable |
| --- | --- | --- |
| Referral Coordinator | patient registration; referral search, creation, triage, assignment/reassignment, progression, and eligible appointment scheduling | full patient list/detail, appointment search/detail and clinical actions, Clinical Notes |
| Clinician | patient reads; permitted referral management; appointment scheduling/search/detail and workflow; Clinical Notes | administrative features; any permission not granted by the API policies |

The Clinician role also satisfies `ReferralManagement`. Administrator is not offered as a recruiter demo identity.

## What to Try: Referral Coordinator

1. From the dashboard, register a clearly synthetic patient.
2. Create a referral for that patient and submit it.
3. Start triage and record a triage assessment.
4. Accept and assign the referral to a configured team.
5. Reassign it while it remains Assigned, if useful.
6. Schedule an appointment from the eligible referral.

After scheduling, the Referral Coordinator does not gain permission to browse appointment detail; that boundary is intentional.

## What to Try: Clinician

1. Inspect the patient, referral, and appointment workspaces.
2. Open a Scheduled appointment and check it in.
3. Start the appointment; a linked Scheduled referral can move to InProgress atomically.
4. Add or edit a Clinical Note. Its creator identity is derived by the API.
5. Complete the appointment.
6. Return to the referral and explicitly complete it once its appointment prerequisites are satisfied.

## Reset Model

CareTrack uses one shared synthetic `CareTrackDb`. An operator runs the guarded `backend/tools/CareTrack.DemoSeeder` manually when the baseline needs restoration. The tool creates 12 patients, 17 referrals, 94 history entries, 10 appointments, and 7 Clinical Notes, and preserves EF migration history. See [database seed data](../database/seed/README.md).

## Evaluation Shortcuts

- Product and screenshots: [root README](../README.md)
- Architecture and auth flow: [system architecture](03-system-architecture.md)
- Exact permissions: [API design](05-api-design.md)
- Workflow state diagrams: [workflow model](11-workflows.md)
- Interview talking points: [portfolio summary](portfolio-summary.md)
