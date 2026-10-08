using Microsoft.EntityFrameworkCore;
using MeritEd.API.Data;
using MeritEd.Core.Entities;
using MeritEd.Core.Enums;
using MeritEd.Core.Interfaces.Services;
using MeritEd.Core.Services;

namespace MeritEd.API.Services;

public class ActivityService : IActivityService
{
    private readonly AppDbContext _context;
    private readonly XPService _xpService;
    private readonly IdentityService _identityService;

    public ActivityService(AppDbContext context, XPService xpService,
        IdentityService identityService)
    {
        _context = context;
        _xpService = xpService;
        _identityService = identityService;
    }

    public async Task<(bool Success, string Error, Activity? Activity)> SubmitAsync(
        Guid studentId, Guid contentItemId)
    {
        var contentItem = await _context.ContentItems
            .Include(c => c.Section)
                .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(c => c.Id == contentItemId);

        if (contentItem == null)
            return (false, "Content item not found.", null);

        if (!contentItem.IsPublished)
            return (false, "This content item is not available.", null);

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId
                && e.CourseId == contentItem.Section.CourseId
                && e.Status == EnrollmentStatus.Active);

        if (enrollment == null)
            return (false, "You are not enrolled in this course.", null);

        // Only assignments and quizzes are submittable
        if (contentItem is not Assignment && contentItem is not Quiz)
            return (false, "This content item cannot be submitted.", null);

        // Check if already submitted and approved
        var existingApproved = await _context.Activities
            .AnyAsync(a => a.EnrollmentId == enrollment.Id
                && a.ContentItemId == contentItemId
                && a.Status == ActivityStatus.Approved
                && !a.IsRecovery);

        if (existingApproved)
            return (false, "You have already completed this activity.", null);

        // Determine if this is a recovery submission
        DateTime? dueDate = contentItem is Assignment a ? a.DueDate :
            contentItem is Quiz q ? q.DueDate : null;
        bool allowsRecovery = contentItem is Assignment a2 ? a2.AllowsRecovery :
            contentItem is Quiz q2 && q2.AllowsRecovery;

        bool isRecovery = _xpService.IsRecoveryEligible(dueDate, allowsRecovery);

        // Check if already submitted a non-recovery and it's not past due
        if (!isRecovery)
        {
            var existingPending = await _context.Activities
                .AnyAsync(a => a.EnrollmentId == enrollment.Id
                    && a.ContentItemId == contentItemId
                    && a.Status == ActivityStatus.Submitted
                    && !a.IsRecovery);

            if (existingPending)
                return (false, "You have already submitted this activity.", null);
        }

        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            EnrollmentId = enrollment.Id,
            ContentItemId = contentItemId,
            IsRecovery = isRecovery,
            Status = ActivityStatus.Submitted,
            SubmittedAt = DateTime.UtcNow
        };

        _context.Activities.Add(activity);

        // Quizzes auto-award XP on submission
        if (contentItem is Quiz quiz)
        {
            var baseXP = quiz.BaseXP;
            var recoveryMultiplier = contentItem.Section.Course.RecoveryMultiplier;
            var xpAwarded = _xpService.CalculateXP(baseXP, isRecovery, recoveryMultiplier);

            activity.Status = ActivityStatus.Approved;
            activity.XPAwarded = xpAwarded;
            activity.GradedAt = DateTime.UtcNow;

            await AwardXPAsync(enrollment, xpAwarded,
                isRecovery ? XPCategory.Recovery : XPCategory.Assessment,
                activity.Id);
        }

        await _context.SaveChangesAsync();
        return (true, string.Empty, activity);
    }

    public async Task<(bool Success, string Error, Activity? Activity)> ApproveAsync(
        Guid instructorId, Guid activityId)
    {
        var activity = await _context.Activities
            .Include(a => a.Enrollment)
                .ThenInclude(e => e.Course)
            .Include(a => a.ContentItem)
                .ThenInclude(c => c.Section)
                    .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(a => a.Id == activityId);

        if (activity == null)
            return (false, "Activity not found.", null);

        if (activity.Enrollment.Course.InstructorId != instructorId)
            return (false, "You do not own this course.", null);

        if (activity.Status != ActivityStatus.Submitted)
            return (false, "Activity is not pending approval.", null);

        var baseXP = activity.ContentItem is Assignment assignment ? assignment.BaseXP :
            activity.ContentItem is Quiz quiz ? quiz.BaseXP : 0;

        var recoveryMultiplier = activity.Enrollment.Course.RecoveryMultiplier;
        var xpAwarded = _xpService.CalculateXP(baseXP, activity.IsRecovery, recoveryMultiplier);

        activity.Status = ActivityStatus.Approved;
        activity.XPAwarded = xpAwarded;
        activity.GradedAt = DateTime.UtcNow;

        await AwardXPAsync(activity.Enrollment, xpAwarded,
            activity.IsRecovery ? XPCategory.Recovery : XPCategory.Assessment,
            activity.Id);

        await _context.SaveChangesAsync();
        return (true, string.Empty, activity);
    }

    public async Task<(bool Success, string Error, Activity? Activity)> RejectAsync(
        Guid instructorId, Guid activityId)
    {
        var activity = await _context.Activities
            .Include(a => a.Enrollment)
                .ThenInclude(e => e.Course)
            .FirstOrDefaultAsync(a => a.Id == activityId);

        if (activity == null)
            return (false, "Activity not found.", null);

        if (activity.Enrollment.Course.InstructorId != instructorId)
            return (false, "You do not own this course.", null);

        if (activity.Status != ActivityStatus.Submitted)
            return (false, "Activity is not pending approval.", null);

        activity.Status = ActivityStatus.Rejected;
        activity.GradedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, string.Empty, activity);
    }

    public async Task<IEnumerable<Activity>> GetCourseActivitiesAsync(
        Guid courseId, Guid instructorId)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

        if (course == null) return Enumerable.Empty<Activity>();

        return await _context.Activities
            .Include(a => a.Enrollment)
                .ThenInclude(e => e.Student)
            .Include(a => a.ContentItem)
            .Where(a => a.Enrollment.CourseId == courseId)
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Activity>> GetItemActivitiesAsync(
        Guid contentItemId, Guid instructorId)
    {
        var contentItem = await _context.ContentItems
            .Include(c => c.Section)
                .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(c => c.Id == contentItemId);

        if (contentItem == null ||
            contentItem.Section.Course.InstructorId != instructorId)
            return Enumerable.Empty<Activity>();

        return await _context.Activities
            .Include(a => a.Enrollment)
                .ThenInclude(e => e.Student)
            .Where(a => a.ContentItemId == contentItemId)
            .OrderByDescending(a => a.SubmittedAt)
            .ToListAsync();
    }

    private async Task AwardXPAsync(Enrollment enrollment, int amount,
        XPCategory category, Guid sourceId)
    {
        var transaction = new XPTransaction
        {
            Id = Guid.NewGuid(),
            EnrollmentId = enrollment.Id,
            Amount = amount,
            Category = category,
            SourceId = sourceId,
            CreatedAt = DateTime.UtcNow
        };

        _context.XPTransactions.Add(transaction);

        enrollment.TotalXP += amount;

        var transactions = await _context.XPTransactions
            .Where(t => t.EnrollmentId == enrollment.Id)
            .Select(t => new { t.Category, t.Amount })
            .ToListAsync();

        var tuples = transactions
            .Select(t => (t.Category, t.Amount))
            .ToList();

        tuples.Add((category, amount));

        enrollment.IdentityType = _identityService.Calculate(tuples);
        enrollment.UpdatedAt = DateTime.UtcNow;
    }
}