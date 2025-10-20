# Service Implementation Plan: CompanionService

## 1. Endpoint Overview

**Purpose**: Manage companions (trip participants) with ownership verification.

**Functionality**:
- Retrieve companions for specific trip
- Add companions to trips
- Update companion information
- Remove companions from trips
- Count companions per trip

**Service Location**: `MotoNomad.App/Infrastructure/Services/CompanionService.cs`

**Interface**: `ICompanionService` in `MotoNomad.App/Application/Interfaces/ICompanionService.cs`

**Key Dependencies**:
- **ISupabaseClientService** - Abstraction for Supabase client access
- **ILogger<CompanionService>** - Logging for companion operations

**Note**: 
- CompanionService does NOT inject IAuthService
- Authentication accessed via `_supabaseClient.GetClient().Auth.CurrentUser`
- Manual trip ownership verification required (indirect relationship: companions → trips → users)

---

## 2. Request Details

### 2.1 GetCompanionsByTripIdAsync
- **Method**: `Task<IEnumerable<CompanionListItemDto>> GetCompanionsByTripIdAsync(Guid tripId)`
- **Parameters**: `tripId` (Guid)
- **Purpose**: Retrieve all companions for specific trip

### 2.2 GetCompanionByIdAsync
- **Method**: `Task<CompanionDto> GetCompanionByIdAsync(Guid companionId)`
- **Parameters**: `companionId` (Guid)
- **Purpose**: Retrieve detailed companion information

### 2.3 AddCompanionAsync
- **Method**: `Task<CompanionDto> AddCompanionAsync(AddCompanionCommand command)`
- **Parameters**:
  - `TripId` (Guid): Trip identifier
  - `FirstName` (string): Max 100 characters
  - `LastName` (string): Max 100 characters
  - `Contact` (string, optional): Email or phone, max 255 characters

### 2.4 UpdateCompanionAsync
- **Method**: `Task<CompanionDto> UpdateCompanionAsync(UpdateCompanionCommand command)`
- **Parameters**: Same as AddCompanionAsync + `Id` (Guid)

### 2.5 RemoveCompanionAsync
- **Method**: `Task RemoveCompanionAsync(Guid companionId)`
- **Parameters**: `companionId` (Guid)

### 2.6 GetCompanionCountAsync
- **Method**: `Task<int> GetCompanionCountAsync(Guid tripId)`
- **Parameters**: `tripId` (Guid)
- **Purpose**: Count companions for trip (internal use)

---

## 3. Utilized Types

### DTOs:
- **CompanionListItemDto**: Lightweight companion representation
- **CompanionDto**: Complete companion information

### Commands:
- **AddCompanionCommand**: Companion creation request
- **UpdateCompanionCommand**: Companion update request
- **RemoveCompanionCommand**: Companion deletion request

### Entities:
- **Companion**: Database entity matching Supabase schema

### Exceptions:
- **UnauthorizedException**: User not authenticated or not trip owner
- **ValidationException**: Invalid input
- **NotFoundException**: Companion or trip not found
- **DatabaseException**: Supabase query failures

---

## 4. Response Details

### GetCompanionsByTripIdAsync Response:
```csharp
IEnumerable<CompanionListItemDto> {
    Id, TripId, FirstName, 
    LastName, Contact
}
```

### GetCompanionByIdAsync Response:
```csharp
CompanionDto {
    Id, TripId, UserId (null in MVP),
    FirstName, LastName, Contact,
    CreatedAt
}
```

---

## 5. Data Flow

### AddCompanionAsync Flow:
```
1. Get Current User
2. Validate AddCompanionCommand
   - FirstName: required, max 100 chars
   - LastName: required, max 100 chars
   - Contact: optional, max 255 chars
3. Verify Trip Ownership (RLS check)
4. Create Companion Entity
5. Insert into Database
6. Return CompanionDto
```

### GetCompanionsByTripIdAsync Flow:
```
1. Get Current User
2. Verify Trip Ownership (RLS check)
3. Query Companions by trip_id
4. Map to CompanionListItemDto
5. Return IEnumerable<CompanionListItemDto>
```

### RemoveCompanionAsync Flow:
```
1. Get Current User
2. Retrieve Companion
3. Verify Trip Ownership (RLS check)
4. Delete from Database
5. Return (void)
```

---

## 6. Security Considerations

- **Authentication**: All methods require authenticated user
- **Trip Ownership Verification**: Required for all operations
- **RLS**: Prevents unauthorized access at database level
- **Data Validation**: FirstName, LastName, Contact length
- **UserId Field**: Always null in MVP (no user linking)

---

## 7. Error Handling

### ValidationException:
- "First name is required"
- "First name cannot exceed 100 characters"
- "Last name is required"
- "Last name cannot exceed 100 characters"
- "Contact cannot exceed 255 characters"

### NotFoundException:
- "Trip not found" (ownership verification failed)
- "Companion not found"

### DatabaseException:
- "Failed to add companion"
- "Failed to update companion"
- "Failed to remove companion"

---

## 8. Performance Considerations

- Trip ownership check for every operation (security > performance)
- Batch companion count queries when possible
- Database indexes on trip_id
- Consider caching trip ownership (invalidate on trip delete)

**Performance Metrics**:
- GetCompanionsByTripIdAsync: < 500ms
- AddCompanionAsync: < 1 second
- UpdateCompanionAsync: < 1 second
- RemoveCompanionAsync: < 500ms
- GetCompanionCountAsync: < 200ms

---

## 9. Implementation Steps

1. Create DTOs and Commands
2. Create Service Interface
3. Implement CompanionService with ownership verification
4. Register Service in Program.cs
5. Create Unit Tests
6. Integration Testing
7. Documentation

---

**Status**: ✅ Ready for Implementation