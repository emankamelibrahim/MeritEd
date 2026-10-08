using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MeritEd.API.DTOs.Enrollments;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Controllers;

[ApiController]
[Route("api/courses/{courseId}")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(sub!);
    }

    [HttpPost("enroll")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> SelfEnroll(Guid courseId)
    {
        var studentId = GetCurrentUserId();
        var (success, error, enrollment) = await _enrollmentService.SelfEnrollAsync(
            studentId, courseId);

        if (!success)
            return BadRequest(new { error });

        return Ok(new EnrollmentResponse
        {
            Id = enrollment!.Id,
            StudentId = enrollment.StudentId,
            StudentName = enrollment.Student?.DisplayName ?? string.Empty,
            StudentEmail = enrollment.Student?.Email ?? string.Empty,
            CourseId = enrollment.CourseId,
            Status = enrollment.Status.ToString(),
            TotalXP = enrollment.TotalXP,
            EnrolledAt = enrollment.EnrolledAt
        });
    }

    [HttpDelete("enroll")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> LeaveCoure(Guid courseId)
    {
        var studentId = GetCurrentUserId();
        var (success, error) = await _enrollmentService.LeaveCoursAsync(studentId, courseId);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { message = "Successfully left the course." });
    }

    [HttpGet("enrollments")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> GetEnrollments(Guid courseId)
    {
        var instructorId = GetCurrentUserId();
        var enrollments = await _enrollmentService.GetCourseEnrollmentsAsync(
            courseId, instructorId);

        var response = enrollments.Select(e => new EnrollmentResponse
        {
            Id = e.Id,
            StudentId = e.StudentId,
            StudentName = e.Student.DisplayName,
            StudentEmail = e.Student.Email!,
            CourseId = e.CourseId,
            Status = e.Status.ToString(),
            TotalXP = e.TotalXP,
            IdentityType = e.IdentityType?.ToString(),
            EnrolledAt = e.EnrolledAt
        });

        return Ok(response);
    }

    [HttpPost("enrollments")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> ManualEnroll(Guid courseId, ManualEnrollRequest request)
    {
        var instructorId = GetCurrentUserId();
        var (success, error, enrollment) = await _enrollmentService.ManualEnrollAsync(
            instructorId, courseId, request.StudentId);

        if (!success)
            return BadRequest(new { error });

        return Ok(new EnrollmentResponse
        {
            Id = enrollment!.Id,
            StudentId = enrollment.StudentId,
            StudentName = enrollment.Student?.DisplayName ?? string.Empty,
            StudentEmail = enrollment.Student?.Email ?? string.Empty,
            CourseId = enrollment.CourseId,
            Status = enrollment.Status.ToString(),
            TotalXP = enrollment.TotalXP,
            EnrolledAt = enrollment.EnrolledAt
        });
    }

    [HttpDelete("enrollments/{studentId}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> RemoveStudent(Guid courseId, Guid studentId)
    {
        var instructorId = GetCurrentUserId();
        var (success, error) = await _enrollmentService.RemoveStudentAsync(
            instructorId, courseId, studentId);

        if (!success)
            return BadRequest(new { error });

        return Ok(new { message = "Student removed from course." });
    }
}