namespace MotoNomad.Application.Commands.Profiles;

/// <summary>
/// Request to update user profile information.
/// Allows partial updates - only provided fields will be updated.
/// </summary>
public record UpdateProfileCommand
{
    /// <summary>
    /// Display name to update. If null, will not be updated.
    /// </summary>
    public string? DisplayName { get; init; }

    /// <summary>
    /// Avatar URL to update. If null, will not be updated.
    /// </summary>
    public string? AvatarUrl { get; init; }
}
