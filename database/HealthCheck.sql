/******************************************************************************
LegacyOS HealthCheck.sql
Run after migrations and seed scripts.
******************************************************************************/

SELECT version();

SELECT 'Tables' AS object_type, COUNT(*)
FROM information_schema.tables
WHERE table_schema = 'public'
UNION ALL
SELECT 'Views', COUNT(*)
FROM information_schema.views
WHERE table_schema = 'public'
UNION ALL
SELECT 'Indexes', COUNT(*)
FROM pg_indexes
WHERE schemaname = 'public';

SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY table_name;

SELECT relname AS table_name, n_live_tup AS estimated_rows
FROM pg_stat_user_tables
ORDER BY relname;

SELECT rule_code, title, active
FROM business_rules
ORDER BY rule_code;

SELECT name, monthly_price
FROM membership_plans
ORDER BY name;

SELECT name, category, standalone_price, capacity_driver
FROM services
ORDER BY category, name;

SELECT name, day_of_week, start_time, end_time, capacity
FROM training_blocks
ORDER BY name;

SELECT pg_size_pretty(pg_database_size(current_database())) AS database_size,
       now() AS healthcheck_timestamp;
