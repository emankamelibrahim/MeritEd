using MeritEd.Core.Enums;

namespace MeritEd.API.DTOs.Badges;

public class BadgeResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int XPBonus { get; set; }
    public string TriggerType { get; set; } = string.Empty;
    public int TriggerValue { get; set; }
    public DateTime CreatedAt { get; set; }
}