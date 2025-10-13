-- profiles table
CREATE TABLE profiles (
  id UUID PRIMARY KEY REFERENCES auth.users(id) ON DELETE CASCADE,
  email TEXT NULL,
  display_name TEXT NULL,
  avatar_url TEXT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- trips table
CREATE TABLE trips (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES auth.users(id) ON DELETE CASCADE,
  name TEXT NOT NULL,
  start_date DATE NOT NULL,
  end_date DATE NOT NULL,
  description TEXT NULL,
  transport_type INTEGER NOT NULL,
  duration_days INTEGER GENERATED ALWAYS AS (end_date - start_date) STORED,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  
  CONSTRAINT valid_date_range CHECK (end_date > start_date),
  CONSTRAINT valid_transport_type CHECK (transport_type BETWEEN 0 AND 4)
);

-- companions table
CREATE TABLE companions (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  trip_id UUID NOT NULL REFERENCES trips(id) ON DELETE CASCADE,
  user_id UUID NULL REFERENCES auth.users(id) ON DELETE SET NULL,
  first_name TEXT NOT NULL,
  last_name TEXT NOT NULL,
  contact TEXT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  
  CONSTRAINT companion_identification CHECK (
    user_id IS NOT NULL OR contact IS NOT NULL
  )
);

-- indexes
CREATE INDEX idx_trips_user_id ON trips(user_id);
CREATE INDEX idx_trips_start_date ON trips(start_date DESC);
CREATE INDEX idx_companions_trip_id ON companions(trip_id);
CREATE INDEX idx_companions_user_id ON companions(user_id) WHERE user_id IS NOT NULL;