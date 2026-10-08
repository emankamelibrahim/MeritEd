namespace MeritEd.API.DTOs.Progress;

public class ProgressResponse
{
    public Guid EnrollmentId { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public int TotalXP { get; set; }
    public string? IdentityType { get; set; }
    public DateTime EnrolledAt { get; set; }
    public List<XPBreakdownItem> XPBreakdown { get; set; } = new();
}