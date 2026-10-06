using MeritEd.Core.Enums;

namespace MeritEd.Core.Entities;

public class Course
{
    public Guid Id { get; set; }
    public Guid InstructorId { get; set; }
    public User Instructor { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public CourseStatus Status { get; set; }
    public decimal RecoveryMultiplier { get; set; } = 0.70m;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Section> Sections { get; set; } = new List<Section>();
}