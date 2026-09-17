using System.ComponentModel.DataAnnotations;

namespace UM.Core.dto
{
    public class CreateChannelRequest
    {
        [Required(ErrorMessage = "Channel name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Channel name must be between 1 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        public long? CategoryId { get; set; }

        [Range(1, 2, ErrorMessage = "Channel Type must be 1 (Text) or 2 (Voice).")]
        public byte Type { get; set; } = 1;

        public int Position { get; set; } = 0;
    }

    public class UpdateChannelRequest
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Channel name must be between 1 and 100 characters.")]
        public string? Name { get; set; }

        public long? CategoryId { get; set; }

        public int? Position { get; set; }
    }

    public class ChannelResponseDto
    {
        public long Id { get; set; }
        public long ServerId { get; set; }
        public long? CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public byte Type { get; set; }
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
