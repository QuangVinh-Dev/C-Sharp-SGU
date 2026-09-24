using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("AuditLogs")]
public class AuditLog
{
    [PrimaryKey, Identity]
    public long Id { get; set; }

    [Column("ActorUserId")]
    public long? ActorUserId { get; set; }

    [Column("Action")]
    public string Action { get; set; } = string.Empty;

    [Column("EntityType")]
    public string EntityType { get; set; } = string.Empty;

    [Column("EntityId")]
    public long EntityId { get; set; }

    [Column("IpAddress")]
    public string? IpAddress { get; set; }

    [Column("Metadata")]
    public string? Metadata { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
