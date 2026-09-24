using LinqToDB.Mapping;

namespace UM.Core.Entities;

[Table("UserProfiles")]
public class UserProfile
{
    [PrimaryKey, Identity]
    public long Id { get; set; }

    [Column("UserId")]
    public long UserId { get; set; }

    [Column("AvatarFileId")]
    public long? AvatarFileId { get; set; }

    [Column("FullName")]
    public string? FullName { get; set; }

    [Column("DateOfBirth")]
    public DateTime? DateOfBirth { get; set; }

    [Column("PhoneNumber")]
    public string? PhoneNumber { get; set; }

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }

    public User User { get; set; } = null!;
}