using Postgrest.Attributes;
using Postgrest.Models;

namespace MotoNomad.App.Infrastructure.Database.Entities;

[Table("companions")]
public class Companion : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("trip_id")]
    public Guid TripId { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [Column("last_name")]
    public string LastName { get; set; } = string.Empty;

    [Column("contact")]
    public string? Contact { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
