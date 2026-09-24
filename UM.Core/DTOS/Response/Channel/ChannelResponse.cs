using UM.Core.Entities;

namespace UM.Core.DTOS.Response.Channel;

public class ChannelResponse
{
    public long Id { get; set; }

    public long ServerId { get; set; }

    public long? CategoryId { get; set; }

    public string Name { get; set; } = string.Empty;

    public ChannelType Type { get; set; }

    public int Position { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}