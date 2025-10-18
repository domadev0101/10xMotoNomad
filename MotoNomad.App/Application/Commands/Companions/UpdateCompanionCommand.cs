namespace MotoNomad.Application.Commands.Companions;

/// <summary>
/// Request to update existing companion information.
/// </summary>
public record UpdateCompanionCommand
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Contact { get; init; }
}
