# AI Rules for MotoNomad

Web application for planning individual and group trips (motorcycle, airplane, train). Centralized trip management with Blazor WebAssembly and Supabase backend.

## Tech Stack
- .NET 9.0
- Blazor WebAssembly (standalone)
- C# 13
- MudBlazor (UI Components)
- Supabase (PostgreSQL + Auth)
- supabase-csharp
- Blazored.LocalStorage
- xUnit + bUnit (Testing)

## Project Structure
When introducing changes to the project, always follow the directory structure below:

- `./MotoNomad.App` - main application project
- `./MotoNomad.App/Infrastructure/` - Infrastructure layer (database, services, configuration)
- `./MotoNomad.App/Infrastructure/Database/Entities/` - Database entity models (Trip.cs, Companion.cs, Profile.cs)
- `./MotoNomad.App/Infrastructure/Database/supabase/migrations/` - Supabase database migrations
- `./MotoNomad.App/Infrastructure/Services/` - Service implementations (TripService.cs, CompanionService.cs, AuthService.cs, ProfileService.cs)
- `./MotoNomad.App/Infrastructure/Configuration/` - Configuration classes (SupabaseSettings.cs)
- `./MotoNomad.App/Application/` - Application layer (interfaces, DTOs, commands, exceptions)
- `./MotoNomad.App/Application/Interfaces/` - Service interfaces (ITripService.cs, ICompanionService.cs, IAuthService.cs, IProfileService.cs)
- `./MotoNomad.App/Application/DTOs/` - Data Transfer Objects (Auth, Trips, Companions, Profiles)
- `./MotoNomad.App/Application/Commands/` - Command objects for CQRS pattern (Trips, Companions, Profiles, Auth)
- `./MotoNomad.App/Application/Exceptions/` - Custom exceptions (ValidationException.cs, NotFoundException.cs, UnauthorizedException.cs)
- `./MotoNomad.App/Pages/` - Blazor pages (Index.razor, Login.razor, Register.razor, Trips/, Profiles/)
- `./MotoNomad.App/Layout/` - Blazor layouts (MainLayout.razor, NavMenu.razor)
- `./MotoNomad.App/Shared/` - Shared components and dialogs
- `./MotoNomad.App/Shared/Components/` - Reusable Blazor components (TripCard.razor, CompanionList.razor, DateRangePicker.razor)
- `./MotoNomad.App/Shared/Dialogs/` - Dialog components (ConfirmDialog.razor, TripFormDialog.razor)
- `./MotoNomad.App/wwwroot/` - Static assets (css, js, images)
- `./MotoNomad.App/appsettings.json` - Configuration (Supabase URL and keys)
- `./MotoNomad.App/Program.cs` - Application startup and DI configuration
- `./MotoNomad.Tests/` - xUnit test project
- `./MotoNomad.Tests/Unit/Services/` - Service unit tests
- `./MotoNomad.Tests/Components/` - bUnit component tests
- `./MotoNomad.Tests/Integration/` - Integration tests
- `./MotoNomad.Tests/E2E/` - End-to-end tests
- `./.ai/` - Architecture documentation (prd.md, db-plan.md, wasm-arch.md, etc.)
- `./.github/` - GitHub configuration (workflows, copilot-instructions.md)

When modifying the directory structure, always update this section.

## Coding Practices

### Architecture Patterns
- **Layered Architecture**: Infrastructure (data access) ? Application (business logic) ? Presentation (UI)
- **Service Layer Pattern**: Interfaces in Application/Interfaces/, implementations in Infrastructure/Services/
- **Repository Pattern**: Services act as repositories for data access
- **DTO Pattern**: Separate entities (database) from DTOs (data transfer)
- **CQRS Pattern**: Use Command objects for write operations in Application/Commands/
- **Exception Handling**: Typed exceptions in Application/Exceptions/

### Blazor WebAssembly Patterns
- Always use `async`/`await` for Supabase API calls and I/O operations
- Implement service layer pattern for data access (never call Supabase directly from components)
- Use constructor injection for all dependencies
- Dispose resources properly (implement `IDisposable` or `IAsyncDisposable`)
- Call `StateHasChanged()` only when necessary to trigger re-renders
- Use `@key` directive for lists to optimize rendering
- **Code-Behind Pattern (MANDATORY)**:
  - **Always** create separate `.razor.cs` files for all Blazor components with C# logic
  - **Never** use `@code` blocks in `.razor` files - move all C# code to code-behind files
  - Mark code-behind classes as `partial` and use the same namespace as the component
  - Use `[Inject]` attribute for dependency injection in code-behind files
  - Add XML documentation comments (`///`) to all public methods and classes in code-behind files

### Supabase Integration
- Store Supabase URL and Anon Key in `appsettings.json` (never hardcode)
- Register Supabase client as Singleton in `Program.cs`
- Always wrap Supabase calls in try-catch blocks
- Implement Row Level Security (RLS) policies in Supabase for data protection
- Use Blazored.LocalStorage for client-side caching (never direct `localStorage`)
- Cache API responses to minimize redundant calls

### State Management
- Use Singleton services for application-wide state
- Use Scoped services for component-tree-specific state
- Implement Cascading Parameters for sharing authentication state
- Store JWT tokens securely using Blazored.LocalStorage

### Data Models
- **Entities**: Database models in Infrastructure/Database/Entities/ matching Supabase schema
- **DTOs**: Data Transfer Objects in Application/DTOs/ for API responses
- **Commands**: Command objects in Application/Commands/ for write operations
- Use `[PrimaryKey]` and `[Column]` attributes for mapping
- Include `Id`, `CreatedAt`, `UpdatedAt` fields for all entities
- Use proper data types (`Guid` for IDs, `DateTime` for dates)

### Error Handling
- Use custom exceptions from Application/Exceptions/ (ValidationException, NotFoundException, UnauthorizedException)
- Use exceptions for exceptional cases only, not for control flow
- Handle errors at the beginning of methods using guard clauses
- Use early returns for error conditions
- Provide user-friendly error messages (use MudBlazor Snackbar)
- Log errors with `ILogger<T>` with sufficient context
- Implement proper error handling for network failures

### Validation
- Use Data Annotations for model validation (`[Required]`, `[StringLength]`)
- Validate business rules in Commands before service calls (e.g., end date > start date)
- Implement client-side validation with `EditForm` and `ValidationMessage`
- Display validation errors clearly in UI
- Throw ValidationException for business rule violations

### MudBlazor UI
- Use MudBlazor components for consistent Material Design
- Leverage `MudForm` with built-in validation
- Use `MudDialog` for confirmations (delete operations)
- Use `MudSnackbar` for success/error notifications
- Use `MudDatePicker` for date selection
- Ensure responsive design (mobile-first)

### Performance Optimization
- Minimize component re-renders using `ShouldRender()`
- Use `Virtualize` component for long lists
- Enable IL trimming and linking in Release builds
- Lazy load components when appropriate
- Batch Supabase operations when possible

### Security
- Never expose Supabase service role key (use anon key only)
- Rely on Row Level Security (RLS) for data protection
- Use `[Authorize]` attribute on protected pages
- Validate and sanitize all user inputs
- Never store sensitive data in client-side code

### Testing
- Write unit tests for business logic and services (xUnit)
- Write component tests using bUnit
- Mock Supabase client using Moq
- Test validation logic thoroughly
- Use descriptive test names (Given_When_Then pattern)
- Run tests in Visual Studio Test Explorer

### Naming Conventions
- **PascalCase**: Classes, methods, public members, components
- **camelCase**: Local variables, private fields, parameters
- **UPPERCASE**: Constants
- **Prefix "I"**: Interfaces (e.g., `ITripService`)

### C# Best Practices
- Use C# 10+ features (record types, pattern matching, file-scoped namespaces)
- Use `var` for implicit typing when type is obvious
- Prefer LINQ and lambda expressions for collections
- Use null-conditional operators (`?.`) and null-coalescing (`??`)
- Follow SOLID principles

### Guidelines for Clean Code
- Use feedback from linters and analyzers to improve code
- Prioritize error handling and edge cases
- Handle errors and edge cases at the beginning of functions
- Use early returns for error conditions to avoid deeply nested if statements
- Place the happy path last in the function for improved readability
- Avoid unnecessary else statements; use if-return pattern instead
- Use guard clauses to handle preconditions and invalid states early
- Implement proper error logging and user-friendly error messages
- Keep components focused on presentation; move logic to services
- Write self-documenting code; prefer clarity over cleverness

## Key Reminders
- **Never** use `localStorage` directly - always use Blazored.LocalStorage
- **Never** expose Supabase service role key in client code
- **Always** configure Row Level Security in Supabase
- **Always** validate user input on client side
- **Always** handle exceptions gracefully with proper logging
- **Always** use MudBlazor components for UI consistency
- **Always** write unit tests for business logic
- **Always** follow layered architecture: Infrastructure ? Application ? Presentation
- **Always** use DTOs for data transfer, Entities for database operations
- **Always** use Command objects for write operations (CQRS)
- **Always** throw typed exceptions from Application/Exceptions/
- **Always** use code-behind pattern (`.razor.cs` files) - never use `@code` blocks in `.razor` files