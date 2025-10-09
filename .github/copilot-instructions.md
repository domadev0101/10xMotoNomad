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

- `./10xMotoNomad` - main application project
- `./10xMotoNomad/Layout` - Blazor layouts (MainLayout.razor, NavMenu.razor)
- `./10xMotoNomad/Components` - Reusable Blazor components
- `./10xMotoNomad/Pages` - Blazor pages (Home.razor, TripList.razor, Login.razor)
- `./10xMotoNomad/Services/Interfaces` - Service interfaces (ITripService.cs, ICompanionService.cs)
- `./10xMotoNomad/Services/Implementations` - Service implementations
- `./10xMotoNomad/Models` - Data models and DTOs matching Supabase schema
- `./10xMotoNomad/Auth` - Authentication (CustomAuthStateProvider.cs)
- `./10xMotoNomad/wwwroot` - Static assets (css, js)
- `./10xMotoNomad/appsettings.json` - Configuration (Supabase URL and keys)
- `./10xMotoNomad/Program.cs` - Application startup and DI configuration
- `./10xMotoNomad.Tests` - xUnit test project
- `./10xMotoNomad.Tests/Services` - Service unit tests
- `./10xMotoNomad.Tests/Components` - bUnit component tests

When modifying the directory structure, always update this section.

## Coding Practices

### Blazor WebAssembly Patterns
- Always use `async`/`await` for Supabase API calls and I/O operations
- Implement service layer pattern for data access (never call Supabase directly from components)
- Use constructor injection for all dependencies
- Dispose resources properly (implement `IDisposable` or `IAsyncDisposable`)
- Call `StateHasChanged()` only when necessary to trigger re-renders
- Use `@key` directive for lists to optimize rendering

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
- Create C# models matching Supabase table schemas
- Use `[PrimaryKey]` and `[Column]` attributes for mapping
- Include `Id`, `CreatedAt`, `UpdatedAt` fields for all entities
- Use proper data types (`Guid` for IDs, `DateTime` for dates)

### Error Handling
- Use exceptions for exceptional cases only, not for control flow
- Handle errors at the beginning of methods using guard clauses
- Use early returns for error conditions
- Provide user-friendly error messages (use MudBlazor Snackbar)
- Log errors with `ILogger<T>` with sufficient context
- Implement proper error handling for network failures

### Validation
- Use Data Annotations for model validation (`[Required]`, `[StringLength]`)
- Validate business rules before Supabase API calls (e.g., end date > start date)
- Implement client-side validation with `EditForm` and `ValidationMessage`
- Display validation errors clearly in UI

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