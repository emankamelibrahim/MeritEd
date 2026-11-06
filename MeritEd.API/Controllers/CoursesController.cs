using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MeritEd.API.DTOs.Courses;
using MeritEd.Core.Entities;
using MeritEd.Core.Enums;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly ICourseAccessService _accessService;

    public CoursesController(ICourseService courseService, ICourseAccessService accessService)
    {
        _courseService = courseService;
        _accessService = accessService;
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(sub!);
    }

    private static CourseResponse MapToResponse(Course c, bool isEnrolled = false) => new()
    {
        Id = c.Id,
        Title = c.Title,
        Description = c.Description,
        StartDate = c.StartDate,
        EndDate = c.EndDate,
        Status = c.Status,
        RecoveryMultiplier = c.RecoveryMultiplier,
        InstructorId = c.InstructorId,
        InstructorName = c.Instructor?.DisplayName ?? string.Empty,
        IsEnrolled = isEnrolled,
        CreatedAt = c.CreatedAt
    };

    [HttpGet]
    public async Task<IActionResult> GetOpenCourses()
    {
        var courses = await _courseService.GetOpenCoursesAsync();
        var enrolledIds = await _courseService.GetEnrolledCourseIdsAsync(GetCurrentUserId());

        return Ok(courses.Select(c => MapToResponse(c, enrolledIds.Contains(c.Id))));
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyCourses()
    {
        var userId = GetCurrentUserId();

        if (User.IsInRole("Instructor"))
        {
            var owned = await _courseService.GetInstructorCoursesAsync(userId);
            return Ok(owned.Select(c => MapToResponse(c)));
        }

        var enrolled = await _courseService.GetEnrolledCoursesAsync(userId);
        return Ok(enrolled.Select(c => MapToResponse(c, true)));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetCourse(Guid id)
    {
        var access = await _accessService.GetAccessAsync(id, GetCurrentUserId());
        if (access == CourseAccess.None) return NotFound();

        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null) return NotFound();

        return Ok(MapToResponse(course, access == CourseAccess.Enrolled));
    }

    [HttpPost]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request)
    {
        var course = await _courseService.CreateCourseAsync(
            GetCurrentUserId(),
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate);

        return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, MapToResponse(course));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> UpdateCourse(Guid id, UpdateCourseRequest request)
    {
        var course = await _courseService.UpdateCourseAsync(
            id, GetCurrentUserId(),
            request.Title, request.Description,
            request.StartDate, request.EndDate,
            request.Status, request.RecoveryMultiplier);

        if (course == null)
            return NotFound(new { error = "Course not found or you do not own this course." });

        return Ok(MapToResponse(course));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> DeleteCourse(Guid id)
    {
        var result = await _courseService.DeleteCourseAsync(id, GetCurrentUserId());

        if (!result)
            return NotFound(new { error = "Course not found or you do not own this course." });

        return Ok(new { message = "Course archived successfully." });
    }
}