# LegacyOS Database

This folder contains the LegacyOS database.

## Migrations

Database changes are additive.

Existing migration files are never modified after being committed.

Migration order:

001_InitialSchema.sql

002_CorePlatform.sql

...

## Health Check

Run HealthCheck.sql after applying migrations to verify database integrity.

## Seed Data

Seed scripts are stored under /seed.
