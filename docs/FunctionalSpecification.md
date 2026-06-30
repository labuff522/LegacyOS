# LegacyOS Functional Specification

**Version:** 1.0 (Living Document)  
**Status:** Active Development (Pre-Alpha)  
**Owner:** Jason LaBuff

---

# 1. Purpose
LegacyOS is the operating platform for a wrestling academy. It is the system of record for academy operations while integrating with specialized third-party services.

# 2. Product Vision
LegacyOS answers:
- Who are my athletes?
- What have they purchased?
- What are they allowed to attend?
- Who is coaching?
- Is there room?
- Who has paid?
- Who is in the building?
- How healthy is the business?

# 3. Guiding Principles
1. Single source of truth.
2. No duplicate data.
3. Buy commodity software; build competitive advantage.
4. Every business rule is documented.
5. Database first.
6. API first.
7. UI is replaceable.

# 4. Core Modules
- Identity
- Families
- Athletes
- Membership Plans
- Services
- Training Blocks
- Reservations
- Attendance
- Private Lessons
- Billing
- Reporting
- Door Access
- Administration

# 5. User Roles
| Role | Responsibilities |
|------|------------------|
| Owner | Full administration |
| Coach | Sessions, attendance, privates |
| Parent | Family, billing, reservations |
| Athlete | Schedule, reservations |
| Staff | Check-in |
| Accountant | Financial reporting |

# 6. Core Entities
- Family
- Athlete
- Membership Plan
- Service
- Training Block
- Reservation
- Coach
- Room

# 7. Business Rules
- BR-001 Every athlete belongs to exactly one family.
- BR-002 Families may contain multiple athletes.
- BR-003 Membership Plans grant Services.
- BR-004 Services are delivered through Training Blocks.
- BR-005 Training Blocks have finite capacity.
- BR-006 Reservations cannot exceed capacity.
- BR-007 Attendance is recorded per athlete, block and date.
- BR-008 Families own invoices and payments.
- BR-009 Private lessons cannot overlap a coach's schedule.
- BR-010 Open Mat requires eligibility.

# 8. Core Workflows
## New Family
1. Create Family
2. Add Guardians
3. Add Athletes
4. Select Membership
5. Sign Waiver
6. Activate Membership

## Open Mat
1. Validate Membership
2. Validate Waiver
3. Validate Capacity
4. Create Reservation
5. Grant Temporary Access

## Attendance
1. Open Roster
2. Record Attendance
3. Save

# 9. Screens
## Parent
Dashboard, Athletes, Membership, Billing, Reservations, Privates, Waivers

## Coach
Today's Schedule, Roster, Attendance, Private Lessons

## Admin
Dashboard, Families, Athletes, Plans, Services, Schedule, Reservations, Billing, Reports, Access

# 10. Integrations
- Stripe
- QuickBooks
- Brivo/Kisi
- Twilio
- SendGrid

# 11. Security
- RBAC
- Audit Logging
- HTTPS
- Password Hashing
- MFA (future)

# 12. Reporting
- Revenue
- Profit
- Capacity
- Coach Utilization
- Membership Counts
- Open Mat Usage
- Alerts

# 13. MVP
Authentication, Families, Athletes, Plans, Services, Schedule, Reservations, Attendance, Billing, Dashboard.

# 14. Roadmap
Phase 2: Messaging, Waitlists
Phase 3: AI Coaching, Video Review
Phase 4: Multi-location, SaaS

# 15. Living Document Policy
This document is the authoritative source for LegacyOS. Update this specification before changing the database or code.
