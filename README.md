# LegacyOS

## Phase 1 parent portal authentication

The parent portal is available at `/portal`. A staff user first creates a one-time guardian invitation with `POST /staff/guardian-invitations`; the parent then registers with the invitation, their existing Guardian email, and a password of at least 12 characters. Invitations expire after 48 hours. Access tokens expire after 12 hours, are stored only as SHA-256 hashes, and are revoked on logout.

Copy `backend/LegacyOS.Api/appsettings.Development.example.json` to `appsettings.Development.json` (which is gitignored) and supply the PostgreSQL connection string plus an initial staff email/password. In deployed environments, provide the equivalent values through your secret manager:

- `ConnectionStrings__LegacyOS`
- `BootstrapStaff__Email`
- `BootstrapStaff__Password`
- `Cors__AllowedOrigins__0` (and subsequent indexed values for each exact frontend origin)

The bootstrap credentials create the first staff account only when that email does not already exist. Remove the bootstrap password from runtime configuration after the account is created. Sign in through `/portal/login`; staff accounts are routed to the existing admin UI, while customer accounts can only access `/portal/me` for their linked family and athletes.

## Stripe Checkout purchase MVP

Customer catalog and checkout endpoints are under `/portal/purchases`. Membership plans create monthly Stripe subscriptions and an inactive Enrollment; only a verified `checkout.session.completed` or `checkout.session.async_payment_succeeded` webhook activates it. Products use one-time Checkout payments. Configure these secrets outside source control:

- `Stripe__SecretKey`
- `Stripe__WebhookSecret`
- `Frontend__BaseUrl`

Register the public `POST /stripe/webhook` URL in Stripe for `checkout.session.completed`, `checkout.session.async_payment_succeeded`, `checkout.session.async_payment_failed`, and `checkout.session.expired`. Use the endpoint-specific signing secret. Prices are derived server-side from LegacyOS records; the browser never supplies an amount.

## USA Wrestling membership verification

Parents submit a required USA Wrestling membership number for each athlete from the family portal. New or changed numbers always return to `Pending`; a parent can never mark a membership current. Staff review submissions through the USA Wrestling administration screen and record `Current`, `Expired`, or `Rejected`, with an optional expiration date and notes. The staff account and verification time are retained for audit purposes. This is intentionally manual until USA Wrestling provides an approved API or roster integration.

LegacyOS is the operating platform solution for organizing a complex gym that provides multiple products and spaces, and requires coaching.

It manages:

- Families
- Athletes
- Memberships
- Scheduling
- Reservations
- Attendance
- Billing
- Door Access
- Coaching
- Reporting

## Status

Current Version:
Pre-Alpha

Current Milestone:
MVP for Academy Launch

Target Launch:
August 2026
