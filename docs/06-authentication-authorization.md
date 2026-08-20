# Authentication & Authorization

## Current Status
Microsoft Entra ID authentication and CareTrack authorization are implemented for the backend.

The Angular/MSAL client remains planned for Phase 6.

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
- referral management
- appointment creation/scheduling

### AdministrativeAccess
Requires:
- authenticated user
- `access_as_user` scope
- `Administrator` role

No production admin endpoint exists yet. The policy is established and tested for future administrative features.

`Administrator` is deliberately not a universal clinical bypass.

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

## Real Entra Verification
Manual verification has confirmed:
- real Entra bearer token acceptance
- `access_as_user` scope handling
- Clinician role handling
- 401 behavior without a token
- server-derived ClinicalNote `CreatedBy`
- inability of client-supplied `createdBy` to override authenticated identity

## Planned Angular Authentication
Phase 6 will use:
- Angular
- MSAL Angular
- Authorization Code Flow with PKCE
- bearer-token attachment for protected API calls

## Future Administrative Access
A future admin dashboard may manage CareTrack-specific configuration.

If staff-role assignment is later exposed through CareTrack, it should integrate with Microsoft Entra/Microsoft Graph rather than creating a parallel local credential system.
