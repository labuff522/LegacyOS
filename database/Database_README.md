# LegacyOS Database

This folder contains the LegacyOS PostgreSQL database scripts.

## Structure

```text
database/
  migrations/
    001_InitialSchema.sql
    002_CorePlatform.sql

  seed/
    001_SystemSeed.sql
    002_AcademySeed.sql

  diagrams/
    ERD.drawio

  HealthCheck.sql
  README.md
```

## Run Order

1. `database/migrations/001_InitialSchema.sql`
2. `database/migrations/002_CorePlatform.sql`
3. `database/seed/001_SystemSeed.sql`
4. `database/seed/002_AcademySeed.sql`
5. `database/HealthCheck.sql`

## Rules

- Apply migrations in numeric order.
- Do not edit committed migrations.
- Add a new migration for every schema change.
- Seed data is configuration, not customer data.
