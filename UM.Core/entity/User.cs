using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UM.Core.entity
{
    /// <summary>
    /// Ánh xạ chính xác 100% bảng Users trong um_database
    /// </summary>
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(16)]
        public string PublicCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public bool IsEmailVerified { get; set; }

        public int FailedLoginCount { get; set; }

        public DateTime? LockedUntil { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public bool IsSuspended { get; set; }

        public DateTime? SuspendedUntil { get; set; }

        // Navigation properties
        public ICollection<Server> OwnedServers { get; set; } = new List<Server>();
        public ICollection<ServerMember> ServerMemberships { get; set; } = new List<ServerMember>();
    }
}
