using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UM.Core.entity
{
    /// <summary>
    /// Ánh xạ chuẩn 100% bảng Channels trong um_database
    /// Type: 1 = TEXT, 2 = VOICE (CK_Channels_Type)
    /// </summary>
    [Table("Channels")]
    public class Channel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long ServerId { get; set; }

        public long? CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public byte Type { get; set; } = 1; // 1: Text, 2: Voice

        public int Position { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        [ForeignKey(nameof(ServerId))]
        public Server Server { get; set; } = null!;

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }
    }
}
