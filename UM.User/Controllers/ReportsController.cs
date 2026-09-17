using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BackendApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    [HttpPost]
    public IActionResult CreateReport() => Ok(new { success = true, data = "Create report" });

    [HttpGet]
    [Authorize(Policy = "reports.read")]
    public IActionResult GetReports() => Ok(new { success = true, data = "List reports" });

    [HttpGet("{id}")]
    [Authorize(Policy = "reports.read")]
    public IActionResult GetReport(long id) => Ok(new { success = true, data = "Get report by ID" });

    [HttpPost("{id}/review")]
    [Authorize(Policy = "reports.review")]
    public IActionResult ReviewReport(long id) => Ok(new { success = true, data = "Review report" });
}
