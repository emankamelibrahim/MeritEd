using MeritEd.API.DTOs.Progress;

namespace MeritEd.API.DTOs.Dashboard;

public class CourseDashboardResponse
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public decimal MedianXP { get; set; }
    public double AverageXP { get; set; }
    public Dictionary<string, int> IdentityDistribution { get; set; } = new();
    public List<StudentRow> Students { get; set; } = new();
}

public class StudentRow
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public int TotalXP { get; set; }
    public string? IdentityType { get; set; }
    public string Outlier { get; set; } = "Normal";
    public List<XPBreakdownItem> XPBreakdown { get; set; } = new();
}