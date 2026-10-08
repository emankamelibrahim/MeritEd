namespace MeritEd.API.DTOs.Progress;

public class XPBreakdownItem
{
    public string Category { get; set; } = string.Empty;
    public int TotalXP { get; set; }
    public double Percentage { get; set; }
}