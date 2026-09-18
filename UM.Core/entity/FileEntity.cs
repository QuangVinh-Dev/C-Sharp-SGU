using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UM.Core.entity
{
    /// <summary>
    /// Ánh xạ bảng Files trong um_database
    /// </summary>
    [Table("Files")]
    public class FileEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long OwnerId { get; set; }

        public long? FolderId { get; set; }

        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Extension { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string MimeType { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? DetectedMimeType { get; set; }

        public long SizeBytes { get; set; }

        public byte[] Sha256Hash { get; set; } = Array.Empty<byte>();

        [Required]
        [MaxLength(20)]
        public string StorageProvider { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string StorageIdentifier { get; set; } = string.Empty;

        public byte ScanStatus { get; set; }

        public DateTime? ScannedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(OwnerId))]
        public User Owner { get; set; } = null!;
    }
}
