# Database Scripts

No hand-written production database script is currently required.

- EF Core migrations under `backend/src/CareTrack.Infrastructure/Migrations` are the authoritative schema history.
- `backend/tools/CareTrack.DemoSeeder` owns the guarded deterministic demo reset.
- The API does not apply migrations or seed data at startup.

If a future operational T-SQL script is added here, it should be idempotent where practical, document its target and rollback behavior, avoid embedded credentials, and be reviewed separately from application startup.
