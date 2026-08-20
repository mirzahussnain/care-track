# Deployment

## Current Status
Deployment remains a future phase.

The backend currently runs locally with:
- .NET 10 / ASP.NET Core
- Microsoft SQL Server
- Microsoft Entra ID authentication
- local development configuration

The Angular frontend has not yet been implemented.

## Current Local Development Components
- `CareTrack.Api`
- `CareTrack.Application`
- `CareTrack.Domain`
- `CareTrack.Infrastructure`
- unit tests
- integration tests
- SQL Server integration database
- temporary Entra authentication test client

## Planned Frontend
Phase 6:
- Angular
- MSAL Angular
- Authorization Code Flow with PKCE
- protected API requests

## Planned Hosting / Deployment Learning Areas

### API
Potential options:
- Azure App Service
- Windows/IIS

Concerns:
- environment-specific configuration
- HTTPS
- logging
- health checks
- Entra configuration
- SQL connection security

### Database
Potential:
- Azure SQL

Concerns:
- migrations
- connection-string management
- least-privilege database access
- backups and recovery concepts

### Frontend
Potential:
- Azure Static Web Apps
- App Service
- other static hosting compatible with Angular

### CI/CD
Planned:
- GitHub Actions
- restore/build/test pipeline
- deployment workflow
- environment separation

## Security Requirements for Deployment
- no secrets committed to Git
- environment-specific Entra identifiers/configuration
- HTTPS
- protected production configuration
- minimal public API surface
- health endpoint suitable for probes
- production OpenAPI exposure reviewed separately
- secure SQL connectivity

## Not Yet Implemented
- production hosting
- IIS configuration
- Azure infrastructure
- CI/CD
- production secrets/configuration
- Angular deployment
- Playwright deployment-gate tests
