using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MeritEd.API.Data;
using MeritEd.API.DTOs.Badges;
using MeritEd.Core.Entities;
using MeritEd.Core.Enums;
using MeritEd.Core.Interfaces.Services;

namespace MeritEd.API.Controllers;

[ApiController]
[Route("api/badges")]
[Authorize]
public class BadgesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IBadgeAwardService _badgeAwardService;

    public BadgesController(AppDbContext context, IBadgeAwardService badgeAwardService)
    {
        _context = context;
        _badgeAwardService = badgeAwardService;
    }

    private Guid GetCurrentUserId()
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(sub!);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBadges()
    {
        var badges = await _context.Badges
            .OrderBy(b => b.TriggerType)
            .ThenBy(b => b.TriggerValue)
            .ToListAsync();

        return Ok(badges.Select(MapToResponse));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateBadge(CreateBadgeRequest request)
    {
        var badge = new Badge
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            IconUrl = request.IconUrl,
            XPBonus = request.XPBonus,
            TriggerType = request.TriggerType,
            TriggerValue = request.TriggerValue,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Badges.Add(badge);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAllBadges), MapToResponse(badge));
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateBadge(Guid id, CreateBadgeRequest request)
    {
        var badge = await _context.Badges.FirstOrDefaultAsync(b => b.Id == id);
        if (badge == null) return NotFound();

        badge.Name = request.Name;
        badge.Description = request.Description;
        badge.IconUrl = request.IconUrl;
        badge.XPBonus = request.XPBonus;
        badge.TriggerType = request.TriggerType;
        badge.TriggerValue = request.TriggerValue;
        badge.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Ok(MapToResponse(badge));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBadge(Guid id)
    {
        var badge = await _context.Badges.FirstOrDefaultAsync(b => b.Id == id);
        if (badge == null) return NotFound();

        _context.Badges.Remove(badge);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Badge deleted successfully." });
    }

    [HttpGet("courses/{courseId}/my-badges")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyBadges(Guid courseId)
    {
        var studentId = GetCurrentUserId();

        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentId == studentId
                && e.CourseId == courseId
                && e.Status == EnrollmentStatus.Active);

        if (enrollment == null)
            return NotFound(new { error = "You are not enrolled in this course." });

        var badges = await _badgeAwardService.GetStudentBadgesAsync(enrollment.Id);

        return Ok(badges.Select(sb => new StudentBadgeResponse
        {
            Id = sb.Id,
            BadgeId = sb.BadgeId,
            BadgeName = sb.Badge.Name,
            BadgeDescription = sb.Badge.Description,
            IconUrl = sb.Badge.IconUrl,
            XPBonus = sb.Badge.XPBonus,
            EarnedAt = sb.EarnedAt
        }));
    }

    private static BadgeResponse MapToResponse(Badge badge) => new()
    {
        Id = badge.Id,
        Name = badge.Name,
        Description = badge.Description,
        IconUrl = badge.IconUrl,
        XPBonus = badge.XPBonus,
        TriggerType = badge.TriggerType.ToString(),
        TriggerValue = badge.TriggerValue,
        CreatedAt = badge.CreatedAt
    };
}