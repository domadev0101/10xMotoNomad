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