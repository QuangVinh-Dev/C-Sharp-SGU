using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UM.Core.entity
{
    /// <summary>
    /// Ánh xạ chuẩn 100% bảng ServerMembers trong um_database
    /// PK: Id (Identity)
    /// Hỗ trợ lịch sử membership: LeftAt != null (đã rời), LeftAt == null (active)
    /// </summary>
    [Table("ServerMembers")]
    public class ServerMember
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long ServerId { get; set; }

        public long UserId { get; set; }

        public long? ServerRoleId { get; set; }

        [MaxLength(100)]
        public string? Nickname { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public bool IsBanned { get; set; }

        public DateTime? BannedAt { get; set; }

        public long? BannedBy { get; set; }

        [MaxLength(500)]
        public string? BanReason { get; set; }

        public DateTime? LeftAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(ServerId))]
        public Server Server { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;

        [ForeignKey(nameof(ServerRoleId))]
        public ServerRole? ServerRole { get; set; }

        [ForeignKey(nameof(BannedBy))]
        public User? BannedByUser { get; set; }
    }
}
