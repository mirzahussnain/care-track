# Database Seed Data

CareTrack uses one shared production/demo Azure SQL database, `CareTrackDb`. The database is for synthetic portfolio data only and is reset manually with the deterministic demo seeder.

The reset tool removes all CareTrack domain records from the configured database and restores the curated recruiter-demo baseline. It preserves EF migration history and does not create, migrate, or reconfigure infrastructure.

## Prerequisites

- .NET 10 SDK
- the existing CareTrack EF migrations already applied to the target database
- secure access to the target SQL connection string
- an explicit decision to discard all current shared demo changes

Never place the connection string in source control, command-line arguments, documentation, logs, or application settings. Supply it only through the process environment for the reset operation.

## Baseline

A successful reset creates:

- 12 Patients
- 17 Referrals
- 94 Referral History entries
- 10 Appointments
- 7 Clinical Notes

All records are synthetic, use visibly demo-prefixed references, and contain no NHS numbers, real patient information, or unsupported contact fields.

## Safe Reset Command

From the repository root in PowerShell:

```powershell
$env:CARETRACK_DEMO_DB_CONNECTION_STRING = '<securely supplied connection string>'
dotnet run --project backend/tools/CareTrack.DemoSeeder/CareTrack.DemoSeeder.csproj --configuration Release -- --target-database CareTrackDb
```

The tool resolves the database name from the open connection and refuses any target other than `CareTrackDb`. Immediately before mutation it displays only:

```text
Target database: CareTrackDb
Operation: destructive reset of CareTrack domain demo records
Migration history: preserved
```

Type this exact confirmation when prompted:

```text
RESET CareTrackDb
```

Any other response cancels the operation without changing domain records.

Clear the environment variable immediately after the process finishes:

```powershell
Remove-Item Env:CARETRACK_DEMO_DB_CONNECTION_STRING
```

## Verification

The tool reports entity counts only. Confirm that the output matches the baseline counts above and that no connection details or record values were printed.

For isolated local verification, run:

```powershell
dotnet test backend/tests/CareTrack.UnitTests/CareTrack.UnitTests.csproj --configuration Release
dotnet test backend/tests/CareTrack.IntegrationTests/CareTrack.IntegrationTests.csproj --configuration Release
```

The integration suite uses `CareTrackIntegrationTests`, not production Azure SQL. It verifies repeat resets, referential integrity, rollback, overlap rules, migration-history preservation, and that non-domain tables are untouched.

## Operating Model

CareTrack intentionally has one production/demo database containing synthetic data only. Recruiter interactions occur in the shared environment, so manual reset discards those interactions and restores the same logical scenarios with dates positioned relative to the reset day.

A scheduled reset may be added later if usage justifies it. No recurring reset infrastructure is currently configured.
