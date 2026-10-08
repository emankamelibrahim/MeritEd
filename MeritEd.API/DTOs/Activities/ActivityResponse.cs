namespace MeritEd.API.DTOs.Activities;

public class ActivityResponse
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid ContentItemId { get; set; }
    public string ContentItemTitle { get; set; } = string.Empty;
    public string ContentItemType { get; set; } = string.Empty;
    public bool IsRecovery { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? XPAwarded { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
    public string? StudentName { get; set; }
    public string? StudentEmail { get; set; }
}