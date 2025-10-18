# Blazor WebAssembly Architecture - MotoNomad

**Project:** MotoNomad MVP  
**Program:** 10xDevs  
**Date:** October 2025  
**Status:** Ready for Implementation

---

## 📁 Project Structure
```
MotoNomad/
├─ MotoNomad.App/
│  ├─ Infrastructure/
│  │  ├─ Database/
│  │  │  ├─ Entities/
│  │  │  │  ├─ Trip.cs
│  │  │  │  ├─ Companion.cs
│  │  │  │  ├─ Profile.cs
│  │  │  │  └─ TransportType.cs
│  │  │  └─ supabase/
│  │  │     └─ migrations/
│  │  │        ├─ 001_initial_schema.sql
│  │  │        ├─ 002_add_rls_policies.sql
│  │  │        ├─ 003_add_triggers.sql
│  │  │        └─ dev_seed.sql
│  │  ├─ Services/
│  │  │  ├─ TripService.cs
│  │  │  ├─ CompanionService.cs
│  │  │  ├─ AuthService.cs
│  │  │  ├─ ProfileService.cs
│  │  │  └─ SupabaseClientService.cs
│  │  └─ Configuration/
│  │     └─ SupabaseSettings.cs
│  ├─ Application/
│  │  ├─ Interfaces/
│  │  │  ├─ ITripService.cs
│  │  │  ├─ ICompanionService.cs
│  │  │  ├─ IAuthService.cs
│  │  │  ├─ IProfileService.cs
│  │  │  └─ ISupabaseClientService.cs
│  │  ├─ DTOs/
│  │  │  ├─ Trips/
│  │  │  │  ├─ TripListItemDto.cs
│  │  │  │  └─ TripDetailDto.cs
│  │  │  ├─ Companions/
│  │  │  │  ├─ CompanionDto.cs
│  │  │  │  └─ CompanionListItemDto.cs
│  │  │  ├─ Profiles/
│  │  │  │  └─ ProfileDto.cs
│  │  │  └─ Auth/
│  │  │     └─ UserDto.cs
│  │  ├─ Commands/
│  │  │  ├─ Trips/
│  │  │  │  ├─ CreateTripCommand.cs
│  │  │  │  ├─ UpdateTripCommand.cs
│  │  │  │  └─ DeleteTripCommand.cs
│  │  │  ├─ Companions/
│  │  │  │  ├─ AddCompanionCommand.cs
│  │  │  │  ├─ UpdateCompanionCommand.cs
│  │  │  │  └─ RemoveCompanionCommand.cs
│  │  │  ├─ Profiles/
│  │  │  │  ├─ UpdateDisplayNameCommand.cs
│  │  │  │  └─ UpdateAvatarUrlCommand.cs
│  │  │  └─ Auth/
│  │  │     ├─ RegisterCommand.cs
│  │  │     └─ LoginCommand.cs
│  │  └─ Exceptions/
│  │     ├─ ValidationException.cs
│  │     ├─ NotFoundException.cs
│  │     ├─ UnauthorizedException.cs
│  │     ├─ AuthException.cs
│  │     └─ DatabaseException.cs
│  ├─ Pages/
│  │  ├─ Index.razor
│  │  ├─ Login.razor
│  │  ├─ Register.razor
│  │  ├─ Trips/
│  │  │  ├─ TripList.razor
│  │  │  ├─ TripDetails.razor
│  │  │  ├─ CreateTrip.razor
│  │  │  └─ EditTrip.razor
│  │  └─ Profiles/
│  │     ├─ UserProfile.razor
│  │     └─ AccountSettings.razor
│  ├─ Layout/
│  │  ├─ MainLayout.razor
│  │  └─ NavMenu.razor
│  ├─ Shared/
│  │  ├─ LoginDisplay.razor
│  │  ├─ Components/
│  │  │  ├─ TripCard.razor
│  │  │  ├─ CompanionList.razor
│  │  │  ├─ DateRangePicker.razor
│  │  │  └─ LoadingSpinner.razor
│  │  └─ Dialogs/
│  │     ├─ ConfirmDialog.razor
│  │     └─ TripFormDialog.razor
│  ├─ wwwroot/
│  │  ├─ css/
│  │  │  ├─ app.css
│  │  │  └─ open-iconic/
│  │  ├─ images/
│  │  │  ├─ logo.png
│  │  │  └─ favicon.ico
│  │  ├─ appsettings.json
│  │  └─ index.html
│  ├─ Properties/
│  │  └─ launchSettings.json
│  ├─ _Imports.razor
│  ├─ App.razor
│  ├─ Program.cs
│  ├─ appsettings.Development.json
│  └─ MotoNomad.App.csproj
├─ MotoNomad.Tests/
│  ├─ Unit/
│  │  ├─ Services/
│  │  │  ├─ TripServiceTests.cs
│  │  │  ├─ CompanionServiceTests.cs
│  │  │  ├─ AuthServiceTests.cs
│  │  │  └─ ProfileServiceTests.cs
│  │  └─ Validators/
│  │     └─ CommandValidatorTests.cs
│  ├─ Integration/
│  │  ├─ TripServiceIntegrationTests.cs
│  │  ├─ CompanionServiceIntegrationTests.cs
│  │  └─ AuthServiceIntegrationTests.cs
│  └─ E2E/
│     ├─ LoginFlowTests.cs
│     └─ TripManagementFlowTests.cs
├─ .ai/
│  ├─ prd.md
│  ├─ db-plan.md
│  ├─ entities-plan.md
│  ├─ services-plan.md
│  ├─ api-contracts.md
│  ├─ wasm-arch.md
│  ├─ tech-stack.md
│  ├─ configuration-guide.md
│  └─ blazor-gh-pages-guide.md
├─ .github/
│  ├─ workflows/
│  │  └─ deploy.yml
│  └─ copilot-instructions.md
├─ .gitignore
├─ LICENSE
└─ README.md
```

---

## 📋 Implementation Status

### ✅ Completed Components

#### Infrastructure Layer
- ✅ **Database Entities**
  - Trip.cs
  - Companion.cs
  - Profile.cs
  - TransportType.cs (enum)

- ✅ **Services Implementation**
  - AuthService.cs - User authentication and session management
  - TripService.cs - CRUD operations for trips with business logic
  - CompanionService.cs - Companion management with ownership verification
  - ProfileService.cs - User profile management
  - SupabaseClientService.cs - Supabase client initialization and management

- ✅ **Configuration**
  - SupabaseSettings.cs - Supabase connection configuration

#### Application Layer
- ✅ **Service Interfaces**
  - IAuthService.cs
  - ITripService.cs
  - ICompanionService.cs
  - IProfileService.cs
  - ISupabaseClientService.cs

- ✅ **DTOs (Data Transfer Objects)**
  - Auth: UserDto
  - Trips: TripListItemDto, TripDetailDto
  - Companions: CompanionDto, CompanionListItemDto
  - Profiles: ProfileDto

- ✅ **Commands (CQRS Pattern)**
  - Auth: RegisterCommand, LoginCommand
  - Trips: CreateTripCommand, UpdateTripCommand, DeleteTripCommand
  - Companions: AddCompanionCommand, UpdateCompanionCommand, RemoveCompanionCommand
  - Profiles: UpdateDisplayNameCommand, UpdateAvatarUrlCommand

- ✅ **Exceptions**
  - ValidationException - Business rule violations
  - NotFoundException - Resource not found
  - UnauthorizedException - Authentication/authorization failures
  - AuthException - Supabase Auth errors
  - DatabaseException - Database operation failures

#### Dependency Injection
- ✅ **Program.cs Configuration**
  - Supabase client registered as Singleton
  - All services registered as Scoped
  - Blazored.LocalStorage integration

### 🚧 Pending Implementation

#### Presentation Layer (Pages)
- ⏳ Index.razor - Home page
- ⏳ Login.razor - User login
- ⏳ Register.razor - User registration
- ⏳ Trips/TripList.razor - List of all trips
- ⏳ Trips/TripDetails.razor - Trip detail view
- ⏳ Trips/CreateTrip.razor - Create new trip
- ⏳ Trips/EditTrip.razor - Edit existing trip
- ⏳ Profiles/UserProfile.razor - User profile view
- ⏳ Profiles/AccountSettings.razor - Account settings

#### Shared Components
- ⏳ LoginDisplay.razor - Login/logout UI component
- ⏳ TripCard.razor - Trip summary card
- ⏳ CompanionList.razor - Companion list component
- ⏳ DateRangePicker.razor - Date range selector
- ⏳ LoadingSpinner.razor - Loading indicator
- ⏳ ConfirmDialog.razor - Confirmation dialog
- ⏳ TripFormDialog.razor - Trip form dialog

#### Layout Components
- ⏳ MainLayout.razor - Main application layout
- ⏳ NavMenu.razor - Navigation menu

#### Testing
- ⏳ Unit Tests for Services
- ⏳ Integration Tests
- ⏳ E2E Tests with Playwright

#### Database
- ⏳ Supabase migrations
- ⏳ RLS policies
- ⏳ Database triggers
- ⏳ Dev seed data

#### CI/CD
- ⏳ GitHub Actions workflow
- ⏳ Automated deployment to GitHub Pages

---

## 🏗️ Architecture Patterns

### Service Layer Pattern
- **Interfaces** define contracts in `Application/Interfaces/`
- **Implementations** in `Infrastructure/Services/`
- **Dependency Injection** via Scoped lifetime for session isolation

### Repository Pattern
- Services act as repositories for data access
- Supabase client abstracted behind `ISupabaseClientService`
- Direct database access only through services

### DTO Pattern
- **Separation of concerns**: Entities vs DTOs
- **Input**: Commands for operations
- **Output**: DTOs for data transfer
- **Validation**: Business rules enforced in services

### Exception Handling
- Typed exceptions for different error scenarios
- Exceptions propagate to UI for user-friendly messages
- No catching in services - let UI handle presentation

### Authentication & Authorization
- Supabase Auth for user management
- Row Level Security (RLS) for database-level authorization
- Session management via Supabase SDK

---

## 🔄 Data Flow

### User Request Flow
```
User Action (Page)
    ↓
Component Event Handler
    ↓
Service Method Call (Interface)
    ↓
Service Implementation
    ↓
Validation (Commands/DTOs)
    ↓
Supabase Client
    ↓
PostgreSQL Database (RLS)
    ↓
Response (DTO)
    ↓
UI Update (StateHasChanged)
```

### Authentication Flow
```
Login Component
    ↓
IAuthService.LoginAsync(LoginCommand)
    ↓
Supabase Auth.SignIn()
    ↓
JWT Token Stored
    ↓
User Session Active
    ↓
UserDto Returned
    ↓
UI Redirects to Dashboard
```

---

## 📦 Key Dependencies

### NuGet Packages
- **Supabase** (supabase-csharp) - Backend connectivity
- **Blazored.LocalStorage** - Client-side storage
- **Microsoft.AspNetCore.Components.WebAssembly** - Blazor WASM framework
- **MudBlazor** (planned) - UI component library

### External Services
- **Supabase** - PostgreSQL database + Auth + Storage
- **GitHub Pages** - Static site hosting

---

## 🔐 Security Considerations

### Client-Side Security
- ✅ No sensitive credentials in WebAssembly
- ✅ Supabase Anon Key only (public key)
- ✅ JWT tokens managed by Supabase SDK
- ✅ HTTPS enforced for all connections

### Database Security
- ✅ Row Level Security (RLS) policies enforce data isolation
- ✅ User can only access their own trips and companions
- ✅ Authentication required for all operations
- ✅ Service account keys never exposed to client

### Validation
- ✅ Input validation in service layer
- ✅ Business rule validation (e.g., EndDate > StartDate)
- ✅ SQL injection prevented by Supabase client
- ✅ XSS protection via Blazor framework

---

## 🚀 Next Steps

1. **Implement Pages**: Create Blazor pages for Trips, Profiles, Auth
2. **Add Shared Components**: Build reusable UI components
3. **Setup Database**: Deploy Supabase schema and RLS policies
4. **Write Tests**: Unit tests for services, E2E tests for flows
5. **Configure CI/CD**: GitHub Actions for automated deployment
6. **User Testing**: 5-10 user testing sessions with feedback
7. **Deploy to Production**: Publish to GitHub Pages

---

**Document Status:** ✅ Updated with Implementation Progress  
**Project:** MotoNomad MVP  
**Program:** 10xDevs  
**Date:** January 2025