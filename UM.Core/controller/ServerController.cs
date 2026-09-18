using Microsoft.AspNetCore.Mvc;
using UM.Core.dto;
using UM.Core.sercurity;
using UM.Core.service;

namespace UM.Core.controller
{
    [ApiController]
    [Route("api/v1/servers")]
    public class ServerController : ControllerBase
    {
        private readonly IServerService _serverService;
        private readonly ICurrentUserService _currentUserService;

        public ServerController(IServerService serverService, ICurrentUserService currentUserService)
        {
            _serverService = serverService;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Tạo mới Server (Tự động tạo Owner/Member role, Category và Channel mặc định trong 1 transaction)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ServerDetailDto>> CreateServer([FromBody] CreateServerRequest request)
        {
            var currentUserId = _currentUserService.GetRequiredUserId();
            var result = await _serverService.CreateServerAsync(request, currentUserId);
            return CreatedAtAction(nameof(GetServerDetail), new { id = result.Id }, result);
        }

        /// <summary>
        /// Lấy danh sách tất cả servers (dành cho Admin / Quản trị hệ thống)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<ServerSummaryDto>>> GetAllServers([FromQuery] bool includeDeleted = false)
        {
            var result = await _serverService.GetAllServersAsync(includeDeleted);
            return Ok(result);
        }

        /// <summary>
        /// Lấy danh sách server của người dùng cho Sidebar (chỉ lấy member active, không load channels)
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<List<ServerSidebarDto>>> GetMyServers()
        {
            var currentUserId = _currentUserService.GetRequiredUserId();
            var result = await _serverService.GetMyServersAsync(currentUserId);
            return Ok(result);
        }

        /// <summary>
        /// Lấy chi tiết Server kèm Categories và Channels sắp xếp theo Position
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ServerDetailDto>> GetServerDetail(long id)
        {
            var currentUserId = _currentUserService.GetRequiredUserId();
            var result = await _serverService.GetServerDetailAsync(id, currentUserId);
            return Ok(result);
        }

        /// <summary>
        /// Cập nhật thông tin Server (Yêu cầu Owner hoặc MANAGE_SERVER)
        /// </summary>
        [HttpPatch("{id}")]
        public async Task<ActionResult<ServerDetailDto>> UpdateServer(long id, [FromBody] UpdateServerRequest request)
        {
            var currentUserId = _currentUserService.GetRequiredUserId();
            var result = await _serverService.UpdateServerAsync(id, request, currentUserId);
            return Ok(result);
        }

        /// <summary>
        /// Xóa Server (Soft delete - Chỉ Owner được phép)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteServer(long id)
        {
            var currentUserId = _currentUserService.GetRequiredUserId();
            await _serverService.DeleteServerAsync(id, currentUserId);
            return NoContent();
        }

        /// <summary>
        /// Chuyển quyền sở hữu Server cho thành viên khác (Chỉ Owner được phép)
        /// </summary>
        [HttpPost("{id}/transfer-ownership")]
        public async Task<IActionResult> TransferOwnership(long id, [FromBody] TransferOwnershipRequest request)
        {
            var currentUserId = _currentUserService.GetRequiredUserId();
            await _serverService.TransferOwnershipAsync(id, request, currentUserId);
            return Ok(new { message = "Ownership transferred successfully." });
        }
    }
}
