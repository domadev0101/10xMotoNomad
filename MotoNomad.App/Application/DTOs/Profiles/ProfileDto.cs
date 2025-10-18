namespace MotoNomad.Application.DTOs.Profiles;

/// <summary>
/// User profile information.
/// </summary>
public record ProfileDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
