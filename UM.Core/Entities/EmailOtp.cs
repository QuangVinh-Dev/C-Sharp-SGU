using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("EmailOtps")]
public class EmailOtp
{
    [PrimaryKey, Identity]
    public long Id { get; set; }

    [Column("UserId")]
    public long UserId { get; set; }

    [Column("CodeHash")]
    public byte[] CodeHash { get; set; } = [];

    [Column("Purpose")]
    public string Purpose { get; set; } = string.Empty;

    [Column("ExpiresAt")]
    public DateTime ExpiresAt { get; set; }

    [Column("AttemptCount")]
    public int AttemptCount { get; set; }

    [Column("MaxAttempts")]
    public int MaxAttempts { get; set; }

    [Column("UsedAt")]
    public DateTime? UsedAt { get; set; }

    [Column("RequestIp")]
    public string? RequestIp { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;
}