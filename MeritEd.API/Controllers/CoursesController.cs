using MeritEd.API.Data;
using MeritEd.API.DTOs.Courses;
using MeritEd.Core.Enums;
using MeritEd.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
namespace MeritEd.API.Controllers;

[ApiController]
[Route("api/courses")]
[Authorize]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(sub!);
    }

    private bool IsInstructor()
    {
        return User.IsInRole("Instructor");
    }

    [HttpGet]
    public async Task<IActionResult> GetOpenCourses()
    {
        var courses = await _courseService.GetOpenCoursesAsync();

        var response = courses.Select(c => new CourseResponse
        {
            Id = c.Id,
            Title = c.Title,
            Description = c.Description,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            Status = c.Status,
            RecoveryMultiplier = c.RecoveryMultiplier,
            InstructorId = c.InstructorId,
            InstructorName = c.Instructor.DisplayName,
            CreatedAt = c.CreatedAt
        });

        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCourse(Guid id)
    {
        var course = await _courseService.GetCourseByIdAsync(id);
        if (course == null) return NotFound();

        return Ok(new CourseResponse
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            StartDate = course.StartDate,
            EndDate = course.EndDate,
            Status = course.Status,
            RecoveryMultiplier = course.RecoveryMultiplier,
            InstructorId = course.InstructorId,
            InstructorName = course.Instructor.DisplayName,
            CreatedAt = course.CreatedAt
        });
    }

    [HttpPost]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> CreateCourse(CreateCourseRequest request)
    {
        var instructorId = GetCurrentUserId();
        var instructor = await _courseService.GetInstructorCoursesAsync(instructorId);

        var course = await _courseService.CreateCourseAsync(
            instructorId,
            request.Title,
            request.Description,
            request.StartDate,
            request.EndDate);

        return CreatedAtAction(nameof(GetCourse), new { id = course.Id }, new CourseResponse
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            StartDate = course.StartDate,
            EndDate = course.EndDate,
            Status = course.Status,
            RecoveryMultiplier = course.RecoveryMultiplier,
            InstructorId = course.InstructorId,
            InstructorName = course.Instructor?.DisplayName ?? string.Empty,
            CreatedAt = course.CreatedAt
        });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> UpdateCourse(Guid id, UpdateCourseRequest request)
    {
        var instructorId = GetCurrentUserId();

        var course = await _courseService.UpdateCourseAsync(
            id, instructorId,
            request.Title, request.Description,
            request.StartDate, request.EndDate,
            request.Status, request.RecoveryMultiplier);

        if (course == null)
            return NotFound(new { error = "Course not found or you do not own this course." });

        return Ok(new CourseResponse
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            StartDate = course.StartDate,
            EndDate = course.EndDate,
            Status = course.Status,
            RecoveryMultiplier = course.RecoveryMultiplier,
            InstructorId = course.InstructorId,
            InstructorName = course.Instructor?.DisplayName ?? string.Empty,
            CreatedAt = course.CreatedAt
        });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> DeleteCourse(Guid id)
    {
        var instructorId = GetCurrentUserId();
        var result = await _courseService.DeleteCourseAsync(id, instructorId);

        if (!result)
            return NotFound(new { error = "Course not found or you do not own this course." });

        return Ok(new { message = "Course archived successfully." });
    }

}