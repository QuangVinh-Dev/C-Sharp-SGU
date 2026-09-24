using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("Servers")]
public class Server
{
    [PrimaryKey, Identity]
    public long Id { get; set; }

    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    [Column("OwnerId")]
    public long OwnerId { get; set; }

    [Column("IconFileId")]
    public long? IconFileId { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }

    [Column("DeletedAt")]
    public DateTime? DeletedAt { get; set; }

    [Column("ScheduledDeleteAt")]
    public DateTime? ScheduledDeleteAt { get; set; }

    [Column("IsSuspended")]
    public bool IsSuspended { get; set; }

    [Column("SuspendedUntil")]
    public DateTime? SuspendedUntil { get; set; }
}