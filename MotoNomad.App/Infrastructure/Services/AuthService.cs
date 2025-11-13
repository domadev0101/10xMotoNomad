using MotoNomad.Application.Commands.Auth;
using MotoNomad.Application.DTOs.Auth;
using MotoNomad.Application.Exceptions;
using MotoNomad.Application.Interfaces;
using MotoNomad.App.Application.Interfaces;
using MotoNomad.App.Infrastructure.Database.Entities;
using Blazored.LocalStorage;
using Supabase;
using System.Net.Mail;

namespace MotoNomad.Infrastructure.Services;

/// <summary>
/// Authentication service implementation using Supabase Auth.
/// </summary>
public class AuthService : IAuthService
{
    private readonly ISupabaseClientService _supabaseClient;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<AuthService> _logger;
    private const string SessionKey = "supabase.auth.session";

    public AuthService(
        ISupabaseClientService supabaseClient,
        ILocalStorageService localStorage,
        ILogger<AuthService> logger)
    {
        _supabaseClient = supabaseClient ?? throw new ArgumentNullException(nameof(supabaseClient));
        _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<UserDto> RegisterAsync(RegisterCommand command)
    {
        // Validate input
        ValidateRegisterCommand(command);

        try
        {
            var client = _supabaseClient.GetClient();

            // Register user with Supabase Auth
            var signUpResponse = await client.Auth.SignUp(command.Email, command.Password);

            if (signUpResponse?.User == null)
            {
                throw new AuthException("Registration failed. Please try again.");
            }

            var user = signUpResponse.User;

            // IMPORTANT: Set session BEFORE any profile operations
            if (signUpResponse.AccessToken != null && signUpResponse.RefreshToken != null)
            {
                await client.Auth.SetSession(signUpResponse.AccessToken, signUpResponse.RefreshToken);
                _logger.LogDebug("Session set for newly registered user {UserId}", user.Id);
            }

            // Save session to localStorage
            if (signUpResponse != null)
            {
                await SaveSessionAsync(signUpResponse);
            }

            // Profile is automatically created by database trigger (handle_new_user)
            // If display name was provided, update it now
            if (!string.IsNullOrWhiteSpace(command.DisplayName))
            {
                try
                {
                    // Small delay to ensure trigger completes
                    await Task.Delay(500);
                    
                    var profile = await client
                        .From<Profile>()
                        .Select("*")
                        .Filter("id", Postgrest.Constants.Operator.Equals, user.Id)
                        .Single();

                    if (profile != null)
                    {
                        profile.DisplayName = command.DisplayName.Trim();
                        profile.UpdatedAt = DateTime.UtcNow;
                        
                        await client.From<Profile>().Update(profile);
                        _logger.LogInformation("Updated display_name during registration for user {UserId}", user.Id);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to update display_name during registration for user {UserId}", user.Id);
                    // Don't fail registration if display_name update fails
                }
            }

            _logger.LogInformation("User registered successfully: {Email}", command.Email);

            return new UserDto
            {
                Id = Guid.Parse(user.Id),
                Email = user.Email ?? command.Email,
                DisplayName = command.DisplayName,
                AvatarUrl = null,
                CreatedAt = user.CreatedAt
            };
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex)
        {
            _logger.LogError(ex, "Supabase Auth error during registration: {Email}", command.Email);

            if (ex.Message.Contains("already registered") || ex.Message.Contains("already exists"))
            {
                throw new AuthException("This email is already registered.", "EMAIL_EXISTS", ex);
            }

            throw new AuthException($"Registration failed: {ex.Message}", ex);
        }
        catch (AuthException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration: {Email}", command.Email);
            throw new AuthException("An unexpected error occurred during registration.", ex);
        }
    }

    public async Task<UserDto> LoginAsync(LoginCommand command)
    {
        // Validate input
        ValidateLoginCommand(command);

        try
        {
            var client = _supabaseClient.GetClient();

            // Sign in with Supabase Auth
            var signInResponse = await client.Auth.SignIn(command.Email, command.Password);

            if (signInResponse?.User == null)
            {
                throw new AuthException("Login failed. Please check your credentials.");
            }

            var user = signInResponse.User;

            // Save session to localStorage
            if (signInResponse != null)
            {
                await SaveSessionAsync(signInResponse);
            }

            // Try to fetch profile for display name and avatar
            Profile? profile = null;
            try
            {
                var profileResponse = await client
                    .From<Profile>()
                    .Select("*")
                    .Filter("id", Postgrest.Constants.Operator.Equals, user.Id)
                    .Single();

                profile = profileResponse;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch profile during login for user {UserId}", user.Id);
                // Continue without profile data
            }

            _logger.LogInformation("User logged in successfully: {Email}", command.Email);

            return new UserDto
            {
                Id = Guid.Parse(user.Id),
                Email = user.Email ?? command.Email,
                DisplayName = profile?.DisplayName,
                AvatarUrl = profile?.AvatarUrl,
                CreatedAt = user.CreatedAt
            };
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex)
        {
            _logger.LogWarning(ex, "Authentication failed for user: {Email}", command.Email);
            throw new AuthException("Invalid email or password.", "INVALID_CREDENTIALS", ex);
        }
        catch (AuthException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login: {Email}", command.Email);
            throw new AuthException("An unexpected error occurred during login.", ex);
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var client = _supabaseClient.GetClient();
            await client.Auth.SignOut();

            // Remove session from localStorage
            await ClearSessionAsync();

            _logger.LogInformation("User logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            throw new AuthException("Logout failed. Please try again.", ex);
        }
    }

    public async Task<UserDto?> GetCurrentUserAsync()
    {
        try
        {
            var client = _supabaseClient.GetClient();

            var user = client.Auth.CurrentUser;

            if (user == null)
            {
                return null;
            }

            // Try to fetch profile
            Profile? profile = null;
            try
            {
                var profileResponse = await client
            .From<Profile>()
             .Select("*")
            .Filter("id", Postgrest.Constants.Operator.Equals, user.Id)
           .Single();

                profile = profileResponse;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to fetch profile for current user {UserId}", user.Id);
            }

            return new UserDto
            {
                Id = Guid.Parse(user.Id),
                Email = user.Email ?? string.Empty,
                DisplayName = profile?.DisplayName,
                AvatarUrl = profile?.AvatarUrl,
                CreatedAt = user.CreatedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current user");
            return null;
        }
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        try
        {
            var client = _supabaseClient.GetClient();

            var session = client.Auth.CurrentSession;

            if (session == null)
            {
                return false;
            }

            // Check if token is expired
            if (session.ExpiresAt() < DateTime.UtcNow)
            {
                await ClearSessionAsync();
                return false;
            }

            return client.Auth.CurrentUser != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication status");
            return false;
        }
    }

    public async Task RefreshSessionAsync()
    {
        try
        {
            var client = _supabaseClient.GetClient();
            var session = client.Auth.CurrentSession;

            if (session == null)
            {
                throw new AuthException("No active session to refresh.");
            }

            var refreshedSession = await client.Auth.RefreshSession();

            // Save refreshed session to localStorage
            if (refreshedSession != null)
            {
                await SaveSessionAsync(refreshedSession);
            }

            _logger.LogInformation("Session refreshed successfully");
        }
        catch (Supabase.Gotrue.Exceptions.GotrueException ex)
        {
            _logger.LogError(ex, "Failed to refresh session");
            await ClearSessionAsync();
            throw new AuthException("Session refresh failed. Please log in again.", "SESSION_EXPIRED", ex);
        }
        catch (AuthException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during session refresh");
            throw new AuthException("An unexpected error occurred during session refresh.", ex);
        }
    }

    #region Session Persistence

    /// <summary>
    /// Saves the current session to browser localStorage.
    /// </summary>
    private async Task SaveSessionAsync(Supabase.Gotrue.Session session)
    {
        try
        {
            await _localStorage.SetItemAsync(SessionKey, session);
            _logger.LogDebug("Session saved to localStorage for user: {UserId}", session.User?.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save session to localStorage");
            // Don't throw - session persistence failure shouldn't break login
        }
    }

    /// <summary>
    /// Clears the session from browser localStorage.
    /// </summary>
    private async Task ClearSessionAsync()
    {
        try
        {
            await _localStorage.RemoveItemAsync(SessionKey);
            _logger.LogDebug("Session cleared from localStorage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear session from localStorage");
            // Don't throw - localStorage clear failure shouldn't break logout
        }
    }

    #endregion

    #region Validation

    private void ValidateRegisterCommand(RegisterCommand command)
    {
        var errors = new Dictionary<string, string[]>();


        // Email validation
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            errors["Email"] = new[] { "Email address is required" };
        }
        else if (!IsValidEmail(command.Email))
        {
            errors["Email"] = new[] { "Email address format is invalid" };
        }
        else if (command.Email.Length > 255)
        {
            errors["Email"] = new[] { "Email address cannot exceed 255 characters" };
        }

        // Password validation
        if (string.IsNullOrWhiteSpace(command.Password))
        {
            errors["Password"] = new[] { "Password is required" };
        }
        else if (command.Password.Length < 8)
        {
            errors["Password"] = new[] { "Password must be at least 8 characters" };
        }
        else if (command.Password.Length > 100)
        {
            errors["Password"] = new[] { "Password cannot exceed 100 characters" };
        }

        // Display name validation
        if (!string.IsNullOrWhiteSpace(command.DisplayName) && command.DisplayName.Length > 100)
        {
            errors["DisplayName"] = new[] { "Display name cannot exceed 100 characters" };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private void ValidateLoginCommand(LoginCommand command)
    {
        var errors = new Dictionary<string, string[]>();

        // Email validation
        if (string.IsNullOrWhiteSpace(command.Email))
        {
            errors["Email"] = new[] { "Email address is required" };
        }
        else if (!IsValidEmail(command.Email))
        {
            errors["Email"] = new[] { "Email address format is invalid" };
        }

        // Password validation
        if (string.IsNullOrWhiteSpace(command.Password))
        {
            errors["Password"] = new[] { "Password is required" };
        }

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}
