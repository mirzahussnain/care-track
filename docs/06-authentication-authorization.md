# Planned Authentication & Authorization

This document describes a **proposed** authentication and authorization model only.

## Planned Approach
- OAuth 2.0 for delegated authorization concepts.
- OpenID Connect for user authentication flows.
- Role-based authorization in the API and application UI.

## Planned Roles
- Coordinator
- Clinician
- ServiceManager

## Proposed OpenID Connect Authentication Flow
```mermaid
sequenceDiagram
    title Proposed OpenID Connect Authentication Flow
    participant U as User
    participant SPA as Angular SPA
    participant IDP as Identity Provider
    participant API as ASP.NET Core API

    U->>SPA: Open application
    SPA->>SPA: Authentication required
    SPA->>IDP: Redirect for sign-in
    U->>IDP: Authenticate
    IDP-->>SPA: Return authentication result/tokens
    SPA->>API: Call API with access token
    API->>IDP: Validate token
    API->>API: Apply role/policy authorization
    API-->>SPA: Return authorised response
```

No credentials, tenant identifiers, or implementation configuration are included in this planning stage.
