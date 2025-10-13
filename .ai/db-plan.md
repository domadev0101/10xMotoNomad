# Database Schema - MotoNomad

## Migration Files

### 001_initial_schema.sql

```sql
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
```

### 002_add_rls_policies.sql

```sql
-- profiles RLS
ALTER TABLE profiles ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can view own profile"
ON profiles FOR SELECT
USING (auth.uid() = id);

CREATE POLICY "Users can update own profile"
ON profiles FOR UPDATE
USING (auth.uid() = id);

-- trips RLS
ALTER TABLE trips ENABLE ROW LEVEL SECURITY;

CREATE POLICY "Users can manage own trips"
ON trips FOR ALL
USING (auth.uid() = user_id);

-- companions RLS
ALTER TABLE companions ENABLE ROW LEVEL SECURITY;

CREATE POLICY "View companions of owned or invited trips"
ON companions FOR SELECT
USING (
  EXISTS (
    SELECT 1 FROM trips 
    WHERE trips.id = companions.trip_id 
    AND trips.user_id = auth.uid()
  )
  OR companions.user_id = auth.uid()
);

CREATE POLICY "Trip owners can add companions"
ON companions FOR INSERT
WITH CHECK (
  EXISTS (
    SELECT 1 FROM trips 
    WHERE trips.id = companions.trip_id 
    AND trips.user_id = auth.uid()
  )
);

CREATE POLICY "Trip owners can update companions"
ON companions FOR UPDATE
USING (
  EXISTS (
    SELECT 1 FROM trips 
    WHERE trips.id = companions.trip_id 
    AND trips.user_id = auth.uid()
  )
);

CREATE POLICY "Trip owners can delete companions"
ON companions FOR DELETE
USING (
  EXISTS (
    SELECT 1 FROM trips 
    WHERE trips.id = companions.trip_id 
    AND trips.user_id = auth.uid()
  )
);
```

### 003_add_triggers.sql

```sql
-- function: auto-create profile
CREATE OR REPLACE FUNCTION handle_new_user()
RETURNS TRIGGER AS $$
BEGIN
  INSERT INTO public.profiles (id, email)
  VALUES (NEW.id, NEW.email);
  RETURN NEW;
END;
$$ LANGUAGE plpgsql SECURITY DEFINER;

CREATE TRIGGER on_auth_user_created
  AFTER INSERT ON auth.users
  FOR EACH ROW EXECUTE FUNCTION handle_new_user();

-- function: auto-update timestamps
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = NOW();
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER update_profiles_updated_at
  BEFORE UPDATE ON profiles
  FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

CREATE TRIGGER update_trips_updated_at
  BEFORE UPDATE ON trips
  FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
```

### dev_seed.sql (Optional - Development Only)

```sql
-- Sample trips for test user: 00000000-0000-0000-0000-000000000001
INSERT INTO trips (id, user_id, name, start_date, end_date, description, transport_type)
VALUES 
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

-- Sample companions
INSERT INTO companions (trip_id, first_name, last_name, contact)
SELECT 
  t.id,
  'Jan',
  'Kowalski',
  'jan.kowalski@example.com'
FROM trips t
WHERE t.name = 'Bieszczady Motorcycle Adventure';

INSERT INTO companions (trip_id, first_name, last_name, contact)
SELECT 
  t.id,
  'Anna',
  'Nowak',
  '+48123456789'
FROM trips t
WHERE t.name = 'Bieszczady Motorcycle Adventure';

INSERT INTO companions (trip_id, first_name, last_name, contact)
SELECT 
  t.id,
  'Maria',
  'Wiśniewska',
  'maria.w@example.com'
FROM trips t
WHERE t.name = 'Berlin Tech Conference';
```

---

## C# Models

### TransportType.cs

```csharp
namespace MotoNomad.Models;

public enum TransportType
{
    Motorcycle = 0,
    Airplane = 1,
    Train = 2,
    Car = 3,
    Other = 4
}
```

### Profile.cs

```csharp
namespace MotoNomad.Models;

public class Profile
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### Trip.cs

```csharp
namespace MotoNomad.Models;

public class Trip
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public string? Description { get; set; }
    public TransportType TransportType { get; set; }
    public int DurationDays { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public List<Companion>? Companions { get; set; }
}
```

### Companion.cs

```csharp
namespace MotoNomad.Models;

public class Companion
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public Guid? UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

---

## Entity Relationships

```
auth.users (Supabase)
    │
    ├─ 1:1 ─► profiles (CASCADE)
    │
    └─ 1:N ─► trips (CASCADE)
            │
            └─ 1:N ─► companions (CASCADE)
                    │
                    └─ N:1 ─► auth.users (SET NULL, optional)
```

---

## Quick Reference

### Data Type Mappings

| PostgreSQL | C# |
|------------|-----|
| UUID | Guid |
| TEXT | string |
| INTEGER | int |
| DATE | DateOnly |
| TIMESTAMPTZ | DateTime (UTC) |

### Transport Type Values

| Value | Type |
|-------|------|
| 0 | Motorcycle |
| 1 | Airplane |
| 2 | Train |
| 3 | Car |
| 4 | Other |

### Common Queries

```sql
-- Get user's trips
SELECT * FROM trips 
WHERE user_id = auth.uid() 
ORDER BY start_date DESC;

-- Get trip with companions
SELECT t.*, json_agg(c.*) as companions
FROM trips t
LEFT JOIN companions c ON c.trip_id = t.id
WHERE t.id = $1 AND t.user_id = auth.uid()
GROUP BY t.id;

-- Get upcoming trips
SELECT * FROM trips
WHERE user_id = auth.uid() 
  AND start_date >= CURRENT_DATE
ORDER BY start_date ASC;
```
