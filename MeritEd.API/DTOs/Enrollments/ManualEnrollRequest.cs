using System.ComponentModel.DataAnnotations;

namespace MeritEd.API.DTOs.Enrollments;

public class ManualEnrollRequest
{
    [Required]
    public Guid StudentId { get; set; }
}