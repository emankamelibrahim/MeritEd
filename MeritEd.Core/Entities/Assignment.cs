namespace MeritEd.Core.Entities;

public class Assignment : ContentItem
{
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }
    public int BaseXP { get; set; }
    public bool AllowsRecovery { get; set; }
}