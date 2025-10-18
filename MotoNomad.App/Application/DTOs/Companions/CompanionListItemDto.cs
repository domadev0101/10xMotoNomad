namespace MotoNomad.Application.DTOs.Companions;

/// <summary>
/// Lightweight companion representation for lists.
/// </summary>
public record CompanionListItemDto
{
    public required Guid Id { get; init; }
    public required Guid TripId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Contact { get; init; }
}
