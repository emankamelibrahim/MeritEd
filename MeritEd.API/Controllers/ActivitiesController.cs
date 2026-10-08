using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MeritEd.API.DTOs.Activities;
using MeritEd.Core.Entities;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Controllers;

[ApiController]
[Authorize]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivitiesController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(sub!);
    }

    private static ActivityResponse MapToResponse(Activity activity)
    {
        return new ActivityResponse
        {
            Id = activity.Id,
            EnrollmentId = activity.EnrollmentId,
            ContentItemId = activity.ContentItemId,
            ContentItemTitle = activity.ContentItem?.Title ?? string.Empty,
            ContentItemType = activity.ContentItem?.GetType().Name ?? string.Empty,
            IsRecovery = activity.IsRecovery,
            Status = activity.Status.ToString(),
            XPAwarded = activity.XPAwarded,
            SubmittedAt = activity.SubmittedAt,
            GradedAt = activity.GradedAt,
            StudentName = activity.Enrollment?.Student?.DisplayName,
            StudentEmail = activity.Enrollment?.Student?.Email
        };
    }

    [HttpPost("api/items/{contentItemId}/submit")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> Submit(Guid contentItemId)
    {
        var studentId = GetCurrentUserId();
        var (success, error, activity) = await _activityService.SubmitAsync(
            studentId, contentItemId);

        if (!success)
            return BadRequest(new { error });

        return Ok(MapToResponse(activity!));
    }

    [HttpGet("api/courses/{courseId}/activities")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> GetCourseActivities(Guid courseId)
    {
        var instructorId = GetCurrentUserId();
        var activities = await _activityService.GetCourseActivitiesAsync(courseId, instructorId);
        return Ok(activities.Select(MapToResponse));
    }

    [HttpGet("api/items/{contentItemId}/activities")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> GetItemActivities(Guid contentItemId)
    {
        var instructorId = GetCurrentUserId();
        var activities = await _activityService.GetItemActivitiesAsync(contentItemId, instructorId);
        return Ok(activities.Select(MapToResponse));
    }

    [HttpPut("api/activities/{activityId}/approve")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Approve(Guid activityId)
    {
        var instructorId = GetCurrentUserId();
        var (success, error, activity) = await _activityService.ApproveAsync(
            instructorId, activityId);

        if (!success)
            return BadRequest(new { error });

        return Ok(MapToResponse(activity!));
    }

    [HttpPut("api/activities/{activityId}/reject")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> Reject(Guid activityId)
    {
        var instructorId = GetCurrentUserId();
        var (success, error, activity) = await _activityService.RejectAsync(
            instructorId, activityId);

        if (!success)
            return BadRequest(new { error });

        return Ok(MapToResponse(activity!));
    }
}