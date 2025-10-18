namespace MotoNomad.Application.DTOs.Companions;

/// <summary>
/// Complete companion information with metadata.
/// </summary>
public record CompanionDto
{
    public required Guid Id { get; init; }
    public required Guid TripId { get; init; }
    public Guid? UserId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Contact { get; init; }
    public required DateTime CreatedAt { get; init; }
}
