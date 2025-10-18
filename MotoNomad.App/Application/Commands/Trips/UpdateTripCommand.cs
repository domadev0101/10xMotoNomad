using MotoNomad.App.Infrastructure.Database.Entities;

namespace MotoNomad.Application.Commands.Trips;

/// <summary>
/// Request to update existing trip with all fields.
/// </summary>
public record UpdateTripCommand
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public string? Description { get; init; }
    public required TransportType TransportType { get; init; }
}
