# Workflow Model

CareTrack uses explicit domain methods and endpoint-specific commands rather than allowing clients to set a status directly. Invalid transitions are returned as `409 Conflict`.

## Referral Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Draft: create
    Draft --> Submitted: submit
    Submitted --> AwaitingTriage: start triage
    AwaitingTriage --> MoreInformationRequired: request more information
    MoreInformationRequired --> Submitted: resubmit
    AwaitingTriage --> Accepted: accept
    AwaitingTriage --> Rejected: reject
    Accepted --> Assigned: assign
    Assigned --> Assigned: reassign
    Assigned --> Scheduled: create first eligible appointment
    Scheduled --> InProgress: start linked appointment
    InProgress --> Completed: explicit referral completion
    Rejected --> [*]
    Completed --> [*]
```

Triage assessment records a priority and note while the referral remains `AwaitingTriage`. Additional appointments can be created while a referral is `Scheduled` or `InProgress` without repeating the referral transition.

`Cancelled` exists in `ReferralStatus`, but the current `Referral` entity has no cancellation method and the API has no referral-cancellation endpoint. The enum value must not be read as an implemented transition.

## Appointment Lifecycle

```mermaid
stateDiagram-v2
    [*] --> Scheduled: create from eligible referral
    Scheduled --> CheckedIn: check in
    CheckedIn --> InProgress: start
    InProgress --> Completed: complete
    Scheduled --> Cancelled: cancel
    CheckedIn --> Cancelled: cancel
    Scheduled --> DidNotAttend: mark Did Not Attend
    Completed --> [*]
    Cancelled --> [*]
    DidNotAttend --> [*]
```

An appointment cannot be started before check-in, completed before it is InProgress, cancelled after it starts, or marked Did Not Attend after leaving Scheduled.

## Referral / Appointment Orchestration

```mermaid
sequenceDiagram
    participant User
    participant App as Application service
    participant Referral
    participant Appointment
    participant DB as SQL transaction

    User->>App: Create appointment for Assigned referral
    App->>Referral: Validate CanScheduleAppointment
    App->>Appointment: Create Scheduled appointment
    App->>Referral: Schedule
    App->>DB: Persist both atomically

    User->>App: Start CheckedIn appointment
    App->>Appointment: Start
    App->>Referral: StartProgress if Scheduled
    App->>DB: Persist both atomically

    User->>App: Complete InProgress appointment
    App->>Appointment: Complete
    Note over App,Referral: Referral is not auto-completed

    User->>App: Explicitly complete referral
    App->>App: Require at least one Completed appointment
    App->>App: Reject if any appointment remains active
    App->>Referral: Complete from InProgress
```

Referral completion requires:

1. the referral is `InProgress`;
2. at least one related appointment exists and is `Completed`;
3. no related appointment remains `Scheduled`, `CheckedIn`, or `InProgress`.

This keeps referral closure deliberate and visible even after clinical appointment work is completed.
