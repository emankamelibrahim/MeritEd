using Microsoft.EntityFrameworkCore;
using MeritEd.API.Data;
using MeritEd.Core.Entities;
using MeritEd.Core.Enums;
using MeritEd.Core.Interfaces.Services;
using MeritEd.Core.Services;

namespace MeritEd.API.Services;

public class BadgeAwardService : IBadgeAwardService
{
    private readonly AppDbContext _context;
    private readonly BadgeService _badgeService;

    public BadgeAwardService(AppDbContext context, BadgeService badgeService)
    {
        _context = context;
        _badgeService = badgeService;
    }

    public async Task EvaluateAndAwardBadgesAsync(Enrollment enrollment, bool isFirstSubmission)
    {
        var allBadges = await _context.Badges.ToListAsync();

        var alreadyEarned = await _context.StudentBadges
            .Where(sb => sb.EnrollmentId == enrollment.Id)
            .Select(sb => sb.BadgeId)
            .ToListAsync();

        var badgesToAward = _badgeService.EvaluateTriggers(
            enrollment, allBadges, alreadyEarned, isFirstSubmission, false);

        foreach (var badge in badgesToAward)
        {
            var studentBadge = new StudentBadge
            {
                Id = Guid.NewGuid(),
                EnrollmentId = enrollment.Id,
                BadgeId = badge.Id,
                EarnedAt = DateTime.UtcNow
            };

            _context.StudentBadges.Add(studentBadge);

            // Award XP bonus
            var transaction = new XPTransaction
            {
                Id = Guid.NewGuid(),
                EnrollmentId = enrollment.Id,
                Amount = badge.XPBonus,
                Category = XPCategory.Badge,
                SourceId = studentBadge.Id,
                CreatedAt = DateTime.UtcNow
            };

            _context.XPTransactions.Add(transaction);
            enrollment.TotalXP += badge.XPBonus;
            enrollment.UpdatedAt = DateTime.UtcNow;
        }

        if (badgesToAward.Any())
            await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<StudentBadge>> GetStudentBadgesAsync(Guid enrollmentId)
    {
        return await _context.StudentBadges
            .Include(sb => sb.Badge)
            .Where(sb => sb.EnrollmentId == enrollmentId)
            .OrderBy(sb => sb.EarnedAt)
            .ToListAsync();
    }
}