using MeritEd.Core.Enums;

namespace MeritEd.Core.Entities;

public class Enrollment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public EnrollmentStatus Status { get; set; }
    public int TotalXP { get; set; }
    public IdentityType? IdentityType { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}