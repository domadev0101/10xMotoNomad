using MotoNomad.Application.Commands.Auth;
using MotoNomad.Application.DTOs.Auth;
using MotoNomad.Application.Exceptions;

namespace MotoNomad.Application.Interfaces;

/// <summary>
/// Authentication and user session management interface.
/// </summary>
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
