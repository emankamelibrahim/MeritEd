namespace MeritEd.API.DTOs.Enrollments;

public class EnrollmentResponse
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int TotalXP { get; set; }
    public string? IdentityType { get; set; }
    public DateTime EnrolledAt { get; set; }
}