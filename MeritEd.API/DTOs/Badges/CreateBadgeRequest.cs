using System.ComponentModel.DataAnnotations;
using MeritEd.Core.Enums;

namespace MeritEd.API.DTOs.Badges;

public class CreateBadgeRequest
{
    [Required, MinLength(3), MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    public string? IconUrl { get; set; }

    [Required, Range(0, 500)]
    public int XPBonus { get; set; }

    [Required]
    public BadgeTriggerType TriggerType { get; set; }

    [Required]
    public int TriggerValue { get; set; }
}