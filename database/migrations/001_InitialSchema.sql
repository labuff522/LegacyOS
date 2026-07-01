/******************************************************************************
LegacyOS Migration: 001

Name: Initial Schema

Purpose:
Creates the initial LegacyOS database schema.

Author:
Jason LaBuff

******************************************************************************/

CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- =========================
-- ENUMS
-- =========================

CREATE TYPE status_active AS ENUM ('active', 'inactive', 'archived');
CREATE TYPE membership_status AS ENUM ('active', 'paused', 'cancelled', 'expired', 'pending');
CREATE TYPE reservation_status AS ENUM ('reserved', 'cancelled', 'completed', 'no_show');
CREATE TYPE invoice_status AS ENUM ('draft', 'open', 'paid', 'void', 'overdue');
CREATE TYPE payment_status AS ENUM ('pending', 'succeeded', 'failed', 'refunded');
CREATE TYPE attendance_status AS ENUM ('present', 'absent', 'excused', 'late');
CREATE TYPE pay_type AS ENUM ('owner', 'w2', 'contractor_1099', 'volunteer');
CREATE TYPE revenue_type AS ENUM ('monthly', 'weekly', 'event', 'per_session', 'rental', 'digital');
CREATE TYPE capacity_driver AS ENUM ('room', 'coach', 'reservation', 'unlimited');

-- =========================
-- PEOPLE
-- =========================

CREATE TABLE families (
    family_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    primary_contact_name TEXT NOT NULL,
    email TEXT NOT NULL,
    phone TEXT,
    billing_address TEXT,
    emergency_contact_name TEXT,
    emergency_contact_phone TEXT,
    status status_active NOT NULL DEFAULT 'active',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE guardians (
    guardian_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    family_id UUID NOT NULL REFERENCES families(family_id) ON DELETE CASCADE,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    email TEXT,
    phone TEXT,
    relationship TEXT,
    is_primary_billing_contact BOOLEAN NOT NULL DEFAULT false,
    status status_active NOT NULL DEFAULT 'active',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE athletes (
    athlete_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    family_id UUID NOT NULL REFERENCES families(family_id) ON DELETE CASCADE,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    date_of_birth DATE,
    grade TEXT,
    school TEXT,
    skill_level TEXT,
    weight_class TEXT,
    usa_wrestling_number TEXT,
    status status_active NOT NULL DEFAULT 'active',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE coaches (
    coach_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    role TEXT,
    pay_type pay_type NOT NULL,
    max_weekly_hours NUMERIC(5,2),
    hourly_rate NUMERIC(10,2),
    base_monthly_pay NUMERIC(10,2),
    revenue_share_rate NUMERIC(5,4),
    status status_active NOT NULL DEFAULT 'active',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- =========================
-- FACILITY
-- =========================

CREATE TABLE facilities (
    facility_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    address TEXT,
    status status_active NOT NULL DEFAULT 'active'
);

CREATE TABLE rooms (
    room_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    facility_id UUID NOT NULL REFERENCES facilities(facility_id) ON DELETE CASCADE,
    name TEXT NOT NULL,
    square_feet INTEGER,
    sq_ft_per_athlete NUMERIC(8,2),
    capacity INTEGER,
    status status_active NOT NULL DEFAULT 'active'
);

CREATE TABLE seasons (
    season_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    start_month INTEGER,
    end_month INTEGER,
    notes TEXT,
    status status_active NOT NULL DEFAULT 'active'
);

-- =========================
-- SERVICES / PLANS
-- =========================

CREATE TABLE services (
    service_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    category TEXT,
    standalone_price NUMERIC(10,2),
    revenue_type revenue_type NOT NULL,
    capacity_driver capacity_driver NOT NULL,
    default_capacity INTEGER,
    variable_cost NUMERIC(10,2) NOT NULL DEFAULT 0,
    active BOOLEAN NOT NULL DEFAULT true
);

CREATE TABLE membership_plans (
    plan_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    monthly_price NUMERIC(10,2) NOT NULL,
    description TEXT,
    active BOOLEAN NOT NULL DEFAULT true
);

CREATE TABLE plan_services (
    plan_id UUID NOT NULL REFERENCES membership_plans(plan_id) ON DELETE CASCADE,
    service_id UUID NOT NULL REFERENCES services(service_id) ON DELETE CASCADE,
    included_quantity NUMERIC(8,2) NOT NULL DEFAULT 1,
    PRIMARY KEY (plan_id, service_id)
);

-- =========================
-- SCHEDULING
-- =========================

CREATE TABLE training_blocks (
    block_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name TEXT NOT NULL,
    service_id UUID REFERENCES services(service_id),
    room_id UUID REFERENCES rooms(room_id),
    coach_id UUID REFERENCES coaches(coach_id),
    day_of_week TEXT NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    capacity INTEGER,
    season_id UUID REFERENCES seasons(season_id),
    active BOOLEAN NOT NULL DEFAULT true
);

CREATE TABLE memberships (
    membership_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    athlete_id UUID NOT NULL REFERENCES athletes(athlete_id) ON DELETE CASCADE,
    plan_id UUID NOT NULL REFERENCES membership_plans(plan_id),
    start_date DATE NOT NULL,
    end_date DATE,
    monthly_price NUMERIC(10,2) NOT NULL,
    status membership_status NOT NULL DEFAULT 'active',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE reservations (
    reservation_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    athlete_id UUID NOT NULL REFERENCES athletes(athlete_id) ON DELETE CASCADE,
    block_id UUID NOT NULL REFERENCES training_blocks(block_id),
    reservation_date DATE NOT NULL,
    status reservation_status NOT NULL DEFAULT 'reserved',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    UNIQUE (athlete_id, block_id, reservation_date)
);

CREATE TABLE private_lessons (
    private_lesson_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    athlete_id UUID NOT NULL REFERENCES athletes(athlete_id),
    coach_id UUID NOT NULL REFERENCES coaches(coach_id),
    room_id UUID REFERENCES rooms(room_id),
    lesson_date DATE NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    price NUMERIC(10,2) NOT NULL,
    status reservation_status NOT NULL DEFAULT 'reserved',
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE attendance (
    attendance_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    athlete_id UUID NOT NULL REFERENCES athletes(athlete_id),
    block_id UUID NOT NULL REFERENCES training_blocks(block_id),
    session_date DATE NOT NULL,
    status attendance_status NOT NULL,
    checked_in_at TIMESTAMPTZ,
    notes TEXT,
    UNIQUE (athlete_id, block_id, session_date)
);

-- =========================
-- BILLING
-- =========================

CREATE TABLE invoices (
    invoice_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    family_id UUID NOT NULL REFERENCES families(family_id),
    invoice_date DATE NOT NULL,
    due_date DATE NOT NULL,
    amount NUMERIC(10,2) NOT NULL,
    status invoice_status NOT NULL DEFAULT 'open',
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE TABLE invoice_lines (
    invoice_line_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    invoice_id UUID NOT NULL REFERENCES invoices(invoice_id) ON DELETE CASCADE,
    description TEXT NOT NULL,
    service_id UUID REFERENCES services(service_id),
    membership_id UUID REFERENCES memberships(membership_id),
    quantity NUMERIC(8,2) NOT NULL DEFAULT 1,
    unit_price NUMERIC(10,2) NOT NULL,
    line_total NUMERIC(10,2) GENERATED ALWAYS AS (quantity * unit_price) STORED
);

CREATE TABLE payments (
    payment_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    invoice_id UUID REFERENCES invoices(invoice_id),
    family_id UUID NOT NULL REFERENCES families(family_id),
    payment_date TIMESTAMPTZ NOT NULL DEFAULT now(),
    amount NUMERIC(10,2) NOT NULL,
    payment_method TEXT,
    processor TEXT,
    processor_transaction_id TEXT,
    status payment_status NOT NULL DEFAULT 'pending'
);

CREATE TABLE scholarships (
    scholarship_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    athlete_id UUID NOT NULL REFERENCES athletes(athlete_id),
    family_id UUID NOT NULL REFERENCES families(family_id),
    amount NUMERIC(10,2) NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE,
    funding_source TEXT,
    notes TEXT,
    status status_active NOT NULL DEFAULT 'active'
);

-- =========================
-- WAIVERS / ACCESS
-- =========================

CREATE TABLE waivers (
    waiver_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    family_id UUID NOT NULL REFERENCES families(family_id),
    athlete_id UUID REFERENCES athletes(athlete_id),
    waiver_type TEXT NOT NULL,
    signed_date TIMESTAMPTZ NOT NULL DEFAULT now(),
    signed_by_guardian_id UUID REFERENCES guardians(guardian_id),
    status status_active NOT NULL DEFAULT 'active'
);

CREATE TABLE door_credentials (
    credential_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    athlete_id UUID REFERENCES athletes(athlete_id),
    guardian_id UUID REFERENCES guardians(guardian_id),
    access_provider TEXT,
    external_credential_id TEXT,
    active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    CHECK (athlete_id IS NOT NULL OR guardian_id IS NOT NULL)
);

CREATE TABLE door_access_events (
    access_event_id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    credential_id UUID NOT NULL REFERENCES door_credentials(credential_id),
    room_id UUID REFERENCES rooms(room_id),
    event_datetime TIMESTAMPTZ NOT NULL DEFAULT now(),
    access_granted BOOLEAN NOT NULL,
    reason TEXT
);

-- =========================
-- INDEXES
-- =========================

CREATE INDEX idx_athletes_family ON athletes(family_id);
CREATE INDEX idx_guardians_family ON guardians(family_id);
CREATE INDEX idx_memberships_athlete ON memberships(athlete_id);
CREATE INDEX idx_reservations_block_date ON reservations(block_id, reservation_date);
CREATE INDEX idx_attendance_block_date ON attendance(block_id, session_date);
CREATE INDEX idx_private_lessons_coach_date ON private_lessons(coach_id, lesson_date);
CREATE INDEX idx_invoices_family_status ON invoices(family_id, status);
CREATE INDEX idx_payments_family ON payments(family_id);
