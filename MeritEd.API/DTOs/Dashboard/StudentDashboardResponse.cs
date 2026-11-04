using MeritEd.API.DTOs.Badges;
using MeritEd.API.DTOs.Progress;

namespace MeritEd.API.DTOs.Dashboard;

public class StudentDashboardResponse
{
    public List<StudentCourseSummary> Courses { get; set; } = new();
}

public class StudentCourseSummary
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int TotalXP { get; set; }
    public string? IdentityType { get; set; }
    public List<XPBreakdownItem> XPBreakdown { get; set; } = new();
    public List<StudentBadgeResponse> Badges { get; set; } = new();
}