using MotoNomad.App.Infrastructure.Database.Entities;

namespace MotoNomad.Application.Commands.Trips;

/// <summary>
/// Request to create a new trip with validation.
/// </summary>
public record CreateTripCommand
{
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public string? Description { get; init; }
    public required TransportType TransportType { get; init; }
}
