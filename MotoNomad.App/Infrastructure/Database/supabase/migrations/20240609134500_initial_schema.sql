-- migration: create initial schema for motonomad app
-- description: creates core tables for profiles, trips and companions
-- tables affected: profiles, trips, companions
-- dependencies: auth.users must exist (provided by supabase)

-- profiles table with rls
create table profiles (
  id uuid primary key references auth.users(id) on delete cascade,
  email text null,
  display_name text null,
  avatar_url text null,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

-- enable rls on profiles
alter table profiles enable row level security;

-- rls policies for profiles - anon role
create policy "anon can read public profiles" 
on profiles for select 
to anon
using (true);

-- rls policies for profiles - authenticated role  
create policy "authenticated users can read all profiles"
on profiles for select 
to authenticated
using (true);

create policy "users can update own profile"
on profiles for update
to authenticated
using (auth.uid() = id)
with check (auth.uid() = id);

-- trips table with rls
create table trips (
  id uuid primary key default gen_random_uuid(),
  user_id uuid not null references auth.users(id) on delete cascade,
  name text not null,
  start_date date not null,
  end_date date not null,
  description text null,
  transport_type integer not null,
  duration_days integer generated always as (end_date - start_date) stored,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  
  constraint valid_date_range check (end_date > start_date),
  constraint valid_transport_type check (transport_type between 0 and 4)
);

-- enable rls on trips
alter table trips enable row level security;

-- rls policies for trips - authenticated role only since trips are private
create policy "users can view own trips"
on trips for select
to authenticated
using (auth.uid() = user_id);

create policy "users can create own trips"
on trips for insert
to authenticated
with check (auth.uid() = user_id);

create policy "users can update own trips"
on trips for update
to authenticated
using (auth.uid() = user_id)
with check (auth.uid() = user_id);

create policy "users can delete own trips"
on trips for delete
to authenticated
using (auth.uid() = user_id);

-- companions table with rls
create table companions (
  id uuid primary key default gen_random_uuid(),
  trip_id uuid not null references trips(id) on delete cascade,
  user_id uuid null references auth.users(id) on delete set null,
  first_name text not null,
  last_name text not null,
  contact text null,
  created_at timestamptz not null default now(),
  
  constraint companion_identification check (
    user_id is not null or contact is not null
  )
);

-- enable rls on companions
alter table companions enable row level security;

-- rls policies for companions - authenticated users only
create policy "users can view companions of owned or invited trips"
on companions for select
to authenticated
using (
  exists (
    select 1 from trips 
    where trips.id = companions.trip_id 
    and trips.user_id = auth.uid()
  )
  or companions.user_id = auth.uid()
);

create policy "trip owners can add companions"
on companions for insert
to authenticated
with check (
  exists (
    select 1 from trips 
    where trips.id = companions.trip_id 
    and trips.user_id = auth.uid()
  )
);

create policy "trip owners can update companions"
on companions for update
to authenticated
using (
  exists (
    select 1 from trips 
    where trips.id = companions.trip_id 
    and trips.user_id = auth.uid()
  )
);

create policy "trip owners can delete companions"
on companions for delete
to authenticated
using (
  exists (
    select 1 from trips 
    where trips.id = companions.trip_id 
    and trips.user_id = auth.uid()
  )
);

-- indexes for better query performance
create index idx_trips_user_id on trips(user_id);
create index idx_trips_start_date on trips(start_date desc);
create index idx_companions_trip_id on companions(trip_id);
create index idx_companions_user_id on companions(user_id) where user_id is not null;

-- triggers for timestamp management and profile creation
create or replace function handle_new_user()
returns trigger as $$
begin
  insert into public.profiles (id, email)
  values (new.id, new.email);
  return new;
end;
$$ language plpgsql security definer;

create trigger on_auth_user_created
  after insert on auth.users
  for each row execute function handle_new_user();

create or replace function update_updated_at_column()
returns trigger as $$
begin
  new.updated_at = now();
  return new;
end;
$$ language plpgsql;

create trigger update_profiles_updated_at
  before update on profiles
  for each row execute function update_updated_at_column();

create trigger update_trips_updated_at
  before update on trips
  for each row execute function update_updated_at_column();