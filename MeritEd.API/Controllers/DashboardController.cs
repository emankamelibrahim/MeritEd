using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MeritEd.API.Services;

namespace MeritEd.API.Controllers;

[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(sub!);
    }

    [HttpGet("api/dashboard/student")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetStudentDashboard()
    {
        return Ok(await _dashboardService.GetStudentDashboardAsync(GetCurrentUserId()));
    }

    [HttpGet("api/dashboard/instructor")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> GetInstructorDashboard()
    {
        return Ok(await _dashboardService.GetInstructorDashboardAsync(GetCurrentUserId()));
    }

    [HttpGet("api/courses/{courseId}/dashboard")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> GetCourseDashboard(Guid courseId)
    {
        var result = await _dashboardService.GetCourseDashboardAsync(
            courseId, GetCurrentUserId());

        if (result == null)
            return NotFound(new { error = "Course not found or you do not own this course." });

        return Ok(result);
    }
}