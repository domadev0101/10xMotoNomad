# Service Implementation Plan: AuthService

## 1. Endpoint Overview

**Purpose**: Manage user authentication and session lifecycle using Supabase Auth.

**Functionality**:
- User registration with email and password
- User login with credentials
- Session management (logout, refresh, current user retrieval)
- Authentication status verification

**Service Location**: `MotoNomad.App/Infrastructure/Services/AuthService.cs`

**Interface**: `IAuthService` in `MotoNomad.App/Application/Interfaces/IAuthService.cs`

**Key Dependencies**:
- ISupabaseClientService - Abstraction for Supabase client access
- ILogger<AuthService> - Logging for authentication operations

**Note**: Token storage is handled automatically by Supabase SDK's session management, not via Blazored.LocalStorage. The SDK persists authentication state internally.

---

## 2. Request Details

### 2.1 RegisterAsync

- **Method**: `Task<UserDto> RegisterAsync(RegisterCommand command)`
- **Parameters**:
  - **Required**: 
    - `Email` (string): Valid email address
    - `Password` (string): Minimum 8 characters
  - **Optional**:
    - `DisplayName` (string): Max 100 characters
- **Purpose**: Create new user account with Supabase Auth

### 2.2 LoginAsync

- **Method**: `Task<UserDto> LoginAsync(LoginCommand command)`
- **Parameters**:
  - **Required**:
    - `Email` (string): Valid email address
    - `Password` (string): Minimum 8 characters
- **Purpose**: Authenticate existing user

### 2.3 LogoutAsync

- **Method**: `Task LogoutAsync()`
- **Parameters**: None
- **Purpose**: Sign out current user and invalidate session

### 2.4 GetCurrentUserAsync

- **Method**: `Task<UserDto?> GetCurrentUserAsync()`
- **Parameters**: None
- **Purpose**: Retrieve currently authenticated user information

### 2.5 IsAuthenticatedAsync

- **Method**: `Task<bool> IsAuthenticatedAsync()`
- **Parameters**: None
- **Purpose**: Check if user has valid active session

### 2.6 RefreshSessionAsync

- **Method**: `Task RefreshSessionAsync()`
- **Parameters**: None
- **Purpose**: Refresh JWT token before expiration

---

## 3. Utilized Types

### DTOs:
- **UserDto**: `MotoNomad.App/Application/DTOs/Auth/UserDto.cs`

### Commands:
- **RegisterCommand**: `MotoNomad.App/Application/Commands/Auth/RegisterCommand.cs`
- **LoginCommand**: `MotoNomad.App/Application/Commands/Auth/LoginCommand.cs`

### Entities:
- **Profile**: `MotoNomad.App/Infrastructure/Database/Entities/Profile.cs`

### Exceptions:
- **ValidationException**: Invalid input data
- **AuthException**: Supabase Auth failures
- **DatabaseException**: Profile creation failures

---

## 4. Response Details

### RegisterAsync Response:
```csharp
UserDto {
    Id = Guid,
    Email = "user@example.com",
    DisplayName = "John Doe",
    AvatarUrl = null,
    CreatedAt = DateTime.UtcNow
}
```

### LoginAsync Response:
```csharp
UserDto {
    Id = Guid,
    Email = "user@example.com",
    DisplayName = "John Doe",
    AvatarUrl = "https://...",
    CreatedAt = DateTime
}
```

### GetCurrentUserAsync Response:
- **Success**: `UserDto` with current user information
- **Not Authenticated**: `null`

### IsAuthenticatedAsync Response:
- **Authenticated**: `true`
- **Not Authenticated**: `false`

---

## 5. Data Flow

### RegisterAsync Flow:
```
1. Validate RegisterCommand (email format, password strength)
2. Get Supabase client via _supabaseClient.GetClient()
3. Call client.Auth.SignUp(email, password)
4. Create Profile in database (if DisplayName provided)
5. JWT Token managed automatically by Supabase SDK
6. Return UserDto with user information
```

### LoginAsync Flow:
```
1. Validate LoginCommand (email format, required fields)
2. Get Supabase client via _supabaseClient.GetClient()
3. Call client.Auth.SignIn(email, password)
4. Retrieve Profile from Database (for DisplayName and AvatarUrl)
5. JWT Token managed automatically by Supabase SDK
6. Return UserDto with user and profile information
```

**Authentication State Management:**
- Supabase SDK automatically persists session tokens
- No manual localStorage operations required
- Session refresh handled by SDK (AutoRefreshToken = true)
- Authentication state available via client.Auth.CurrentUser

---

## 6. Security Considerations

- JWT tokens stored securely in Blazored.LocalStorage
- HTTPS for all communication
- Row Level Security (RLS) for profiles table
- Password never stored in client code or logs
- Auto-logout after 15 minutes of inactivity

---

## 7. Error Handling

### ValidationException:
- "Email address is required"
- "Email address format is invalid"
- "Password must be at least 8 characters"

### AuthException:
- "Invalid email or password"
- "Email address is already registered"

### DatabaseException:
- "Failed to create user profile"

---

## 8. Performance Considerations

- Token caching in Blazored.LocalStorage
- Profile caching in memory after login
- Supabase client configured as Singleton
- Async operations throughout

**Performance Metrics**:
- Registration: < 2 seconds
- Login: < 1 second
- Logout: < 500ms
- GetCurrentUser: < 100ms (cached)

---

## 9. Implementation Steps

1. Create Exception Classes
2. Create DTOs and Commands
3. Create Service Interface
4. Implement AuthService
5. Register Service in Program.cs
6. Create Unit Tests
7. Integration Testing
8. Documentation

---

**Status**: ✅ Ready for Implementation