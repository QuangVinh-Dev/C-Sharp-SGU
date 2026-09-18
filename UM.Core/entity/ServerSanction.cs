using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UM.Core.entity
{
    /// <summary>
    /// Ánh xạ chuẩn 100% bảng ServerSanctions trong um_database
    /// </summary>
    [Table("ServerSanctions")]
    public class ServerSanction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long ServerId { get; set; }

        public byte Type { get; set; } = 1;

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        public long IssuedBy { get; set; }

        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }

        public long? RevokedBy { get; set; }

        public DateTime? RevokedAt { get; set; }

        [MaxLength(500)]
        public string? RevokeReason { get; set; }

        // Navigation properties
        [ForeignKey(nameof(ServerId))]
        public Server Server { get; set; } = null!;

        [ForeignKey(nameof(IssuedBy))]
        public User Issuer { get; set; } = null!;

        [ForeignKey(nameof(RevokedBy))]
        public User? Revoker { get; set; }
    }
}
