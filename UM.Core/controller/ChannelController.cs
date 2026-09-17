using Microsoft.AspNetCore.Mvc;
using UM.Core.dto;
using UM.Core.sercurity;
using UM.Core.service;

namespace UM.Core.controller
{
    [ApiController]
    public class ChannelController : ControllerBase
    {
        private readonly IChannelService _channelService;
        private readonly ICurrentUserService _currentUserService;

        public ChannelController(IChannelService channelService, ICurrentUserService currentUserService)
        {
            _channelService = channelService;
            _currentUserService = currentUserService;
        }

        [HttpPost("api/v1/servers/{serverId}/channels")]
        public async Task<ActionResult<ChannelResponseDto>> CreateChannel(long serverId, [FromBody] CreateChannelRequest request)
        {
            var currentUserId = _currentUserService.GetRequiredUserId();
            var result = await _channelService.CreateChannelAsync(serverId, request, currentUserId);
            return StatusCode(201, result);
        }

        [HttpGet("api/v1/servers/{serverId}/channels")]
        public async Task<ActionResult<List<ChannelResponseDto>>> GetChannels(long serverId)
        {
            var currentUserId = _currentUserService.GetRequiredUserId();
            var result = await _channelService.GetChannelsByServerAsync(serverId, currentUserId);
            return Ok(result);
        }

        [HttpPatch("api/v1/channels/{channelId}")]
        public async Task<ActionResult<ChannelResponseDto>> UpdateChannel(long channelId, [FromBody] UpdateChannelRequest request)
        {
            var currentUserId = _currentUserService.GetRequiredUserId();
            var result = await _channelService.UpdateChannelAsync(channelId, request, currentUserId);
            return Ok(result);
        }

        [HttpDelete("api/v1/channels/{channelId}")]
        public async Task<IActionResult> DeleteChannel(long channelId)
        {
            var currentUserId = _currentUserService.GetRequiredUserId();
            await _channelService.DeleteChannelAsync(channelId, currentUserId);
            return NoContent();
        }
    }
}
