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
│  │  └─ Services/
│  │     ├─ SupabaseTripService.cs
│  │     ├─ SupabaseCompanionService.cs
│  │     └─ SupabaseAuthService.cs
│  ├─ Application/
│  │  ├─ Interfaces/
│  │  │  ├─ ITripService.cs
│  │  │  ├─ ICompanionService.cs
│  │  │  └─ IAuthService.cs
│  │  ├─ DTOs/
│  │  │  ├─ Trips/
│  │  │  │  ├─ TripDto.cs
│  │  │  │  ├─ TripListItemDto.cs
│  │  │  │  └─ TripDetailDto.cs
│  │  │  ├─ Companions/
│  │  │  │  ├─ CompanionDto.cs
│  │  │  │  └─ CompanionListItemDto.cs
│  │  │  └─ Auth/
│  │  │     ├─ LoginDto.cs
│  │  │     ├─ RegisterDto.cs
│  │  │     └─ UserDto.cs
│  │  ├─ Commands/
│  │  │  ├─ Trips/
│  │  │  │  ├─ CreateTripCommand.cs
│  │  │  │  ├─ UpdateTripCommand.cs
│  │  │  │  └─ DeleteTripCommand.cs
│  │  │  └─ Companions/
│  │  │     ├─ AddCompanionCommand.cs
│  │  │     ├─ UpdateCompanionCommand.cs
│  │  │     └─ RemoveCompanionCommand.cs
│  │  ├─ Validators/
│  │  │  ├─ CreateTripCommandValidator.cs
│  │  │  └─ AddCompanionCommandValidator.cs
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
│  │  └─ Profile/
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
│  │  │  └─ CompanionServiceTests.cs
│  │  └─ Validators/
│  │     └─ CommandValidatorTests.cs
│  ├─ Integration/
│  │  ├─ TripServiceIntegrationTests.cs
│  │  └─ AuthServiceIntegrationTests.cs
│  └─ E2E/
│     ├─ LoginFlowTests.cs
│     └─ TripManagementFlowTests.cs
├─ .ai/
│  ├─ prd.md
│  ├─ db-plan.md
│  ├─ services-plan.md
│  ├─ api-contracts.md
│  ├─ wasm-arch.md
│  ├─ tech-stack.md
│  ├─ configuration-guide.md
│  └─ blazor-gh-pages-guide.md
├─ .github/
│  └─ workflows/
│     └─ deploy.yml
├─ .gitignore
├─ LICENSE
└─ README.md
```

---

**Document Status:** ✅ Ready for Implementation  
**Project:** MotoNomad MVP  
**Program:** 10xDevs  
**Date:** October 2025