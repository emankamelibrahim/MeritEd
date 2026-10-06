using System.ComponentModel.DataAnnotations;

namespace MeritEd.API.DTOs.Courses;

public class CreateCourseRequest
{
    [Required, MinLength(3), MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }
}