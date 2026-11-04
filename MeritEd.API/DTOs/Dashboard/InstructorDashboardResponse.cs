namespace MeritEd.API.DTOs.Dashboard;

public class InstructorDashboardResponse
{
    public List<InstructorCourseSummary> Courses { get; set; } = new();
}

public class InstructorCourseSummary
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int StudentCount { get; set; }
    public decimal MedianXP { get; set; }
    public int OutlierCount { get; set; }
}