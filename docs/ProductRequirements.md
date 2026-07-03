# LegacyOS Product Requirements Document (PRD)

**Version:** 2.0  
**Status:** Living Document  
**Owner:** Jason LaBuff  
**Product:** LegacyOS

---

# Chapter 1 - Vision

## Purpose

LegacyOS exists to remove administrative friction so coaches, athletes, and families can focus on developing people.

Everything in LegacyOS should contribute toward that goal.

Technology is not the product.

People are the product.

Technology simply enables them.

---

# The Problem

Youth sports organizations spend an enormous amount of time performing administrative work.

Memberships.

Attendance.

Scheduling.

Communication.

Waivers.

Billing.

Door access.

Fundraising.

Reporting.

Every minute spent managing administration is a minute that cannot be spent coaching athletes.

Today's software generally solves isolated pieces of the problem.

Organizations are forced to use multiple disconnected systems.

Examples include:

• Membership software

• Payment processing

• Scheduling

• Team communication

• Email marketing

• Facility access

• Reporting

• Accounting

These systems duplicate information, require repeated data entry, and rarely communicate effectively with one another.

LegacyOS exists to eliminate that fragmentation.

---

# Vision Statement

LegacyOS is the operating system for youth sports organizations.

Rather than solving one problem, LegacyOS provides a single platform that manages the operational lifecycle of an organization.

From the first inquiry through years of athlete development, LegacyOS should become the system that powers the organization.

---

# Mission Statement

Remove administrative friction so coaches can coach, athletes can develop, and families can enjoy the experience.

---

# Product Goal

Build the simplest, fastest, most intuitive operating system available for youth sports organizations.

LegacyOS should feel less like enterprise software and more like a trusted assistant.

---

# Success Definition

LegacyOS succeeds when...

A coach spends more time on the mat than behind a computer.

A parent can register a child in under five minutes.

An administrator can answer nearly any operational question in seconds.

An owner always knows the health of the organization.

Athletes simply show up and train.

---

# Guiding Principles

## 1. Business Before Software

LegacyOS models the business.

The business never changes to satisfy software limitations.

Software follows reality.

---

## 2. One Source of Truth

Every piece of information should exist once.

Families.

Athletes.

Memberships.

Invoices.

Attendance.

Everything should reference one authoritative source.

Duplicate information creates confusion.

---

## 3. Workflows Over Features

LegacyOS is designed around the work people perform.

Users should never have to understand the database or internal architecture.

They simply accomplish their goals.

---

## 4. Simplicity Wins

Every screen should eliminate work.

Every click should have a purpose.

Complexity is considered a defect.

---

## 5. Automation By Default

Whenever LegacyOS can perform work automatically, it should.

Examples include:

Automatic invoices

Automatic reminders

Automatic eligibility

Automatic attendance notifications

Automatic door access

Automatic reporting

---

## 6. Shared Ecosystem

Organizations may share facilities, coaches, athletes, and families.

Operational data should remain unified.

Financial data should remain separated.

---

## 7. APIs First

Every capability within LegacyOS must be available through an API.

The web application is only one consumer of those APIs.

Future consumers may include:

Mobile applications

Kiosks

AI assistants

Partner integrations

Door access systems

Future products

---

## 8. AI Is Native

Artificial Intelligence is not an add-on.

LegacyOS should be designed so AI can understand the organization's operational data from day one.

AI should eventually assist every role within the platform.

---

# Non-Goals

LegacyOS is not intended to become:

An accounting package

A payroll platform

A social network

A website builder

A marketing platform

A replacement for specialized accounting software

Instead, LegacyOS integrates with specialized systems whenever appropriate.

---

# Product Scope

LegacyOS manages the operation of youth sports organizations.

This includes:

Customer management

Membership management

Scheduling

Attendance

Billing

Facility operations

Communication

Reporting

Door access

Coach tools

Parent tools

Athlete tools

Artificial Intelligence

Future expansion into additional sports

---

# Definition of Success

Every feature added to LegacyOS must answer one question.

Does this reduce administrative work?

If the answer is no, the feature should be reconsidered.

---

# Closing Statement

LegacyOS is not being built to manage wrestling.

LegacyOS is being built to help organizations develop people through sport.

Every architectural decision, database table, API endpoint, and user interface should reinforce that purpose.

---

# Chapter 2 - Organizations

## Purpose

LegacyOS is designed to support one or more organizations operating within a shared ecosystem.

An organization represents a legal, financial, or operational entity.

Organizations own programs, services, memberships, financial reporting, and operational policies.

Organizations do not own people.

Families, Guardians, Athletes, and Coaches exist independently and may participate in multiple organizations simultaneously.

---

# Initial Organizations

The initial implementation of LegacyOS is expected to support two organizations.

## The Den at Legacy

Type:

For-Profit Operating Company

Purpose:

• After-school programs

• Camps

• Clinics

• Facility rentals

• Private lessons

• Facility operations

• General business operations

The Den primarily operates the physical facility.

---

## Wolfpack Wrestling Club

Type:

Non-Profit Organization

Purpose:

• Competitive wrestling

• Memberships

• Scholarships

• Donations

• Team development

• Community outreach

Wolfpack primarily operates the wrestling club.

---

# Shared Ecosystem

Although organizations remain financially separate, they operate inside one shared ecosystem.

The following information is shared.

Families

Guardians

Athletes

Facilities

Rooms

Some coaches

Communications

Door credentials

Medical information

Emergency contacts

This prevents duplicate records and creates one consistent experience for families.

---

# Financial Separation

Financial information belongs to the organization responsible for the transaction.

Examples include:

Membership revenue

Camp revenue

Facility rental income

Scholarships

Donations

Refunds

Invoices

Payments

Each transaction must always be attributable to exactly one organization.

---

# Organization Ownership

Organizations own:

Membership Plans

Services

Training Blocks

Invoices

Payments

Scholarships

Donations

Operational reports

Financial reports

Policies

Organizations do not own:

Families

Athletes

Guardians

Rooms

Facilities

Attendance history

Those records belong to the LegacyOS ecosystem.

---

# Shared Facilities

Facilities may host activities from multiple organizations.

Example

Legacy Facility

contains

Mat Room A

Mat Room B

Weight Room

Recovery Area

Classroom

The Den and Wolfpack may both schedule activities inside the same rooms.

Scheduling conflicts are prevented at the room level rather than the organization level.

---

# Organization Relationships

```text
LegacyOS

│

├── Organization

│      ├── Membership Plans

│      ├── Services

│      ├── Training Blocks

│      ├── Financial Records

│      └── Policies

│

├── Families

├── Guardians

├── Athletes

├── Coaches

└── Facilities
```

---

# Business Rules

## ORG-001

Every Membership Plan belongs to exactly one Organization.

---

## ORG-002

Every Service belongs to exactly one Organization.

---

## ORG-003

Every Invoice belongs to exactly one Organization.

---

## ORG-004

Every Payment belongs to exactly one Organization.

---

## ORG-005

An Athlete may participate in Services owned by multiple Organizations.

---

## ORG-006

A Family should never be duplicated because they participate in multiple Organizations.

---

## ORG-007

Organizations may share Facilities and Rooms.

---

## ORG-008

Organizations may define independent policies for:

Waivers

Door access

Membership eligibility

Scholarships

Communications

Pricing

Refunds

---

# Future Expansion

LegacyOS should support unlimited organizations.

Examples may include:

Legacy Volleyball

Legacy Soccer

Legacy Baseball

Legacy Basketball

Legacy Jiu-Jitsu

Partner organizations

Community organizations

The software architecture should not require redesign to support additional organizations.

---

# Architectural Decision

Organizations are first-class business entities.

They are not labels.

They are not categories.

They define ownership, reporting, financial responsibility, and operational behavior throughout LegacyOS.

Every future module should assume Organizations exist.

---

# Chapter 3 - Core Business Domains

## Purpose

LegacyOS models the operation of a youth sports organization through a collection of business domains.

Each domain represents a distinct area of responsibility.

Every feature, database table, API endpoint, and user interface belongs to one—and only one—domain.

Domains should communicate with one another through well-defined relationships while remaining independently understandable.

---

# Domain Overview

LegacyOS currently consists of the following core domains.

People

Organizations

Programs

Scheduling

Facilities

Finance

Compliance

Communications

Security

Analytics

Artificial Intelligence

Additional domains may be introduced in future versions without changing the underlying philosophy.

---

# People Domain

## Purpose

The People Domain represents every person who interacts with LegacyOS.

### Entities

Family

Guardian

Athlete

Coach

Staff (future)

Volunteer (future)

Official (future)

---

## Responsibilities

Manage relationships between people.

Manage contact information.

Manage communication preferences.

Manage athlete participation.

Provide identity throughout the platform.

---

## Guiding Principle

People exist independently of organizations.

Organizations may change.

People remain.

---

# Organization Domain

## Purpose

Organizations represent legal and operational entities.

Examples include:

The Den at Legacy

Wolfpack Wrestling Club

Future volleyball organizations

Future soccer organizations

Organizations own programs.

Organizations do not own people.

---

# Program Domain

## Purpose

Programs define what an organization offers.

Programs consist of several related concepts.

### Membership Plan

Defines what a customer purchases.

Examples:

Foundation

Competitor

Elite

---

### Service

Defines what is delivered.

Examples:

After School

Open Mat

Private Lesson

Camp

Clinic

Tournament Training

---

### Enrollment

Represents an athlete actively participating in a Membership Plan.

Enrollment connects an Athlete to a Membership Plan.

---

### Plan Services

Defines which Services belong to which Membership Plans.

---

# Scheduling Domain

## Purpose

Scheduling determines when services occur.

### Entities

Training Block

Reservation

Attendance

Private Lesson

Season (future)

Calendar (future)

---

## Guiding Principle

Scheduling exists to deliver Services.

Not Membership Plans.

Membership Plans grant access.

Training Blocks deliver experiences.

---

# Facility Domain

## Purpose

Represents the physical locations where activities occur.

### Entities

Facility

Room

Equipment (future)

Mat Area (future)

Locker (future)

---

## Responsibilities

Capacity

Availability

Scheduling conflicts

Resource allocation

---

# Finance Domain

## Purpose

Tracks all financial activity.

### Entities

Invoice

Invoice Line

Payment

Scholarship

Donation

Refund (future)

Discount (future)

---

## Responsibilities

Revenue

Expenses (future)

Receivables

Financial reporting

Organization reporting

---

# Compliance Domain

## Purpose

Ensure safe participation.

### Entities

Waiver

Medical Information

Emergency Contact

Incident Report (future)

Background Check (future)

Certification (future)

---

## Responsibilities

Eligibility

Risk management

Legal compliance

Medical awareness

---

# Communications Domain

## Purpose

Deliver information.

### Future Features

Email

SMS

Push Notifications

Announcements

News

Event reminders

AI-generated communication

---

# Security Domain

## Purpose

Control access.

### Entities

User

Role

Permission

Door Credential

Access Policy

Audit Log

---

## Responsibilities

Authentication

Authorization

Door access

Audit history

Permission management

---

# Analytics Domain

## Purpose

Measure organizational health.

### Example Metrics

Retention

Attendance

Revenue

Enrollment

Capacity utilization

Coach workload

Scholarship usage

Growth

---

# Artificial Intelligence Domain

## Purpose

Provide operational intelligence.

AI should never replace coaching.

AI should amplify coaching.

### Future Responsibilities

Predict attendance

Forecast revenue

Detect retention risk

Recommend scheduling improvements

Suggest communication

Generate reports

Assist coaches

Assist administrators

Assist families

---

# Domain Relationships

Organizations provide Programs.

Programs contain Membership Plans.

Membership Plans grant Services.

Services are delivered through Training Blocks.

Training Blocks accept Reservations.

Reservations become Attendance.

Attendance contributes to Analytics.

Analytics powers Artificial Intelligence.

Artificial Intelligence improves every domain.

---

# Domain Philosophy

Every feature added to LegacyOS should belong to exactly one primary business domain.

If a feature appears to belong to multiple domains, the feature should be decomposed until each responsibility has a clear owner.

This keeps the platform understandable, maintainable, and scalable as LegacyOS grows.

---

# Chapter 4 - Business Rules

## Purpose

Business rules define how LegacyOS behaves.

Unlike implementation details, business rules describe how the organization operates regardless of the technology used.

If the database changes, the business rules remain.

If the user interface changes, the business rules remain.

If LegacyOS is rebuilt in another programming language twenty years from now, these rules should still be valid.

---

# Guiding Principle

Software does not create business rules.

Software enforces business rules.

---

# Family Rules

## FAM-001

Every athlete belongs to exactly one Family account.

---

## FAM-002

A Family may contain one or more Guardians.

---

## FAM-003

A Guardian may be responsible for multiple Athletes.

---

## FAM-004

A Guardian may be associated with more than one Family if required.

Example:

Shared custody.

Blended families.

Legal guardians.

---

## FAM-005

Families own financial responsibility.

Athletes never own financial responsibility.

---

# Athlete Rules

## ATH-001

Every Athlete has one active Family.

---

## ATH-002

An Athlete may participate in multiple Organizations.

Example:

Wolfpack Membership

The Den After School

Summer Camp

Private Lessons

---

## ATH-003

Participation history is permanent.

Attendance records should never be deleted.

---

## ATH-004

Athletes may become inactive.

Inactive athletes should remain in the system for reporting purposes.

---

# Membership Rules

## MEM-001

Membership Plans belong to Organizations.

---

## MEM-002

Enrollment belongs to an Athlete.

---

## MEM-003

An Athlete may have multiple active Enrollments if the Organizations permit it.

---

## MEM-004

Membership Plans grant Services.

Membership Plans do not create schedules.

---

## MEM-005

Enrollment expiration immediately affects eligibility.

---

# Scheduling Rules

## SCH-001

Training Blocks deliver Services.

---

## SCH-002

Reservations consume capacity.

---

## SCH-003

Attendance records actual participation.

Reservations and Attendance are separate concepts.

---

## SCH-004

Capacity is enforced by the Room and Training Block.

---

## SCH-005

Organizations cannot double-book the same Room.

---

## SCH-006

Private Lessons are scheduled independently from Training Blocks.

---

# Billing Rules

## FIN-001

Invoices belong to Families.

---

## FIN-002

Payments satisfy Invoices.

---

## FIN-003

Every financial transaction belongs to exactly one Organization.

---

## FIN-004

Payments should never be deleted.

Corrections should be recorded through adjustments or refunds.

---

## FIN-005

Scholarships reduce financial responsibility but remain reportable.

---

# Attendance Rules

## ATT-001

Attendance should never be editable after administrative lock unless specifically authorized.

---

## ATT-002

Attendance history contributes to reporting.

---

## ATT-003

Attendance contributes to future AI recommendations.

---

# Communication Rules

## COM-001

Communication preferences belong to Guardians.

---

## COM-002

LegacyOS should never send communications to people who have opted out unless legally required.

---

## COM-003

Emergency communications ignore marketing preferences.

Safety takes priority.

---

# Door Access Rules

## ACC-001

Door access should be granted only if all required eligibility rules are satisfied.

---

## ACC-002

Possible eligibility checks include:

Active Enrollment

Required Waivers

Valid Reservation

Approved Time Window

Administrative Standing

---

## ACC-003

Every door access attempt should be logged.

Successful or denied.

---

# Organization Rules

## ORG-001

Organizations share people.

---

## ORG-002

Organizations separate finances.

---

## ORG-003

Organizations may define different pricing.

---

## ORG-004

Organizations may define different waiver requirements.

---

## ORG-005

Organizations may define different eligibility rules.

---

# Artificial Intelligence Rules

## AI-001

AI may recommend decisions.

AI may never silently make irreversible business decisions.

---

## AI-002

AI recommendations should always be explainable.

Users should understand why a recommendation was made.

---

## AI-003

AI should improve administrative efficiency before attempting to improve coaching.

---

# Data Integrity Rules

## DATA-001

LegacyOS should never duplicate business data unnecessarily.

---

## DATA-002

Every business object should have a single owner.

---

## DATA-003

Historical records should be preserved whenever practical.

LegacyOS values history over deletion.

---

## DATA-004

Soft deletion should be preferred over permanent deletion.

---

## DATA-005

Audit history should exist for critical business actions.

---

# Future Business Rules

Additional rule sets will be added for:

Tournament Management

Video Review

Fundraising

Volunteer Management

Equipment Checkout

Coach Certifications

Payroll Integration

Mobile Applications

AI Coaching

---

# Closing Statement

Business rules define the behavior of LegacyOS.

When implementation details conflict with these rules, the implementation should change—not the business rule.

These rules represent how Legacy organizations operate and should evolve only when the business itself evolves.

---

# Chapter 5 - Core User Workflows

## Purpose

LegacyOS is designed around workflows rather than features.

A workflow represents a complete task that delivers value to a user.

Every workflow should minimize clicks, reduce administrative effort, and provide a predictable experience.

Future user interfaces should be evaluated based on how efficiently they complete these workflows.

---

# Primary User Roles

LegacyOS serves six primary roles.

Owner

Organization Administrator

Front Desk

Coach

Guardian

Athlete

Each workflow should identify which role performs it.

---

# Workflow 1 - Register a New Family

Role:

Front Desk or Guardian

Goal:

Create a new Family account and register the first Athlete.

Success Criteria:

• Family created

• Guardian created

• Athlete created

• Required waivers assigned

• Welcome communication sent

Estimated Completion Time:

Under 5 minutes

Future Automation:

Address verification

Duplicate family detection

Automatic welcome email

---

# Workflow 2 - Enroll an Athlete

Role:

Administrator

Goal:

Enroll an Athlete into a Membership Plan.

Success Criteria:

Membership selected

Pricing confirmed

Enrollment created

Billing initiated

Eligibility updated

Door access updated

Future Automation:

Prorated billing

Scholarship application

Automatic recurring payments

---

# Workflow 3 - Schedule Programs

Role:

Administrator

Goal:

Create seasonal Training Blocks.

Success Criteria:

Coach assigned

Room assigned

Capacity defined

Schedule published

Reservation availability enabled

Future Automation:

Conflict detection

Coach availability suggestions

Room optimization

---

# Workflow 4 - Reserve a Spot

Role:

Guardian

Goal:

Reserve attendance for a limited-capacity event.

Examples:

Open Mat

Camp

Clinic

Special Practice

Success Criteria:

Reservation confirmed

Capacity updated

Confirmation delivered

Waitlist handled if necessary

Future Automation:

Automatic waitlist promotion

Reminder notifications

Calendar integration

---

# Workflow 5 - Check In

Role:

Front Desk

Goal:

Record athlete arrival.

Success Criteria:

Athlete located quickly

Eligibility verified

Attendance recorded

Door access validated

Estimated Completion Time:

Less than 10 seconds

Future Automation:

QR code

Phone check-in

RFID

Facial recognition (optional)

---

# Workflow 6 - Take Attendance

Role:

Coach

Goal:

Record attendance for an entire practice.

Success Criteria:

Roster displayed

Attendance completed quickly

Notes recorded if needed

Estimated Completion Time:

Less than 60 seconds

Future Automation:

Automatic attendance from check-in

Voice entry

AI attendance verification

---

# Workflow 7 - Sell a Private Lesson

Role:

Front Desk or Coach

Goal:

Schedule and collect payment for a private lesson.

Success Criteria:

Coach selected

Time selected

Payment collected

Calendar updated

Confirmation delivered

Future Automation:

Coach availability optimization

Revenue sharing calculations

Reminder messages

---

# Workflow 8 - Process Payment

Role:

Guardian

Goal:

Pay outstanding balance.

Success Criteria:

Invoice located

Payment completed

Receipt delivered

Account updated

Future Automation:

AutoPay

Stored payment methods

Payment reminders

---

# Workflow 9 - Organization Dashboard

Role:

Owner

Goal:

Understand organization health in under two minutes.

Dashboard should include:

Revenue

Enrollment

Attendance

Capacity

Retention

Outstanding invoices

Upcoming events

Scholarships

Coach workload

Future AI:

Highlight unusual trends

Forecast future revenue

Identify retention risks

Recommend operational improvements

---

# Workflow 10 - Coach Dashboard

Role:

Coach

Goal:

Know exactly what to do today.

Dashboard should include:

Today's practices

Roster

Attendance

Private lessons

Athlete notes

Announcements

Future AI:

Practice suggestions

Attendance concerns

Athlete development reminders

---

# Workflow 11 - Guardian Dashboard

Role:

Guardian

Goal:

Manage family participation.

Dashboard should include:

Upcoming practices

Invoices

Reservations

Attendance

Announcements

Messages

Waivers

Door credentials

Future AI:

Suggested programs

Upcoming deadlines

Recommended camps

Training reminders

---

# Workflow 12 - AI Operational Assistant

Role:

Administrator

Goal:

Ask operational questions using natural language.

Examples:

Show unpaid invoices.

Who has missed practice twice this month?

Which classes are over capacity?

Who has inactive waivers?

What programs are growing?

Future Goal:

Every report should eventually be available through conversational AI.

---

# Workflow Priorities

Phase 1

Family Registration

Enrollment

Scheduling

Attendance

Billing

Payments

---

Phase 2

Reservations

Private Lessons

Communications

Door Access

Reporting

---

Phase 3

AI Assistant

Mobile Applications

Coach Tools

Guardian Portal

Advanced Analytics

---

# Development Philosophy

LegacyOS should always be built vertically.

Complete one workflow before starting another.

Each completed workflow should include:

Database

Business Logic

API

User Interface

Testing

Documentation

Deployment

A workflow is not complete until a real user can perform it successfully.

---

# Closing Statement

The success of LegacyOS will not be measured by the number of features it contains.

It will be measured by how effortlessly users complete the workflows that matter most.

Every sprint should improve at least one workflow.