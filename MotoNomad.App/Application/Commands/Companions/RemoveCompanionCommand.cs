namespace MotoNomad.Application.Commands.Companions;

/// <summary>
/// Request to remove companion from trip.
/// </summary>
public record RemoveCompanionCommand
{
    public required Guid Id { get; init; }
}
