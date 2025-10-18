namespace MotoNomad.Application.Commands.Auth;

/// <summary>
/// User registration request with email, password, and optional display name.
/// </summary>
public record RegisterCommand
{
    public required string Email { get; init; }
    public required string Password { get; init; }
    public string? DisplayName { get; init; }
}
