# Technical Specification: User Authentication Module (MotoNomad)

**Project:** MotoNomad MVP  
**Program:** 10xDevs  
**Date:** October 2025  
**Status:** Ready for Implementation

---

## 1. Introduction

This document describes the architecture and implementation of the registration, login, logout, and password recovery modules in the MotoNomad application. The specification is based on the requirements defined in the PRD and the adopted technology stack (Blazor WASM + Supabase).

## 2. User Interface Architecture (Frontend)

### 2.1. Changes to Application Structure

#### New Pages (Pages)
- **`Pages/Auth/Register.razor`**: Page with registration form for new users. Publicly accessible.
- **`Pages/Auth/Login.razor`**: Page with login form. Publicly accessible.
- **`Pages/Auth/ForgotPassword.razor`**: Page with form to send password reset link.
- **`Pages/Auth/ResetPassword.razor`**: Page allowing setting a new password after clicking the link from email. Will read token from URL address.

#### Layout and Component Modifications
- **`Layout/MainLayout.razor`**:
  - Will be wrapped in `CascadingAuthenticationState` component so authentication state is available throughout the application.
  - Will conditionally render `NavMenu` depending on login state.
- **`Layout/NavMenu.razor`**:
  - Will use `AuthorizeView` to dynamically display links.
  - **For unauthenticated (`<NotAuthorized>`)**: Will display "Login" and "Register" links.
  - **For authenticated (`<Authorized>`)**: Will display "My Trips", "Profile" links and "Logout" button.
- **`Shared/RedirectToLogin.razor`**:
  - Helper component to be used on pages requiring authorization. If user is not logged in, will automatically redirect to `/login` page.
- **`App.razor`**:
  - Will be modified to use `AuthorizeRouteView` for pages requiring authorization. This will enable access protection for e.g. `/trips` and automatic redirect to login page defined in `RedirectToLogin`.
- **Protected Pages (e.g. `Trips/TripList.razor`)**:
  - `@attribute [Authorize]` attribute will be added at top of file to prevent access by unauthenticated users.

### 2.2. Forms and Components

- **Registration Form (`Register.razor`)**:
  - Fields: `Email`, `Password`, `ConfirmPassword`.
  - Validation:
    - All fields required.
    - Email must have valid format.
    - Password must be min. 8 characters (according to PRD).
    - `ConfirmPassword` must be identical to `Password`.
  - Logic: After successful validation calls `IAuthService.RegisterAsync()`. On success redirects to trip list (`/trips`). On error (e.g. user already exists) displays message with `MudSnackbar`.

- **Login Form (`Login.razor`)**:
  - Fields: `Email`, `Password`.
  - Validation: Required fields.
  - Logic: Calls `IAuthService.LoginAsync()`. On success saves session and redirects to `/trips`. On error ("Invalid credentials") displays message.

- **Password Recovery Form (`ForgotPassword.razor`)**:
  - Field: `Email`.
  - Logic: Calls `IAuthService.SendPasswordResetEmailAsync()`. After call always displays success message (for security reasons, to not reveal whether given email exists in database).

### 2.3. User Scenarios and Error Handling

- **Registration**:
  - **Success**: User is created in Supabase, automatically logged in, and session saved in `LocalStorage`. Redirect to `/trips`.
  - **Error**: Message "User with this email address already exists." or "Password is too weak.".
- **Login**:
  - **Success**: Session is retrieved from Supabase and saved in `LocalStorage`. Authentication state refresh and redirect.
  - **Error**: Message "Invalid email or password.".
- **Protected Page Access**: Unauthenticated user trying to access `/trips` is redirected to `/login`.
- **Logout**: Session is removed from `LocalStorage` and Supabase. User is redirected to home page.

## 3. Application and Infrastructure Layer Logic

Since the application operates in Blazor WASM Standalone model, all logic resides on client side. Communication with "backend" occurs through Supabase API.

### 3.1. Contracts (Interfaces)

- **`Application/Interfaces/IAuthService.cs`**:
  ```csharp
  public interface IAuthService
  {
  Task<User?> GetCurrentUser();
      Task LoginAsync(string email, string password);
      Task RegisterAsync(string email, string password, string confirmPassword);
      Task LogoutAsync();
      Task SendPasswordResetEmailAsync(string email);
      Task UpdatePasswordAsync(string newPassword);
  }
  ```

### 3.2. Services (Implementations)

- **`Infrastructure/Services/AuthService.cs`**:
  - Implements `IAuthService`.
  - Injects Supabase client (`Supabase.Client`) and `Blazored.LocalStorage.ILocalStorageService`.
  - Service methods will wrap calls to `supabase.Auth`, e.g. `supabase.Auth.SignUp()`, `supabase.Auth.SignInWithPassword()`.
  - Will manage user session by saving it in `LocalStorage` after successful login/registration and removing after logout.
  - Will handle exceptions returned by Supabase (e.g. `Gotrue.Exceptions.BadRequestException`) and map them to user-friendly errors.

### 3.3. Data Models (DTOs)

- **`Application/DTOs/Auth/RegisterRequest.cs`**: Model for registration form with validation annotations.
- **`Application/DTOs/Auth/LoginRequest.cs`**: Model for login form with annotations.

## 4. Authentication System (Supabase Integration)

### 4.1. Configuration
- **`Program.cs`**:
  - Supabase client will be registered as Singleton, retrieving `Url` and `AnonKey` from `appsettings.json`.
  - Services will be registered: `IAuthService` and `AuthService`.
  - Blazor authorization support will be added: `AddAuthorizationCore()`.
  - `SupabaseAuthenticationStateProvider` will be registered as implementation of `AuthenticationStateProvider`.
- **`Infrastructure/Auth/SupabaseAuthenticationStateProvider.cs`**:
  - New class inheriting from `AuthenticationStateProvider`.
  - Will inject `ILocalStorageService` and Supabase client in constructor.
  - Method `GetAuthenticationStateAsync` will check if saved session exists in `LocalStorage`.
    - If yes, will restore session in Supabase client and return `ClaimsPrincipal` with user data (ID, email).
    - If no, will return empty `ClaimsPrincipal` for anonymous user.
  - Will contain methods `MarkUserAsAuthenticated()` and `MarkUserAsLoggedOut()` which will update state and notify Blazor of change (`NotifyAuthenticationStateChanged`).

### 4.2. Security (Supabase)
- **Row Level Security (RLS)**: Key security element.
  - For `Trips` table, RLS policy will be enabled allowing read/write/modify of record only when `user_id` in record equals `auth.uid()`. This guarantees users see only their data, even if using the same `AnonKey`.
- **Email Templates**: Email message templates will be configured in Supabase panel for registration confirmation (if enabled) and password reset.

## 5. Summary of Key Implementation Steps

1.  **Structure**: Create new files for pages, services and DTOs in appropriate directories.
2.  **DI Configuration**: Register `Supabase.Client`, `IAuthService`, `AuthenticationStateProvider` and `ILocalStorageService` in `Program.cs`.
3.  **UI**: Implement forms in `Register.razor` and `Login.razor` using `MudForm` and validation.
4.  **Layout**: Modify `MainLayout.razor` and `NavMenu.razor` to dynamically render UI depending on authorization state.
5.  **Logic**: Implement `AuthService` with logic wrapping Supabase client.
6.  **Application State**: Create `SupabaseAuthenticationStateProvider` to manage authentication state throughout application.
7.  **Routing**: Protect pages (e.g. `/trips`) using `[Authorize]` attribute and `AuthorizeRouteView`.
8.  **Supabase**: Enable and configure RLS policies for tables in database.

---

**Document ready for implementation** ✅  
**Project**: MotoNomad MVP  
**Program**: 10xDevs  
**Date**: October 2025  
**Certification deadline**: November 2025
