using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UM.Core.entity
{
    /// <summary>
    /// Ánh xạ chuẩn 100% bảng Servers trong um_database
    /// </summary>
    [Table("Servers")]
    public class Server
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public long OwnerId { get; set; }

        public long? IconFileId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        public DateTime? ScheduledDeleteAt { get; set; }

        public bool IsSuspended { get; set; }

        public DateTime? SuspendedUntil { get; set; }

        // Navigation properties
        [ForeignKey(nameof(OwnerId))]
        public User Owner { get; set; } = null!;

        [ForeignKey(nameof(IconFileId))]
        public FileEntity? IconFile { get; set; }

        public ICollection<ServerMember> ServerMembers { get; set; } = new List<ServerMember>();
        public ICollection<ServerRole> ServerRoles { get; set; } = new List<ServerRole>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
        public ICollection<Channel> Channels { get; set; } = new List<Channel>();
        public ICollection<ServerSanction> ServerSanctions { get; set; } = new List<ServerSanction>();
    }
}
