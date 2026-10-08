using MeritEd.Core.Enums;

namespace MeritEd.Core.Entities;

public class XPTransaction
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public Enrollment Enrollment { get; set; } = null!;
    public int Amount { get; set; }
    public XPCategory Category { get; set; }
    public Guid? SourceId { get; set; }
    public DateTime CreatedAt { get; set; }
}