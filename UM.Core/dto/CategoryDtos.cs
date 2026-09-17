using System.ComponentModel.DataAnnotations;

namespace UM.Core.dto
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        public int Position { get; set; } = 0;
    }

    public class UpdateCategoryRequest
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Category name must be between 1 and 100 characters.")]
        public string? Name { get; set; }

        public int? Position { get; set; }
    }

    public class CategoryResponseDto
    {
        public long Id { get; set; }
        public long ServerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Position { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ChannelResponseDto> Channels { get; set; } = new();
    }
}
