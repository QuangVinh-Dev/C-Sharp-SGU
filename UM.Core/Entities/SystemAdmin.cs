using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("SystemAdmins")]
public class SystemAdmin
{
    [PrimaryKey, Identity]
    public long Id { get; set; }

    [Column("UserId")]
    public long UserId { get; set; }

    [Column("SystemRoleId")]
    public int SystemRoleId { get; set; }

    [Column("RequireMfa")]
    public bool RequireMfa { get; set; }

    [Column("GrantedBy")]
    public long? GrantedBy { get; set; }

    [Column("GrantedAt")]
    public DateTime GrantedAt { get; set; }

    [Column("RevokedBy")]
    public long? RevokedBy { get; set; }

    [Column("RevokedAt")]
    public DateTime? RevokedAt { get; set; }
}
