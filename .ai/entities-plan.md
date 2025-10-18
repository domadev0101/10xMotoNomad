# Database Entities - Documentation

## ?? Overview

All entities inherit from `Postgrest.Models.BaseModel` and are properly mapped to Supabase tables using attributes.

---

## ??? Entities

### 1. Trip

**Table:** `trips`

**File:** `MotoNomad.App/Infrastructure/Database/Entities/Trip.cs`

```csharp
[Table("trips")]
public class Trip : BaseModel
```

#### Properties:

| Property | Type | DB Column | Description |
|----------|------|-----------|-------------|
| `Id` | `Guid` | `id` | Unique identifier (PK) |
| `UserId` | `Guid` | `user_id` | Owner user ID (FK) |
| `Name` | `string` | `name` | Trip name |
| `StartDate` | `DateOnly` | `start_date` | Start date |
| `EndDate` | `DateOnly` | `end_date` | End date |
| `Description` | `string?` | `description` | Description (optional) |
| `TransportType` | `TransportType` | `transport_type` | Transport type (enum) |
| `DurationDays` | `int` | `duration_days` | Duration in days |
| `CreatedAt` | `DateTime` | `created_at` | Created timestamp |
| `UpdatedAt` | `DateTime` | `updated_at` | Last updated timestamp |
| `Companions` | `List<Companion>?` | - | Navigation to companions |

---

### 2. Companion

**Table:** `companions`

**File:** `MotoNomad.App/Infrastructure/Database/Entities/Companion.cs`

```csharp
[Table("companions")]
public class Companion : BaseModel
```

#### Properties:

| Property | Type | DB Column | Description |
|----------|------|-----------|-------------|
| `Id` | `Guid` | `id` | Unique identifier (PK) |
| `TripId` | `Guid` | `trip_id` | Trip ID (FK) |
| `UserId` | `Guid?` | `user_id` | User ID (optional, FK) |
| `FirstName` | `string` | `first_name` | First name |
| `LastName` | `string` | `last_name` | Last name |
| `Contact` | `string?` | `contact` | Contact info (optional) |
| `CreatedAt` | `DateTime` | `created_at` | Created timestamp |

---

### 3. Profile

**Table:** `profiles`

**File:** `MotoNomad.App/Infrastructure/Database/Entities/Profile.cs`

```csharp
[Table("profiles")]
public class Profile : BaseModel
```

#### Properties:

| Property | Type | DB Column | Description |
|----------|------|-----------|-------------|
| `Id` | `Guid` | `id` | Unique identifier (PK) |
| `Email` | `string?` | `email` | Email address (optional) |
| `DisplayName` | `string?` | `display_name` | Display name (optional) |
| `AvatarUrl` | `string?` | `avatar_url` | Avatar URL (optional) |
| `CreatedAt` | `DateTime` | `created_at` | Created timestamp |
| `UpdatedAt` | `DateTime` | `updated_at` | Last updated timestamp |

---

### 4. TransportType

**File:** `MotoNomad.App/Infrastructure/Database/Entities/TransportType.cs`

```csharp
public enum TransportType
{
    Motorcycle = 0,
    Airplane = 1,
    Train = 2,
    Car = 3,
    Other = 4
}
```

---

## ?? Postgrest Attributes

### `[Table("table_name")]`
- Maps C# class to specific database table
- **Required** for all entities

### `[PrimaryKey("column_name")]`
- Marks column as primary key
- **Required** for ID

### `[Column("column_name")]`
- Maps C# property to database column
- Use snake_case in database, PascalCase in C#

---

## ?? Usage Examples

### Fetching data:

```csharp
var client = supabaseClient.GetClient();

// Get all trips
var trips = await client
    .From<Trip>()
    .Get();

// Get user trips
var userTrips = await client
    .From<Trip>()
    .Where(t => t.UserId == userId)
    .Get();

// Get single trip
var trip = await client
    .From<Trip>()
    .Where(t => t.Id == tripId)
    .Single();
```

### Creating:

```csharp
var newTrip = new Trip
{
    Id = Guid.NewGuid(),
    UserId = currentUserId,
    Name = "Summer Vacation 2025",
    StartDate = new DateOnly(2025, 7, 1),
    EndDate = new DateOnly(2025, 7, 15),
    TransportType = TransportType.Motorcycle,
    DurationDays = 14
};

var response = await client
    .From<Trip>()
    .Insert(newTrip);
```

### Updating:

```csharp
var tripToUpdate = await client
    .From<Trip>()
    .Where(t => t.Id == tripId)
    .Single();

tripToUpdate.Name = "Updated name";
tripToUpdate.UpdatedAt = DateTime.UtcNow;

await client
    .From<Trip>()
    .Update(tripToUpdate);
```

### Deleting:

```csharp
await client
    .From<Trip>()
    .Where(t => t.Id == tripId)
    .Delete();
```

---

## ?? Important Notes

1. **BaseModel is required** - all entities must inherit from `Postgrest.Models.BaseModel`

2. **Naming convention:**
   - Tables: `snake_case` (e.g., `trips`, `companions`)
   - Columns: `snake_case` (e.g., `user_id`, `created_at`)
   - C# Classes: `PascalCase` (e.g., `Trip`, `Companion`)
   - C# Properties: `PascalCase` (e.g., `UserId`, `CreatedAt`)

3. **Date types:**
   - `DateOnly` - for dates without time (start_date, end_date)
   - `DateTime` - for timestamps with time (created_at, updated_at)

4. **Nullable types:**
   - Use `?` for optional fields (e.g., `string?`, `Guid?`)

5. **Enum:**
   - `TransportType` is a plain enum (does not inherit from BaseModel)
   - Values stored as int (0, 1, 2...)

6. **Navigation properties:**
   - `Companions` in `Trip` is not mapped to DB
   - Used for relationships in C# code
   - Requires separate query or join

---

**Status:** ? Ready to use  
**Last Updated:** 2025-01-18  
**Version:** 1.0
