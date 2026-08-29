# Authentication & Authorization

## Current Status

Microsoft Entra ID authentication is implemented end to end. Angular uses MSAL redirects and an HTTP interceptor for protected API calls; ASP.NET Core validates delegated access tokens and enforces named scope/role policies.

## Identity Provider
CareTrack uses Microsoft Entra ID as the identity provider.

The API:
- is configured as a single-tenant protected API
- validates bearer access tokens using `Microsoft.Identity.Web`
- exposes delegated scope `access_as_user`
- does not require a client secret to validate incoming bearer tokens

CareTrack does not implement its own:
- password storage
- login endpoint
- MFA
- password reset
- refresh-token store

## Application Roles

- `Clinician`
- `ReferralCoordinator`
- `Administrator`

## Named Policies

### ClinicianAccess
Requires:
- authenticated user
- `access_as_user` scope
- `Clinician` role

Used for:
- patient reads/search
- appointment reads/search
- appointment clinical workflow
- clinical note create/read/list/update

### ReferralManagement
Requires:
- authenticated user
- `access_as_user` scope
- `ReferralCoordinator` **or** `Clinician` role

Used for:
- patient create/update
- reduced patient lookup and identity summaries used by referral workflows
- referral management
- appointment creation/scheduling

### AdministrativeAccess
Requires:
- authenticated user
- `access_as_user` scope
- `Administrator` role

No production admin endpoint exists yet. The policy is established and tested for future administrative features.

`Administrator` is deliberately not a universal clinical bypass.

### ApiAccess

Requires an authenticated user and `access_as_user`, with no additional role. It is used only by `GET /api/me` so the SPA can load identity, roles, and presentation metadata after sign-in.

## Current User Abstraction
The Application layer exposes:

```csharp
public interface ICurrentUser
{
    string UserId { get; }
}
```

The API implements this through `HttpCurrentUser`.

`HttpCurrentUser`:
- requires an authenticated principal
- resolves the Microsoft Entra object-ID claim
- rejects missing/blank object IDs
- does not fall back to email, display name, or client-supplied identity

## Clinical Note Ownership
Clinical-note creation accepts note content from the client, but `CreatedBy` is derived server-side from `ICurrentUser.UserId`.

This prevents a caller from spoofing ownership by submitting another user identifier.

## Authentication vs Authorization

```text
Authentication
→ proves who the caller is

Scope
→ proves the client may call the API on behalf of the user

Role
→ expresses the user's CareTrack business role

Policy
→ combines authentication + scope + role requirements
```

## Status-Code Semantics
- `401 Unauthorized` — authentication is absent or unsuccessful
- `403 Forbidden` — caller is authenticated but does not satisfy the required policy

## Angular / MSAL Flow

The SPA uses Authorization Code Flow with PKCE. It requests the delegated API scope, stores MSAL state in session storage, and attaches the access token only to the configured API base URL. The SPA is a public client and has no client secret.

Role-aware navigation and controls reflect roles returned by `GET /api/me`, but never replace endpoint authorization.

## Recruiter Demo Identities

The landing page offers separate Clinician and Referral Coordinator demo identities. Both are real restricted Entra users and follow the same MSAL/token/policy path as other identities. Credentials are shown only in the landing-page interaction and are not repeated in repository documentation.

`GET /api/me` returns `isDemoAccount` when the authenticated object ID belongs to the demo directory. This controls the synthetic-data banner only. It does not grant a role, satisfy a policy, alter resource access, or bypass the delegated scope.

## Automated Test Authentication
Integration tests replace external Entra authentication with a deterministic test authentication scheme.

This allows tests to model:
- anonymous callers
- authenticated callers
- missing scope
- missing role
- Clinician
- ReferralCoordinator
- Administrator

The real production authorization policies remain in use during integration tests.

`factory.CreateClient()` intentionally remains anonymous. Authenticated tests explicitly use the dedicated authenticated-client helper.

The referral-safe patient lookup/detail endpoints use `ReferralManagement`; this does not broaden `ClinicianAccess` on the existing full patient GET/search endpoints. `Administrator` has no referral or clinical bypass.

## Real Entra Verification
Manual verification has confirmed:
- real Entra bearer token acceptance
- `access_as_user` scope handling
- Clinician role handling
- 401 behavior without a token
- server-derived ClinicalNote `CreatedBy`
- inability of client-supplied `createdBy` to override authenticated identity

## Future Administrative Access
A future admin dashboard may manage CareTrack-specific configuration.

If staff-role assignment is later exposed through CareTrack, it should integrate with Microsoft Entra/Microsoft Graph rather than creating a parallel local credential system.
