using System.ComponentModel.DataAnnotations;
using MeritEd.Core.Enums;

namespace MeritEd.API.DTOs.Courses;

public class UpdateCourseRequest
{
    [Required, MinLength(3), MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [Required]
    public CourseStatus Status { get; set; }

    [Range(0.1, 1.0)]
    public decimal RecoveryMultiplier { get; set; } = 0.70m;
}