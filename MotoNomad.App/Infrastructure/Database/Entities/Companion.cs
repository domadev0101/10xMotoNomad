namespace MotoNomad.App.Infrastructure.Database.Entities;

public class Companion
{
    public Guid Id { get; set; }
    public Guid TripId { get; set; }
    public Guid? UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public DateTime CreatedAt { get; set; }
}
