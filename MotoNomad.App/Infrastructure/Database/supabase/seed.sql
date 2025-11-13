-- Seed data for local development
-- This file is automatically loaded after `supabase db reset`
-- Add your test data here

-- Example: Create test user (you'll need to adjust based on your setup)
-- INSERT INTO auth.users (id, email, encrypted_password, email_confirmed_at, created_at, updated_at)
-- VALUES (
--   '00000000-0000-0000-0000-000000000001',
--   'test@example.com',
--   crypt('password123', gen_salt('bf')),
--   NOW(),
--   NOW(),
--   NOW()
-- );

-- Note: Add your test trips and companions here
-- They will be automatically created after every `supabase db reset`

-- Example trips:
-- INSERT INTO trips (user_id, name, start_date, end_date, description, transport_type)
-- VALUES 
--   ('00000000-0000-0000-0000-000000000001', 'Test Trip', '2025-06-01', '2025-06-05', 'Test description', 0);

-- Example companions:
-- INSERT INTO companions (trip_id, first_name, last_name, contact)
-- SELECT id, 'John', 'Doe', 'john@example.com'
-- FROM trips WHERE name = 'Test Trip';
