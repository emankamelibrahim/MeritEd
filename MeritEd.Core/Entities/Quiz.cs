using System.Text.Json;

namespace MeritEd.Core.Entities;

public class Quiz : ContentItem
{
    public DateTime? DueDate { get; set; }
    public int BaseXP { get; set; }
    public bool AllowsRecovery { get; set; }
    public JsonDocument? Questions { get; set; }
}