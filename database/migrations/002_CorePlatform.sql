-- LegacyOS migration v0.2
-- Non-destructive update for academy_platform_schema_v0_1.sql
-- Adds business-rule registry, roles/permissions, users, audit log, integration tracking, and access policies.

BEGIN;

-- =========================
-- BUSINESS RULE REGISTRY
-- =========================
CREATE TABLE IF NOT EXISTS business_rules (
    rule_id TEXT PRIMARY KEY,
    name TEXT NOT NULL,
    description TEXT NOT NULL,
    category TEXT NOT NULL DEFAULT 'general',
    severity TEXT NOT NULL DEFAULT 'required',
    active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- =========================
-- APPLICATION ROLES / PERMISSIONS
-- =========================
CREATE TABLE IF NOT EXISTS app_roles (
    role_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    role_code TEXT UNIQUE NOT NULL,
    role_name TEXT NOT NULL,
    description TEXT,
    active BOOLEAN NOT NULL DEFAULT true
);

CREATE TABLE IF NOT EXISTS app_permissions (
    permission_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    permission_code TEXT UNIQUE NOT NULL,
    permission_name TEXT NOT NULL,
    description TEXT,
    active BOOLEAN NOT NULL DEFAULT true
);

CREATE TABLE IF NOT EXISTS role_permissions (
    role_id UUID NOT NULL REFERENCES app_roles(role_id) ON DELETE CASCADE,
    permission_id UUID NOT NULL REFERENCES app_permissions(permission_id) ON DELETE CASCADE,
    PRIMARY KEY (role_id, permission_id)
);

CREATE TABLE IF NOT EXISTS app_users (
    user_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    family_id UUID REFERENCES families(family_id) ON DELETE SET NULL,
    guardian_id UUID REFERENCES guardians(guardian_id) ON DELETE SET NULL,
    coach_id UUID REFERENCES coaches(coach_id) ON DELETE SET NULL,
    email TEXT UNIQUE NOT NULL,
    display_name TEXT NOT NULL,
    external_auth_provider TEXT,
    external_auth_id TEXT,
    active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    CHECK (family_id IS NOT NULL OR guardian_id IS NOT NULL OR coach_id IS NOT NULL OR external_auth_id IS NOT NULL)
);

CREATE TABLE IF NOT EXISTS user_roles (
    user_id UUID NOT NULL REFERENCES app_users(user_id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES app_roles(role_id) ON DELETE CASCADE,
    PRIMARY KEY (user_id, role_id)
);

-- =========================
-- AUDIT / INTEGRATION SUPPORT
-- =========================
CREATE TABLE IF NOT EXISTS audit_log (
    audit_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    actor_user_id UUID REFERENCES app_users(user_id) ON DELETE SET NULL,
    entity_type TEXT NOT NULL,
    entity_id UUID,
    action TEXT NOT NULL,
    occurred_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    details JSONB NOT NULL DEFAULT '{}'::jsonb
);

CREATE TABLE IF NOT EXISTS integration_accounts (
    integration_account_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    provider TEXT NOT NULL,
    purpose TEXT NOT NULL,
    external_account_id TEXT,
    status TEXT NOT NULL DEFAULT 'active',
    config JSONB NOT NULL DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    UNIQUE (provider, purpose, external_account_id)
);

CREATE TABLE IF NOT EXISTS integration_events (
    integration_event_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    integration_account_id UUID REFERENCES integration_accounts(integration_account_id) ON DELETE SET NULL,
    provider TEXT NOT NULL,
    event_type TEXT NOT NULL,
    external_event_id TEXT,
    received_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    processed_at TIMESTAMPTZ,
    payload JSONB NOT NULL DEFAULT '{}'::jsonb,
    status TEXT NOT NULL DEFAULT 'received'
);

-- =========================
-- ACCESS POLICY SUPPORT
-- =========================
CREATE TABLE IF NOT EXISTS access_policies (
    access_policy_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    service_id UUID REFERENCES services(service_id) ON DELETE SET NULL,
    block_id UUID REFERENCES training_blocks(block_id) ON DELETE SET NULL,
    minutes_before_start INTEGER NOT NULL DEFAULT 15,
    minutes_after_end INTEGER NOT NULL DEFAULT 15,
    requires_active_membership BOOLEAN NOT NULL DEFAULT true,
    requires_waiver BOOLEAN NOT NULL DEFAULT true,
    requires_reservation BOOLEAN NOT NULL DEFAULT true,
    active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- =========================
-- USEFUL VIEWS
-- =========================
CREATE OR REPLACE VIEW active_athlete_memberships AS
SELECT
    a.athlete_id,
    a.family_id,
    a.first_name,
    a.last_name,
    m.membership_id,
    m.plan_id,
    mp.name AS plan_name,
    m.start_date,
    m.end_date,
    m.status
FROM athletes a
JOIN memberships m ON m.athlete_id = a.athlete_id
JOIN membership_plans mp ON mp.plan_id = m.plan_id
WHERE a.status = 'active'
  AND m.status = 'active'
  AND (m.end_date IS NULL OR m.end_date >= CURRENT_DATE);

CREATE OR REPLACE VIEW plan_service_access AS
SELECT
    mp.plan_id,
    mp.name AS plan_name,
    s.service_id,
    s.name AS service_name,
    ps.included_quantity,
    s.capacity_driver,
    s.revenue_type
FROM membership_plans mp
JOIN plan_services ps ON ps.plan_id = mp.plan_id
JOIN services s ON s.service_id = ps.service_id
WHERE mp.active = true
  AND s.active = true;

CREATE OR REPLACE VIEW training_block_capacity AS
SELECT
    tb.block_id,
    tb.name,
    tb.day_of_week,
    tb.start_time,
    tb.end_time,
    tb.capacity,
    r.name AS room_name,
    c.first_name || ' ' || c.last_name AS coach_name,
    s.name AS service_name
FROM training_blocks tb
LEFT JOIN rooms r ON r.room_id = tb.room_id
LEFT JOIN coaches c ON c.coach_id = tb.coach_id
LEFT JOIN services s ON s.service_id = tb.service_id
WHERE tb.active = true;

-- =========================
-- SEED BUSINESS RULES
-- =========================
INSERT INTO business_rules (rule_id, name, description, category, severity) VALUES
('BR-001','Families own billing','Every athlete must belong to exactly one family. Billing happens at the family level.','identity','required'),
('BR-002','Families may have multiple athletes','A family account can contain multiple athletes and guardians.','identity','required'),
('BR-003','Athletes own participation','Memberships, reservations, private lessons, and attendance are athlete-specific.','identity','required'),
('BR-004','Membership plans grant services','A membership plan grants access through plan_services.','membership','required'),
('BR-005','Services are delivered by training blocks','A service may be delivered through one or more recurring training blocks.','scheduling','required'),
('BR-006','Capacity must be enforced','A reservation cannot exceed the capacity of the training block unless an admin override is later implemented.','capacity','required'),
('BR-007','Waivers are required','No athlete may participate or receive door access without required waiver records.','safety','required'),
('BR-008','Active membership required','Member-only services require an active membership unless a standalone purchase is recorded.','membership','required'),
('BR-009','Open mat requires reservation','Open mat access requires a valid reservation unless admin grants exception.','access','required'),
('BR-010','Door access is temporary','Door access for reservations is valid only inside the configured access window.','access','required'),
('BR-011','Private lessons cannot overlap','A coach cannot be double-booked for private lessons or assigned sessions.','scheduling','required'),
('BR-012','Attendance records actual usage','Attendance is recorded against athlete, training block, and date.','attendance','required'),
('BR-013','Payment status can affect eligibility','Access may be blocked if membership payment status is delinquent, depending on admin policy.','billing','policy'),
('BR-014','Invoices preserve history','Invoice lines should preserve the price charged at the time of billing even if plan/service prices change later.','billing','required'),
('BR-015','Scholarships reduce family cost, not service value','Scholarships are recorded separately from payments and should not change the list price of the plan or service.','billing','required'),
('BR-016','Plans can change over time','Membership history must be preserved when an athlete upgrades, downgrades, pauses, or cancels.','membership','required'),
('BR-017','Coaches have capacity','Coach availability and assigned workload must be modeled separately from room capacity.','operations','required'),
('BR-018','Rooms have capacity','Room capacity is a property of rooms/training blocks and should not be recalculated in unrelated logic.','capacity','required'),
('BR-019','Vendor IDs are external references','Stripe, door access, and messaging IDs are references, not primary business identifiers.','integrations','required'),
('BR-020','LegacyOS owns eligibility decisions','External systems may execute payment or access actions, but LegacyOS should determine whether a user is eligible.','access','required')
ON CONFLICT (rule_id) DO UPDATE SET
    name = EXCLUDED.name,
    description = EXCLUDED.description,
    category = EXCLUDED.category,
    severity = EXCLUDED.severity,
    updated_at = now();

-- =========================
-- SEED ROLES AND PERMISSIONS
-- =========================
INSERT INTO app_roles (role_code, role_name, description) VALUES
('owner_admin','Owner/Admin','Full system administrator'),
('coach','Coach','Coach portal access'),
('parent_guardian','Parent/Guardian','Family portal access'),
('athlete','Athlete','Athlete self-service access'),
('front_desk','Front Desk','Operational check-in and support'),
('accountant','Accountant','Billing/reporting access')
ON CONFLICT (role_code) DO UPDATE SET role_name = EXCLUDED.role_name, description = EXCLUDED.description;

INSERT INTO app_permissions (permission_code, permission_name, description) VALUES
('families.manage','Manage Families','Create and edit families, guardians, and athletes'),
('memberships.manage','Manage Memberships','Assign and update memberships'),
('schedule.manage','Manage Schedule','Create and edit training blocks'),
('reservations.manage','Manage Reservations','Create/cancel reservations for any athlete'),
('reservations.own','Own Reservations','Create/cancel reservations for own family/athlete'),
('attendance.take','Take Attendance','Record attendance for assigned sessions'),
('billing.view','View Billing','View invoices and payments'),
('billing.manage','Manage Billing','Create/update invoices and payments'),
('access.manage','Manage Access','Manage credentials and access policies'),
('reports.view','View Reports','View dashboards and operational reports'),
('privates.manage','Manage Private Lessons','Book and manage private lessons')
ON CONFLICT (permission_code) DO UPDATE SET permission_name = EXCLUDED.permission_name, description = EXCLUDED.description;

-- Owner/Admin gets every permission.
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM app_roles r CROSS JOIN app_permissions p
WHERE r.role_code = 'owner_admin'
ON CONFLICT DO NOTHING;

-- Coach permissions.
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM app_roles r
JOIN app_permissions p ON p.permission_code IN ('attendance.take','reports.view','privates.manage')
WHERE r.role_code = 'coach'
ON CONFLICT DO NOTHING;

-- Parent permissions.
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM app_roles r
JOIN app_permissions p ON p.permission_code IN ('reservations.own','billing.view')
WHERE r.role_code = 'parent_guardian'
ON CONFLICT DO NOTHING;

-- Front desk permissions.
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM app_roles r
JOIN app_permissions p ON p.permission_code IN ('families.manage','reservations.manage','attendance.take','privates.manage')
WHERE r.role_code = 'front_desk'
ON CONFLICT DO NOTHING;

-- Accountant permissions.
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.role_id, p.permission_id
FROM app_roles r
JOIN app_permissions p ON p.permission_code IN ('billing.view','billing.manage','reports.view')
WHERE r.role_code = 'accountant'
ON CONFLICT DO NOTHING;

CREATE INDEX IF NOT EXISTS idx_audit_log_entity ON audit_log(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_audit_log_actor ON audit_log(actor_user_id);
CREATE INDEX IF NOT EXISTS idx_integration_events_provider ON integration_events(provider, event_type, received_at);
CREATE INDEX IF NOT EXISTS idx_app_users_email ON app_users(email);

COMMIT;
