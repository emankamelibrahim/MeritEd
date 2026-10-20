using MeritEd.Core.Enums;

namespace MeritEd.Core.Entities;

public class Badge
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int XPBonus { get; set; }
    public BadgeTriggerType TriggerType { get; set; }
    public int TriggerValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}