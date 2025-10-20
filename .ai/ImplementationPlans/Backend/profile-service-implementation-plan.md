# Service Implementation Plan: ProfileService

## 1. Endpoint Overview

**Purpose**: Manage user profile information with partial update support.

**Functionality**:
- Retrieve current user's profile
- Update display name and avatar URL
- Support partial updates (only provided fields)

**Service Location**: `MotoNomad.App/Infrastructure/Services/ProfileService.cs`

**Interface**: `IProfileService` in `MotoNomad.App/Application/Interfaces/IProfileService.cs`

**Key Dependencies**:
- **ISupabaseClientService** - Abstraction for Supabase client access
- **ILogger<ProfileService>** - Logging for profile operations

**Note**: 
- ProfileService does NOT inject IAuthService
- Authentication accessed via `_supabaseClient.GetClient().Auth.CurrentUser`
- Email field is read-only (managed by Supabase Auth, not profiles table)

---

## 2. Request Details

### 2.1 GetCurrentProfileAsync
- **Method**: `Task<ProfileDto> GetCurrentProfileAsync()`
- **Parameters**: None
- **Purpose**: Retrieve profile for currently authenticated user

### 2.2 UpdateProfileAsync
- **Method**: `Task<ProfileDto> UpdateProfileAsync(UpdateProfileCommand command)`
- **Parameters**:
  - `DisplayName` (string, optional): Max 100 characters
  - `AvatarUrl` (string, optional): Valid URL, max 500 characters
- **Purpose**: Update user profile with partial update support

**Note**: At least one field must be provided (not both null).

---

## 3. Utilized Types

### DTOs:
- **ProfileDto**: Complete profile information

### Commands:
- **UpdateProfileCommand**: Profile update request with partial support

### Entities:
- **Profile**: Database entity matching Supabase schema

### Exceptions:
- **UnauthorizedException**: User not authenticated
- **ValidationException**: Invalid update data or no fields provided
- **NotFoundException**: Profile not found (rare)
- **DatabaseException**: Supabase query failures

---

## 4. Response Details

### GetCurrentProfileAsync Response:
```csharp
ProfileDto {
    Id, Email, DisplayName, 
    AvatarUrl, CreatedAt, UpdatedAt
}
```

### UpdateProfileAsync Response:
```csharp
ProfileDto {
    Updated fields + new UpdatedAt timestamp
}
```

---

## 5. Data Flow

### GetCurrentProfileAsync Flow:
```
1. Get Current User
2. Query Profile by currentUser.Id (RLS)
3. Check Profile Exists
4. Map to ProfileDto
5. Return ProfileDto
```

### UpdateProfileAsync Flow:
```
1. Get Current User
2. Validate UpdateProfileCommand
   - At least one field provided
   - DisplayName: max 100 chars (if provided)
   - AvatarUrl: valid URL format (if provided)
3. Retrieve Existing Profile
4. Update Profile Entity (Partial Update)
   - Only update provided fields
5. Update in Database
6. Retrieve Updated Profile
7. Map to ProfileDto
8. Return ProfileDto
```

---

## 6. Security Considerations

- **Authentication**: All methods require authenticated user
- **RLS**: Users can only access their own profile
- **Email Field**: Read-only (managed by Supabase Auth)
- **URL Validation**: Valid HTTP/HTTPS format
- **Partial Updates**: Only provided fields are updated

---

## 7. Error Handling

### ValidationException:
- "At least one field must be provided for update"
- "Display name cannot be empty"
- "Display name cannot exceed 100 characters"
- "Avatar URL format is invalid"
- "Avatar URL cannot exceed 500 characters"

### NotFoundException:
- "Profile not found" (rare - should not happen)

### DatabaseException:
- "Failed to update profile"

---

## 8. Performance Considerations

- Profile caching in AuthService (< 50ms if cached)
- Partial update optimization (only modified fields)
- Single database query for retrieval

**Performance Metrics**:
- GetCurrentProfileAsync: < 200ms (< 50ms if cached)
- UpdateProfileAsync: < 1 second

---

## 9. Implementation Steps

1. Create DTOs and Commands
2. Create Service Interface
3. Implement ProfileService with partial updates
4. Register Service in Program.cs
5. Create Unit Tests
6. Integration Testing
7. Documentation

---

**Status**: ✅ Ready for Implementation