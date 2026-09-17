using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class AdminsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAdmins() => Ok(new { success = true, data = "List admins" });

    [HttpPost]
    public IActionResult CreateAdmin() => Ok(new { success = true, data = "Create admin" });

    [HttpPut("{id}")]
    public IActionResult UpdateAdmin(long id) => Ok(new { success = true, data = "Update admin" });

    [HttpDelete("{id}")]
    public IActionResult DeleteAdmin(long id) => Ok(new { success = true, data = "Delete admin" });
}
