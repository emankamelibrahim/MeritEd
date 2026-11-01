using Microsoft.EntityFrameworkCore;
using MeritEd.API.Data;
using MeritEd.Core.Entities;
using MeritEd.Core.Enums;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Services;

public class DiscussionService : IDiscussionService
{
    private readonly AppDbContext _context;

    public DiscussionService(AppDbContext context)
    {
        _context = context;
    }

    private async Task<bool> IsEnrolledOrInstructorAsync(Guid userId, Guid courseId)
    {
        var isEnrolled = await _context.Enrollments
            .AnyAsync(e => e.StudentId == userId && e.CourseId == courseId
                && e.Status == EnrollmentStatus.Active);

        if (isEnrolled) return true;

        return await _context.Courses
            .AnyAsync(c => c.Id == courseId && c.InstructorId == userId);
    }

    public async Task<(bool Success, string Error, DiscussionThread? Thread)> CreateThreadAsync(
        Guid authorId, Guid courseId, string title, string body, string? tags)
    {
        if (!await IsEnrolledOrInstructorAsync(authorId, courseId))
            return (false, "You are not enrolled in this course.", null);

        var thread = new DiscussionThread
        {
            Id = Guid.NewGuid(),
            CourseId = courseId,
            AuthorId = authorId,
            Title = title,
            Body = body,
            Tags = tags,
            IsClosed = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.DiscussionThreads.Add(thread);
        await _context.SaveChangesAsync();

        return (true, string.Empty, await _context.DiscussionThreads
            .Include(t => t.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == thread.Id));
    }

    public async Task<IEnumerable<DiscussionThread>> GetThreadsAsync(Guid courseId, Guid userId)
    {
        if (!await IsEnrolledOrInstructorAsync(userId, courseId))
            return Enumerable.Empty<DiscussionThread>();

        return await _context.DiscussionThreads
            .Include(t => t.Author)
            .Include(t => t.Replies)
            .Where(t => t.CourseId == courseId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<DiscussionThread?> GetThreadByIdAsync(Guid threadId, Guid userId)
    {
        var thread = await _context.DiscussionThreads
            .Include(t => t.Author)
            .Include(t => t.Replies)
                .ThenInclude(r => r.Author)
            .Include(t => t.Replies)
                .ThenInclude(r => r.ChildReplies)
                    .ThenInclude(cr => cr.Author)
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread == null) return null;

        if (!await IsEnrolledOrInstructorAsync(userId, thread.CourseId))
            return null;

        return thread;
    }

    public async Task<(bool Success, string Error, DiscussionReply? Reply)> CreateReplyAsync(
        Guid authorId, Guid threadId, string body, Guid? parentReplyId)
    {
        var thread = await _context.DiscussionThreads
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread == null)
            return (false, "Thread not found.", null);

        if (thread.IsClosed)
            return (false, "This thread is closed.", null);

        if (!await IsEnrolledOrInstructorAsync(authorId, thread.CourseId))
            return (false, "You are not enrolled in this course.", null);

        // Validate parent reply exists and belongs to this thread
        if (parentReplyId.HasValue)
        {
            var parentReply = await _context.DiscussionReplies
                .FirstOrDefaultAsync(r => r.Id == parentReplyId && r.ThreadId == threadId);

            if (parentReply == null)
                return (false, "Parent reply not found.", null);

            // Enforce one level nesting — parent must be a top-level reply
            if (parentReply.ParentReplyId.HasValue)
                return (false, "Replies can only be nested one level deep.", null);
        }

        var reply = new DiscussionReply
        {
            Id = Guid.NewGuid(),
            ThreadId = threadId,
            AuthorId = authorId,
            Body = body,
            IsAcceptedAnswer = false,
            IsInstructorVerified = false,
            ParentReplyId = parentReplyId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.DiscussionReplies.Add(reply);
        await _context.SaveChangesAsync();

        return (true, string.Empty, await _context.DiscussionReplies
            .Include(r => r.Author)
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == reply.Id));
    }
    public async Task<(bool Success, string Error)> MarkAcceptedAnswerAsync(
        Guid threadId, Guid replyId, Guid userId)
    {
        var thread = await _context.DiscussionThreads
            .Include(t => t.Replies)
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread == null)
            return (false, "Thread not found.");

        if (thread.AuthorId != userId)
            return (false, "Only the thread author can mark an accepted answer.");

        var reply = thread.Replies.FirstOrDefault(r => r.Id == replyId);
        if (reply == null)
            return (false, "Reply not found.");

        if (reply.ParentReplyId != null)
            return (false, "Only top-level replies can be marked as the accepted answer.");

        foreach (var r in thread.Replies)
        {
            r.IsAcceptedAnswer = false;
            r.UpdatedAt = DateTime.UtcNow;
        }

        reply.IsAcceptedAnswer = true;
        reply.UpdatedAt = DateTime.UtcNow;
        thread.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }
    public async Task<(bool Success, string Error)> VerifyReplyAsync(
    Guid threadId, Guid replyId, Guid instructorId)
    {
        var thread = await _context.DiscussionThreads
            .Include(t => t.Course)
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread == null)
            return (false, "Thread not found.");

        if (thread.Course.InstructorId != instructorId)
            return (false, "Only the course instructor can verify replies.");

        var reply = await _context.DiscussionReplies
            .FirstOrDefaultAsync(r => r.Id == replyId && r.ThreadId == threadId);

        if (reply == null)
            return (false, "Reply not found.");

        reply.IsInstructorVerified = !reply.IsInstructorVerified;
        reply.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }
    public async Task<(bool Success, string Error)> DeleteThreadAsync(
        Guid threadId, Guid userId)
    {
        var thread = await _context.DiscussionThreads
            .Include(t => t.Course)
            .FirstOrDefaultAsync(t => t.Id == threadId);

        if (thread == null)
            return (false, "Thread not found.");

        var isAuthor = thread.AuthorId == userId;
        var isInstructor = thread.Course.InstructorId == userId;

        if (!isAuthor && !isInstructor)
            return (false, "You do not have permission to delete this thread.");

        _context.DiscussionThreads.Remove(thread);
        await _context.SaveChangesAsync();
        return (true, string.Empty);
    }
}