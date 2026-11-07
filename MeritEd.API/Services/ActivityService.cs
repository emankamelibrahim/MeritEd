using Microsoft.EntityFrameworkCore;
using Npgsql;
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
    private readonly QuizGradingService _quizGrading;
    private readonly IBadgeAwardService _badgeAwardService;

    public ActivityService(AppDbContext context, XPService xpService,
        IdentityService identityService, QuizGradingService quizGrading,
        IBadgeAwardService badgeAwardService)
    {
        _context = context;
        _xpService = xpService;
        _identityService = identityService;
        _quizGrading = quizGrading;
        _badgeAwardService = badgeAwardService;
    }

    public async Task<(bool Success, string Error, Activity? Activity)> SubmitAsync(
        Guid studentId, Guid contentItemId, IReadOnlyList<int>? answers)
    {
        var contentItem = await _context.ContentItems
            .Include(c => c.Section)
                .ThenInclude(s => s.Course)
            .FirstOrDefaultAsync(c => c.Id == contentItemId);

        if (contentItem == null)
            return (false, "Content item not found.", null);

        if (!contentItem.IsPublished || !contentItem.Section.IsPublished)
            return (false, "This content item is not available.", null);

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId
                && e.CourseId == contentItem.Section.CourseId
                && e.Status == EnrollmentStatus.Active);

        if (enrollment == null)
            return (false, "You are not enrolled in this course.", null);

        if (contentItem is not Assignment && contentItem is not Quiz)
            return (false, "This content item cannot be submitted.", null);

        var (dueDate, allowsRecovery) = contentItem switch
        {
            Assignment asg => (asg.DueDate, asg.AllowsRecovery),
            Quiz qz => (qz.DueDate, qz.AllowsRecovery),
            _ => ((DateTime?)null, false)
        };

        var isPastDue = dueDate.HasValue && DateTime.UtcNow > dueDate.Value;
        if (isPastDue && !allowsRecovery)
            return (false, "The deadline has passed and this activity does not accept late work.", null);

        var isRecovery = _xpService.IsRecoveryEligible(dueDate, allowsRecovery);

        var alreadySubmitted = await _context.Activities.AnyAsync(a =>
            a.EnrollmentId == enrollment.Id
            && a.ContentItemId == contentItemId
            && a.Status != ActivityStatus.Rejected);

        if (alreadySubmitted)
            return (false, "You have already submitted this activity.", null);

        var quiz = contentItem as Quiz;
        int[]? selected = null;
        int correct = 0, total = 0;

        if (quiz != null)
        {
            if (quiz.Questions == null)
                return (false, "This quiz has no questions yet.", null);

            var (valid, _, questions) = _quizGrading.Parse(quiz.Questions.RootElement);
            if (!valid)
                return (false, "This quiz is not set up correctly. Please contact your instructor.", null);

            if (answers == null)
                return (false, "Answers are required to submit a quiz.", null);

            var (gradable, gradeError, correctCount) = _quizGrading.Grade(questions, answers);
            if (!gradable)
                return (false, gradeError, null);

            correct = correctCount;
            total = questions.Count;
            selected = answers.ToArray();
        }

        var activity = new Activity
        {
            Id = Guid.NewGuid(),
            EnrollmentId = enrollment.Id,
            ContentItemId = contentItemId,
            IsRecovery = isRecovery,
            Status = ActivityStatus.Submitted,
            SubmittedAt = DateTime.UtcNow,
            CorrectAnswers = quiz != null ? correct : null,
            TotalQuestions = quiz != null ? total : null,
            SelectedAnswers = selected
        };

        _context.Activities.Add(activity);

        if (quiz != null)
        {
            var xp = _xpService.CalculateScoredXP(quiz.BaseXP, correct, total,
                isRecovery, contentItem.Section.Course.RecoveryMultiplier);

            activity.Status = ActivityStatus.Approved;
            activity.XPAwarded = xp;
            activity.GradedAt = DateTime.UtcNow;

            if (xp > 0)
                await AwardXPAsync(enrollment, xp,
                    isRecovery ? XPCategory.Recovery : XPCategory.Assessment, activity.Id);
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return (false, "You have already submitted this activity.", null);
        }

        if (quiz != null)
            await EvaluateBadgesAsync(enrollment);

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

        var baseXP = activity.ContentItem switch
        {
            Assignment assignment => assignment.BaseXP,
            Quiz quiz => quiz.BaseXP,
            _ => 0
        };

        var xp = _xpService.CalculateXP(baseXP, activity.IsRecovery,
            activity.Enrollment.Course.RecoveryMultiplier);

        activity.Status = ActivityStatus.Approved;
        activity.XPAwarded = xp;
        activity.GradedAt = DateTime.UtcNow;

        if (xp > 0)
            await AwardXPAsync(activity.Enrollment, xp,
                activity.IsRecovery ? XPCategory.Recovery : XPCategory.Assessment, activity.Id);

        await _context.SaveChangesAsync();
        await EvaluateBadgesAsync(activity.Enrollment);

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

    private async Task EvaluateBadgesAsync(Enrollment enrollment)
    {
        var isFirstSubmission = await _context.Activities
            .CountAsync(a => a.EnrollmentId == enrollment.Id
                && a.Status == ActivityStatus.Approved) == 1;

        await _badgeAwardService.EvaluateAndAwardBadgesAsync(enrollment, isFirstSubmission);
    }

    private async Task AwardXPAsync(Enrollment enrollment, int amount,
        XPCategory category, Guid sourceId)
    {
        _context.XPTransactions.Add(new XPTransaction
        {
            Id = Guid.NewGuid(),
            EnrollmentId = enrollment.Id,
            Amount = amount,
            Category = category,
            SourceId = sourceId,
            CreatedAt = DateTime.UtcNow
        });

        enrollment.TotalXP += amount;

        var rows = await _context.XPTransactions
            .Where(t => t.EnrollmentId == enrollment.Id)
            .Select(t => new { t.Category, t.Amount })
            .ToListAsync();

        var tuples = rows.Select(t => (t.Category, t.Amount)).ToList();
        tuples.Add((category, amount));

        enrollment.IdentityType = _identityService.Calculate(tuples);
        enrollment.UpdatedAt = DateTime.UtcNow;
    }
}   