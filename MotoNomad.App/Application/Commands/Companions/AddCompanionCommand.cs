namespace MotoNomad.Application.Commands.Companions;

/// <summary>
/// Request to add new companion to a trip.
/// </summary>
public record AddCompanionCommand
{
    public required Guid TripId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Contact { get; init; }
}
