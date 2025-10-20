# Service Implementation Plan: SupabaseClientService

## 1. Service Overview

**Purpose**: Provide centralized abstraction layer for Supabase client management and lifecycle control.

**Functionality**:
- Initialize Supabase client connection during application startup
- Provide thread-safe access to initialized client instance
- Validate configuration settings
- Manage client lifecycle (initialization state)
- Enable testability through interface abstraction

**Service Location**: `MotoNomad.App/Infrastructure/Services/SupabaseClientService.cs`

**Interface**: `ISupabaseClientService` in `MotoNomad.App/Application/Interfaces/ISupabaseClientService.cs`

**Key Dependencies**:
- SupabaseSettings - Configuration model for URL and API keys
- ILogger<SupabaseClientService> - Logging for initialization operations
- Supabase.Client (supabase-csharp v0.16.2) - Underlying Supabase SDK client

**Service Lifetime**: **Singleton** - Single shared instance across entire application

**Rationale for Singleton**:
- Shared authentication state across all services
- Single connection pool for database operations
- Consistent session management
- Blazor WebAssembly is single-threaded (no concurrency issues)
- Reduced memory overhead

---

## 2. Method Details

### 2.1 InitializeAsync

- **Method**: `Task InitializeAsync()`
- **Parameters**: None
- **Purpose**: Initialize Supabase client connection and verify connectivity
- **When to Call**: During application startup in `Program.cs` before `app.RunAsync()`
- **Behavior**: 
  - Idempotent (safe to call multiple times)
  - Logs warning if already initialized
  - Throws exception on connection failure

### 2.2 GetClient

- **Method**: `Client GetClient()`
- **Parameters**: None
- **Returns**: Initialized `Supabase.Client` instance
- **Purpose**: Provide access to Supabase client for database operations
- **Throws**: `InvalidOperationException` if client not initialized

### 2.3 IsInitialized

- **Property**: `bool IsInitialized { get; }`
- **Purpose**: Check if client has been successfully initialized
- **Returns**: 
  - `true` if `InitializeAsync()` completed successfully
  - `false` if not yet initialized

---

## 3. Utilized Types

### Configuration:
- **SupabaseSettings**: `MotoNomad.App/Infrastructure/Configuration/SupabaseSettings.cs`
  - Properties: `Url` (string), `AnonKey` (string)
  - Method: `IsValid()` - validates non-empty values

### External SDK:
- **Supabase.Client**: From `supabase-csharp` NuGet package
- **SupabaseOptions**: Configuration options for client
  - `AutoConnectRealtime` (bool): Enable/disable realtime subscriptions
  - `AutoRefreshToken` (bool): Automatic JWT token refresh

### Logging:
- **ILogger<SupabaseClientService>**: For initialization logging

---

## 4. Response Details

### InitializeAsync Response:
- **Success**: Task completes, `IsInitialized` becomes `true`
- **Failure**: Throws exception with error details

### GetClient Response:
```csharp
Supabase.Client {
    Auth = Supabase.Gotrue.Client,
    From<T>() = Query builder for database operations,
    Rpc() = Remote procedure call interface,
    Storage = File storage interface
}
```

### IsInitialized Response:
- **Before Initialization**: `false`
- **After Successful Initialization**: `true`
- **After Failed Initialization**: `false`

---

## 5. Data Flow

### Initialization Flow:
```
1. Application Startup (Program.cs)
   ↓
2. Load SupabaseSettings from appsettings.json
   ↓
3. Register SupabaseClientService as Singleton in DI container
   ↓
4. Build application (app = builder.Build())
   ↓
5. Resolve ISupabaseClientService from DI
   ↓
6. Call supabaseClient.InitializeAsync()
   ↓ (validates settings, creates client, calls client.InitializeAsync())
   ↓
7. Set _isInitialized = true
   ↓
8. Application Ready (app.RunAsync())
```

### Service Usage Flow:
```
1. Service constructor receives ISupabaseClientService via DI
   ↓
2. Service method calls _supabaseClient.GetClient()
   ↓
3. GetClient() validates _isInitialized flag
   ↓ (throws if false)
   ↓
4. Returns initialized Supabase.Client instance
   ↓
5. Service performs database operation
```

---

## 6. Configuration Requirements

### Configuration Files:

**Production**: `wwwroot/appsettings.json`
```json
{
  "Supabase": {
    "Url": "https://your-project.supabase.co",
    "AnonKey": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

**Development**: `wwwroot/appsettings.Development.json`
```json
{
  "Supabase": {
    "Url": "http://127.0.0.1:54321",
    "AnonKey": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
  }
}
```

### Required NuGet Packages:
- `supabase-csharp` v0.16.2
- `Blazored.LocalStorage` v4.5.0
- `Microsoft.Extensions.Configuration.Binder` v9.0.10

### Dependency Injection Configuration:
```csharp
// Load configuration
var supabaseSettings = new SupabaseSettings();
builder.Configuration.GetSection("Supabase").Bind(supabaseSettings);

// Register configuration
builder.Services.AddSingleton(supabaseSettings);

// Register client service
builder.Services.AddSingleton<ISupabaseClientService, SupabaseClientService>();

// Register LocalStorage
builder.Services.AddBlazoredLocalStorage();
```

### Supabase Client Options:
```csharp
var options = new SupabaseOptions
{
    AutoConnectRealtime = true,   // Enable realtime subscriptions
    AutoRefreshToken = true        // Automatic JWT token refresh
};
```

---

## 7. Error Handling

### ArgumentException:
- **When**: Invalid SupabaseSettings during construction
- **Message**: "Invalid Supabase configuration"
- **Cause**: Empty Url or AnonKey in settings

### InvalidOperationException:
- **When**: `GetClient()` called before initialization
- **Message**: "Supabase client is not initialized. Call InitializeAsync() first."
- **Cause**: `InitializeAsync()` not called or failed

### Exception (during InitializeAsync):
- **When**: Connection to Supabase fails
- **Logged**: Full exception details with stack trace
- **Action**: Application startup fails, exception propagates

### Validation Checks:
1. **Constructor**: Validates `SupabaseSettings.IsValid()`
2. **GetClient**: Validates `_isInitialized` flag
3. **InitializeAsync**: Wraps `client.InitializeAsync()` in try-catch

---

## 8. Performance Considerations

**Singleton Benefits**:
- Single client instance (no repeated initialization overhead)
- Shared connection pool across all services
- Persistent authentication state
- Memory efficient (one instance vs. multiple)

**Initialization**:
- One-time cost during application startup
- Parallel with other startup tasks (async)
- Does not block UI thread (Blazor WebAssembly)

**Runtime Performance**:
- GetClient() is O(1) operation (simple property access)
- No locking required (single-threaded Blazor WebAssembly)
- No additional overhead vs. direct Supabase.Client usage

**Performance Metrics**:
- Initialization: < 2 seconds (network dependent)
- GetClient() call: < 1ms (in-memory)
- Memory overhead: ~1MB (single client instance)

---

## 9. Implementation Steps

### Step 1: Create Configuration Model
**File**: `MotoNomad.App/Infrastructure/Configuration/SupabaseSettings.cs`
- Create `SupabaseSettings` class
- Add `Url` and `AnonKey` properties
- Implement `IsValid()` validation method

### Step 2: Create Service Interface
**File**: `MotoNomad.App/Application/Interfaces/ISupabaseClientService.cs`
- Define `ISupabaseClientService` interface
- Add `InitializeAsync()` method
- Add `GetClient()` method
- Add `IsInitialized` property

### Step 3: Implement Service
**File**: `MotoNomad.App/Infrastructure/Services/SupabaseClientService.cs`
- Implement `ISupabaseClientService` interface
- Add constructor with SupabaseSettings and ILogger
- Validate settings in constructor
- Create Supabase.Client with options
- Implement `InitializeAsync()` with error handling
- Implement `GetClient()` with validation
- Add logging for all operations

### Step 4: Configure Dependency Injection
**File**: `MotoNomad.App/Program.cs`
- Load SupabaseSettings from configuration
- Register SupabaseSettings as Singleton
- Register ISupabaseClientService as Singleton
- Add Blazored.LocalStorage
- Call `InitializeAsync()` before `app.RunAsync()`
- Add try-catch with logging

### Step 5: Create Configuration Files
**Files**: 
- `wwwroot/appsettings.json`
- `wwwroot/appsettings.Development.json`
- Add Supabase section with Url and AnonKey

### Step 6: Create Database Entities
**Location**: `MotoNomad.App/Infrastructure/Database/Entities/`
- Create `Trip.cs`, `Companion.cs`, `Profile.cs`, `TransportType.cs`
- Add `[Table]`, `[PrimaryKey]`, `[Column]` attributes
- Inherit from `BaseModel`

### Step 7: Write Unit Tests
**File**: `MotoNomad.Tests/Unit/Services/SupabaseClientServiceTests.cs`
- Test constructor validation
- Test `GetClient()` before initialization (should throw)
- Test `InitializeAsync()` success
- Test `IsInitialized` property
- Mock ILogger for tests

### Step 8: Write Integration Tests
**File**: `MotoNomad.Tests/Integration/SupabaseClientServiceIntegrationTests.cs`
- Test initialization with real Supabase instance
- Test database connectivity
- Test authentication flow
- Use test Supabase project

### Step 9: Create Health Check Page
**File**: `MotoNomad.App/Pages/Health.razor`
- Add client initialization test
- Add database connectivity test
- Add table access test
- Add authentication status test
- Add troubleshooting tips

### Step 10: Documentation
- Update `.ai/supabase-client-service-implementation-plan.md`
- Create usage examples for other services
- Document common patterns
- Add troubleshooting guide

---

## 10. Testing Strategy

### Unit Tests:
```csharp
[Fact]
public void Constructor_ThrowsArgumentException_WhenSettingsInvalid()
{
    // Arrange
    var invalidSettings = new SupabaseSettings { Url = "", AnonKey = "" };
    var logger = Mock.Of<ILogger<SupabaseClientService>>();
    
    // Act & Assert
    Assert.Throws<ArgumentException>(() => 
        new SupabaseClientService(invalidSettings, logger));
}

[Fact]
public void GetClient_ThrowsInvalidOperationException_WhenNotInitialized()
{
    // Arrange
    var settings = new SupabaseSettings 
    { 
        Url = "http://test.supabase.co", 
        AnonKey = "test-key" 
    };
    var logger = Mock.Of<ILogger<SupabaseClientService>>();
    var service = new SupabaseClientService(settings, logger);
    
    // Act & Assert
    Assert.Throws<InvalidOperationException>(() => service.GetClient());
}

[Fact]
public async Task InitializeAsync_SetsIsInitialized_WhenSuccessful()
{
    // Arrange
    var settings = new SupabaseSettings 
    { 
        Url = "http://test.supabase.co", 
        AnonKey = "test-key" 
    };
    var logger = Mock.Of<ILogger<SupabaseClientService>>();
    var service = new SupabaseClientService(settings, logger);
    
    // Act
    await service.InitializeAsync();
    
    // Assert
    Assert.True(service.IsInitialized);
    Assert.NotNull(service.GetClient());
}
```

### Integration Tests:
```csharp
[Fact]
public async Task InitializeAsync_ConnectsToRealSupabase_Successfully()
{
    // Arrange - use test Supabase project
    var settings = new SupabaseSettings
    {
        Url = "http://127.0.0.1:54321",
        AnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    };
    var logger = Mock.Of<ILogger<SupabaseClientService>>();
    var service = new SupabaseClientService(settings, logger);
    
    // Act
    await service.InitializeAsync();
    var client = service.GetClient();
    
    // Assert
    Assert.NotNull(client);
    Assert.NotNull(client.Auth);
    
    // Test database connectivity
    var response = await client.From<Trip>().Get();
    Assert.NotNull(response);
}
```

---

## 11. Design Decisions

### Why Interface Abstraction?

**Benefits**:
1. **Testability**: Easy to mock for unit tests
2. **Initialization Control**: Explicit `InitializeAsync()` prevents race conditions
3. **Encapsulation**: Services don't depend on concrete Supabase.Client
4. **SOLID Principles**: Dependency Inversion (depend on abstractions)
5. **Future-Proof**: Easy to swap implementation or upgrade SDK

### Why Singleton Lifetime?

**Rationale**:
1. **Shared Authentication**: All services need same auth session
2. **Connection Pool**: Single connection for efficiency
3. **Performance**: Avoid repeated initialization
4. **Thread Safety**: Blazor WebAssembly is single-threaded
5. **State Consistency**: Same configuration across all services

### Why Separate Configuration Model?

**Rationale**:
1. **Validation**: Centralized settings validation
2. **Type Safety**: Strongly-typed configuration
3. **Testability**: Easy to create test configurations
4. **Flexibility**: Easy to add new settings

---

## 12. Common Patterns for Dependent Services

All services using `ISupabaseClientService` should follow this pattern:

```csharp
public class ExampleService : IExampleService
{
    private readonly ISupabaseClientService _supabaseClient;
    private readonly ILogger<ExampleService> _logger;

    public ExampleService(
        ISupabaseClientService supabaseClient,
        ILogger<ExampleService> logger)
    {
        _supabaseClient = supabaseClient;
        _logger = logger;
    }

    public async Task<Result> PerformOperationAsync()
    {
        try
        {
            var client = _supabaseClient.GetClient();
            
            // Use client for database operations
            var response = await client.From<Entity>().Get();
            
            return ProcessResponse(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Supabase client not initialized");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database operation failed");
            throw new DatabaseException("Operation failed", ex);
        }
    }
}
```

**Registration Pattern**:
```csharp
builder.Services.AddScoped<IExampleService, ExampleService>();
```

---

**Status**: ✅ Ready for Implementation  
