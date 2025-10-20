# Backend Implementation Roadmap - MotoNomad Services

## 📋 Overview

This roadmap guides the implementation of all backend services for MotoNomad MVP. The services follow a layered architecture with clear separation of concerns:

- **Infrastructure Layer**: Service implementations, database access
- **Application Layer**: Interfaces, DTOs, Commands, Exceptions
- **Presentation Layer**: Blazor components (separate implementation)

---

## 🎯 Service Summary

### 1. AuthService
**Purpose**: User authentication and session management  
**Methods**: RegisterAsync, LoginAsync, LogoutAsync, GetCurrentUserAsync, IsAuthenticatedAsync, RefreshSessionAsync  
**Dependencies**: ISupabaseClientService, ILogger<AuthService>  
**Priority**: ⭐⭐⭐ Critical (blocking all other services)

### 2. TripService
**Purpose**: Trip CRUD operations with business logic  
**Methods**: GetAllTripsAsync, GetTripByIdAsync, CreateTripAsync, UpdateTripAsync, DeleteTripAsync, GetUpcomingTripsAsync, GetPastTripsAsync  
**Dependencies**: ISupabaseClientService, ILogger<TripService>  
**Priority**: ⭐⭐⭐ Critical (core functionality)

### 3. CompanionService
**Purpose**: Companion management with ownership verification  
**Methods**: GetCompanionsByTripIdAsync, GetCompanionByIdAsync, AddCompanionAsync, UpdateCompanionAsync, RemoveCompanionAsync, GetCompanionCountAsync  
**Dependencies**: ISupabaseClientService, ILogger<CompanionService>  
**Priority**: ⭐⭐ High (core functionality)

### 4. ProfileService
**Purpose**: User profile management with partial updates  
**Methods**: GetCurrentProfileAsync, UpdateProfileAsync  
**Dependencies**: ISupabaseClientService, ILogger<ProfileService>  
**Priority**: ⭐ Medium (nice-to-have for MVP)

---

## 🗂️ Implementation Order

### Phase 1: Foundation 

#### Step 1.1: Create Shared Infrastructure
**Location**: `MotoNomad.App/Application/Exceptions/`

Create all exception classes:
- ✅ ValidationException.cs
- ✅ NotFoundException.cs
- ✅ UnauthorizedException.cs
- ✅ AuthException.cs
- ✅ DatabaseException.cs

**Acceptance Criteria**:
- All exception classes inherit from Exception
- Constructors accept message and optional inner exception
- No additional logic required

---

#### Step 1.2: Create Database Entities 
**Location**: `MotoNomad.App/Infrastructure/Database/Entities/`

Entities to create:
- ✅ TransportType.cs (enum)
- ✅ Profile.cs
- ✅ Trip.cs
- ✅ Companion.cs

**Acceptance Criteria**:
- All entities inherit from `Postgrest.Models.BaseModel`
- Proper `[Table]`, `[PrimaryKey]`, `[Column]` attributes
- Snake_case column names match Supabase schema
- Navigation properties where appropriate

---

#### Step 1.3: Deploy Database Schema 
**Location**: Database migrations in Supabase

Execute migrations in order:
1. ✅ 001_initial_schema.sql
2. ✅ 002_add_rls_policies.sql
3. ✅ 003_add_triggers.sql
4. ⚠️ dev_seed.sql (development only)

**Acceptance Criteria**:
- All tables created successfully
- RLS policies enabled and tested
- Triggers functioning correctly
- Test data inserted (dev environment)

---

### Phase 2: Authentication 

#### Step 2.1: Create Auth DTOs and Commands
**Locations**:
- `MotoNomad.App/Application/DTOs/Auth/UserDto.cs`
- `MotoNomad.App/Application/Commands/Auth/RegisterCommand.cs`
- `MotoNomad.App/Application/Commands/Auth/LoginCommand.cs`

**Acceptance Criteria**:
- All DTOs use `record` type with `required` and `init` accessors
- Commands include all necessary fields
- XML documentation for all public members

---

#### Step 2.2: Implement AuthService 
**Locations**:
- `MotoNomad.App/Application/Interfaces/IAuthService.cs`
- `MotoNomad.App/Infrastructure/Services/AuthService.cs`

**Acceptance Criteria**:
- All 6 methods implemented
- Input validation for all commands
- Proper error handling with typed exceptions
- JWT token storage via Blazored.LocalStorage
- Profile creation/retrieval after registration/login

---

#### Step 2.3: Register AuthService 
**Location**: `MotoNomad.App/Program.cs`

```csharp
builder.Services.AddScoped<IAuthService, AuthService>();
```

**Acceptance Criteria**:
- Service registered as Scoped
- DI container resolves service correctly
- No circular dependencies

---

#### Step 2.4: Test AuthService
**Location**: `MotoNomad.Tests/Unit/Services/AuthServiceTests.cs`

Minimum test coverage:
- ✅ RegisterAsync with valid command
- ✅ RegisterAsync with invalid email
- ✅ RegisterAsync with weak password
- ✅ LoginAsync with valid credentials
- ✅ LoginAsync with invalid credentials
- ✅ LogoutAsync successful
- ✅ GetCurrentUserAsync when authenticated
- ✅ IsAuthenticatedAsync with valid session

**Acceptance Criteria**:
- All tests pass (green)
- Code coverage > 80%
- Integration test against real Supabase instance

---

### Phase 3: Trip Management

#### Step 3.1: Create Trip DTOs and Commands 
**Locations**:
- `MotoNomad.App/Application/DTOs/Trips/`
- `MotoNomad.App/Application/Commands/Trips/`

Files to create:
- TripListItemDto.cs
- TripDetailDto.cs
- CreateTripCommand.cs
- UpdateTripCommand.cs
- DeleteTripCommand.cs

---

#### Step 3.2: Implement TripService 
**Locations**:
- `MotoNomad.App/Application/Interfaces/ITripService.cs`
- `MotoNomad.App/Infrastructure/Services/TripService.cs`

**Acceptance Criteria**:
- All 7 methods implemented
- Business logic validation (EndDate > StartDate)
- RLS verification for all operations
- Parallel queries with Task.WhenAll (GetTripByIdAsync)
- Companion count integration

---

#### Step 3.3: Register TripService
**Location**: `MotoNomad.App/Program.cs`

```csharp
builder.Services.AddScoped<ITripService, TripService>();
```

---

#### Step 3.4: Test TripService 
**Location**: `MotoNomad.Tests/Unit/Services/TripServiceTests.cs`

Minimum test coverage:
- ✅ CreateTripAsync with valid command
- ✅ CreateTripAsync with invalid date range
- ✅ UpdateTripAsync with valid command
- ✅ DeleteTripAsync cascades companions
- ✅ GetTripByIdAsync with non-existent trip
- ✅ GetUpcomingTripsAsync filters correctly
- ✅ GetPastTripsAsync filters correctly

---

### Phase 4: Companion Management 

#### Step 4.1: Create Companion DTOs and Commands 
**Locations**:
- `MotoNomad.App/Application/DTOs/Companions/`
- `MotoNomad.App/Application/Commands/Companions/`

Files to create:
- CompanionListItemDto.cs
- CompanionDto.cs
- AddCompanionCommand.cs
- UpdateCompanionCommand.cs
- RemoveCompanionCommand.cs

---

#### Step 4.2: Implement CompanionService 
**Locations**:
- `MotoNomad.App/Application/Interfaces/ICompanionService.cs`
- `MotoNomad.App/Infrastructure/Services/CompanionService.cs`

**Acceptance Criteria**:
- All 6 methods implemented
- Trip ownership verification for all operations
- Proper validation (FirstName, LastName, Contact)
- GetCompanionCountAsync for TripService integration

---

#### Step 4.3: Register CompanionService 
**Location**: `MotoNomad.App/Program.cs`

```csharp
builder.Services.AddScoped<ICompanionService, CompanionService>();
```

---

#### Step 4.4: Test CompanionService 
**Location**: `MotoNomad.Tests/Unit/Services/CompanionServiceTests.cs`

Minimum test coverage:
- ✅ AddCompanionAsync with valid command
- ✅ AddCompanionAsync with invalid first name
- ✅ AddCompanionAsync to non-existent trip
- ✅ UpdateCompanionAsync with valid command
- ✅ RemoveCompanionAsync successful
- ✅ GetCompanionCountAsync accurate

---

### Phase 5: Profile Management 

#### Step 5.1: Create Profile DTOs and Commands 
**Locations**:
- `MotoNomad.App/Application/DTOs/Profiles/ProfileDto.cs`
- `MotoNomad.App/Application/Commands/Profiles/UpdateProfileCommand.cs`

---

#### Step 5.2: Implement ProfileService 
**Locations**:
- `MotoNomad.App/Application/Interfaces/IProfileService.cs`
- `MotoNomad.App/Infrastructure/Services/ProfileService.cs`

**Acceptance Criteria**:
- Both methods implemented
- Partial update support (only provided fields)
- URL validation for AvatarUrl
- Profile caching consideration

---

#### Step 5.3: Register ProfileService 
**Location**: `MotoNomad.App/Program.cs`

```csharp
builder.Services.AddScoped<IProfileService, ProfileService>();
```

---

#### Step 5.4: Test ProfileService 
**Location**: `MotoNomad.Tests/Unit/Services/ProfileServiceTests.cs`

Minimum test coverage:
- ✅ GetCurrentProfileAsync returns profile
- ✅ UpdateProfileAsync with DisplayName only
- ✅ UpdateProfileAsync with AvatarUrl only
- ✅ UpdateProfileAsync with both fields
- ✅ UpdateProfileAsync with no fields throws exception

---

## 🧪 Testing Strategy

### Unit Testing
- **Framework**: xUnit
- **Mocking**: Moq for Supabase client
- **Coverage Goal**: > 80% for all services
- **Run Frequency**: On every commit (CI/CD)

### Integration Testing
- **Environment**: Development Supabase instance
- **Scope**: Full CRUD operations with real database
- **Test Data**: Automatically seeded and cleaned up
- **Run Frequency**: Before merging to main

### E2E Testing (Future)
- **Framework**: Playwright for .NET
- **Scope**: Full user flows (registration → create trip → add companions)
- **Run Frequency**: Before release

---

## 📊 Progress Tracking

### Definition of Done (Per Service)

✅ **Completed** when ALL criteria met:
1. Interface defined in Application/Interfaces/
2. Implementation in Infrastructure/Services/
3. All methods implemented with error handling
4. DTOs and Commands created
5. Service registered in Program.cs
6. Unit tests written with > 80% coverage
7. Integration tests pass against Supabase
8. Code reviewed and merged

---

## 🚨 Critical Dependencies

### Service Dependencies Graph

```
AuthService (no dependencies)
    ↓
ProfileService → IAuthService
    ↓
TripService → IAuthService + ICompanionService
    ↓
CompanionService → IAuthService
```

**Key Points:**
- **No inter-service dependencies**: Services do NOT depend on each other
- **Authentication via ISupabaseClientService**: All services use `_supabaseClient.GetClient().Auth.CurrentUser`
- **Independent implementations**: Services can be implemented in parallel without blocking
- **Shared Supabase client**: Singleton lifetime ensures consistent authentication state

**Implementation Order (Recommended):**
1. **ISupabaseClientService** - Foundation for all services
2. **AuthService** - User authentication (no dependencies)
3. **ProfileService** - User profiles (independent)
4. **CompanionService** - Companion management (independent)
5. **TripService** - Trip management (queries companion counts via DB, not via DI)

**Note**: While TripService queries companion counts, it does so directly via Supabase queries, NOT by injecting ICompanionService. This maintains loose coupling and allows parallel development.

---

## ⚠️ Known Risks and Mitigation

### Risk 1: RLS Policy Issues
**Impact**: High - Users could access other users' data  
**Probability**: Medium  
**Mitigation**: Thorough testing of RLS policies in Supabase dashboard before implementation

### Risk 2: Circular Dependencies
**Impact**: High - Services cannot be instantiated  
**Probability**: Low  
**Mitigation**: Follow dependency graph, use interfaces, avoid circular references

### Risk 3: Supabase Client Configuration
**Impact**: Medium - Services cannot connect to database  
**Probability**: Low  
**Mitigation**: Verify Supabase URL and key in appsettings.json, test connection early

### Risk 4: DateTime/DateOnly Mapping
**Impact**: Medium - Date fields may not map correctly  
**Probability**: Medium  
**Mitigation**: Test DateOnly <-> PostgreSQL DATE mapping, verify in integration tests

---

## 📚 Additional Resources

### Documentation
- [Supabase C# Client Docs](https://supabase.com/docs/reference/csharp/introduction)
- [Postgrest C# Docs](https://github.com/supabase-community/postgrest-csharp)
- [MotoNomad PRD](../prd.md)
- [Database Schema](../db-plan.md)
- [API Contracts](../api-contracts.md)

### Individual Service Plans
- [auth-service-implementation-plan.md](./auth-service-implementation-plan.md)
- [trip-service-implementation-plan.md](./trip-service-implementation-plan.md)
- [companion-service-implementation-plan.md](./companion-service-implementation-plan.md)
- [profile-service-implementation-plan.md](./profile-service-implementation-plan.md)

---

## ✅ Final Checklist

Before declaring implementation complete:

- [ ] All 4 services implemented
- [ ] All unit tests passing (> 80% coverage)
- [ ] All integration tests passing
- [ ] Services registered in Program.cs
- [ ] Database migrations deployed to Supabase
- [ ] RLS policies tested and verified
- [ ] No console errors or warnings
- [ ] Code reviewed by team
- [ ] Documentation updated

---

**Roadmap Status**: ✅ Ready for Implementation  
**Project**: MotoNomad MVP  
**Program**: 10xDevs