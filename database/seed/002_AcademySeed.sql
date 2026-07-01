/******************************************************************************
LegacyOS Seed: 002_AcademySeed.sql

Purpose:
Loads academy-specific configuration for the initial wrestling academy launch.
This is configuration data only. It does not create real families, athletes, payments, or reservations.
******************************************************************************/

INSERT INTO facilities (name, address, status)
VALUES ('Main Academy', 'TBD', 'active')
ON CONFLICT DO NOTHING;

INSERT INTO rooms (facility_id, name, square_feet, sq_ft_per_athlete, capacity, status)
SELECT facility_id, 'Main Mat Room', 3000, 83, 36, 'active'
FROM facilities
WHERE name = 'Main Academy'
ON CONFLICT DO NOTHING;

INSERT INTO seasons (name, start_month, end_month, notes, status)
VALUES
    ('All Year', NULL, NULL, 'Available all year.', 'active'),
    ('School Year', 8, 5, 'August through May school-year programming.', 'active'),
    ('Summer', 6, 7, 'June and July summer camp season.', 'active'),
    ('Preseason', 8, 10, 'August through October preseason build.', 'active'),
    ('In Season', 11, 2, 'November through February folkstyle/in-season period.', 'active'),
    ('Postseason', 3, 5, 'March through May freestyle/Greco/postseason period.', 'active')
ON CONFLICT DO NOTHING;

INSERT INTO coaches (first_name, last_name, role, pay_type, max_weekly_hours, hourly_rate, base_monthly_pay, revenue_share_rate, status)
VALUES
    ('Jason', 'LaBuff', 'Owner / Business Lead / Coach', 'owner', 40, NULL, NULL, NULL, 'active'),
    ('Ken', 'TBD', 'Head Coach', 'w2', 40, NULL, 5500, NULL, 'active'),
    ('Assistant', 'Coach', 'Assistant Coach', 'contractor_1099', 15, 25, NULL, NULL, 'active')
ON CONFLICT DO NOTHING;

INSERT INTO services (name, category, standalone_price, revenue_type, capacity_driver, default_capacity, variable_cost, active)
VALUES
    ('After School Wrestling 1 Day', 'After School', 149, 'monthly', 'room', 72, 10, true),
    ('After School Wrestling 2 Day', 'After School', 249, 'monthly', 'room', 36, 18, true),
    ('2-A-Day P1', 'Performance', 50, 'monthly', 'coach', 40, 5, true),
    ('Homeschool Program', 'Daytime', 450, 'monthly', 'room', 20, 30, true),
    ('HS Wrestling', 'Serious Training', 115, 'monthly', 'room', 20, 8, true),
    ('Serious Youth Wrestlers', 'Serious Training', 115, 'monthly', 'room', 20, 8, true),
    ('Open Mat Add-On', 'Access', 50, 'monthly', 'reservation', 25, 3, true),
    ('Private Lesson', 'Private Training', 60, 'per_session', 'coach', 34, 5, true),
    ('Summer Camp', 'Camp', 275, 'weekly', 'room', 36, 40, true),
    ('School-Year Clinic/Camp', 'Event', 125, 'event', 'room', 20, 15, true),
    ('Online Training Program', 'Digital', 100, 'monthly', 'unlimited', NULL, 10, true),
    ('Space Rental 90-Min', 'Rental', 125, 'rental', 'room', 20, 10, true)
ON CONFLICT DO NOTHING;

INSERT INTO membership_plans (name, monthly_price, description, active)
VALUES
    ('Foundation', 149, 'Entry membership: After School Wrestling 1 Day.', true),
    ('Performance', 279, 'Development membership: After School 2 Day, Open Mat, and Online Training.', true),
    ('Competitor', 379, 'Serious wrestler membership: Performance benefits plus serious training access.', true),
    ('Elite', 479, 'Top-track membership with full access and future priority benefits.', true)
ON CONFLICT DO NOTHING;

INSERT INTO plan_services (plan_id, service_id, included_quantity)
SELECT p.plan_id, s.service_id, 1
FROM membership_plans p
JOIN services s ON
    (p.name = 'Foundation' AND s.name IN ('After School Wrestling 1 Day'))
    OR (p.name = 'Performance' AND s.name IN ('After School Wrestling 2 Day','Open Mat Add-On','Online Training Program'))
    OR (p.name = 'Competitor' AND s.name IN ('After School Wrestling 2 Day','Open Mat Add-On','Online Training Program','HS Wrestling','Serious Youth Wrestlers','2-A-Day P1'))
    OR (p.name = 'Elite' AND s.name IN ('After School Wrestling 2 Day','Open Mat Add-On','Online Training Program','HS Wrestling','Serious Youth Wrestlers','2-A-Day P1'))
ON CONFLICT DO NOTHING;

INSERT INTO training_blocks (name, service_id, room_id, coach_id, day_of_week, start_time, end_time, capacity, season_id, active)
SELECT x.name, s.service_id, r.room_id, c.coach_id, x.day_of_week, x.start_time::time, x.end_time::time, x.capacity, se.season_id, true
FROM (
    VALUES
        ('After School 1-Day A', 'After School Wrestling 1 Day', 'Jason', 'Monday', '16:15', '17:30', 36, 'School Year'),
        ('After School 1-Day B', 'After School Wrestling 1 Day', 'Jason', 'Wednesday', '16:15', '17:30', 36, 'School Year'),
        ('After School 2-Day Tuesday', 'After School Wrestling 2 Day', 'Jason', 'Tuesday', '16:15', '17:30', 36, 'School Year'),
        ('After School 2-Day Thursday', 'After School Wrestling 2 Day', 'Jason', 'Thursday', '16:15', '17:30', 36, 'School Year'),
        ('Serious Youth', 'Serious Youth Wrestlers', 'Ken', 'Monday/Wednesday', '18:00', '19:00', 20, 'School Year'),
        ('HS Wrestling', 'HS Wrestling', 'Ken', 'Tuesday/Thursday', '18:00', '19:00', 20, 'School Year'),
        ('Open Mat Evening', 'Open Mat Add-On', NULL, 'Daily', '20:00', '22:00', 15, 'All Year')
) AS x(name, service_name, coach_first_name, day_of_week, start_time, end_time, capacity, season_name)
JOIN services s ON s.name = x.service_name
JOIN rooms r ON r.name = 'Main Mat Room'
LEFT JOIN coaches c ON c.first_name = x.coach_first_name
JOIN seasons se ON se.name = x.season_name
ON CONFLICT DO NOTHING;
