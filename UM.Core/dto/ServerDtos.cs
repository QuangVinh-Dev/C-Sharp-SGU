using System.ComponentModel.DataAnnotations;

namespace UM.Core.dto
{
    public class CreateServerRequest
    {
        [Required(ErrorMessage = "Server name is required.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Server name must be between 1 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        public long? IconFileId { get; set; }
    }

    public class UpdateServerRequest
    {
        [StringLength(100, MinimumLength = 1, ErrorMessage = "Server name must be between 1 and 100 characters.")]
        public string? Name { get; set; }

        public long? IconFileId { get; set; }

        public bool? IsSuspended { get; set; }
    }

    public class TransferOwnershipRequest
    {
        [Required(ErrorMessage = "newOwnerId is required.")]
        public long NewOwnerId { get; set; }
    }

    public class ServerSidebarDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IconUrl { get; set; }
    }

    public class ServerSummaryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPublicCode { get; set; } = string.Empty;
        public long? IconFileId { get; set; }
        public int MembersCount { get; set; }
        public int ChannelsCount { get; set; }
        public string StorageUsed { get; set; } = "0 MB";
        public bool IsSuspended { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ScheduledDeleteAt { get; set; }
    }

    public class ServerDetailDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public long OwnerId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPublicCode { get; set; } = string.Empty;
        public long? IconFileId { get; set; }
        public int MembersCount { get; set; }
        public bool IsSuspended { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime? ScheduledDeleteAt { get; set; }
        public List<CategoryResponseDto> Categories { get; set; } = new();
        public List<ChannelResponseDto> Channels { get; set; } = new();
    }
}
