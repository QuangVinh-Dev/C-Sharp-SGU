using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("Users")]
public class User
{
    [PrimaryKey, Identity]
    public long Id { get; set; }

    [Column("PublicCode")]
    public string PublicCode { get; set; } = string.Empty;

    [Column("Email")]
    public string Email { get; set; } = string.Empty;

    [Column("PasswordHash")]
    public string PasswordHash { get; set; } = string.Empty;

    [Column("IsActive")]
    public bool IsActive { get; set; }

    [Column("IsEmailVerified")]
    public bool IsEmailVerified { get; set; }

    [Column("FailedLoginCount")]
    public int FailedLoginCount { get; set; }

    [Column("LockedUntil")]
    public DateTime? LockedUntil { get; set; }

    [Column("LastLoginAt")]
    public DateTime? LastLoginAt { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }

    [Column("IsSuspended")]
    public bool IsSuspended { get; set; }

    [Column("SuspendedUntil")]
    public DateTime? SuspendedUntil { get; set; }

    public UserProfile? UserProfile { get; set; }
}