using Postgrest.Attributes;
using Postgrest.Models;

namespace MotoNomad.App.Infrastructure.Database.Entities;

[Table("trips")]
public class Trip : BaseModel
{
    [PrimaryKey("id")]
    public Guid Id { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("start_date")]
    public DateOnly StartDate { get; set; }

    [Column("end_date")]
    public DateOnly EndDate { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("transport_type")]
    public TransportType TransportType { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
