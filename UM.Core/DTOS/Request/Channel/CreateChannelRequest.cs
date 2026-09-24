using UM.Core.Entities;

namespace UM.Core.DTOS.Request.Channel;

public class CreateChannelRequest
{
    public string Name { get; set; } = string.Empty;

    public ChannelType Type { get; set; }
}