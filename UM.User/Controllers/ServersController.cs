using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ServersController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateServer() => Ok(new { success = true, data = "Create server" });

    [HttpGet("{id}")]
    public IActionResult GetServer(long id) => Ok(new { success = true, data = "Get server" });

    [HttpPut("{id}")]
    public IActionResult UpdateServer(long id) => Ok(new { success = true, data = "Update server" });

    [HttpDelete("{id}")]
    public IActionResult DeleteServer(long id) => Ok(new { success = true, data = "Delete server" });

    [HttpPost("{id}/join")]
    public IActionResult JoinServer(long id) => Ok(new { success = true, data = "Join server" });

    [HttpPost("{id}/leave")]
    public IActionResult LeaveServer(long id) => Ok(new { success = true, data = "Leave server" });
}
