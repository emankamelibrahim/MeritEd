using MeritEd.Core.Enums;

namespace MeritEd.Core.Entities;

public class Activity
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public Enrollment Enrollment { get; set; } = null!;
    public Guid ContentItemId { get; set; }
    public ContentItem ContentItem { get; set; } = null!;
    public bool IsRecovery { get; set; }
    public ActivityStatus Status { get; set; }
    public int? XPAwarded { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
}