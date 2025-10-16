-- migration: development seed data
-- description: adds sample data for development environment
-- tables affected: trips, companions
-- warning: this migration should only be run in development

-- Sample trips for test user
insert into trips (id, user_id, name, start_date, end_date, description, transport_type)
values 
  (
    gen_random_uuid(), 
    '00000000-0000-0000-0000-000000000001', 
    'Bieszczady Motorcycle Adventure', 
    '2025-06-15', 
    '2025-06-22', 
    'Summer motorcycle trip through Bieszczady Mountains', 
    0
  ),
  (
    gen_random_uuid(), 
    '00000000-0000-0000-0000-000000000001',
    'Berlin Tech Conference', 
    '2025-09-10', 
    '2025-09-12',
    'Annual tech conference in Berlin', 
    1
  ),
  (
    gen_random_uuid(),
    '00000000-0000-0000-0000-000000000001',
    'Warsaw Weekend', 
    '2025-11-01', 
    '2025-11-03',
    'Quick weekend trip to Warsaw', 
    2
  );

-- Sample companions for trips
insert into companions (trip_id, first_name, last_name, contact)
select 
  t.id,
  'Jan',
  'Kowalski',
  'jan.kowalski@example.com'
from trips t
where t.name = 'Bieszczady Motorcycle Adventure';

insert into companions (trip_id, first_name, last_name, contact)
select 
  t.id,
  'Anna',
  'Nowak',
  '+48123456789'
from trips t
where t.name = 'Bieszczady Motorcycle Adventure';

insert into companions (trip_id, first_name, last_name, contact)
select 
  t.id,
  'Maria',
  'Wi?niewska',
  'maria.w@example.com'
from trips t
where t.name = 'Berlin Tech Conference';