SELECT
    'Tables' AS object_type,
    COUNT(*)
FROM information_schema.tables
WHERE table_schema='public'

UNION ALL

SELECT
    'Views',
    COUNT(*)
FROM information_schema.views
WHERE table_schema='public'

UNION ALL

SELECT
    'Indexes',
    COUNT(*)
FROM pg_indexes
WHERE schemaname='public';
