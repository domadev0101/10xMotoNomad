namespace MotoNomad.Application.DTOs.Auth;

/// <summary>
/// Authenticated user information returned after login or registration.
/// </summary>
public record UserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public string? DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public required DateTime CreatedAt { get; init; }
}
