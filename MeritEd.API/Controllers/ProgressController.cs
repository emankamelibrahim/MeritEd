using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MeritEd.API.DTOs.Progress;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Controllers;

[ApiController]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;

    public ProgressController(IProgressService progressService)
    {
        _progressService = progressService;
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(sub!);
    }

    [HttpGet("api/courses/{courseId}/my-progress")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyProgress(Guid courseId)
    {
        var studentId = GetCurrentUserId();
        var enrollment = await _progressService.GetStudentProgressAsync(studentId, courseId);

        if (enrollment == null)
            return NotFound(new { error = "You are not enrolled in this course." });

        var transactions = await _progressService.GetXPTransactionsAsync(studentId, courseId);

        var breakdown = transactions
            .GroupBy(t => t.Category)
            .Select(g => new XPBreakdownItem
            {
                Category = g.Key.ToString(),
                TotalXP = g.Sum(t => t.Amount),
                Percentage = enrollment.TotalXP > 0
                    ? Math.Round((double)g.Sum(t => t.Amount) / enrollment.TotalXP * 100, 1)
                    : 0
            })
            .ToList();

        return Ok(new ProgressResponse
        {
            EnrollmentId = enrollment.Id,
            StudentId = enrollment.StudentId,
            StudentName = enrollment.Student.DisplayName,
            CourseId = enrollment.CourseId,
            TotalXP = enrollment.TotalXP,
            IdentityType = enrollment.IdentityType?.ToString(),
            EnrolledAt = enrollment.EnrolledAt,
            XPBreakdown = breakdown
        });
    }

    [HttpGet("api/courses/{courseId}/my-progress/transactions")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyTransactions(Guid courseId)
    {
        var studentId = GetCurrentUserId();
        var transactions = await _progressService.GetXPTransactionsAsync(studentId, courseId);

        return Ok(transactions.Select(t => new XPTransactionResponse
        {
            Id = t.Id,
            Amount = t.Amount,
            Category = t.Category.ToString(),
            SourceId = t.SourceId,
            CreatedAt = t.CreatedAt
        }));
    }

    [HttpGet("api/courses/{courseId}/enrollments/{studentId}/progress")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> GetStudentProgress(Guid courseId, Guid studentId)
    {
        var instructorId = GetCurrentUserId();
        var enrollment = await _progressService.GetStudentProgressForInstructorAsync(
            instructorId, courseId, studentId);

        if (enrollment == null)
            return NotFound(new { error = "Student not found or you do not own this course." });

        var transactions = await _progressService.GetXPTransactionsAsync(studentId, courseId);

        var breakdown = transactions
            .GroupBy(t => t.Category)
            .Select(g => new XPBreakdownItem
            {
                Category = g.Key.ToString(),
                TotalXP = g.Sum(t => t.Amount),
                Percentage = enrollment.TotalXP > 0
                    ? Math.Round((double)g.Sum(t => t.Amount) / enrollment.TotalXP * 100, 1)
                    : 0
            })
            .ToList();

        return Ok(new ProgressResponse
        {
            EnrollmentId = enrollment.Id,
            StudentId = enrollment.StudentId,
            StudentName = enrollment.Student.DisplayName,
            CourseId = enrollment.CourseId,
            TotalXP = enrollment.TotalXP,
            IdentityType = enrollment.IdentityType?.ToString(),
            EnrolledAt = enrollment.EnrolledAt,
            XPBreakdown = breakdown
        });
    }
}