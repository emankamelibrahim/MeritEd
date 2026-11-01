using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MeritEd.API.DTOs.Discussions;
using MeritEd.Core.Entities;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Controllers;

[ApiController]
[Route("api/courses/{courseId}/discussions")]
[Authorize]
public class DiscussionsController : ControllerBase
{
    private readonly IDiscussionService _discussionService;

    public DiscussionsController(IDiscussionService discussionService)
    {
        _discussionService = discussionService;
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(sub!);
    }

    private static ReplyResponse MapReply(DiscussionReply reply) => new()
    {
        Id = reply.Id,
        ThreadId = reply.ThreadId,
        AuthorId = reply.AuthorId,
        AuthorName = reply.Author?.DisplayName ?? string.Empty,
        Body = reply.Body,
        IsAcceptedAnswer = reply.IsAcceptedAnswer,
        IsInstructorVerified = reply.IsInstructorVerified,
        ParentReplyId = reply.ParentReplyId,
        ChildReplies = reply.ChildReplies?.Select(MapReply).ToList() ?? new(),
        CreatedAt = reply.CreatedAt
    };

    private static ThreadResponse MapThread(DiscussionThread thread,
        bool includeReplies = false) => new()
        {
            Id = thread.Id,
            CourseId = thread.CourseId,
            AuthorId = thread.AuthorId,
            AuthorName = thread.Author?.DisplayName ?? string.Empty,
            Title = thread.Title,
            Body = thread.Body,
            Tags = thread.Tags,
            IsClosed = thread.IsClosed,
            ReplyCount = thread.Replies?.Count ?? 0,
            CreatedAt = thread.CreatedAt,
            Replies = includeReplies
            ? thread.Replies?
                .Where(r => r.ParentReplyId == null)
                .Select(MapReply)
                .ToList() ?? new()
            : new()
        };

    [HttpGet]
    public async Task<IActionResult> GetThreads(Guid courseId)
    {
        var userId = GetCurrentUserId();
        var threads = await _discussionService.GetThreadsAsync(courseId, userId);
        return Ok(threads.Select(t => MapThread(t, false)));
    }

    [HttpPost]
    public async Task<IActionResult> CreateThread(Guid courseId, CreateThreadRequest request)
    {
        var userId = GetCurrentUserId();
        var (success, error, thread) = await _discussionService.CreateThreadAsync(
            userId, courseId, request.Title, request.Body, request.Tags);

        if (!success) return BadRequest(new { error });

        return CreatedAtAction(nameof(GetThreads), new { courseId }, MapThread(thread!));
    }

    [HttpGet("{threadId}")]
    public async Task<IActionResult> GetThread(Guid courseId, Guid threadId)
    {
        var userId = GetCurrentUserId();
        var thread = await _discussionService.GetThreadByIdAsync(threadId, userId);

        if (thread == null) return NotFound();

        return Ok(MapThread(thread, includeReplies: true));
    }

    [HttpDelete("{threadId}")]
    public async Task<IActionResult> DeleteThread(Guid courseId, Guid threadId)
    {
        var userId = GetCurrentUserId();
        var (success, error) = await _discussionService.DeleteThreadAsync(threadId, userId);

        if (!success) return BadRequest(new { error });

        return Ok(new { message = "Thread deleted successfully." });
    }

    [HttpPost("{threadId}/replies")]
    public async Task<IActionResult> CreateReply(Guid courseId, Guid threadId,
        CreateReplyRequest request)
    {
        var userId = GetCurrentUserId();
        var (success, error, reply) = await _discussionService.CreateReplyAsync(
            userId, threadId, request.Body, request.ParentReplyId);

        if (!success) return BadRequest(new { error });

        return CreatedAtAction(nameof(GetThread),
            new { courseId, threadId }, MapReply(reply!));
    }
    [HttpPut("{threadId}/replies/{replyId}/verify")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult> VerifyReply(Guid courseId, Guid threadId, Guid replyId)
    {
        var userId = GetCurrentUserId();
        var (success, error) = await _discussionService.VerifyReplyAsync(
            threadId, replyId, userId);

        if (!success) return BadRequest(new { error });

        return Ok(new { message = "Reply verification status updated." });
    }

    [HttpPut("{threadId}/replies/{replyId}/accept")]
    public async Task<IActionResult> MarkAcceptedAnswer(Guid courseId,
        Guid threadId, Guid replyId)
    {
        var userId = GetCurrentUserId();
        var (success, error) = await _discussionService.MarkAcceptedAnswerAsync(
            threadId, replyId, userId);

        if (!success) return BadRequest(new { error });

        return Ok(new { message = "Reply marked as accepted answer." });
    }
}