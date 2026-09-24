using UM.Core.DTOS.Request.Channel;
using UM.Core.DTOS.Response.Channel;

namespace UM.Core.Service;

public interface IChannelService
{
	Task<ChannelResponse> CreateChannelAsync(
		long categoryId,
		CreateChannelRequest request,
		long userId);
}