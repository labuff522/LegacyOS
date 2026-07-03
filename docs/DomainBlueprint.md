# LegacyOS Domain Blueprint

**Version:** 1.0  
**Status:** Active  
**Owner:** Jason LaBuff  
**Purpose:** Define how LegacyOS thinks about the wrestling academy business domain.

---

## 1. Core Philosophy

LegacyOS is built around the business, not around screens or database tables.

The system should model the academy using clear business concepts:

- Families are customer accounts.
- Guardians communicate and manage logistics.
- Athletes participate.
- Membership Plans grant Services.
- Services are delivered through Training Blocks.
- Reservations consume capacity.
- Attendance records participation.
- Invoices and Payments belong to Families.
- Door Access is granted only when business rules allow it.

---

## 2. Domain Map

```text
People
  Family
  Guardian
  Athlete
  Coach

Programs
  MembershipPlan
  Service
  PlanService
  Enrollment

Scheduling
  TrainingBlock
  Reservation
  Attendance
  PrivateLesson

Facilities
  Facility
  Room

Finance
  Invoice
  InvoiceLine
  Payment
  Scholarship

Compliance
  Waiver
  MedicalInfo
  EmergencyContact

Access
  DoorCredential
  DoorAccessEvent
  AccessPolicy
```

---

## 3. People Domain

### Family

A Family is the customer account. A Family is not a person.

A Family owns:

- Guardians
- Athletes
- Billing relationship
- Invoices
- Payments
- Emergency contact structure
- Scholarships or discounts

### Guardian

A Guardian is an adult attached to a Family.

A Guardian may be:

- Billing contact
- Emergency contact
- Pickup contact
- SMS contact
- Email contact
- Portal user

### Athlete

An Athlete is the participant.

An Athlete owns:

- Enrollments
- Reservations
- Attendance
- Waivers
- Private lessons
- Future skill/progress tracking

### Coach

A Coach delivers training.

A Coach owns:

- Assigned Training Blocks
- Private Lessons
- Availability
- Future certifications and notes

---

## 4. Programs Domain

### MembershipPlan

A MembershipPlan is a sellable package.

Examples:

- Foundation
- Performance
- Competitor
- Elite

A MembershipPlan does not occur on the calendar. It grants Services.

### Service

A Service is something the academy sells or grants access to.

Examples:

- After School Wrestling 1 Day
- After School Wrestling 2 Day
- Open Mat
- Serious Youth Wrestlers
- HS Wrestling
- Private Lesson
- Summer Camp
- Online Training

### PlanService

PlanService is the bridge between MembershipPlans and Services.

Example:

```text
Performance includes:
  After School Wrestling 2 Day
  Open Mat
  Online Training
```

### Enrollment

Enrollment is the athlete's actual participation in a MembershipPlan.

Example:

```text
Carter LaBuff is enrolled in Competitor starting August 1.
```

This avoids confusion between the plan itself and the athlete's active membership.

---

## 5. Scheduling Domain

### TrainingBlock

A TrainingBlock is a scheduled delivery of a Service.

Examples:

- Monday 4:15 After School 1-Day A
- Tuesday 4:15 After School 2-Day
- Daily 8:00 Open Mat
- Wednesday 6:00 Serious Youth

TrainingBlocks define:

- Day
- Time
- Coach
- Room
- Capacity
- Season
- Active status

### Reservation

A Reservation is a booked spot in a TrainingBlock.

Reservations are especially important for:

- Open Mat
- Camps
- Clinics
- Limited capacity events

### Attendance

Attendance records actual participation.

Attendance is separate from Reservation.

A Reservation means the athlete planned to attend.  
Attendance means the athlete actually attended.

### PrivateLesson

PrivateLesson is its own appointment object.

It should not be treated as a normal Reservation because it has:

- One athlete
- One coach
- Specific time
- Price
- Possible revenue share
- Cancellation policy
- Coaching notes

---

## 6. Facilities Domain

### Facility

A Facility is a physical academy location.

### Room

A Room is a specific usable space within a Facility.

Room owns:

- Square footage
- Capacity
- Availability
- Equipment context

Capacity belongs primarily to the Room and the TrainingBlock.

---

## 7. Finance Domain

### Invoice

An Invoice belongs to a Family.

Athletes do not receive invoices.

### InvoiceLine

InvoiceLine records what was charged.

Examples:

- Monthly Competitor Membership
- Private Lesson
- Camp Registration
- Space Rental

### Payment

Payment records money received.

Payments may come from:

- Stripe
- ACH
- Cash
- Check
- External processor

### Scholarship

Scholarship records financial assistance.

Scholarship should be tracked separately from Payment so donor-funded support can be reported cleanly.

---

## 8. Compliance Domain

### Waiver

A Waiver records required legal consent.

Participation should be blocked if required waivers are missing.

### MedicalInfo

MedicalInfo stores relevant emergency/medical information.

Access should be restricted by role.

### EmergencyContact

Emergency contacts can be derived from Guardians or stored explicitly if needed.

---

## 9. Access Domain

### DoorCredential

A DoorCredential links an Athlete or Guardian to an access provider credential.

### DoorAccessEvent

DoorAccessEvent records attempted and successful access.

### AccessPolicy

AccessPolicy defines the rules for entry.

Example:

```text
Open Mat access requires:
  Active membership
  Open Mat service access
  Active waiver
  Valid reservation
  Access time window
```

---

## 10. Ownership Rules

### OR-001

Family owns billing.

### OR-002

Guardian owns communication preferences.

### OR-003

Athlete owns participation history.

### OR-004

MembershipPlan owns package definition.

### OR-005

Service owns what is being sold or granted.

### OR-006

TrainingBlock owns schedule and capacity context.

### OR-007

Reservation owns intended usage.

### OR-008

Attendance owns actual usage.

### OR-009

Coach owns instructional delivery.

### OR-010

Room owns physical limitations.

---

## 11. Naming Decisions

| Business Concept | LegacyOS Term |
|---|---|
| Customer account | Family |
| Parent/adult contact | Guardian |
| Kid/wrestler | Athlete |
| Package | MembershipPlan |
| Product/offering | Service |
| Active membership | Enrollment |
| Class/session time | TrainingBlock |
| Booking | Reservation |
| Check-in record | Attendance |

---

## 12. Development Rules

1. The Domain Blueprint should be updated before major domain changes.
2. New database migrations should follow the domain language.
3. API names should match domain names.
4. UI labels may be friendlier, but should not contradict the domain model.
5. Avoid duplicate concepts.
6. If two concepts have different lifecycles, they should probably be separate entities.
7. If a concept only exists as a join between two entities, model it as a bridge table.
8. Do not store person-specific fields directly on Family unless they truly belong to the account.

---

## 13. Current Architectural Decisions

### ADR-001: Family is a customer account

Family does not represent a person. It represents the account that owns athletes, billing, and household-level records.

### ADR-002: Guardians are separate from Family

Guardians are modeled separately because a family may have multiple adults with different responsibilities.

### ADR-003: Enrollment replaces Membership

MembershipPlan is the package. Enrollment is an athlete's active participation in that package.

### ADR-004: PrivateLesson is separate from Reservation

Private lessons have appointment-specific business rules and should not be forced into the general reservation model.

### ADR-005: TrainingBlocks deliver Services

Services are abstract offerings. TrainingBlocks are scheduled delivery instances.

---

## 14. Future Domains

LegacyOS may eventually include:

- VideoReview
- TournamentManagement
- Recruiting
- AIRecommendations
- ProgressTracking
- EquipmentManagement
- Messaging
- MultiLocationManagement

These should be added only when the core academy workflow is stable.
