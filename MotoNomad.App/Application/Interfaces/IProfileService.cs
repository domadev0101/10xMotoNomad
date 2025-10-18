using MotoNomad.Application.Commands.Profiles;
using MotoNomad.Application.DTOs.Profiles;
using MotoNomad.Application.Exceptions;

namespace MotoNomad.Application.Interfaces;

/// <summary>
/// User profile management interface.
/// </summary>
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
    /// Updates user profile with provided fields.
    /// Only non-null fields will be updated (partial update).
    /// </summary>
    /// <param name="command">Profile update command with optional DisplayName and AvatarUrl</param>
    /// <returns>Updated profile information</returns>
    /// <exception cref="UnauthorizedException">User not authenticated</exception>
    /// <exception cref="ValidationException">Invalid field values or no fields provided</exception>
    /// <exception cref="DatabaseException">Database update failed</exception>
    Task<ProfileDto> UpdateProfileAsync(UpdateProfileCommand command);
}
