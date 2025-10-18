using MotoNomad.App.Infrastructure.Database.Entities;
using MotoNomad.Application.DTOs.Companions;

namespace MotoNomad.Application.DTOs.Trips;

/// <summary>
/// Complete trip information including companions for detail views.
/// </summary>
public record TripDetailDto
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public string? Description { get; init; }
    public required TransportType TransportType { get; init; }
    public required int DurationDays { get; init; }
    public required List<CompanionListItemDto> Companions { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}
