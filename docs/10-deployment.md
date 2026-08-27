# Deployment

## Production Architecture

CareTrack is a synthetic-data portfolio application deployed with:

- Angular on Azure Static Web Apps
- ASP.NET Core .NET 10 on Azure App Service
- Azure SQL Database serverless
- Microsoft Entra ID authentication

The API is hosted at `https://caretrack-api-g3ghhnddhefvg8c4.centralus-01.azurewebsites.net` and the frontend at `https://caretrack.hussnainali.me`.

## Health Probes

- `GET /api/health` is anonymous liveness. It confirms that the API process is responding and never queries SQL.
- `GET /api/health/ready` is anonymous readiness. It verifies database connectivity and returns a sanitized `200` or `503` JSON response.

Configure Azure App Service Health Check to use `/api/health/ready`. Azure SQL serverless can cold-start, so readiness can briefly report unavailable while the database resumes.

## Runtime Configuration

Production configuration remains in Azure App Service environment variables and connection-string settings. In particular:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`
- `AzureAd__TenantId`
- `AzureAd__ClientId`
- the `CareTrack` SQL connection string

The production SQL connection timeout is configured in the App Service connection string and is currently 60 seconds. Tenant IDs, client IDs, SQL credentials, connection strings, tokens, and publishing credentials must not be committed to source control.

Production CORS permits only `https://caretrack.hussnainali.me`. HTTPS Only and a modern supported minimum TLS version must remain enabled.

## Operational Verification

After deployment, verify both health endpoints, App Service configuration, production CORS, and that no secrets entered Git. Database failures are logged only as sanitized categories, numeric SQL error numbers where available, retry state, route template, status, and trace identifier.
