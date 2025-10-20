# Service Implementation Plan: TripService

## 1. Endpoint Overview

**Purpose**: Manage CRUD operations for trips with business logic validation.

**Functionality**:
- Create new trips with date validation
- Retrieve trips (all, by ID, upcoming, past)
- Update existing trips
- Delete trips (CASCADE companions)
- Automatic duration calculation

**Service Location**: `MotoNomad.App/Infrastructure/Services/TripService.cs`

**Interface**: `ITripService` in `MotoNomad.App/Application/Interfaces/ITripService.cs`

**Key Dependencies**:
- **ISupabaseClientService** - Abstraction for Supabase client access
- **ILogger<TripService>** - Logging for trip operations

**Note**: 
- TripService does NOT inject IAuthService or ICompanionService
- Authentication accessed via `_supabaseClient.GetClient().Auth.CurrentUser`
- Companion counts retrieved via direct Supabase queries (not via ICompanionService)
- This maintains loose coupling and allows parallel service development

---

## 2. Request Details

### 2.1 GetAllTripsAsync
- **Method**: `Task<IEnumerable<TripListItemDto>> GetAllTripsAsync()`
- **Parameters**: None
- **Purpose**: Retrieve all trips for current user

### 2.2 GetTripByIdAsync
- **Method**: `Task<TripDetailDto> GetTripByIdAsync(Guid tripId)`
- **Parameters**: `tripId` (Guid)
- **Purpose**: Retrieve detailed trip information

### 2.3 CreateTripAsync
- **Method**: `Task<TripDetailDto> CreateTripAsync(CreateTripCommand command)`
- **Parameters**:
  - `Name` (string): Trip name, max 200 characters
  - `StartDate` (DateOnly): Trip start date
  - `EndDate` (DateOnly): Trip end date (must be > StartDate)
  - `TransportType` (enum): Transportation mode (0-4)
  - `Description` (string, optional): Max 2000 characters

### 2.4 UpdateTripAsync
- **Method**: `Task<TripDetailDto> UpdateTripAsync(UpdateTripCommand command)`
- **Parameters**: Same as CreateTripAsync + `Id` (Guid)

### 2.5 DeleteTripAsync
- **Method**: `Task DeleteTripAsync(Guid tripId)`
- **Parameters**: `tripId` (Guid)

### 2.6 GetUpcomingTripsAsync
- **Method**: `Task<IEnumerable<TripListItemDto>> GetUpcomingTripsAsync()`
- **Parameters**: None

### 2.7 GetPastTripsAsync
- **Method**: `Task<IEnumerable<TripListItemDto>> GetPastTripsAsync()`
- **Parameters**: None

---

## 3. Utilized Types

### DTOs:
- **TripListItemDto**: Lightweight trip representation for lists
- **TripDetailDto**: Complete trip information with companions

### Commands:
- **CreateTripCommand**: Trip creation request
- **UpdateTripCommand**: Trip update request
- **DeleteTripCommand**: Trip deletion request

### Entities:
- **Trip**: Database entity matching Supabase schema
- **TransportType**: Enum (Motorcycle, Airplane, Train, Car, Other)

### Exceptions:
- **UnauthorizedException**: User not authenticated
- **ValidationException**: Invalid input or business rules
- **NotFoundException**: Trip not found or unauthorized
- **DatabaseException**: Supabase query failures

---

## 4. Response Details

### GetAllTripsAsync Response:
```csharp
IEnumerable<TripListItemDto> {
    Id, Name, StartDate, EndDate, 
    DurationDays, TransportType, 
    CompanionCount, CreatedAt
}
```

### GetTripByIdAsync Response:
```csharp
TripDetailDto {
    All TripListItemDto fields +
    UserId, Description, 
    Companions (List), UpdatedAt
}
```

---

## 5. Data Flow

### CreateTripAsync Flow:
```
1. Get Current User
2. Validate CreateTripCommand
   - Name: required, max 200 chars
   - EndDate > StartDate
   - Description: optional, max 2000 chars
3. Create Trip Entity
4. Insert into Database
5. Retrieve Created Trip (with DurationDays)
6. Return TripDetailDto
```

### GetTripByIdAsync Flow:
```
1. Get Current User
2. Parallel Queries (Task.WhenAll):
   - Query Trip by ID (RLS check)
   - Query Companions by trip_id
3. Check Trip Exists
4. Map to TripDetailDto
5. Return TripDetailDto
```

---

## 6. Security Considerations

- **Authentication**: All methods require authenticated user
- **Authorization**: Row Level Security (RLS) at database level
- **RLS Policy**: `auth.uid() = user_id`
- **Data Validation**: Name, dates, description length
- **Business Rules**: EndDate must be after StartDate

---

## 7. Error Handling

### ValidationException:
- "Trip name is required"
- "Trip name cannot exceed 200 characters"
- "End date must be after start date"
- "Description cannot exceed 2000 characters"

### NotFoundException:
- "Trip not found" (also for unauthorized access)

### DatabaseException:
- "Failed to create trip"
- "Failed to update trip"
- "Failed to delete trip"

---

## 8. Performance Considerations

- Parallel queries with Task.WhenAll
- Database indexes on user_id and start_date
- Companion count optimization (batch queries)
- Pagination support (future enhancement)

**Performance Metrics**:
- GetAllTripsAsync: < 1 second (up to 100 trips)
- GetTripByIdAsync: < 500ms (parallel queries)
- CreateTripAsync: < 1 second
- UpdateTripAsync: < 1 second
- DeleteTripAsync: < 500ms

---

## 9. Implementation Steps

1. Create DTOs and Commands
2. Create Service Interface
3. Implement TripService with all methods
4. Register Service in Program.cs
5. Create Unit Tests
6. Integration Testing
7. Documentation

---

**Status**: ✅ Ready for Implementation