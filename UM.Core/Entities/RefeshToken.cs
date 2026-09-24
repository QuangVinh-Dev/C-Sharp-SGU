using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("RefreshTokens")]
public class RefreshToken
{
    [PrimaryKey]
    public Guid Id { get; set; }

    [Column("UserId")]
    public long UserId { get; set; }

    [Column("TokenHash")]
    public byte[] TokenHash { get; set; } = [];

    [Column("ExpiresAt")]
    public DateTime ExpiresAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("CreatedByIp")]
    public string? CreatedByIp { get; set; }

    [Column("RevokedAt")]
    public DateTime? RevokedAt { get; set; }

    [Column("RevokedByIp")]
    public string? RevokedByIp { get; set; }

    [Column("ReplacedByTokenId")]
    public Guid? ReplacedByTokenId { get; set; }

    public User User { get; set; } = null!;
}