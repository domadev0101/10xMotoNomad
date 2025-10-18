using MotoNomad.App.Infrastructure.Database.Entities;

namespace MotoNomad.Application.DTOs.Trips;

/// <summary>
/// Lightweight trip representation for list views.
/// </summary>
public record TripListItemDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public required int DurationDays { get; init; }
    public required TransportType TransportType { get; init; }
    public required int CompanionCount { get; init; }
    public required DateTime CreatedAt { get; init; }
}
