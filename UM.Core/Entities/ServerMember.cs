using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("ServerMembers")]
public class ServerMember
{
    [PrimaryKey, Identity]
    public long Id { get; set; }

    [Column("ServerId")]
    public long ServerId { get; set; }

    [Column("UserId")]
    public long UserId { get; set; }

    [Column("ServerRoleId")]
    public long? ServerRoleId { get; set; }

    [Column("Nickname")]
    public string? Nickname { get; set; }

    [Column("JoinedAt")]
    public DateTime JoinedAt { get; set; }

    [Column("IsBanned")]
    public bool IsBanned { get; set; }

    [Column("BannedAt")]
    public DateTime? BannedAt { get; set; }

    [Column("BannedBy")]
    public long? BannedBy { get; set; }

    [Column("BanReason")]
    public string? BanReason { get; set; }

    [Column("LeftAt")]
    public DateTime? LeftAt { get; set; }
}