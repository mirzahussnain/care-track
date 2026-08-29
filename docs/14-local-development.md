# Local Development

Local execution still uses Microsoft Entra; there is no development login bypass.

## Prerequisites

- .NET 10 SDK
- Node.js 24 and npm 11
- a developer-owned SQL Server instance
- `dotnet-ef` compatible with EF Core 10
- development Entra SPA/API registration values and app-role assignments
- trusted .NET HTTPS development certificate when using the HTTPS profile

The repository does not provide Docker Compose or automatic disposable SQL orchestration.

## Backend Configuration

Copy `backend/src/CareTrack.Api/appsettings.Development.example.json` to the ignored `appsettings.Development.json`, or prefer .NET user secrets/environment variables.

```powershell
dotnet user-secrets --project backend/src/CareTrack.Api/CareTrack.Api.csproj set 'ConnectionStrings:CareTrack' '<local SQL connection string>'
dotnet user-secrets --project backend/src/CareTrack.Api/CareTrack.Api.csproj set 'AzureAd:TenantId' '<development tenant ID>'
dotnet user-secrets --project backend/src/CareTrack.Api/CareTrack.Api.csproj set 'AzureAd:ClientId' '<development API application ID>'
```

Equivalent environment variable names:

```text
ConnectionStrings__CareTrack
AzureAd__TenantId
AzureAd__ClientId
Cors__AllowedOrigins__0
ReferralAssignment__Targets__0
ReferralAssignment__Targets__1
```

Set the development CORS origin to `http://localhost:4200`. Never commit connection strings, credentials, tenant secrets, tokens, or demo passwords.

## Restore, Build, and Migrate

```powershell
dotnet restore backend/CareTrack.slnx
dotnet build backend/CareTrack.slnx
dotnet ef database update --project backend/src/CareTrack.Infrastructure/CareTrack.Infrastructure.csproj --startup-project backend/src/CareTrack.Api/CareTrack.Api.csproj
```

The API does not migrate at startup. Create reviewed migrations with the same project arguments and `dotnet ef migrations add <MigrationName>`.

## Run the API

```powershell
dotnet run --project backend/src/CareTrack.Api/CareTrack.Api.csproj --launch-profile http
```

The HTTP profile listens on `http://localhost:5001`, matching Angular development. OpenAPI is Development-only. Verify:

```powershell
Invoke-RestMethod http://localhost:5001/api/health
Invoke-RestMethod http://localhost:5001/api/health/ready
```

## Frontend Configuration and Run

`frontend/caretrack-web/src/environments/environment.development.ts` holds the public development API URL, Entra tenant/client identifiers, delegated scope, and redirect URI. Update these for a different registration; do not add a client secret. The SPA registration must permit the local redirect URI, and the identity needs an appropriate API app role.

```powershell
Set-Location frontend/caretrack-web
npm ci
npm start
```

Open [http://localhost:4200](http://localhost:4200). MSAL requests `access_as_user` and attaches access tokens to configured API calls.

## Automated Tests

```powershell
dotnet test backend/tests/CareTrack.UnitTests/CareTrack.UnitTests.csproj --configuration Release
```

Integration tests use the real SQL Server provider and `backend/tests/CareTrack.IntegrationTests/appsettings.Integration.json`: local server, `CareTrackIntegrationTests` database, Windows authentication. The suite applies migrations and clears CareTrack domain records. Use only a disposable developer-owned database; never point it at the shared demo or valuable data.

```powershell
dotnet test backend/tests/CareTrack.IntegrationTests/CareTrack.IntegrationTests.csproj --configuration Release
```

There is no repository-managed Docker SQL strategy. A developer may use a local SQL container, but must configure it explicitly and keep the database disposable.

```powershell
Set-Location frontend/caretrack-web
npm test -- --watch=false
npm run build
```

## Demo Seeder

The seeder is destructive and should run only for an intended reset. It requires a database named exactly `CareTrackDb`, an environment-only connection string, no pending migrations, and exact interactive confirmation.

```powershell
$env:CARETRACK_DEMO_DB_CONNECTION_STRING = '<securely supplied connection string>'
dotnet run --project backend/tools/CareTrack.DemoSeeder/CareTrack.DemoSeeder.csproj --configuration Release -- --target-database CareTrackDb
Remove-Item Env:CARETRACK_DEMO_DB_CONNECTION_STRING
```

Read [database seed data](../database/seed/README.md) before use. Never incorporate the tool into API startup.

## Retained Authentication Test Client

`tools/CareTrack.AuthTestClient` is a standalone development aid for direct Entra token verification. It is not part of the Angular production bundle or backend solution. Its `.env` and token files are ignored. Moving it under `backend/tools` would not clarify its dependencies, so its current location is retained.
