namespace MotoNomad.Application.Commands.Trips;

/// <summary>
/// Request to delete trip (simple wrapper for clarity).
/// </summary>
public record DeleteTripCommand
{
    public required Guid Id { get; init; }
}
