# Service Layer Specification - MotoNomad

**Project:** MotoNomad MVP  
**Program:** 10xDevs  
**Date:** October 2025  
**Status:** Ready for Implementation

---

## 1. Architecture Overview

The service layer acts as an orchestration boundary between Blazor UI components and Supabase backend. Services implement business logic, coordinate data operations, handle validation, and translate between database entities and application DTOs. The architecture follows Repository Pattern principles with Dependency Injection for loose coupling and testability. Data flows unidirectionally: Pages → Services → Supabase Client → PostgreSQL, with services returning DTOs to maintain separation of concerns and prevent tight coupling to database schemas.

---

## 2. Service Interfaces

### 2.1 IAuthService

Authentication and user session management interface.

```csharp
namespace MotoNomad.Application.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Registers a new user with email and password.
    /// </summary>
    /// <param name="command">Registration details (email, password, display name)</param>
    /// <returns>Authenticated user information</returns>
    /// <exception cref="ValidationException">Invalid email format or password too weak</exception>
    /// <exception cref="AuthException">Email already registered or Supabase error</exception>
    Task<UserDto> RegisterAsync(RegisterCommand command);

    /// <summary>
    /// Authenticates user with email and password credentials.
    /// </summary>
    /// <param name="command">Login credentials (email, password)</param>
    /// <returns>Authenticated user information with session token</returns>
    /// <exception cref="ValidationException">Missing email or password</exception>
    /// <exception cref="AuthException">Invalid credentials or account locked</exception>
    Task<UserDto> LoginAsync(LoginCommand command);

    /// <summary>
    /// Logs out current user and invalidates session.
    /// </summary>
    /// <exception cref="AuthException">Logout failed or session already expired</exception>
    Task LogoutAsync();

    /// <summary>
    /// Retrieves currently authenticated user information.
    /// </summary>
    /// <returns>Current user or null if not authenticated</returns>
    Task<UserDto?> GetCurrentUserAsync();

    /// <summary>
    /// Checks if user is currently authenticated with valid session.
    /// </summary>
    /// <returns>True if authenticated, false otherwise</returns>
    Task<bool> IsAuthenticatedAsync();

    /// <summary>
    /// Refreshes authentication session token.
    /// </summary>
    /// <exception cref="AuthException">Refresh failed or session expired</exception>
    Task RefreshSessionAsync();
}
```

### 2.2 ITripService

Trip management and business logic orchestration interface.

```csharp
namespace MotoNomad.Application.Interfaces;

public interface ITripService
{
    /// <summary>
    /// Retrieves all trips for currently authenticated user.
    /// </summary>
    /// <returns>List of trip summaries ordered by start date descending</returns>
    /// <exception cref="UnauthorizedException">User not authenticated</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<IEnumerable<TripListItemDto>> GetAllTripsAsync();

    /// <summary>
    /// Retrieves detailed information for specific trip including companions.
    /// </summary>
    /// <param name="tripId">Unique trip identifier</param>
    /// <returns>Complete trip details with companion list</returns>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Trip does not exist</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<TripDetailDto> GetTripByIdAsync(Guid tripId);

    /// <summary>
    /// Creates new trip with validation and automatic duration calculation.
    /// </summary>
    /// <param name="command">Trip creation details (name, dates, description, transport type)</param>
    /// <returns>Created trip with generated ID and calculated duration</returns>
    /// <exception cref="UnauthorizedException">User not authenticated</exception>
    /// <exception cref="ValidationException">End date not after start date or invalid transport type</exception>
    /// <exception cref="DatabaseException">Database insert failed</exception>
    Task<TripDetailDto> CreateTripAsync(CreateTripCommand command);

    /// <summary>
    /// Updates existing trip with validation.
    /// </summary>
    /// <param name="command">Trip update details including trip ID</param>
    /// <returns>Updated trip with recalculated duration</returns>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Trip does not exist</exception>
    /// <exception cref="ValidationException">Invalid date range or transport type</exception>
    /// <exception cref="DatabaseException">Database update failed</exception>
    Task<TripDetailDto> UpdateTripAsync(UpdateTripCommand command);

    /// <summary>
    /// Deletes trip and all associated companions via CASCADE.
    /// </summary>
    /// <param name="tripId">Unique trip identifier</param>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Trip does not exist</exception>
    /// <exception cref="DatabaseException">Database delete failed</exception>
    Task DeleteTripAsync(Guid tripId);

    /// <summary>
    /// Retrieves upcoming trips (start date >= today) for current user.
    /// </summary>
    /// <returns>List of upcoming trips ordered by start date ascending</returns>
    /// <exception cref="UnauthorizedException">User not authenticated</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<IEnumerable<TripListItemDto>> GetUpcomingTripsAsync();

    /// <summary>
    /// Retrieves past trips (start date < today) for current user.
    /// </summary>
    /// <returns>List of past trips ordered by start date descending</returns>
    /// <exception cref="UnauthorizedException">User not authenticated</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<IEnumerable<TripListItemDto>> GetPastTripsAsync();
}
```

### 2.3 ICompanionService

Companion management interface for trip participants.

```csharp
namespace MotoNomad.Application.Interfaces;

public interface ICompanionService
{
    /// <summary>
    /// Retrieves all companions for specific trip.
    /// </summary>
    /// <param name="tripId">Unique trip identifier</param>
    /// <returns>List of companions associated with trip</returns>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Trip does not exist</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<IEnumerable<CompanionListItemDto>> GetCompanionsByTripIdAsync(Guid tripId);

    /// <summary>
    /// Retrieves detailed information for specific companion.
    /// </summary>
    /// <param name="companionId">Unique companion identifier</param>
    /// <returns>Complete companion details</returns>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Companion does not exist</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<CompanionDto> GetCompanionByIdAsync(Guid companionId);

    /// <summary>
    /// Adds new companion to specific trip with validation.
    /// </summary>
    /// <param name="command">Companion details (trip ID, first name, last name, contact)</param>
    /// <returns>Created companion with generated ID</returns>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Trip does not exist</exception>
    /// <exception cref="ValidationException">Missing required fields or invalid contact format</exception>
    /// <exception cref="DatabaseException">Database insert failed</exception>
    Task<CompanionDto> AddCompanionAsync(AddCompanionCommand command);

    /// <summary>
    /// Updates existing companion information.
    /// </summary>
    /// <param name="command">Companion update details including companion ID</param>
    /// <returns>Updated companion information</returns>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Companion does not exist</exception>
    /// <exception cref="ValidationException">Invalid update data</exception>
    /// <exception cref="DatabaseException">Database update failed</exception>
    Task<CompanionDto> UpdateCompanionAsync(UpdateCompanionCommand command);

    /// <summary>
    /// Removes companion from trip.
    /// </summary>
    /// <param name="companionId">Unique companion identifier</param>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Companion does not exist</exception>
    /// <exception cref="DatabaseException">Database delete failed</exception>
    Task RemoveCompanionAsync(Guid companionId);

    /// <summary>
    /// Counts total companions for specific trip.
    /// </summary>
    /// <param name="tripId">Unique trip identifier</param>
    /// <returns>Number of companions associated with trip</returns>
    /// <exception cref="UnauthorizedException">User not authenticated or not trip owner</exception>
    /// <exception cref="NotFoundException">Trip does not exist</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<int> GetCompanionCountAsync(Guid tripId);
}
```

### 2.4 IProfileService

User profile management interface.

```csharp
namespace MotoNomad.Application.Interfaces;

public interface IProfileService
{
    /// <summary>
    /// Retrieves profile for currently authenticated user.
    /// </summary>
    /// <returns>Current user profile information</returns>
    /// <exception cref="UnauthorizedException">User not authenticated</exception>
    /// <exception cref="NotFoundException">Profile does not exist</exception>
    /// <exception cref="DatabaseException">Database query failed</exception>
    Task<ProfileDto> GetCurrentProfileAsync();

    /// <summary>
    /// Updates user profile with provided fields (partial update).
    /// Only non-null fields will be updated.
    /// </summary>
    /// <param name="command">Profile update command with optional DisplayName and AvatarUrl</param>
    /// <returns>Updated profile information</returns>
    /// <exception cref="UnauthorizedException">User not authenticated</exception>
    /// <exception cref="ValidationException">Invalid field values or no fields provided</exception>
    /// <exception cref="DatabaseException">Database update failed</exception>
    Task<ProfileDto> UpdateProfileAsync(UpdateProfileCommand command);
}
```

**Design Rationale:**
- Single `UpdateProfileAsync` method supports partial updates
- More flexible than separate methods for each field
- Reduces number of database operations (one update instead of multiple)
- Follows REST API best practices (PATCH semantics)

---

## 3. Dependency Injection Configuration

### 3.1 Service Registration

Register all services in `Program.cs` using scoped lifetime for Blazor WebAssembly:

```csharp
// Program.cs
using MotoNomad.Application.Interfaces;
using MotoNomad.Infrastructure.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Load Supabase configuration from appsettings.json
var supabaseSettings = new SupabaseSettings();
builder.Configuration.GetSection("Supabase").Bind(supabaseSettings);

// Register Supabase configuration
builder.Services.AddSingleton(supabaseSettings);

// Register Supabase client service as Singleton (shared state for authentication)
builder.Services.AddSingleton<ISupabaseClientService, SupabaseClientService>();

// Register application services as Scoped (isolated per request/circuit)
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITripService, TripService>();
builder.Services.AddScoped<ICompanionService, CompanionService>();
builder.Services.AddScoped<IProfileService, ProfileService>();

// Initialize Supabase client during startup
var app = builder.Build();
var supabaseClient = app.Services.GetRequiredService<ISupabaseClientService>();
await supabaseClient.InitializeAsync();

await builder.Build().RunAsync();
```

### 3.2 Configuration Structure

Required configuration in `wwwroot/appsettings.json`:

```json
{
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "Key": "your-anon-key"
  }
}
```

Development override in `appsettings.Development.json`:

```json
{
  "Supabase": {
    "Url": "https://dev-project.supabase.co",
    "Key": "dev-anon-key"
  }
}
```

### 3.3 Service Lifetime Rationale

**Supabase Client Service Lifetime:** Singleton: ISupabaseClientService is registered as Singleton because:
- Supabase client maintains connection state and authentication session
- Authentication tokens are shared across all requests for the same user
- Reduces overhead of creating multiple client instances
- Thread-safe for Blazor WebAssembly (single-threaded environment)

**Application Services Lifetime:** Scoped: All business logic services use Scoped lifetime because:
- Each Blazor circuit/session requires isolated state
- Logging contexts are request-specific
- Aligns with Blazor WebAssembly component lifecycle
- Prevents cross-user data leakage (although single-user in WASM)

---

## 4. Error Handling Strategy

### 4.1 Exception Types

Services throw typed exceptions defined in `Application/Exceptions/`:

- **ValidationException:** Business rule violations (invalid dates, missing required fields)
- **NotFoundException:** Requested entity does not exist
- **UnauthorizedException:** User not authenticated or lacks permission
- **AuthException:** Authentication/registration failures
- **DatabaseException:** Supabase query/command failures

### 4.2 Exception Propagation

Services do NOT catch exceptions - they propagate to Pages/Components for UI handling:

```csharp
// Page Example
try
{
    await TripService.CreateTripAsync(command);
    Snackbar.Add("Trip created successfully!", Severity.Success);
    NavigationManager.NavigateTo("/trips");
}
catch (ValidationException ex)
{
    Snackbar.Add(ex.Message, Severity.Warning);
}
catch (UnauthorizedException)
{
    Snackbar.Add("Please log in to continue", Severity.Error);
    NavigationManager.NavigateTo("/login");
}
catch (DatabaseException ex)
{
    Snackbar.Add("An error occurred. Please try again.", Severity.Error);
    Logger.LogError(ex, "Database error creating trip");
}
```

---

## 5. Service Implementation Guidelines

### 5.1 Authentication Context

All services except `IAuthService` must:
1. **Inject `ISupabaseClientService`** as constructor dependency
2. **Retrieve Supabase client** via `_supabaseClient.GetClient()`
3. **Get current user** via `client.Auth.CurrentUser`
4. **Throw `UnauthorizedException`** if user is null
5. **Use `user.Id`** as `user_id` for RLS policy enforcement

**Example Implementation:**

```csharp
public class TripService : ITripService
{
    private readonly ISupabaseClientService _supabaseClient;
    private readonly ILogger<TripService> _logger;

    public TripService(
        ISupabaseClientService supabaseClient,
        ILogger<TripService> logger)
    {
        _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private Guid GetCurrentUserId()
    {
        var client = _supabaseClient.GetClient();
        var currentUser = client.Auth.CurrentUser;

        if (currentUser == null)
        {
            throw new UnauthorizedException("You must be logged in to manage trips.");
        }

        return Guid.Parse(currentUser.Id);
    }

    public async Task<IEnumerable<TripListItemDto>> GetAllTripsAsync()
    {
        var userId = GetCurrentUserId(); // ← Authentication check
        
        // Use userId for RLS-filtered query
        var trips = await client
            .From<Trip>()
            .Where(t => t.UserId == userId)
            .Get();
        
        // ...
    }
}
```

**Key Points:**
- Always use `ISupabaseClientService`, never direct `Supabase.Client` injection
- Extract `GetCurrentUserId()` as private helper method for reuse
- Call authentication check at the start of every service method
- RLS policies in database provide additional authorization layer

### 5.2 Validation Order

Service methods validate in this sequence:
1. **Authentication:** Check user is logged in
2. **Input Validation:** Validate command/DTO fields
3. **Business Rules:** Check dates, constraints (e.g., end_date > start_date)
4. **Authorization:** Verify user owns resource (for update/delete)
5. **Database Operation:** Execute Supabase query/command

### 5.3 RLS Policy Enforcement

Services rely on Supabase RLS policies for authorization:
- RLS policies automatically filter queries by `auth.uid()` at database level
- If query returns empty result, throw `NotFoundException`
- Trust RLS to prevent unauthorized access for direct user-owned resources (Trips, Profiles)

**Exception: Manual Ownership Checks for Indirect Relationships**

While RLS policies handle authorization for direct user-owned tables, **manual ownership verification is required** for resources with indirect user relationships:

**When Manual Checks Are Needed:**
- **Companions → Trips → Users**: Companions don't have `user_id`, so RLS can't filter them directly
- **Better error messages**: Distinguish "Trip not found" from "Trip exists but you don't own it"
- **Performance optimization**: Prevent unnecessary queries before expensive operations

**Example: CompanionService.VerifyTripOwnership()**

```csharp
private async Task VerifyTripOwnership(Guid tripId, Guid userId)
{
    try
    {
        var client = _supabaseClient.GetClient();
        
        var trip = await client
            .From<Trip>()
            .Where(t => t.Id == tripId && t.UserId == userId)
            .Single();

        if (trip == null)
        {
            throw new NotFoundException("Trip", tripId);
        }
    }
    catch (NotFoundException)
    {
        throw;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to verify trip ownership for trip {TripId}", tripId);
        throw new DatabaseException("VerifyTripOwnership", "Failed to verify trip ownership.", ex);
    }
}

// Usage in companion operations
public async Task<CompanionDto> AddCompanionAsync(AddCompanionCommand command)
{
    var userId = GetCurrentUserId();
    ValidateAddCompanionCommand(command);
    
    // Manual check required: companions don't have user_id
    await VerifyTripOwnership(command.TripId, userId);
    
    // Proceed with companion creation...
}
```

**Authorization Strategy Summary:**
- ✅ **Direct relationships** (Trips, Profiles): Rely on RLS policies
- ✅ **Indirect relationships** (Companions): Manual ownership checks
- ✅ **Combine both**: RLS prevents SQL injection, manual checks provide better UX

### 5.4 DTO Mapping

Services translate between entities and DTOs:
- **Input:** Commands → Entities (for INSERT/UPDATE)
- **Output:** Entities → DTOs (for SELECT results)
- Mapping logic lives in service implementations
- Consider using AutoMapper or Mapster for complex mappings

### 5.5 Date Handling

- PostgreSQL stores `DATE` type (no timezone)
- C# uses `DateOnly` (.NET 6+) for date-only values
- `TIMESTAMPTZ` maps to `DateTime` in UTC
- Services must handle timezone conversions if needed

---

## 6. Performance Considerations

### 6.1 Query Optimization

Services should:
- Use `SELECT` with specific columns (avoid `SELECT *`)
- Leverage indexes on `user_id`, `start_date`, `trip_id`
- Use JOIN for trip + companions in single query where possible
- Implement pagination for large result sets (future)

### 6.2 Caching Strategy

MVP does NOT implement caching:
- Every operation queries Supabase directly
- Future: Consider client-side cache for trip list (IndexedDB)
- Future: Implement cache invalidation on mutations

### 6.3 Concurrency

Blazor WebAssembly is single-threaded:
- No locking mechanisms required
- Use `await` for all async operations
- Supabase handles server-side concurrency

---

## 7. Security Considerations

### 7.1 Sensitive Data

Services MUST NOT:
- Log passwords or authentication tokens
- Return sensitive data in DTOs (e.g., password hashes)
- Expose internal database IDs in URLs (use UUIDs)

### 7.2 Input Sanitization

All user input validated before database operations:
- Trim whitespace from text fields
- Validate email format with regex
- Enforce length limits (name, description)
- Reject SQL injection attempts (Supabase client handles)

### 7.3 RLS Bypass Prevention

Services cannot bypass RLS policies:
- All queries execute with user JWT token
- Service account credentials NOT stored in WebAssembly
- Admin operations require separate server-side API (future)

---

## 8. Implementation Checklist

Before implementing services, ensure:

- [ ] Database schema deployed to Supabase (initial migration)
- [ ] RLS policies tested manually in Supabase SQL editor
- [ ] Supabase C# client configured in `Program.cs`
- [ ] Exception classes created in `Application/Exceptions/`
- [ ] DTOs and Commands defined (see `api-contracts.md`)
- [ ] Service interfaces copied to `Application/Interfaces/`

---

**Document ready for implementation** ✅  
**Project**: MotoNomad MVP  
**Program**: 10xDevs  
**Date**: October 2025  
**Certification deadline**: November 2025
