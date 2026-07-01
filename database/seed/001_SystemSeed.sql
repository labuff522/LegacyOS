/******************************************************************************
LegacyOS Seed: 001_SystemSeed.sql

Purpose:
Loads baseline system-level configuration that every LegacyOS installation needs.
Run after migrations 001 and 002.
******************************************************************************/

INSERT INTO roles (name, description)
VALUES
    ('Owner', 'Full system access and business administration.'),
    ('Admin', 'Operational administration without ownership-level control.'),
    ('Coach', 'Coach portal access for rosters, attendance, and private lessons.'),
    ('Parent', 'Parent portal access for family, athlete, billing, and reservations.'),
    ('Athlete', 'Athlete portal access where enabled.'),
    ('Staff', 'Front desk or operations staff.'),
    ('Accountant', 'Financial reporting and billing visibility.')
ON CONFLICT DO NOTHING;

INSERT INTO permissions (name, description)
VALUES
    ('families.read', 'View family records.'),
    ('families.write', 'Create and edit family records.'),
    ('athletes.read', 'View athlete records.'),
    ('athletes.write', 'Create and edit athlete records.'),
    ('memberships.read', 'View memberships.'),
    ('memberships.write', 'Create and edit memberships.'),
    ('schedule.read', 'View training blocks and schedule.'),
    ('schedule.write', 'Create and edit training blocks.'),
    ('reservations.read', 'View reservations.'),
    ('reservations.write', 'Create and edit reservations.'),
    ('attendance.read', 'View attendance.'),
    ('attendance.write', 'Record attendance.'),
    ('billing.read', 'View invoices and payments.'),
    ('billing.write', 'Create and edit billing records.'),
    ('access.read', 'View access control records.'),
    ('access.write', 'Create and edit access control records.'),
    ('reports.read', 'View reports and dashboards.'),
    ('admin.full', 'Full administrative access.')
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM roles r CROSS JOIN permissions p
WHERE r.name = 'Owner'
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM roles r
JOIN permissions p ON p.name IN (
    'families.read','families.write',
    'athletes.read','athletes.write',
    'memberships.read','memberships.write',
    'schedule.read','schedule.write',
    'reservations.read','reservations.write',
    'attendance.read','attendance.write',
    'billing.read','billing.write',
    'access.read','access.write',
    'reports.read'
)
WHERE r.name = 'Admin'
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM roles r
JOIN permissions p ON p.name IN (
    'athletes.read','schedule.read','reservations.read','attendance.read','attendance.write'
)
WHERE r.name = 'Coach'
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM roles r
JOIN permissions p ON p.name IN (
    'families.read','athletes.read','memberships.read','schedule.read',
    'reservations.read','reservations.write','billing.read'
)
WHERE r.name = 'Parent'
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM roles r
JOIN permissions p ON p.name IN ('families.read','billing.read','reports.read')
WHERE r.name = 'Accountant'
ON CONFLICT DO NOTHING;

INSERT INTO business_rules (rule_code, title, description, active)
VALUES
    ('BR-001', 'Athlete belongs to Family', 'Every athlete belongs to exactly one family account.', true),
    ('BR-002', 'Family may contain multiple athletes', 'A family may have multiple athlete profiles.', true),
    ('BR-003', 'Membership Plans grant Services', 'Membership plans determine which services an athlete may access.', true),
    ('BR-004', 'Services are delivered through Training Blocks', 'Scheduled services are delivered through training blocks.', true),
    ('BR-005', 'Training Blocks have finite capacity', 'Each training block may define a maximum number of athletes.', true),
    ('BR-006', 'Reservations cannot exceed capacity', 'Reservations may not exceed training block capacity.', true),
    ('BR-007', 'Attendance is athlete-specific', 'Attendance is recorded by athlete, training block, and date.', true),
    ('BR-008', 'Families own billing', 'Invoices and payments are tied to family accounts.', true),
    ('BR-009', 'Private lessons cannot overlap', 'A coach cannot be double-booked for private lessons or assigned sessions.', true),
    ('BR-010', 'Open Mat requires eligibility', 'Open Mat access requires an eligible membership, valid waiver, and available capacity.', true),
    ('BR-011', 'Waiver required before participation', 'Athletes must have required waivers before attendance or reservations.', true),
    ('BR-012', 'Door access is time-bound', 'Door access should only be granted within the approved reservation or access window.', true)
ON CONFLICT (rule_code) DO UPDATE
SET title = EXCLUDED.title,
    description = EXCLUDED.description,
    active = EXCLUDED.active;
