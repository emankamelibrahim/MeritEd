namespace MeritEd.API.DTOs.Badges;

public class StudentBadgeResponse
{
    public Guid Id { get; set; }
    public Guid BadgeId { get; set; }
    public string BadgeName { get; set; } = string.Empty;
    public string BadgeDescription { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int XPBonus { get; set; }
    public DateTime EarnedAt { get; set; }
}