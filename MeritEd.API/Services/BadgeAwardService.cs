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
    private readonly IdentityService _identityService;

    public BadgeAwardService(AppDbContext context, BadgeService badgeService,
        IdentityService identityService)
    {
        _context = context;
        _badgeService = badgeService;
        _identityService = identityService;
    }

    public async Task EvaluateAndAwardBadgesAsync(Enrollment enrollment, bool isFirstSubmission)
    {
        var allBadges = await _context.Badges.ToListAsync();

        var alreadyEarned = await _context.StudentBadges
            .Where(sb => sb.EnrollmentId == enrollment.Id)
            .Select(sb => sb.BadgeId)
            .ToListAsync();

        var badgesToAward = _badgeService.EvaluateTriggers(
            enrollment, allBadges, alreadyEarned, isFirstSubmission, false).ToList();

        if (badgesToAward.Count == 0)
            return;

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

            _context.XPTransactions.Add(new XPTransaction
            {
                Id = Guid.NewGuid(),
                EnrollmentId = enrollment.Id,
                Amount = badge.XPBonus,
                Category = XPCategory.Badge,
                SourceId = studentBadge.Id,
                CreatedAt = DateTime.UtcNow
            });

            enrollment.TotalXP += badge.XPBonus;
        }

        var rows = await _context.XPTransactions
            .Where(t => t.EnrollmentId == enrollment.Id)
            .Select(t => new { t.Category, t.Amount })
            .ToListAsync();

        var tuples = rows.Select(t => (t.Category, t.Amount)).ToList();
        foreach (var badge in badgesToAward)
            tuples.Add((XPCategory.Badge, badge.XPBonus));

        enrollment.IdentityType = _identityService.Calculate(tuples);
        enrollment.UpdatedAt = DateTime.UtcNow;

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