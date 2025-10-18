namespace MotoNomad.Application.Commands.Auth;

/// <summary>
/// User authentication request with credentials.
/// </summary>
public record LoginCommand
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
